using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Contracts.Auditing;
using StudentRegistration.IdentityAccess.Application;
using StudentRegistration.IdentityAccess.Application.Ports;
using StudentRegistration.IdentityAccess.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence;

/// <summary>
/// SQL Server adapter for bounded identity administration. It deliberately
/// uses the sole shared DbContext so user, security, and audit facts commit
/// together.
/// </summary>
public sealed class AdminUserLifecycleStore : IAdminUserLifecycleStore
{
    private static readonly char[] SecretAlphabet =
        "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789-_".ToCharArray();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly StudentRegistrationDbContext _dbContext;
    private readonly IAuditEventWriter _auditWriter;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly IProvisionedCredentialHandoff _credentialHandoff;
    private readonly TimeProvider _timeProvider;

    public AdminUserLifecycleStore(
        StudentRegistrationDbContext dbContext,
        IAuditEventWriter auditWriter,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IProvisionedCredentialHandoff credentialHandoff,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _auditWriter = auditWriter ?? throw new ArgumentNullException(nameof(auditWriter));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _credentialHandoff = credentialHandoff
            ?? throw new ArgumentNullException(nameof(credentialHandoff));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<AdminUserPageSnapshot> ListUsersAsync(
        AdminUserSearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        var users = _dbContext.Set<ApplicationUser>().AsNoTracking();
        if (criteria.Search is { Length: > 0 } search)
        {
            users = users.Where(user =>
                user.UserName.Contains(search)
                || user.NormalizedUserName.Contains(search)
                || (user.UniversityId != null && user.UniversityId.Contains(search))
                || _dbContext.Set<Staff>().Any(staff =>
                    staff.ApplicationUserId == user.Id
                    && (staff.DisplayName.Contains(search)
                        || staff.StaffNumber.Contains(search))));
        }

        var totalCount = await users.CountAsync(cancellationToken).ConfigureAwait(false);
        var projected = users.Select(user => new UserListRow(
            user.Id,
            _dbContext.Set<Staff>()
                .Where(staff => staff.ApplicationUserId == user.Id)
                .Select(staff => staff.DisplayName)
                .FirstOrDefault() ?? user.UserName,
            user.UniversityId ?? user.UserName,
            user.IsEnabled,
            user.Version));
        projected = criteria.Sort switch
        {
            "displayName desc,id" => projected
                .OrderByDescending(user => user.DisplayName)
                .ThenBy(user => user.Id),
            "loginIdentifier,id" => projected
                .OrderBy(user => user.LoginIdentifier)
                .ThenBy(user => user.Id),
            "loginIdentifier desc,id" => projected
                .OrderByDescending(user => user.LoginIdentifier)
                .ThenBy(user => user.Id),
            "id" => projected.OrderBy(user => user.Id),
            _ => projected.OrderBy(user => user.DisplayName).ThenBy(user => user.Id)
        };

        var offset = (long)(criteria.Page - 1) * criteria.PageSize;
        var pageRows = offset > int.MaxValue
            ? []
            : await projected
                .Skip((int)offset)
                .Take(criteria.PageSize)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
        if (pageRows.Length == 0)
        {
            return new AdminUserPageSnapshot([], totalCount);
        }

        var pageUserIds = pageRows.Select(user => user.Id).ToArray();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var roleRows = await _dbContext.Set<RoleAssignment>()
            .AsNoTracking()
            .Where(role => pageUserIds.Contains(role.ApplicationUserId)
                && role.EffectiveFromUtc <= utcNow
                && (role.EffectiveToUtc == null || utcNow < role.EffectiveToUtc))
            .Select(role => new { role.ApplicationUserId, role.RoleCode })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var rolesByUser = roleRows
            .GroupBy(role => role.ApplicationUserId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group.Select(role => role.RoleCode)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray());

        var snapshots = pageRows.Select(user => new AdminUserSnapshot(
            user.Id,
            user.DisplayName,
            user.LoginIdentifier,
            user.Enabled,
            rolesByUser.GetValueOrDefault(user.Id) ?? [],
            user.Version.ToArray())).ToArray();
        return new AdminUserPageSnapshot(snapshots, totalCount);
    }

    public async Task<AdminStoreResult<IdentityImportSnapshot>> CreateImportAsync(
        CreateIdentityImport command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        try
        {
            return await strategy.ExecuteAsync(async () =>
            {
                _dbContext.ChangeTracker.Clear();
                await using var transaction = await _dbContext.Database
                    .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                    .ConfigureAwait(false);

                var keyed = await _dbContext.Set<IdentityImportBatch>()
                    .AsNoTracking()
                    .SingleOrDefaultAsync(batch =>
                        batch.RequestedByUserId == command.RequestedByUserId
                        && batch.ClientRequestId == command.ClientRequestId,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (keyed is not null)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return string.Equals(keyed.SourceName, command.SourceName, StringComparison.Ordinal)
                        && string.Equals(keyed.SourceHash, command.SourceHash, StringComparison.Ordinal)
                        ? Success(ToImportSnapshot(keyed))
                        : Failure<IdentityImportSnapshot>(AdminStoreOutcome.IdempotencyKeyReused);
                }

                var duplicateContent = await _dbContext.Set<IdentityImportBatch>()
                    .AsNoTracking()
                    .AnyAsync(
                        batch => batch.SourceHash == command.SourceHash,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (duplicateContent)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return Failure<IdentityImportSnapshot>(AdminStoreOutcome.ImportContentExists);
                }

                var errors = await ValidateCandidatesAsync(command.Users, cancellationToken)
                    .ConfigureAwait(false);
                var importId = Guid.NewGuid();
                var batch = new IdentityImportBatch(
                    importId,
                    command.RequestedByUserId,
                    command.ClientRequestId,
                    command.SourceName,
                    command.SourceHash,
                    command.RequestedAtUtc);
                batch.RecordValidation(
                    errors.Count == 0
                        ? IdentityImportStates.Validated
                        : IdentityImportStates.Invalid,
                    SerializeErrors(errors));
                _dbContext.Add(batch);

                for (var index = 0; index < command.Users.Count; index++)
                {
                    var candidate = command.Users[index];
                    var roles = candidate.Kind == "student"
                        ? ["Student"]
                        : candidate.Roles;
                    _dbContext.Add(new IdentityImportCandidateRow(
                        StableGuid(importId, $"candidate:{index + 1}"),
                        importId,
                        index + 1,
                        candidate.ExternalReference,
                        candidate.Kind,
                        candidate.UniversityId,
                        candidate.UserName,
                        candidate.StaffNumber,
                        candidate.DisplayName,
                        roles));
                }

                await AppendImportValidationAuditAsync(batch, command, errors, cancellationToken)
                    .ConfigureAwait(false);
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                var snapshot = ToImportSnapshot(batch);
                _dbContext.ChangeTracker.Clear();
                return Success(snapshot);
            }).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (IsStorageFailure(exception))
        {
            _dbContext.ChangeTracker.Clear();
            return Failure<IdentityImportSnapshot>(AdminStoreOutcome.StorageFailure);
        }
    }

    public async Task<IdentityImportSnapshot?> GetImportAsync(
        Guid requestedByUserId,
        Guid importId,
        CancellationToken cancellationToken)
    {
        var batch = await _dbContext.Set<IdentityImportBatch>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.Id == importId
                    && candidate.RequestedByUserId == requestedByUserId,
                cancellationToken)
            .ConfigureAwait(false);
        return batch is null ? null : ToImportSnapshot(batch);
    }

    public async Task<AdminStoreResult<IdentityImportSnapshot>> PublishImportAsync(
        PublishIdentityImport command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var handoffPrepared = false;
        var commitStarted = false;
        var committed = false;
        var generatedSecrets = new Dictionary<Guid, string>();
        try
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            var result = await strategy.ExecuteAsync(async () =>
            {
                _dbContext.ChangeTracker.Clear();
                await using var transaction = await _dbContext.Database
                    .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                    .ConfigureAwait(false);
                var batch = await _dbContext.Set<IdentityImportBatch>()
                    .FromSqlInterpolated($$"""
                        SELECT *
                        FROM [auth].[IdentityImportBatches] WITH (UPDLOCK, HOLDLOCK)
                        WHERE [Id] = {{command.ImportId}}
                          AND [RequestedByUserId] = {{command.RequestedByUserId}}
                        """)
                    .SingleOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (batch is null)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return Failure<IdentityImportSnapshot>(AdminStoreOutcome.NotFound);
                }

                if (batch.State == IdentityImportStates.Published)
                {
                    var publication = DeserializePublication(batch.ResultSummaryJson);
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    if (publication is null
                        || !string.Equals(
                            publication.ClientRequestId,
                            command.ClientRequestId,
                            StringComparison.Ordinal))
                    {
                        return Failure<IdentityImportSnapshot>(
                            AdminStoreOutcome.ImportNotValidated);
                    }

                    if (!string.Equals(
                        publication.RequestHash,
                        command.RequestHash,
                        StringComparison.Ordinal))
                    {
                        return Failure<IdentityImportSnapshot>(
                            AdminStoreOutcome.IdempotencyKeyReused);
                    }

                    return Success(ToImportSnapshot(batch));
                }

                if (!batch.Version.SequenceEqual(command.ExpectedVersion))
                {
                    var currentVersion = batch.Version.ToArray();
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return Failure<IdentityImportSnapshot>(
                        AdminStoreOutcome.StaleVersion,
                        currentVersion);
                }

                if (batch.State != IdentityImportStates.Validated)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return Failure<IdentityImportSnapshot>(AdminStoreOutcome.ImportNotValidated);
                }

                var candidates = await _dbContext.Set<IdentityImportCandidateRow>()
                    .AsNoTracking()
                    .Where(row => row.IdentityImportBatchId == batch.Id)
                    .OrderBy(row => row.Ordinal)
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);
                var conflicts = await ValidateStagedCandidatesAsync(candidates, cancellationToken)
                    .ConfigureAwait(false);
                if (conflicts.Count > 0)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return Failure<IdentityImportSnapshot>(AdminStoreOutcome.StorageFailure);
                }

