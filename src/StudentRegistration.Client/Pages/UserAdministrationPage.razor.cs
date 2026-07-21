using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using StudentRegistration.Client.Components.Forms;
using StudentRegistration.Client.Features.Identity;
using StudentRegistration.Client.Features.Academics;
using StudentRegistration.Client.Features.Frontend.Models;
using StudentRegistration.Client.Localization;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Identity;

namespace StudentRegistration.Client.Pages;

public partial class UserAdministrationPage : ComponentBase
{
    private const int PageSize = 20;
    private const string DefaultSort = "displayName,id";

    private static readonly string[] AssignableRoles =
        ["Admin", "Lecturer", "TeachingAssistant"];

    private static readonly JsonSerializerOptions ImportJsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    private static readonly string InitialImportRows =
        """
        [
          {
            "externalReference": "DEMO-STUDENT-001",
            "kind": "student",
            "universityId": "20260001",
            "userName": null,
            "staffNumber": null,
            "displayName": "Demo Student",
            "roles": []
          }
        ]
        """;

    [Inject]
    private IdentityApiClient IdentityApi { get; set; } = null!;

    [Inject]
    private AcademicApiClient AcademicApi { get; set; } = null!;

    private AppButton? _statusChangeButton;
    private IReadOnlyList<IdentityUserSummaryDto> _users = [];
    private IdentityUserSummaryDto? _selectedUser;
    private HashSet<string> _selectedRoles = new(StringComparer.Ordinal);
    private string _search = string.Empty;
    private int _currentPage = 1;
    private int _totalCount;
    private bool _isLoadingUsers;
    private bool _isChangingStatus;
    private bool _isReplacingRoles;
    private bool _isCreatingImport;
    private bool _isRefreshingImport;
    private bool _isPublishingImport;
    private bool _showStatusConfirmation;
    private string _statusReason = string.Empty;
    private string _roleReason = string.Empty;
    private IReadOnlyList<AccessibleValidationSummary.ValidationItem> _statusErrors = [];
    private IReadOnlyList<AccessibleValidationSummary.ValidationItem> _roleErrors = [];
    private IReadOnlyList<AccessibleValidationSummary.ValidationItem> _importValidationErrors = [];
    private string _importSource = "demo-identity-import.json";
    private string _importRowsJson = InitialImportRows;
    private IReadOnlyList<IdentityImportUserRequest> _importPreview = [];
    private string? _importContentHash;
    private string? _createImportClientRequestId;
    private string? _publishImportClientRequestId;
    private IdentityImportBatchDto? _importBatch;
    private string? _feedbackCode;
    private string? _feedbackMessage;
    private string _feedbackHeading = LocalizedUiText.Get("Administration status");
    private string? _feedbackState;
    private string? _correlationId;
    private bool _feedbackIsError;
    private FrontendAppContextView? _shellContext;
    private bool _isLoadingContext = true;

    private bool IsBusy =>
        _isLoadingUsers ||
        _isChangingStatus ||
        _isReplacingRoles ||
        _isCreatingImport ||
        _isRefreshingImport ||
        _isPublishingImport;

    private int TotalPages => Math.Max(1, (int)Math.Ceiling(_totalCount / (double)PageSize));

    private bool IsStudentIdentity =>
        _selectedUser?.Roles.Contains("Student", StringComparer.Ordinal) == true;

    private bool CanPublishImport =>
        _importBatch is not null &&
        string.Equals(_importBatch.State, "validated", StringComparison.Ordinal);

    private string StatusConfirmationTitle => _selectedUser?.Enabled == true
        ? LocalizedUiText.Get("Disable this account?")
        : LocalizedUiText.Get("Enable this account?");

    private string StatusConfirmationMessage => _selectedUser?.Enabled == true
        ? LocalizedUiText.Get("The account will be signed out on every replica. The final enabled Admin cannot be disabled.")
        : LocalizedUiText.Get("The account will become eligible for its server-authorized roles.");

