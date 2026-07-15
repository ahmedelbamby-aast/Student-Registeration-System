using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Academics;

namespace StudentRegistration.Academics.Application;

public enum AdminAcademicQueryOutcome
{
    Succeeded,
    PageSizeInvalid,
    ValidationError,
    StorageUnavailable
}

public sealed record AdminAcademicQueryResult<T>(
    AdminAcademicQueryOutcome Outcome,
    Page<T>? Page = null)
    where T : class
{
    public string? ErrorCode => Outcome switch
    {
        AdminAcademicQueryOutcome.PageSizeInvalid => "PAGE_SIZE_INVALID",
        AdminAcademicQueryOutcome.ValidationError => "VALIDATION_ERROR",
        AdminAcademicQueryOutcome.StorageUnavailable => "CONTEXT_UNAVAILABLE",
        _ => null
    };
}

public sealed class AdminAcademicManagementService
{
    public const int DefaultPageSize = 20;
    public const int MaximumPageSize = 100;
    public const int MinimumQueryLength = 3;
    public const int MaximumQueryLength = 50;
    public const int MinimumSearchLength = 3;
    public const int MaximumSearchLength = 50;

    private const string DefaultTermSort = "code,id";
    private const string TeachingStartsOnSort = "teachingStartsOn,id";
    private const string TermStateSort = "state,id";
    private const string DefaultStudentSort = "universityId,studentId";
    private const string ProgramSort = "program,studentId";
    private const string CohortSort = "cohort,studentId";
    private const string StandingSort = "standing,studentId";

    private readonly IAdminAcademicStore _store;
    private readonly RegistrationWindowService _termOwner;
    private readonly StudentAcademicProfileService _profileOwner;