                var credentials = new List<DemoCredential>(candidates.Length);
                foreach (var candidate in candidates)
                {
                    AddPublishedIdentity(
                        batch.Id,
                        candidate,
                        command.RequestedAtUtc,
                        generatedSecrets,
                        credentials);
                }

                await _credentialHandoff.PrepareAsync(
                        batch.Id,
                        credentials,
                        cancellationToken)
                    .ConfigureAwait(false);
                handoffPrepared = true;

                batch.Publish(JsonSerializer.Serialize(
                    new PublicationSummary(
                        command.ClientRequestId,
                        command.RequestHash,
                        candidates.Length),
                    JsonOptions));
                await AppendImportAuditAsync(batch, command, cancellationToken)
                    .ConfigureAwait(false);
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                commitStarted = true;
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                committed = true;
                return Success(ToImportSnapshot(batch));
            }).ConfigureAwait(false);

            if (result.Outcome == AdminStoreOutcome.Succeeded
                && result.Value?.State == IdentityImportStates.Published)
            {
                await _credentialHandoff.CompleteAsync(command.ImportId, cancellationToken)
                    .ConfigureAwait(false);
            }

            _dbContext.ChangeTracker.Clear();
            return result;
        }
        catch (OperationCanceledException)
        {
            if (handoffPrepared && !committed && !commitStarted)
            {
                await AbortHandoffAsync(command.ImportId).ConfigureAwait(false);
            }

            _dbContext.ChangeTracker.Clear();
            throw;
        }
        catch (Exception exception) when (IsStorageFailure(exception))
        {
            if (handoffPrepared && !committed && !commitStarted)
            {
                await AbortHandoffAsync(command.ImportId).ConfigureAwait(false);
            }

            _dbContext.ChangeTracker.Clear();
            return Failure<IdentityImportSnapshot>(AdminStoreOutcome.StorageFailure);
        }
    }

    public async Task<AdminStoreResult<AdminUserSnapshot>> SetUserStatusAsync(
        SetUserStatus command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        try
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                _dbContext.ChangeTracker.Clear();
                await using var transaction = await _dbContext.Database
                    .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                    .ConfigureAwait(false);
                await LockAdminGuardAsync(cancellationToken).ConfigureAwait(false);

                var user = await _dbContext.Set<ApplicationUser>()
                    .SingleOrDefaultAsync(candidate => candidate.Id == command.UserId, cancellationToken)
                    .ConfigureAwait(false);
                if (user is null)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return Failure<AdminUserSnapshot>(AdminStoreOutcome.NotFound);
                }

                var committedAfter = JsonSerializer.Serialize(
                    new { enabled = command.Enabled },
                    JsonOptions);
                if (await HasCommittedMutationAsync(
                        user.Id,
                        "IdentityUserStatusChanged",
                        command.ActorReference,
                        command.SubjectReference,
                        command.Reason,
                        committedAfter,
                        command.CorrelationId,
                        command.RequestedAtUtc,
                        cancellationToken).ConfigureAwait(false))
                {
                    var replay = await CreateUserSnapshotAsync(
                            user,
                            command.RequestedAtUtc,
                            cancellationToken)
                        .ConfigureAwait(false);
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    _dbContext.ChangeTracker.Clear();
                    return Success(replay);
                }

                if (!user.Version.SequenceEqual(command.ExpectedVersion))
                {
                    var currentVersion = user.Version.ToArray();
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return Failure<AdminUserSnapshot>(
                        AdminStoreOutcome.StaleVersion,
                        currentVersion);
                }

                var currentRoles = await GetEffectiveRoleCodesAsync(
                        user.Id,
                        command.RequestedAtUtc,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (user.IsEnabled
                    && !command.Enabled
                    && currentRoles.Contains("Admin", StringComparer.Ordinal)
                    && await CountEnabledAdminsAsync(command.RequestedAtUtc, cancellationToken)
                        .ConfigureAwait(false) <= 1)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return Failure<AdminUserSnapshot>(AdminStoreOutcome.FinalAdminRequired);
                }

                if (user.IsEnabled != command.Enabled)
                {
                    var before = JsonSerializer.Serialize(
                        new { enabled = user.IsEnabled },
                        JsonOptions);
                    user.SetEnabled(command.Enabled, command.NewSecurityStamp);
                    var after = JsonSerializer.Serialize(
                        new { enabled = user.IsEnabled },
                        JsonOptions);
                    await AppendMutationAuditAsync(
                            user.Id,
                            "IdentityUserStatusChanged",
                            command.ActorReference,
                            command.SubjectReference,
                            command.Reason,
                            before,
                            after,
                            command.CorrelationId,
                            command.RequestedAtUtc,
                            cancellationToken)
                        .ConfigureAwait(false);
                    await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                var snapshot = await CreateUserSnapshotAsync(
                        user,
                        command.RequestedAtUtc,
                        cancellationToken)
                    .ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                _dbContext.ChangeTracker.Clear();
                return Success(snapshot);
            }).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (IsStorageFailure(exception))
        {
            _dbContext.ChangeTracker.Clear();
            return Failure<AdminUserSnapshot>(AdminStoreOutcome.StorageFailure);
        }
    }

    public async Task<AdminStoreResult<AdminUserSnapshot>> ReplaceUserRolesAsync(
        ReplaceUserRoles command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        try
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                _dbContext.ChangeTracker.Clear();
                await using var transaction = await _dbContext.Database
                    .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                    .ConfigureAwait(false);
                await LockAdminGuardAsync(cancellationToken).ConfigureAwait(false);

                var user = await _dbContext.Set<ApplicationUser>()
                    .SingleOrDefaultAsync(candidate => candidate.Id == command.UserId, cancellationToken)
                    .ConfigureAwait(false);
                var staffExists = user is not null && await _dbContext.Set<Staff>()
                    .AnyAsync(staff => staff.ApplicationUserId == command.UserId, cancellationToken)
                    .ConfigureAwait(false);
                if (user is null || !staffExists)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return Failure<AdminUserSnapshot>(AdminStoreOutcome.NotFound);
                }

                var desiredRoleCodes = command.Roles
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray();
                var committedAfter = JsonSerializer.Serialize(
                    new { roles = desiredRoleCodes },
                    JsonOptions);
                if (await HasCommittedMutationAsync(
                        user.Id,
                        "IdentityUserRolesReplaced",
                        command.ActorReference,
                        command.SubjectReference,
                        command.Reason,
                        committedAfter,
                        command.CorrelationId,
                        command.RequestedAtUtc,
                        cancellationToken).ConfigureAwait(false))
                {
                    var replay = await CreateUserSnapshotAsync(
                            user,
                            command.RequestedAtUtc,
                            cancellationToken)
                        .ConfigureAwait(false);
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    _dbContext.ChangeTracker.Clear();
                    return Success(replay);
                }

                if (!user.Version.SequenceEqual(command.ExpectedVersion))
                {
                    var currentVersion = user.Version.ToArray();
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return Failure<AdminUserSnapshot>(
                        AdminStoreOutcome.StaleVersion,
                        currentVersion);
                }

                var activeRoles = await _dbContext.Set<RoleAssignment>()
                    .Where(role => role.ApplicationUserId == user.Id
                        && role.EffectiveFromUtc <= command.RequestedAtUtc
                        && (role.EffectiveToUtc == null
                            || command.RequestedAtUtc < role.EffectiveToUtc))
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);
                var currentRoleCodes = activeRoles.Select(role => role.RoleCode)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToArray();
                if (user.IsEnabled
                    && currentRoleCodes.Contains("Admin", StringComparer.Ordinal)
                    && !desiredRoleCodes.Contains("Admin", StringComparer.Ordinal)
                    && await CountEnabledAdminsAsync(command.RequestedAtUtc, cancellationToken)
                        .ConfigureAwait(false) <= 1)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return Failure<AdminUserSnapshot>(AdminStoreOutcome.FinalAdminRequired);
                }

                if (!currentRoleCodes.SequenceEqual(desiredRoleCodes, StringComparer.Ordinal))
                {
                    foreach (var role in activeRoles.Where(
                        role => !desiredRoleCodes.Contains(role.RoleCode, StringComparer.Ordinal)))
                    {
                        if (command.RequestedAtUtc <= role.EffectiveFromUtc)
                        {
                            throw new InvalidOperationException(
                                "IDENTITY_ROLE_EFFECTIVE_TIME_INVALID: A replacement must follow the active assignment start.");
                        }

                        role.EndAt(command.RequestedAtUtc);
                    }

                    foreach (var roleCode in desiredRoleCodes.Where(
                        role => !currentRoleCodes.Contains(role, StringComparer.Ordinal)))
                    {
                        _dbContext.Add(new RoleAssignment(
                            Guid.NewGuid(),
                            user.Id,
                            roleCode,
                            command.RequestedAtUtc,
                            effectiveToUtc: null,
                            command.ActorReference));
                    }

                    var before = JsonSerializer.Serialize(
                        new { roles = currentRoleCodes },
                        JsonOptions);
                    var after = JsonSerializer.Serialize(
                        new { roles = desiredRoleCodes },
                        JsonOptions);
                    user.RotateSecurityStamp(command.NewSecurityStamp);
                    await AppendMutationAuditAsync(
                            user.Id,
                            "IdentityUserRolesReplaced",
                            command.ActorReference,
                            command.SubjectReference,
                            command.Reason,
                            before,
                            after,
                            command.CorrelationId,
                            command.RequestedAtUtc,
                            cancellationToken)
                        .ConfigureAwait(false);
                    await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                var snapshot = await CreateUserSnapshotAsync(
                        user,
                        command.RequestedAtUtc,
                        cancellationToken)
                    .ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                _dbContext.ChangeTracker.Clear();
                return Success(snapshot);
            }).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (IsStorageFailure(exception))
        {
            _dbContext.ChangeTracker.Clear();
            return Failure<AdminUserSnapshot>(AdminStoreOutcome.StorageFailure);
        }
    }

    private async Task<IReadOnlyList<IdentityImportError>> ValidateCandidatesAsync(
        IReadOnlyList<IdentityImportCandidate> candidates,
        CancellationToken cancellationToken)
    {
        var universityIds = candidates
            .Where(candidate => candidate.UniversityId is not null)
            .Select(candidate => candidate.UniversityId!)
            .ToArray();
        var normalizedUserNames = candidates
            .Where(candidate => candidate.UserName is not null)
            .Select(candidate => candidate.UserName!.ToUpperInvariant())
            .ToArray();
        var staffNumbers = candidates
            .Where(candidate => candidate.StaffNumber is not null)
            .Select(candidate => candidate.StaffNumber!)
            .ToArray();

        var existingUniversityIds = await _dbContext.Set<ApplicationUser>()
            .AsNoTracking()
            .Where(user => user.UniversityId != null && universityIds.Contains(user.UniversityId))
            .Select(user => user.UniversityId!)
            .ToHashSetAsync(cancellationToken)
            .ConfigureAwait(false);
        var existingUserNames = await _dbContext.Set<ApplicationUser>()
            .AsNoTracking()
            .Where(user => normalizedUserNames.Contains(user.NormalizedUserName))
            .Select(user => user.NormalizedUserName)
            .ToHashSetAsync(cancellationToken)
            .ConfigureAwait(false);
        var existingStaffNumbers = await _dbContext.Set<Staff>()
            .AsNoTracking()
            .Where(staff => staffNumbers.Contains(staff.StaffNumber))
            .Select(staff => staff.StaffNumber)
            .ToHashSetAsync(cancellationToken)
            .ConfigureAwait(false);

        var errors = new List<IdentityImportError>();
        for (var index = 0; index < candidates.Count; index++)
        {
            var candidate = candidates[index];
            if (candidate.UniversityId is not null
                && existingUniversityIds.Contains(candidate.UniversityId))
            {
                errors.Add(new IdentityImportError(
                    index + 1,
                    "UNIVERSITY_ID_EXISTS",
                    "The University ID is already provisioned."));
            }

            if (candidate.UserName is not null
                && existingUserNames.Contains(candidate.UserName.ToUpperInvariant()))
            {
                errors.Add(new IdentityImportError(
                    index + 1,
                    "USERNAME_EXISTS",
                    "The username is already provisioned."));
            }

            if (candidate.StaffNumber is not null
                && existingStaffNumbers.Contains(candidate.StaffNumber))
            {
                errors.Add(new IdentityImportError(
                    index + 1,
                    "STAFF_NUMBER_EXISTS",
                    "The staff number is already provisioned."));
            }
        }

        return errors;
    }

    private Task<IReadOnlyList<IdentityImportError>> ValidateStagedCandidatesAsync(
        IReadOnlyList<IdentityImportCandidateRow> candidates,
        CancellationToken cancellationToken)
    {
        var transient = candidates.Select(candidate => new IdentityImportCandidate(
            candidate.ExternalReference,
            candidate.Kind,
            candidate.UniversityId,
            candidate.UserName,
            candidate.StaffNumber,
            candidate.DisplayName,
            ParseRoles(candidate.Roles))).ToArray();
        return ValidateCandidatesAsync(transient, cancellationToken);
    }

    private void AddPublishedIdentity(
        Guid importId,
        IdentityImportCandidateRow candidate,
        DateTime provisionedAtUtc,
        IDictionary<Guid, string> generatedSecrets,
        ICollection<DemoCredential> credentials)
    {
        var loginIdentifier = candidate.Kind == "student"
            ? candidate.UniversityId!
            : candidate.UserName!;
        var normalizedUserName = loginIdentifier.Trim().ToUpperInvariant();
        var userId = StableGuid(importId, $"user:{candidate.ExternalReference}");
        var securityStamp = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        if (!generatedSecrets.TryGetValue(userId, out var secret))
        {
            secret = RandomNumberGenerator.GetString(SecretAlphabet, 24);
            generatedSecrets.Add(userId, secret);
        }

        var transient = new ApplicationUser(
            userId,
            loginIdentifier,
            normalizedUserName,
            candidate.UniversityId,
            "TRANSIENT",
            securityStamp);
        var user = new ApplicationUser(
            userId,
            loginIdentifier,
            normalizedUserName,
            candidate.UniversityId,
            _passwordHasher.HashPassword(transient, secret),
            securityStamp);
        _dbContext.Add(user);

        if (candidate.Kind == "student")
        {
            _dbContext.Add(new StudentActivation(
                StableGuid(userId, "student-activation"),
                userId,
                provisionedAtUtc));
        }
        else
        {
            _dbContext.Add(new Staff(
                StableGuid(userId, "staff"),
                userId,
                candidate.StaffNumber!,
                candidate.DisplayName));
        }

        foreach (var roleCode in ParseRoles(candidate.Roles))
        {
            _dbContext.Add(new RoleAssignment(
                StableGuid(userId, $"role:{roleCode}"),
                userId,
                roleCode,
                provisionedAtUtc,
                effectiveToUtc: null,
                "identity-import"));
        }

        credentials.Add(new DemoCredential(loginIdentifier, secret, provisionedAtUtc));
    }

    private async Task LockAdminGuardAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlRawAsync(
            """
            IF NOT EXISTS
            (
                SELECT 1
                FROM [auth].[AdminSecurityGuards] WITH (UPDLOCK, HOLDLOCK)
                WHERE [Id] = 1
            )
            BEGIN
                INSERT INTO [auth].[AdminSecurityGuards] ([Id]) VALUES (1);
            END;
            """,
            cancellationToken).ConfigureAwait(false);

        _ = await _dbContext.Set<AdminSecurityGuard>()
            .FromSqlRaw(
                "SELECT [Id], [Version] FROM [auth].[AdminSecurityGuards] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = 1")
            .AsNoTracking()
            .SingleAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private Task<int> CountEnabledAdminsAsync(
        DateTime utcNow,
        CancellationToken cancellationToken) =>
        _dbContext.Set<ApplicationUser>()
            .Where(user => user.IsEnabled
                && _dbContext.Set<RoleAssignment>().Any(role =>
                    role.ApplicationUserId == user.Id
                    && role.RoleCode == "Admin"
                    && role.EffectiveFromUtc <= utcNow
                    && (role.EffectiveToUtc == null || utcNow < role.EffectiveToUtc)))
            .CountAsync(cancellationToken);

    private async Task<IReadOnlyList<string>> GetEffectiveRoleCodesAsync(
        Guid userId,
        DateTime utcNow,
        CancellationToken cancellationToken) =>
        await _dbContext.Set<RoleAssignment>()
            .AsNoTracking()
            .Where(role => role.ApplicationUserId == userId
                && role.EffectiveFromUtc <= utcNow
                && (role.EffectiveToUtc == null || utcNow < role.EffectiveToUtc))
            .Select(role => role.RoleCode)
            .Distinct()
            .OrderBy(role => role)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

    private async Task<AdminUserSnapshot> CreateUserSnapshotAsync(
        ApplicationUser user,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var displayName = await _dbContext.Set<Staff>()
            .AsNoTracking()
            .Where(staff => staff.ApplicationUserId == user.Id)
            .Select(staff => staff.DisplayName)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false) ?? user.UserName;
        var roles = await GetEffectiveRoleCodesAsync(user.Id, utcNow, cancellationToken)
            .ConfigureAwait(false);
        return new AdminUserSnapshot(
            user.Id,
            displayName,
            user.UniversityId ?? user.UserName,
            user.IsEnabled,
            roles,
            user.Version.ToArray());
    }

    private Task<bool> HasCommittedMutationAsync(
        Guid applicationUserId,
        string eventType,
        string actorReference,
        string subjectReference,
        string reason,
        string afterSummaryJson,
        string correlationId,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken) =>
        _dbContext.Set<SecurityEvent>()
            .AsNoTracking()
            .AnyAsync(securityEvent =>
                    securityEvent.ApplicationUserId == applicationUserId
                    && securityEvent.EventType == eventType
                    && securityEvent.ActorReference == actorReference
                    && securityEvent.SubjectReference == subjectReference
                    && securityEvent.Reason == reason
                    && securityEvent.AfterSummaryJson == afterSummaryJson
                    && securityEvent.CorrelationId == correlationId
                    && securityEvent.OccurredAtUtc == occurredAtUtc,
                cancellationToken);

    private async Task AppendMutationAuditAsync(
        Guid applicationUserId,
        string eventType,
        string actorReference,
        string subjectReference,
        string reason,
        string beforeSummaryJson,
        string afterSummaryJson,
        string correlationId,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken)
    {
        _dbContext.Add(new SecurityEvent(
            Guid.NewGuid(),
            applicationUserId,
            eventType,
            actorReference,
            subjectReference,
            reason,
            beforeSummaryJson,
            afterSummaryJson,
            metadataJson: null,
            correlationId,
            occurredAtUtc));
        await _auditWriter.AppendAsync(
                new AuditEventDraft(
                    actorReference,
                    subjectReference,
                    eventType,
                    nameof(ApplicationUser),
                    applicationUserId.ToString("N"),
                    reason,
                    beforeSummaryJson,
                    afterSummaryJson,
                    correlationId,
                    occurredAtUtc),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task AppendImportAuditAsync(
        IdentityImportBatch batch,
        PublishIdentityImport command,
        CancellationToken cancellationToken)
    {
        const string eventType = "IdentityImportPublished";
        const string reason = "Pre-provisioned identity import published.";
        var subject = $"identity-import:{batch.Id:N}";
        var after = JsonSerializer.Serialize(
            new { state = IdentityImportStates.Published },
            JsonOptions);
        _dbContext.Add(new SecurityEvent(
            Guid.NewGuid(),
            command.RequestedByUserId,
            eventType,
            command.ActorReference,
            subject,
            reason,
            JsonSerializer.Serialize(new { state = IdentityImportStates.Validated }, JsonOptions),
            after,
            metadataJson: null,
            command.CorrelationId,
            command.RequestedAtUtc));
        await _auditWriter.AppendAsync(
                new AuditEventDraft(
                    command.ActorReference,
                    subject,
                    eventType,
                    nameof(IdentityImportBatch),
                    batch.Id.ToString("N"),
                    reason,
                    null,
                    after,
                    command.CorrelationId,
                    command.RequestedAtUtc),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task AppendImportValidationAuditAsync(
        IdentityImportBatch batch,
        CreateIdentityImport command,
        IReadOnlyList<IdentityImportError> errors,
        CancellationToken cancellationToken)
    {
        var eventType = batch.State == IdentityImportStates.Validated
            ? "IdentityImportValidated"
            : "IdentityImportRejected";
        const string reason = "Pre-provisioned identity import validation recorded.";
        var subject = $"identity-import:{batch.Id:N}";
        var before = JsonSerializer.Serialize(
            new { state = IdentityImportStates.Uploaded },
            JsonOptions);
        var after = JsonSerializer.Serialize(
            new
            {
                state = batch.State,
                rowCount = command.Users.Count,
                errorCount = errors.Count
            },
            JsonOptions);
        _dbContext.Add(new SecurityEvent(
            Guid.NewGuid(),
            command.RequestedByUserId,
            eventType,
            command.ActorReference,
            subject,
            reason,
            before,
            after,
            metadataJson: null,
            command.CorrelationId,
            command.RequestedAtUtc));
        await _auditWriter.AppendAsync(
                new AuditEventDraft(
                    command.ActorReference,
                    subject,
                    eventType,
                    nameof(IdentityImportBatch),
                    batch.Id.ToString("N"),
                    reason,
                    before,
                    after,
                    command.CorrelationId,
                    command.RequestedAtUtc),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task AbortHandoffAsync(Guid importId)
    {
        try
        {
            await _credentialHandoff.AbortAsync(importId, CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch
        {
            // Preserve the SQL/publish failure. The adapter's bounded cleanup
            // owns any unreachable pending artifact.
        }
    }

    private static IdentityImportSnapshot ToImportSnapshot(IdentityImportBatch batch) =>
        new(
            batch.Id,
            batch.SourceName,
            batch.SourceHash,
            batch.State,
            DeserializeErrors(batch.ErrorSummaryJson),
            batch.Version.ToArray());

    private static string SerializeErrors(IReadOnlyList<IdentityImportError> errors)
    {
        var bounded = errors.Take(100).ToArray();
        var json = JsonSerializer.Serialize(bounded, JsonOptions);
        return json.Length <= 16_000
            ? json
            : JsonSerializer.Serialize(bounded.Take(50), JsonOptions);
    }

    private static IReadOnlyList<IdentityImportError> DeserializeErrors(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<IdentityImportError[]>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [new IdentityImportError(null, "IMPORT_STATE_INVALID", "Import errors are unavailable.")];
        }
    }

    private static PublicationSummary? DeserializePublication(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<PublicationSummary>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static IReadOnlyList<string> ParseRoles(string roles) =>
        roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static Guid StableGuid(Guid ownerId, string discriminator)
    {
        var input = Encoding.UTF8.GetBytes($"{ownerId:N}|{discriminator}");
        var hash = SHA256.HashData(input);
        return new Guid(hash.AsSpan(0, 16));
    }

    private static bool IsStorageFailure(Exception exception) =>
        exception is DbUpdateException
            or SqlException
            or TimeoutException
            or InvalidOperationException
            or JsonException
            or IOException
            or UnauthorizedAccessException;

    private static AdminStoreResult<T> Success<T>(T value)
        where T : class =>
        new(AdminStoreOutcome.Succeeded, value);

    private static AdminStoreResult<T> Failure<T>(
        AdminStoreOutcome outcome,
        byte[]? currentVersion = null)
        where T : class =>
        new(outcome, null, currentVersion);

    private sealed record UserListRow(
        Guid Id,
        string DisplayName,
        string LoginIdentifier,
        bool Enabled,
        byte[] Version);

    private sealed record PublicationSummary(
        string ClientRequestId,
        string RequestHash,
        int PublishedUserCount);
}