    protected override async Task OnInitializedAsync()
    {
        var context = await AcademicApi.GetAppContextAsync();
        if (context.IsSuccess && context.Value is not null)
        {
            _shellContext = AcademicContextViewMapper.ToView(context.Value);
        }
        _isLoadingContext = false;
        await LoadUsersAsync(1);
    }

    private Task SearchAsync() => LoadUsersAsync(1);

    private async Task ClearSearchAsync()
    {
        _search = string.Empty;
        await LoadUsersAsync(1);
    }

    private Task ChangePageAsync(int page) => LoadUsersAsync(page);

    private async Task LoadUsersAsync(int page, Guid? selectUserId = null)
    {
        if (_isLoadingUsers || page < 1)
        {
            return;
        }

        _isLoadingUsers = true;
        try
        {
            var selectedId = selectUserId ?? _selectedUser?.Id;
            var result = await IdentityApi.ListAdminUsersAsync(
                _search,
                page,
                PageSize,
                DefaultSort);
            if (!result.IsSuccess || result.Value is null)
            {
                ApplyFailure(result.Error, result.StatusCode);
                return;
            }

            _users = result.Value.Items;
            _currentPage = result.Value.PageNumber;
            _totalCount = result.Value.TotalCount;
            if (_currentPage > TotalPages)
            {
                _currentPage = TotalPages;
            }

            if (selectedId is not null)
            {
                var refreshed = _users.FirstOrDefault(user => user.Id == selectedId);
                if (refreshed is not null)
                {
                    SelectUser(refreshed);
                }
                else
                {
                    ClearSelection();
                }
            }
        }
        catch (HttpRequestException)
        {
            ApplyFailure(null, HttpStatusCode.ServiceUnavailable);
        }
        finally
        {
            _isLoadingUsers = false;
        }
    }

    private void SelectUser(IdentityUserSummaryDto user)
    {
        _selectedUser = user;
        _selectedRoles = user.Roles
            .Where(role => AssignableRoles.Contains(role, StringComparer.Ordinal))
            .ToHashSet(StringComparer.Ordinal);
        _statusReason = string.Empty;
        _roleReason = string.Empty;
        _statusErrors = [];
        _roleErrors = [];
    }

    private void ClearSelection()
    {
        _selectedUser = null;
        _selectedRoles.Clear();
        _statusErrors = [];
        _roleErrors = [];
    }

    private async Task RefreshSelectedUserAsync()
    {
        if (_selectedUser is not null)
        {
            await LoadUsersAsync(_currentPage, _selectedUser.Id);
        }
    }

    private void OpenStatusConfirmation()
    {
        _statusErrors = [];
        if (_selectedUser is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_statusReason))
        {
            _statusErrors =
            [
                new AccessibleValidationSummary.ValidationItem(
                    "status-change-reason",
                    LocalizedUiText.Get("Enter a reason for the status change."))
            ];
            return;
        }