    public AdminAcademicManagementService(
        IAdminAcademicStore store,
        RegistrationWindowService termOwner,
        StudentAcademicProfileService profileOwner)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _termOwner = termOwner ?? throw new ArgumentNullException(nameof(termOwner));
        _profileOwner = profileOwner ?? throw new ArgumentNullException(nameof(profileOwner));
    }

    public async Task<AdminAcademicQueryResult<AdminTermDto>> ListTermsAsync(
        string? query = null,
        int page = 1,
        int pageSize = DefaultPageSize,
        TermState? state = null,
        string? sort = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidPage(page, pageSize))
        {
            return new(AdminAcademicQueryOutcome.PageSizeInvalid);
        }

        if (!TryNormalizeOptionalQuery(query, out var normalizedQuery) ||
            state is not null && !Enum.IsDefined(state.Value) ||
            !TryNormalizeTermSort(sort, out var normalizedSort))
        {
            return new(AdminAcademicQueryOutcome.ValidationError);
        }

        var stored = await _store.ListTermsAsync(
            new AdminTermQuery(
                normalizedQuery,
                state,
                page,
                pageSize,
                normalizedSort),
            cancellationToken);
        if (!IsUsable(stored, pageSize))
        {
            return new(AdminAcademicQueryOutcome.StorageUnavailable);
        }

        var items = OrderTerms(stored.Items, normalizedSort).ToArray();
        return new(
            AdminAcademicQueryOutcome.Succeeded,
            new Page<AdminTermDto>(
                items,
                page,
                pageSize,
                stored.TotalCount,
                normalizedSort));
    }

    public async Task<AdminAcademicQueryResult<AdminStudentLocatorDto>> ListStudentsAsync(
        Guid termId,
        string? query,
        int page = 1,
        int pageSize = DefaultPageSize,
        string? sort = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidPage(page, pageSize))
        {
            return new(AdminAcademicQueryOutcome.PageSizeInvalid);
        }

        if (termId == Guid.Empty ||
            !TryNormalizeRequiredSearch(query, out var normalizedQuery) ||
            !TryNormalizeStudentSort(sort, out var normalizedSort))
        {
            return new(AdminAcademicQueryOutcome.ValidationError);
        }

        var stored = await _store.ListStudentsAsync(
            new AdminStudentLocatorQuery(
                termId,
                normalizedQuery,
                page,
                pageSize,
                normalizedSort),
            cancellationToken);
        if (!IsUsable(stored, pageSize))
        {
            return new(AdminAcademicQueryOutcome.StorageUnavailable);
        }

        var items = OrderStudents(stored.Items, normalizedSort).ToArray();
        return new(
            AdminAcademicQueryOutcome.Succeeded,
            new Page<AdminStudentLocatorDto>(
                items,
                page,
                pageSize,
                stored.TotalCount,
                normalizedSort));
    }

    public Task<AcademicTermCommandResult> CreateTermAsync(
        CreateTermRequest request,
        AcademicCommandContext context,
        CancellationToken cancellationToken = default) =>
        _termOwner.CreateTermAsync(request, context, cancellationToken);

    public Task<AcademicTermCommandResult> UpdateTermAsync(
        Guid termId,
        UpdateTermRequest request,
        AcademicCommandContext context,
        CancellationToken cancellationToken = default) =>
        _termOwner.UpdateTermAsync(termId, request, context, cancellationToken);

    public Task<AcademicTermCommandResult> PublishTermRegistrationWindowAsync(
        Guid termId,
        Guid windowId,
        PublishRegistrationWindowRequest request,
        AcademicCommandContext context,
        CancellationToken cancellationToken = default) =>
        _termOwner.PublishAsync(termId, windowId, request, context, cancellationToken);

    public Task<AcademicProfileResult<AdminStudentAcademicContextDto>> ReadStudentProfileAsync(
        Guid studentId,
        Guid termId,
        int transcriptPage = 1,
        int transcriptPageSize = DefaultPageSize,
        int provenancePage = 1,
        int provenancePageSize = DefaultPageSize,
        CancellationToken cancellationToken = default) =>
        _profileOwner.ReadAdminAsync(
            studentId,
            termId,
            transcriptPage,
            transcriptPageSize,
            provenancePage,
            provenancePageSize,
            cancellationToken);

    public Task<AcademicProfileResult<AdminStudentAcademicContextDto>> CorrectStudentProfileAsync(
        Guid studentId,
        AcademicProfileCorrectionRequest request,
        AcademicCommandContext context,
        CancellationToken cancellationToken = default) =>
        _profileOwner.CorrectProfileAsync(studentId, request, context, cancellationToken);

    private static bool IsUsable<T>(
        AdminAcademicStorePage<T> stored,
        int pageSize)
        where T : class =>
        stored is not null &&
        stored.Outcome is AdminAcademicStoreOutcome.Succeeded &&
        stored.Items is not null &&
        stored.Items.Count <= pageSize &&
        stored.TotalCount >= stored.Items.Count &&
        stored.Items.All(item => item is not null);

    private static IOrderedEnumerable<AdminTermDto> OrderTerms(
        IEnumerable<AdminTermDto> items,
        string sort) => sort switch
        {
            TeachingStartsOnSort => items
                .OrderBy(item => item.TeachingStartsOn)
                .ThenBy(item => item.Id, StringComparer.Ordinal),
            TermStateSort => items
                .OrderBy(item => item.State)
                .ThenBy(item => item.Id, StringComparer.Ordinal),
            _ => items
                .OrderBy(item => item.Code, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.Id, StringComparer.Ordinal)
        };

    private static IOrderedEnumerable<AdminStudentLocatorDto> OrderStudents(
        IEnumerable<AdminStudentLocatorDto> items,
        string sort) => sort switch
        {
            ProgramSort => items
                .OrderBy(item => item.ProgramCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.StudentId, StringComparer.Ordinal),
            CohortSort => items
                .OrderBy(item => item.Cohort, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.StudentId, StringComparer.Ordinal),
            StandingSort => items
                .OrderBy(item => item.Standing, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.StudentId, StringComparer.Ordinal),
            _ => items
                .OrderBy(item => item.UniversityId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.StudentId, StringComparer.Ordinal)
        };

    private static bool TryNormalizeOptionalQuery(
        string? query,
        out string? normalized)
    {
        if (query is null)
        {
            normalized = null;
            return true;
        }

        normalized = query.Trim();
        return normalized.Length is >= MinimumQueryLength and <= MaximumQueryLength;
    }

    private static bool TryNormalizeRequiredSearch(
        string? query,
        out string normalized)
    {
        normalized = query?.Trim() ?? string.Empty;
        return normalized.Length is >= MinimumSearchLength and <= MaximumSearchLength;
    }

    private static bool TryNormalizeTermSort(string? sort, out string normalized) =>
        TryNormalizeSort(
            sort,
            DefaultTermSort,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["code"] = DefaultTermSort,
                [DefaultTermSort] = DefaultTermSort,
                ["teachingStartsOn"] = TeachingStartsOnSort,
                [TeachingStartsOnSort] = TeachingStartsOnSort,
                ["state"] = TermStateSort,
                [TermStateSort] = TermStateSort
            },
            out normalized);

    private static bool TryNormalizeStudentSort(string? sort, out string normalized) =>
        TryNormalizeSort(
            sort,
            DefaultStudentSort,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["universityId"] = DefaultStudentSort,
                [DefaultStudentSort] = DefaultStudentSort,
                ["program"] = ProgramSort,
                [ProgramSort] = ProgramSort,
                ["cohort"] = CohortSort,
                [CohortSort] = CohortSort,
                ["standing"] = StandingSort,
                [StandingSort] = StandingSort
            },
            out normalized);

    private static bool TryNormalizeSort(
        string? sort,
        string defaultSort,
        IReadOnlyDictionary<string, string> allowedSorts,
        out string normalized)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            normalized = defaultSort;
            return true;
        }

        return allowedSorts.TryGetValue(sort.Trim(), out normalized!);
    }

    private static bool IsValidPage(int page, int pageSize) =>
        page >= 1 && pageSize is >= 1 and <= MaximumPageSize;
}
