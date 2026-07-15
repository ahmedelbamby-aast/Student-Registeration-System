using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Academics;

namespace StudentRegistration.Academics.Application.Ports;

public sealed record AdminTermQuery(
    string? Query,
    TermState? State,
    int Page,
    int PageSize,
    string Sort);

public sealed record AdminStudentLocatorQuery(
    Guid TermId,
    string Query,
    int Page,
    int PageSize,
    string Sort);

public enum AdminAcademicStoreOutcome
{
    Succeeded,
    StorageUnavailable
}

public sealed record AdminAcademicStorePage<T>(
    AdminAcademicStoreOutcome Outcome,
    IReadOnlyList<T> Items,
    int TotalCount)
    where T : class;

public interface IAdminAcademicStore
{
    /// <summary>
    /// Applies the normalized filter and sort, including the ID tie-breaker,
    /// before taking the requested page.
    /// </summary>
    Task<AdminAcademicStorePage<AdminTermDto>> ListTermsAsync(
        AdminTermQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches only within the named term and applies the normalized sort,
    /// including the Student ID tie-breaker, before taking the requested page.
    /// </summary>
    Task<AdminAcademicStorePage<AdminStudentLocatorDto>> ListStudentsAsync(
        AdminStudentLocatorQuery query,
        CancellationToken cancellationToken = default);
}