        _showStatusConfirmation = true;
    }

    private void CloseStatusConfirmation()
    {
        _showStatusConfirmation = false;
    }

    private async Task ChangeStatusAsync()
    {
        if (_selectedUser is null || _isChangingStatus)
        {
            return;
        }

        _isChangingStatus = true;
        try
        {
            var result = await IdentityApi.ChangeAdminUserStatusAsync(
                _selectedUser.Id,
                new UserStatusRequest(
                    !_selectedUser.Enabled,
                    _selectedUser.RowVersion,
                    _statusReason));
            if (result.IsSuccess && result.Value is not null)
            {
                UpdateUser(result.Value);
                _statusReason = string.Empty;
                ApplySuccess(
                    LocalizedUiText.Get("Account status updated"),
                    LocalizedUiText.Get("The server accepted and audited the status change."));
                return;
            }

            ApplyFailure(result.Error, result.StatusCode);
        }
        catch (HttpRequestException)
        {
            ApplyFailure(null, HttpStatusCode.ServiceUnavailable);
        }
        finally
        {
            _showStatusConfirmation = false;
            _isChangingStatus = false;
        }
    }

    private async Task RestoreStatusFocusAsync()
    {
        if (_statusChangeButton is not null) await _statusChangeButton.FocusAsync();
    }

    private void ToggleRole(string role, ChangeEventArgs args)
    {
        if (!AssignableRoles.Contains(role, StringComparer.Ordinal) || IsBusy)
        {
            return;
        }

        var selected = args.Value is true ||
            bool.TryParse(args.Value?.ToString(), out var parsed) && parsed;
        if (selected)
        {
            _selectedRoles.Add(role);
        }
        else
        {
            _selectedRoles.Remove(role);
        }
    }

    private async Task ReplaceRolesAsync()
    {
        _roleErrors = [];
        if (_selectedUser is null || IsStudentIdentity || _isReplacingRoles)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_roleReason))
        {
            _roleErrors =
            [
                new AccessibleValidationSummary.ValidationItem(
                    "role-change-reason",
                    LocalizedUiText.Get("Enter a reason for the role change."))
            ];
            return;
        }

        _isReplacingRoles = true;
        try
        {
            var result = await IdentityApi.ReplaceAdminUserRolesAsync(
                _selectedUser.Id,
                new UserRolesRequest(
                    _selectedRoles.Order(StringComparer.Ordinal).ToArray(),
                    _selectedUser.RowVersion,
                    _roleReason));
            if (result.IsSuccess && result.Value is not null)
            {
                UpdateUser(result.Value);
                _roleReason = string.Empty;
                ApplySuccess(
                    LocalizedUiText.Get("Roles updated"),
                    LocalizedUiText.Get("The server accepted and audited the role replacement."));
                return;
            }

            ApplyFailure(result.Error, result.StatusCode);
        }
        catch (HttpRequestException)
        {
            ApplyFailure(null, HttpStatusCode.ServiceUnavailable);
        }
        finally
        {
            _isReplacingRoles = false;
        }
    }

    private void PreviewImport()
    {
        _importValidationErrors = [];
        _importPreview = [];
        _importContentHash = null;
        _importBatch = null;
        _createImportClientRequestId = null;
        _publishImportClientRequestId = null;

        if (string.IsNullOrWhiteSpace(_importSource))
        {
            SetImportError(
                "identity-import-source",
                LocalizedUiText.Get("Enter the import source name."));
            return;
        }

        try
        {
            var rows = JsonSerializer.Deserialize<IdentityImportUserRequest[]>(
                _importRowsJson,
                ImportJsonOptions);
            if (rows is null || rows.Length is < 1 or > 500)
            {
                SetImportError(
                    "identity-import-rows",
                    LocalizedUiText.Get("Provide between 1 and 500 user rows."));
                return;
            }

            if (!TryNormalizeImportRows(rows, out var normalized, out var message))
            {
                SetImportError("identity-import-rows", message);
                return;
            }

            _importPreview = normalized;
            _importContentHash = IdentityImportContentHash.Compute(normalized);
            _createImportClientRequestId = Guid.NewGuid().ToString("N");
            ApplySuccess(
                LocalizedUiText.Get("Import preview ready"),
                LocalizedUiText.Get("Review the bounded rows before server validation."));
        }
        catch (JsonException)
        {
            SetImportError(
                "identity-import-rows",
                LocalizedUiText.Get("Use a JSON array containing only the documented pre-provisioning fields."));
        }
    }

    private async Task CreateImportAsync()
    {
        if (_isCreatingImport ||
            _importPreview.Count == 0 ||
            _importContentHash is null ||
            _createImportClientRequestId is null)
        {
            return;
        }

        _isCreatingImport = true;
        try
        {
            var result = await IdentityApi.CreateAdminImportAsync(
                new IdentityImportRequest(
                    _importSource,
                    _importContentHash,
                    _createImportClientRequestId,
                    _importPreview));
            if (result.IsSuccess && result.Value is not null)
            {
                _importBatch = result.Value;
                _publishImportClientRequestId = Guid.NewGuid().ToString("N");
                ApplySuccess(
                    LocalizedUiText.Get("Import received"),
                    LocalizedUiText.Get("The server returned the authoritative validation state."));
                return;
            }

            ApplyFailure(result.Error, result.StatusCode);
        }
        catch (HttpRequestException)
        {
            ApplyFailure(null, HttpStatusCode.ServiceUnavailable);
        }
        finally
        {
            _isCreatingImport = false;
        }
    }

    private async Task RefreshImportAsync()
    {
        if (_importBatch is null || _isRefreshingImport)
        {
            return;
        }

        _isRefreshingImport = true;
        try
        {
            var result = await IdentityApi.GetAdminImportAsync(_importBatch.Id);
            if (result.IsSuccess && result.Value is not null)
            {
                _importBatch = result.Value;
                return;
            }

            ApplyFailure(result.Error, result.StatusCode);
        }
        catch (HttpRequestException)
        {
            ApplyFailure(null, HttpStatusCode.ServiceUnavailable);
        }
        finally
        {
            _isRefreshingImport = false;
        }
    }

    private async Task PublishImportAsync()
    {
        if (!CanPublishImport || _isPublishingImport || _publishImportClientRequestId is null)
        {
            return;
        }

        _isPublishingImport = true;
        try
        {
            var result = await IdentityApi.PublishAdminImportAsync(
                _importBatch!.Id,
                new IdentityImportPublishRequest(
                    _importBatch.RowVersion,
                    _publishImportClientRequestId));
            if (result.IsSuccess && result.Value is not null)
            {
                _importBatch = result.Value;
                ApplySuccess(
                    LocalizedUiText.Get("Import published"),
                    LocalizedUiText.Get("All validated identities were published atomically."));
                await LoadUsersAsync(1);
                return;
            }

            ApplyFailure(result.Error, result.StatusCode);
        }
        catch (HttpRequestException)
        {
            ApplyFailure(null, HttpStatusCode.ServiceUnavailable);
        }
        finally
        {
            _isPublishingImport = false;
        }
    }

    private void UpdateUser(IdentityUserSummaryDto updated)
    {
        _users = _users.Select(user => user.Id == updated.Id ? updated : user).ToArray();
        SelectUser(updated);
    }

    private void ApplySuccess(string heading, string message)
    {
        _feedbackCode = null;
        _feedbackHeading = heading;
        _feedbackMessage = message;
        _feedbackState = "success";
        _correlationId = null;
        _feedbackIsError = false;
    }

    private void ApplyFailure(ApiError? error, HttpStatusCode statusCode)
    {
        _feedbackCode = error?.Code ?? statusCode switch
        {
            HttpStatusCode.Unauthorized => "UNAUTHORIZED",
            HttpStatusCode.Forbidden => "FORBIDDEN",
            _ => "SERVICE_UNAVAILABLE"
        };
        _feedbackHeading = _feedbackCode switch
        {
            "FINAL_ADMIN_REQUIRED" => LocalizedUiText.Get("Final Admin must remain"),
            "STALE_VERSION" => LocalizedUiText.Get("User information changed"),
            "UNAUTHORIZED" or "FORBIDDEN" => LocalizedUiText.Get("Access unavailable"),
            _ => LocalizedUiText.Get("Identity operation not completed")
        };
        _feedbackMessage = _feedbackCode switch
        {
            "FINAL_ADMIN_REQUIRED" =>
                LocalizedUiText.Get("This change would remove the final enabled Admin. No change was made."),
            "STALE_VERSION" =>
                LocalizedUiText.Get("Refresh and review the current user or import before submitting again."),
            "IMPORT_INVALID" => LocalizedUiText.Get("Correct the import preview and submit it again."),
            "IMPORT_NOT_VALIDATED" => LocalizedUiText.Get("Wait for a validated import before publishing."),
            "IDEMPOTENCY_KEY_REUSED" => LocalizedUiText.Get("The same request key was used for different content."),
            "IMPORT_CONTENT_EXISTS" => LocalizedUiText.Get("This source content was already imported."),
            "UNAUTHORIZED" or "FORBIDDEN" =>
                LocalizedUiText.Get("Your current context cannot manage identities. Return to an authorized Admin context."),
            _ => LocalizedUiText.Get("The operation could not be completed. Retry or use the support path.")
        };
        _feedbackState = IdentityPageFeedback.StateFor(_feedbackCode);
        _correlationId = error?.CorrelationId;
        _feedbackIsError = true;

        if (_feedbackCode is "UNAUTHORIZED" or "FORBIDDEN")
        {
            _users = [];
            _totalCount = 0;
            ClearSelection();
            _importPreview = [];
            _importBatch = null;
        }
    }

    private void SetImportError(string controlId, string message)
    {
        _importValidationErrors =
        [
            new AccessibleValidationSummary.ValidationItem(controlId, message)
        ];
    }

    private static bool TryNormalizeImportRows(
        IReadOnlyList<IdentityImportUserRequest> rows,
        out IReadOnlyList<IdentityImportUserRequest> normalized,
        out string message)
    {
        var output = new List<IdentityImportUserRequest>(rows.Count);
        var references = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var universityIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var userNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var staffNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            if (row is null ||
                string.IsNullOrWhiteSpace(row.ExternalReference) ||
                !references.Add(row.ExternalReference.Trim()) ||
                string.IsNullOrWhiteSpace(row.Kind) ||
                string.IsNullOrWhiteSpace(row.DisplayName) ||
                row.Roles is null)
            {
                normalized = [];
                message = LocalizedUiText.Get("Every row needs a unique external reference, kind, display name, and roles array.");
                return false;
            }

            var kind = row.Kind.Trim().ToLowerInvariant();
            var roles = row.Roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role.Trim())
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();
            if (roles.Any(role => !AssignableRoles.Contains(role, StringComparer.Ordinal)))
            {
                normalized = [];
                message = LocalizedUiText.Get("Staff roles are limited to Admin, Lecturer, and Teaching Assistant.");
                return false;
            }

            if (kind == "student")
            {
                if (string.IsNullOrWhiteSpace(row.UniversityId) ||
                    !universityIds.Add(row.UniversityId.Trim()) ||
                    !string.IsNullOrWhiteSpace(row.UserName) ||
                    !string.IsNullOrWhiteSpace(row.StaffNumber) ||
                    roles.Length != 0)
                {
                    normalized = [];
                    message = LocalizedUiText.Get("Student rows need a unique University ID and no staff fields or roles.");
                    return false;
                }

                output.Add(new IdentityImportUserRequest(
                    row.ExternalReference.Trim(),
                    kind,
                    row.UniversityId.Trim().ToUpperInvariant(),
                    null,
                    null,
                    row.DisplayName.Trim(),
                    []));
                continue;
            }

            if (kind == "staff")
            {
                if (!string.IsNullOrWhiteSpace(row.UniversityId) ||
                    string.IsNullOrWhiteSpace(row.UserName) ||
                    string.IsNullOrWhiteSpace(row.StaffNumber) ||
                    !userNames.Add(row.UserName.Trim()) ||
                    !staffNumbers.Add(row.StaffNumber.Trim()) ||
                    roles.Length == 0)
                {
                    normalized = [];
                    message = LocalizedUiText.Get("Staff rows need unique username and staff number values plus at least one role.");
                    return false;
                }

                output.Add(new IdentityImportUserRequest(
                    row.ExternalReference.Trim(),
                    kind,
                    null,
                    row.UserName.Trim(),
                    row.StaffNumber.Trim(),
                    row.DisplayName.Trim(),
                    roles));
                continue;
            }

            normalized = [];
            message = LocalizedUiText.Get("Each row kind must be student or staff.");
            return false;
        }

        normalized = output;
        message = string.Empty;
        return true;
    }

    private static string DisplayRoles(IReadOnlyList<string> roles) =>
        roles.Count == 0
            ? LocalizedUiText.Get("No active roles")
            : string.Join(LocalizedUiText.Get(", "), roles.Select(DisplayRole));

    private static string DisplayRole(string role) =>
        LocalizedUiText.Get(role == "TeachingAssistant" ? "Teaching Assistant" : role);
}
