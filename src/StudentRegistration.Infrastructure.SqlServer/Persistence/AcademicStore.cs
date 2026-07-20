using System.Data;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Academics.Application.Ports;
using StudentRegistration.Academics.Domain;
using StudentRegistration.Contracts;
using StudentRegistration.Contracts.Academics;
using StudentRegistration.Contracts.Auditing;
using StudentRegistration.IdentityAccess.Domain;
using ContractWindowLifecycle = StudentRegistration.Contracts.Academics.RegistrationWindowLifecycle;
using ContractWindowScope = StudentRegistration.Contracts.Academics.RegistrationWindowScope;
using DomainWindowLifecycle = StudentRegistration.Academics.Domain.RegistrationWindowLifecycleState;
using DomainWindowScope = StudentRegistration.Academics.Domain.RegistrationWindowScopeType;

namespace StudentRegistration.Infrastructure.SqlServer.Persistence;

/// <summary>
/// The single SQL Server adapter for the SPEC-008 Academics persistence ports.
/// It deliberately shares the modular monolith DbContext and keeps each write
/// inside one short serializable transaction.
/// </summary>
public sealed class AcademicStore :
    IAcademicContextReader,
    IAcademicStudentScopeReader,
    IRegistrationWindowStore,
    IStudentAcademicProfileStore,
    IDemoStudentProfileSeedStore,
    IAdminAcademicStore
{
    private const int MaximumWindowsPerTerm = 20;
    private const int MaximumActiveHolds = 100;
    private const int MaximumContextTerms = 20;
    private const string LegacySeedProfileVersion = "synthetic-fixture/1.0";
    private const string CurrentSeedProfileVersion = "synthetic-fixture/2.0";
    private const string SyntheticSeedSource = "Synthetic";

    private readonly StudentRegistrationDbContext _dbContext;
    private readonly IAuditEventWriter _auditWriter;
    private readonly TimeProvider _timeProvider;

    public AcademicStore(
        StudentRegistrationDbContext dbContext,
        IAuditEventWriter auditWriter,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _auditWriter = auditWriter ?? throw new ArgumentNullException(nameof(auditWriter));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<AcademicContextSnapshot> ResolveContextAsync(
        AcademicStudentScope? studentScope,
        CancellationToken cancellationToken = default)
    {
        var terms = await _dbContext.Set<AcademicTerm>()
            .AsNoTracking()
            .OrderByDescending(term => term.TeachingStartsOn)
            .ThenBy(term => term.Id)
            .Take(MaximumContextTerms + 1)
            .ToArrayAsync(cancellationToken);
        if (terms.Length > MaximumContextTerms)
        {
            throw new InvalidOperationException(
                "ACADEMIC_CONTEXT_TERM_LIMIT_EXCEEDED: The configured term set is not bounded.");
        }

        var termRecords = terms
            .Select(term => new AcademicTermContextRecord(
                term.Id,
                term.Code,
                term.DisplayName,
                term.TimeZoneId,
                term.TeachingStartsOn,
                term.TeachingEndsOn,
                term.State,
                term.Version))
            .ToArray();
        var termIds = terms.Select(term => term.Id).ToArray();
        var windows = termIds.Length == 0
            ? []
            : await _dbContext.Set<RegistrationWindow>()
                .AsNoTracking()
                .Where(window =>
                    termIds.Contains(window.TermId) &&
                    window.State == DomainWindowLifecycle.Published)
                .OrderBy(window => window.TermId)
                .ThenBy(window => window.OpensAtUtc)
                .ThenBy(window => window.Id)
                .Take((MaximumContextTerms * MaximumWindowsPerTerm) + 1)
                .ToArrayAsync(cancellationToken);
        if (windows.Length > MaximumContextTerms * MaximumWindowsPerTerm)
        {
            throw new InvalidOperationException(
                "ACADEMIC_CONTEXT_WINDOW_LIMIT_EXCEEDED: The configured window set is not bounded.");
        }

        return new AcademicContextSnapshot(
            termRecords,
            windows.Select(window => new RegistrationWindowContextRecord(
                window.Id,
                window.TermId,
                window.ScopeType,
                window.ScopeValue,
                window.OpensAtUtc,
                window.ClosesAtUtc,
                window.Version)).ToArray());
    }

    public async Task<AcademicStudentScope?> FindByApplicationUserIdAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken = default)
    {
        var value = await _dbContext.Set<Student>()
            .AsNoTracking()
            .Where(student => student.ApplicationUserId == applicationUserId && student.IsActive)
            .Select(student => new { student.ProgramCode, student.Cohort })
            .SingleOrDefaultAsync(cancellationToken);
        return value is null
            ? null
            : new AcademicStudentScope(value.ProgramCode, value.Cohort);
    }

    public async Task<AcademicTermCreationStoreResult> CreateOrReplayTermAsync(
        CreateAcademicTermStoreCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.Windows.Count > MaximumWindowsPerTerm ||
            command.Windows.Any(window =>
                window.TermId != command.Term.Id ||
                window.State is not DomainWindowLifecycle.Draft))
        {
            return new(AcademicTermCreationOutcome.TermStateConflict);
        }

        try
        {
            return await ExecuteMutationAsync(async () =>
            {
                _dbContext.ChangeTracker.Clear();
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var replay = await _dbContext.Set<AcademicTerm>()
                    .SingleOrDefaultAsync(
                        term => term.CreationClientRequestId ==
                            command.Term.CreationClientRequestId,
                        cancellationToken);
                if (replay is not null)
                {
                    var replayResult = string.Equals(
                        replay.CreationPayloadHash,
                        command.Term.CreationPayloadHash,
                        StringComparison.Ordinal)
                        ? new AcademicTermCreationStoreResult(
                            AcademicTermCreationOutcome.Replayed,
                            await ToAdminTermAsync(replay, cancellationToken))
                        : new AcademicTermCreationStoreResult(
                            AcademicTermCreationOutcome.IdempotencyKeyReused);
                    await transaction.CommitAsync(cancellationToken);
                    return replayResult;
                }

                if (await _dbContext.Set<AcademicTerm>().AnyAsync(
                    term => term.Code == command.Term.Code,
                    cancellationToken))
                {
                    await transaction.CommitAsync(cancellationToken);
                    return new(AcademicTermCreationOutcome.TermCodeExists);
                }

                if (IsSingletonState(command.Term.State) &&
                    await _dbContext.Set<AcademicTerm>().AnyAsync(
                        term => term.State == command.Term.State,
                        cancellationToken))
                {
                    await transaction.CommitAsync(cancellationToken);
                    return new(AcademicTermCreationOutcome.TermStateConflict);
                }

                _dbContext.Add(command.Term);
                _dbContext.AddRange(command.Windows.OrderBy(window => window.Id));
                await _auditWriter.AppendAsync(command.AuditEvent, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                var dto = await ToAdminTermAsync(command.Term, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return new AcademicTermCreationStoreResult(
                    AcademicTermCreationOutcome.Created,
                    dto);
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbUpdateException)
        {
            _dbContext.ChangeTracker.Clear();
            return await ResolveCreationConflictAsync(command, cancellationToken);
        }
        catch (Exception)
        {
            _dbContext.ChangeTracker.Clear();
            return new(AcademicTermCreationOutcome.StorageUnavailable);
        }
    }

    public async Task<AcademicTermMutationStoreResult> UpdateTermAsync(
        UpdateAcademicTermStoreCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.Windows.Count > MaximumWindowsPerTerm)
        {
            return new(AcademicTermMutationOutcome.TermStateConflict);
        }

        try
        {
            return await ExecuteMutationAsync(async () =>
            {
                _dbContext.ChangeTracker.Clear();
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);
                var term = await _dbContext.Set<AcademicTerm>()
                    .SingleOrDefaultAsync(candidate => candidate.Id == command.TermId, cancellationToken);
                if (term is null)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return new(AcademicTermMutationOutcome.NotFound);
                }

                if (!VersionsEqual(term.Version, command.ExpectedTermRowVersion))
                {
                    var currentVersion = EncodeVersion(term.Version);
                    await transaction.CommitAsync(cancellationToken);
                    return new(
                        AcademicTermMutationOutcome.StaleVersion,
                        CurrentVersion: currentVersion);
                }

                var existingWindows = await _dbContext.Set<RegistrationWindow>()
                    .Where(window => window.TermId == command.TermId)
                    .OrderBy(window => window.Id)
                    .ToArrayAsync(cancellationToken);
                var inputById = command.Windows
                    .Where(window => window.Id is not null)
                    .ToDictionary(window => ParseId(window.Id!), window => window);
                if (!existingWindows.Select(window => window.Id).SequenceEqual(
                        command.ExpectedWindowRowVersions.Keys.Order()) ||
                    !existingWindows.Select(window => window.Id).Order().SequenceEqual(
                        inputById.Keys.Order()))
                {
                    await transaction.CommitAsync(cancellationToken);
                    return new(
                        AcademicTermMutationOutcome.StaleVersion,
                        CurrentVersion: EncodeVersion(term.Version));
                }

                foreach (var window in existingWindows)
                {
                    if (!command.ExpectedWindowRowVersions.TryGetValue(window.Id, out var expected) ||
                        !VersionsEqual(window.Version, expected))
                    {
                        await transaction.CommitAsync(cancellationToken);
                        return new(
                            AcademicTermMutationOutcome.StaleVersion,
                            CurrentVersion: EncodeVersion(window.Version));
                    }
                }

                if (await _dbContext.Set<AcademicTerm>().AnyAsync(
                    candidate => candidate.Id != term.Id && candidate.Code == command.Term.Code,
                    cancellationToken))
                {
                    await transaction.CommitAsync(cancellationToken);
                    return new(AcademicTermMutationOutcome.TermCodeExists);
                }

                if (IsSingletonState(command.Term.State) &&
                    await _dbContext.Set<AcademicTerm>().AnyAsync(
                        candidate => candidate.Id != term.Id &&
                            candidate.State == command.Term.State,
                        cancellationToken))
                {
                    await transaction.CommitAsync(cancellationToken);
                    return new(AcademicTermMutationOutcome.TermStateConflict);
                }

                foreach (var window in existingWindows)
                {
                    var input = inputById[window.Id];
                    if (!CanApplyWindowUpdate(window, input))
                    {
                        await transaction.CommitAsync(cancellationToken);
                        return new(AcademicTermMutationOutcome.TermStateConflict);
                    }

                    ApplyWindowInput(window, input);
                }

                foreach (var input in command.Windows.Where(window => window.Id is null))
                {
                    if (input.LifecycleState is not ContractWindowLifecycle.Draft)
                    {
                        await transaction.CommitAsync(cancellationToken);
                        return new(AcademicTermMutationOutcome.TermStateConflict);
                    }

                    _dbContext.Add(ToDomainWindow(Guid.NewGuid(), term.Id, input));
                }

                if (HasPublishedOverlap(existingWindows))
                {
                    await transaction.CommitAsync(cancellationToken);
                    return new(AcademicTermMutationOutcome.WindowOverlap);
                }

                ApplyTermInput(term, command.Term);
                ForceVersionAdvance(term, nameof(AcademicTerm.DisplayName));
                await _auditWriter.AppendAsync(command.AuditEvent, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                var dto = await ToAdminTermAsync(term, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return new AcademicTermMutationStoreResult(
                    AcademicTermMutationOutcome.Succeeded,
                    dto);
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbUpdateConcurrencyException)
        {
            _dbContext.ChangeTracker.Clear();
            var current = await CurrentTermVersionAsync(command.TermId, cancellationToken);
            return new(AcademicTermMutationOutcome.StaleVersion, CurrentVersion: current);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            _dbContext.ChangeTracker.Clear();
            if (IsSingletonState(command.Term.State) &&
                await _dbContext.Set<AcademicTerm>().AsNoTracking().AnyAsync(
                    candidate => candidate.Id != command.TermId &&
                        candidate.State == command.Term.State,
                    cancellationToken))
            {
                return new(AcademicTermMutationOutcome.TermStateConflict);
            }

            return new(AcademicTermMutationOutcome.TermCodeExists);
        }
        catch (Exception)
        {
            _dbContext.ChangeTracker.Clear();
            return new(AcademicTermMutationOutcome.StorageUnavailable);
        }
    }

    public async Task<AcademicTermMutationStoreResult> PublishRegistrationWindowAsync(
        PublishRegistrationWindowStoreCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        try
        {
            return await ExecuteMutationAsync(async () =>
            {
                _dbContext.ChangeTracker.Clear();
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);
                var term = await _dbContext.Set<AcademicTerm>()
                    .SingleOrDefaultAsync(candidate => candidate.Id == command.TermId, cancellationToken);
                if (term is null)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return new(AcademicTermMutationOutcome.NotFound);
                }

                if (!VersionsEqual(term.Version, command.ExpectedTermRowVersion))
                {
                    var version = EncodeVersion(term.Version);
                    await transaction.CommitAsync(cancellationToken);
                    return new(AcademicTermMutationOutcome.StaleVersion, CurrentVersion: version);
                }

                var windows = await _dbContext.Set<RegistrationWindow>()
                    .Where(window => window.TermId == command.TermId)
                    .OrderBy(window => window.Id)
                    .ToArrayAsync(cancellationToken);
                var candidate = windows.SingleOrDefault(window => window.Id == command.WindowId);
                if (candidate is null)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return new(AcademicTermMutationOutcome.NotFound);
                }

                if (!VersionsEqual(candidate.Version, command.ExpectedWindowRowVersion))
                {
                    var version = EncodeVersion(candidate.Version);
                    await transaction.CommitAsync(cancellationToken);
                    return new(AcademicTermMutationOutcome.StaleVersion, CurrentVersion: version);
                }

                if (candidate.State is not DomainWindowLifecycle.Draft)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return new(AcademicTermMutationOutcome.TermStateConflict);
                }

                if (windows.Any(window =>
                    window.Id != candidate.Id &&
                    window.State == DomainWindowLifecycle.Published &&
                    Overlaps(window, candidate)))
                {
                    await transaction.CommitAsync(cancellationToken);
                    return new(AcademicTermMutationOutcome.WindowOverlap);
                }

                candidate.Publish();
                ForceVersionAdvance(term, nameof(AcademicTerm.DisplayName));
                await _auditWriter.AppendAsync(command.AuditEvent, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                var dto = await ToAdminTermAsync(term, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return new AcademicTermMutationStoreResult(
                    AcademicTermMutationOutcome.Succeeded,
                    dto);
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbUpdateConcurrencyException)
        {
            _dbContext.ChangeTracker.Clear();
            var current = await CurrentTermVersionAsync(command.TermId, cancellationToken);
            return new(AcademicTermMutationOutcome.StaleVersion, CurrentVersion: current);
        }
        catch (Exception)
        {
            _dbContext.ChangeTracker.Clear();
            return new(AcademicTermMutationOutcome.StorageUnavailable);
        }
    }

    public async Task<AcademicProfileStoreResult> ReadByApplicationUserIdAsync(
        Guid applicationUserId,
        AcademicProfileReadRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var student = await _dbContext.Set<Student>()
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    candidate => candidate.ApplicationUserId == applicationUserId,
                    cancellationToken);
            if (student is null)
            {
                return new(AcademicProfileStoreOutcome.NotFound);
            }

            var termId = await (
                from state in _dbContext.Set<StudentTermAcademicState>().AsNoTracking()
                join term in _dbContext.Set<AcademicTerm>().AsNoTracking()
                    on state.TermId equals term.Id
                where state.StudentId == student.Id
                orderby term.TeachingStartsOn descending, term.Id
                select (Guid?)state.TermId)
                .FirstOrDefaultAsync(cancellationToken);
            return termId is null
                ? new(AcademicProfileStoreOutcome.ProfileNotReady)
                : await BuildProfileAsync(student, termId.Value, request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return new(AcademicProfileStoreOutcome.StorageUnavailable);
        }
    }

    public async Task<AcademicProfileStoreResult> ReadByStudentIdAsync(
        Guid studentId,
        Guid termId,
        AcademicProfileReadRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var student = await _dbContext.Set<Student>()
                .AsNoTracking()
                .SingleOrDefaultAsync(candidate => candidate.Id == studentId, cancellationToken);
            return student is null
                ? new(AcademicProfileStoreOutcome.NotFound)
                : await BuildProfileAsync(student, termId, request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return new(AcademicProfileStoreOutcome.StorageUnavailable);
        }
    }

    public async Task<AcademicProfileStoreResult> CorrectAsync(
        CorrectAcademicProfileStoreCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        try
        {
            var outcome = await ExecuteMutationAsync(async () =>
            {
                _dbContext.ChangeTracker.Clear();
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);
                var state = await _dbContext.Set<StudentTermAcademicState>()
                    .SingleOrDefaultAsync(
                        candidate => candidate.StudentId == command.StudentId &&
                            candidate.TermId == command.TermId,
                        cancellationToken);
                var student = await _dbContext.Set<Student>()
                    .SingleOrDefaultAsync(candidate => candidate.Id == command.StudentId, cancellationToken);
                if (state is null || student is null)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return new AcademicProfileStoreResult(AcademicProfileStoreOutcome.NotFound);
                }

                if (!VersionsEqual(student.Version, command.ExpectedStudentRowVersion) ||
                    !VersionsEqual(state.Version, command.ExpectedStudentTermStateRowVersion))
                {
                    var version = !VersionsEqual(student.Version, command.ExpectedStudentRowVersion)
                        ? EncodeVersion(student.Version)
                        : EncodeVersion(state.Version);
                    await transaction.CommitAsync(cancellationToken);
                    return new AcademicProfileStoreResult(
                        AcademicProfileStoreOutcome.StaleVersion,
                        CurrentVersion: version);
                }

                var activeHoldCount = await ActiveHoldsQuery(
                        student.Id,
                        command.TermId,
                        command.ResponseRead.ServerNowUtc)
                    .CountAsync(cancellationToken);
                if (activeHoldCount > MaximumActiveHolds)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return new AcademicProfileStoreResult(AcademicProfileStoreOutcome.ProfileNotReady);
                }

                foreach (var mutation in command.Mutations)
                {
                    var mutationOutcome = await ApplyProfileMutationAsync(
                        student,
                        command,
                        mutation,
                        cancellationToken);
                    if (mutationOutcome is not AcademicProfileStoreOutcome.Succeeded)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        _dbContext.ChangeTracker.Clear();
                        return new AcademicProfileStoreResult(mutationOutcome);
                    }
                }

                ForceVersionAdvance(student, nameof(Student.DataVersion));
                ForceVersionAdvance(state, nameof(StudentTermAcademicState.DataVersion));
                await _auditWriter.AppendAsync(command.AuditEvent, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return new AcademicProfileStoreResult(AcademicProfileStoreOutcome.Succeeded);
            });

            if (outcome.Outcome is not AcademicProfileStoreOutcome.Succeeded)
            {
                return outcome;
            }

            _dbContext.ChangeTracker.Clear();
            var student = await _dbContext.Set<Student>()
                .AsNoTracking()
                .SingleAsync(candidate => candidate.Id == command.StudentId, cancellationToken);
            return await BuildProfileAsync(
                student,
                command.TermId,
                command.ResponseRead,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbUpdateConcurrencyException)
        {
            _dbContext.ChangeTracker.Clear();
            var version = await CurrentStudentTermVersionAsync(
                command.StudentId,
                command.TermId,
                cancellationToken);
            return new(AcademicProfileStoreOutcome.StaleVersion, CurrentVersion: version);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            _dbContext.ChangeTracker.Clear();
            return new(AcademicProfileStoreOutcome.InvalidSupersession);
        }
        catch (Exception)
        {
            _dbContext.ChangeTracker.Clear();
            return new(AcademicProfileStoreOutcome.StorageUnavailable);
        }
    }

    public async Task<AcademicProfileStoreResult> ExecuteRegistrationBoundaryAsync(
        RegistrationBoundaryStoreCommand command,
        Func<CancellationToken, Task> commitCallback,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(commitCallback);
        try
        {
            return await ExecuteMutationAsync(async () =>
            {
                _dbContext.ChangeTracker.Clear();
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);
                var state = await _dbContext.Set<StudentTermAcademicState>()
                    .SingleOrDefaultAsync(
                        candidate => candidate.StudentId == command.StudentId &&
                            candidate.TermId == command.TermId,
                        cancellationToken);
                if (state is null)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return new(AcademicProfileStoreOutcome.NotFound);
                }

                if (!VersionsEqual(state.Version, command.ExpectedStudentTermStateRowVersion))
                {
                    var version = EncodeVersion(state.Version);
                    await transaction.CommitAsync(cancellationToken);
                    return new AcademicProfileStoreResult(
                        AcademicProfileStoreOutcome.StaleVersion,
                        CurrentVersion: version);
                }

                var activeHolds = await ActiveHoldsQuery(
                        command.StudentId,
                        command.TermId,
                        command.ServerReceivedAtUtc)
                    .OrderBy(hold => hold.Id)
                    .Take(MaximumActiveHolds + 1)
                    .ToArrayAsync(cancellationToken);
                if (activeHolds.Length > MaximumActiveHolds)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return new(AcademicProfileStoreOutcome.ProfileNotReady);
                }

                if (activeHolds.Any(hold => hold.BlocksRegistration))
                {
                    await transaction.CommitAsync(cancellationToken);
                    return new(AcademicProfileStoreOutcome.HoldBlocked);
                }

                await commitCallback(cancellationToken);
                ForceVersionAdvance(state, nameof(StudentTermAcademicState.DataVersion));
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return new(AcademicProfileStoreOutcome.Succeeded);
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbUpdateConcurrencyException)
        {
            _dbContext.ChangeTracker.Clear();
            var version = await CurrentStudentTermVersionAsync(
                command.StudentId,
                command.TermId,
                cancellationToken);
            return new(AcademicProfileStoreOutcome.StaleVersion, CurrentVersion: version);
        }
        catch (Exception)
        {
            _dbContext.ChangeTracker.Clear();
            return new(AcademicProfileStoreOutcome.StorageUnavailable);
        }
    }

    public async Task<DemoStudentProfileSeedResult> ReconcileAsync(
        DemoStudentProfileSeedCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateSeedCommand(command);
        try
        {
            return await ExecuteMutationAsync(async () =>
            {
                _dbContext.ChangeTracker.Clear();
                await using var transaction = _dbContext.Database.CurrentTransaction is null
                    ? await _dbContext.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable,
                        cancellationToken)
                    : null;
                var identity = await _dbContext.Set<ApplicationUser>()
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        user => user.Id == command.Student.ApplicationUserId,
                        cancellationToken)
                    ?? throw new InvalidOperationException(
                        "ACADEMIC_SEED_IDENTITY_REQUIRED: The synthetic identity must exist first.");
                if (!string.Equals(identity.UniversityId, command.UniversityId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "ACADEMIC_SEED_IDENTITY_MISMATCH: University ID does not match the identity row.");
                }

                var existing = await _dbContext.Set<Student>()
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        student => student.ApplicationUserId == command.Student.ApplicationUserId,
                        cancellationToken);
                if (existing is not null)
                {
                    if (!string.Equals(
                            existing.DataVersion,
                            command.SeedProfileVersion,
                            StringComparison.Ordinal))
                    {
                        await UpgradeDevelopmentSeedIfSupportedAsync(
                                existing,
                                command,
                                cancellationToken)
                            .ConfigureAwait(false);
                        existing = await _dbContext.Set<Student>()
                            .AsNoTracking()
                            .SingleAsync(
                                student => student.ApplicationUserId
                                    == command.Student.ApplicationUserId,
                                cancellationToken)
                            .ConfigureAwait(false);
                    }
                    await EnsureSeedMatchesAsync(existing, command, cancellationToken);
                    if (transaction is not null)
                    {
                        await transaction.CommitAsync(cancellationToken);
                    }
                    return new DemoStudentProfileSeedResult(existing.Id, Created: false);
                }

                _dbContext.Add(command.Student);
                _dbContext.AddRange(command.TranscriptAttempts.OrderBy(attempt => attempt.Id));
                _dbContext.AddRange(command.Holds.OrderBy(hold => hold.Id));
                _dbContext.Add(command.StudentTermAcademicState);
                await _dbContext.SaveChangesAsync(cancellationToken);
                if (transaction is not null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }
                return new DemoStudentProfileSeedResult(command.Student.Id, Created: true);
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            _dbContext.ChangeTracker.Clear();
            var existing = await _dbContext.Set<Student>()
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    student => student.ApplicationUserId == command.Student.ApplicationUserId,
                    cancellationToken);
            if (existing is null)
            {
                throw;
            }

            await EnsureSeedMatchesAsync(existing, command, cancellationToken);
            return new DemoStudentProfileSeedResult(existing.Id, Created: false);
        }
    }

    public async Task<AdminAcademicStorePage<AdminTermDto>> ListTermsAsync(
        AdminTermQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var source = _dbContext.Set<AcademicTerm>().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                var pattern = $"%{EscapeLike(query.Query.Trim())}%";
                source = source.Where(term =>
                    EF.Functions.Like(term.Code, pattern, "\\") ||
                    EF.Functions.Like(term.DisplayName, pattern, "\\"));
            }

            if (query.State is { } state)
            {
                source = source.Where(term => term.State == state);
            }

            var total = await source.CountAsync(cancellationToken);
            source = query.Sort switch
            {
                "teachingStartsOn,id" => source.OrderBy(term => term.TeachingStartsOn)
                    .ThenBy(term => term.Id),
                "state,id" => source.OrderBy(term => term.State).ThenBy(term => term.Id),
                _ => source.OrderBy(term => term.Code).ThenBy(term => term.Id)
            };
            var terms = await source
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToArrayAsync(cancellationToken);
            var items = new List<AdminTermDto>(terms.Length);
            foreach (var term in terms)
            {
                items.Add(await ToAdminTermAsync(term, cancellationToken));
            }

            return new(AdminAcademicStoreOutcome.Succeeded, items, total);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return new(AdminAcademicStoreOutcome.StorageUnavailable, [], 0);
        }
    }

    public async Task<AdminAcademicStorePage<AdminStudentLocatorDto>> ListStudentsAsync(
        AdminStudentLocatorQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pattern = $"%{EscapeLike(query.Query.Trim())}%";
            var source =
                from state in _dbContext.Set<StudentTermAcademicState>().AsNoTracking()
                join student in _dbContext.Set<Student>().AsNoTracking()
                    on state.StudentId equals student.Id
                join user in _dbContext.Set<ApplicationUser>().AsNoTracking()
                    on student.ApplicationUserId equals user.Id
                where state.TermId == query.TermId &&
                    user.UniversityId != null &&
                    (EF.Functions.Like(user.UniversityId, pattern, "\\") ||
                     EF.Functions.Like(student.ProgramCode, pattern, "\\") ||
                     EF.Functions.Like(student.Cohort, pattern, "\\") ||
                     EF.Functions.Like(student.Standing, pattern, "\\"))
                select new
                {
                    Student = student,
                    UniversityId = user.UniversityId!
                };
            var total = await source.CountAsync(cancellationToken);
            var ordered = query.Sort switch
            {
                "program,studentId" => source.OrderBy(value => value.Student.ProgramCode)
                    .ThenBy(value => value.Student.Id),
                "cohort,studentId" => source.OrderBy(value => value.Student.Cohort)
                    .ThenBy(value => value.Student.Id),
                "standing,studentId" => source.OrderBy(value => value.Student.Standing)
                    .ThenBy(value => value.Student.Id),
                _ => source.OrderBy(value => value.UniversityId)
                    .ThenBy(value => value.Student.Id)
            };
            var rows = await ordered
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToArrayAsync(cancellationToken);
            var items = rows.Select(value => new AdminStudentLocatorDto(
                value.Student.Id.ToString("D"),
                value.UniversityId,
                value.Student.ProgramCode,
                value.Student.Cohort,
                value.Student.Standing,
                value.Student.DataVersion)).ToArray();
            return new(AdminAcademicStoreOutcome.Succeeded, items, total);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return new(AdminAcademicStoreOutcome.StorageUnavailable, [], 0);
        }
    }

    private async Task<AcademicProfileStoreResult> BuildProfileAsync(
        Student student,
        Guid termId,
        AcademicProfileReadRequest request,
        CancellationToken cancellationToken)
    {
        var termState = await _dbContext.Set<StudentTermAcademicState>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                state => state.StudentId == student.Id && state.TermId == termId,
                cancellationToken);
        if (termState is null)
        {
            return new(AcademicProfileStoreOutcome.NotFound);
        }

        var universityId = await _dbContext.Set<ApplicationUser>()
            .AsNoTracking()
            .Where(user => user.Id == student.ApplicationUserId)
            .Select(user => user.UniversityId)
            .SingleOrDefaultAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(universityId))
        {
            return new(AcademicProfileStoreOutcome.ProfileNotReady);
        }

        var activeHolds = await ActiveHoldsQuery(student.Id, termId, request.ServerNowUtc)
            .AsNoTracking()
            .OrderBy(hold => hold.Code)
            .ThenBy(hold => hold.Id)
            .Take(MaximumActiveHolds + 1)
            .ToArrayAsync(cancellationToken);
        if (activeHolds.Length > MaximumActiveHolds)
        {
            return new(AcademicProfileStoreOutcome.ProfileNotReady);
        }

        var attempts = _dbContext.Set<TranscriptAttempt>()
            .AsNoTracking()
            .Where(attempt => attempt.StudentId == student.Id);
        var leaves = attempts.Where(attempt =>
                !_dbContext.Set<TranscriptAttempt>().Any(
                    successor => successor.SupersedesAttemptId == attempt.Id));
        var transcriptTotal = await attempts.CountAsync(cancellationToken);
        var currentLeafCount = await leaves.CountAsync(cancellationToken);
        var attemptedCredits = await leaves
            .Select(attempt => (decimal?)attempt.Credits)
            .SumAsync(cancellationToken) ?? 0m;
        var earnedCredits = await leaves
            .Where(attempt => attempt.Status == TranscriptAttemptStatus.Passed)
            .Select(attempt => (decimal?)attempt.Credits)
            .SumAsync(cancellationToken) ?? 0m;
        var transcriptRows = await (
            from attempt in attempts
            join term in _dbContext.Set<AcademicTerm>().AsNoTracking()
                on attempt.TermId equals term.Id
            orderby attempt.ImportedAtUtc descending, attempt.Id
            select new
            {
                attempt.Id,
                attempt.SupersedesAttemptId,
                attempt.CourseCode,
                TermCode = term.Code,
                attempt.Credits,
                attempt.GradeCode,
                attempt.Status,
                attempt.Source,
                attempt.SourceReference
            })
            .Skip((request.TranscriptPage - 1) * request.TranscriptPageSize)
            .Take(request.TranscriptPageSize)
            .ToArrayAsync(cancellationToken);
        var transcriptDtos = transcriptRows.Select(row => new TranscriptAttemptDto(
            row.Id.ToString("D"),
            row.SupersedesAttemptId?.ToString("D"),
            row.CourseCode,
            row.TermCode,
            row.Credits,
            row.GradeCode,
            ToStatusToken(row.Status),
            row.SourceReference)).ToArray();

        var provenance = _dbContext.Set<Student>()
            .AsNoTracking()
            .Where(value => value.Id == student.Id)
            .Select(value => new
            {
                value.Source,
                Reference = value.SourceReference,
                value.ImportedAtUtc
            })
            .Concat(_dbContext.Set<StudentTermAcademicState>()
                .AsNoTracking()
                .Where(value => value.StudentId == student.Id && value.TermId == termId)
                .Select(value => new
                {
                    value.Source,
                    Reference = value.SourceReference,
                    ImportedAtUtc = value.DataAsOfUtc
                }))
            .Concat(_dbContext.Set<TranscriptAttempt>()
                .AsNoTracking()
                .Where(value => value.StudentId == student.Id)
                .Select(value => new
                {
                    value.Source,
                    Reference = value.SourceReference,
                    value.ImportedAtUtc
                }))
            .Concat(_dbContext.Set<StudentHold>()
                .AsNoTracking()
                .Where(value => value.StudentId == student.Id && value.TermId == termId)
                .Select(value => new
                {
                    value.Source,
                    Reference = value.SourceReference,
                    value.ImportedAtUtc
                }));
        var provenanceTotal = await provenance.CountAsync(cancellationToken);
        var provenanceRows = await provenance
            .OrderByDescending(value => value.ImportedAtUtc)
            .ThenBy(value => value.Reference)
            .Skip((request.ProvenancePage - 1) * request.ProvenancePageSize)
            .Take(request.ProvenancePageSize)
            .ToArrayAsync(cancellationToken);
        var provenanceDtos = provenanceRows.Select(value => new AcademicProvenanceDto(
            value.Source,
            value.Reference,
            value.ImportedAtUtc)).ToArray();

        return new AcademicProfileStoreResult(
            AcademicProfileStoreOutcome.Succeeded,
            new AcademicProfileSnapshot(
                universityId,
                student,
                termState,
                student.Version,
                termState.Version,
                new TranscriptSummaryDto(attemptedCredits, earnedCredits, currentLeafCount),
                new Page<TranscriptAttemptDto>(
                    transcriptDtos,
                    request.TranscriptPage,
                    request.TranscriptPageSize,
                    transcriptTotal,
                    "importedAtUtc-desc,attemptId"),
                activeHolds,
                new Page<AcademicProvenanceDto>(
                    provenanceDtos,
                    request.ProvenancePage,
                    request.ProvenancePageSize,
                    provenanceTotal,
                    "importedAtUtc-desc,reference")));
    }

    private async Task<AcademicProfileStoreOutcome> ApplyProfileMutationAsync(
        Student student,
        CorrectAcademicProfileStoreCommand command,
        AcademicProfileMutation mutation,
        CancellationToken cancellationToken)
    {
        switch (mutation)
        {
            case SetGpaMutation setGpa:
                SetCurrentValue(student, nameof(Student.CurrentGpa), setGpa.CurrentGpa);
                ApplyStudentProvenance(student, command.Source, setGpa.SourceReference, command.AuditEvent.OccurredAtUtc);
                return AcademicProfileStoreOutcome.Succeeded;
            case SetEarnedCreditsMutation setCredits:
                SetCurrentValue(student, nameof(Student.EarnedCredits), setCredits.EarnedCredits);
                ApplyStudentProvenance(student, command.Source, setCredits.SourceReference, command.AuditEvent.OccurredAtUtc);
                return AcademicProfileStoreOutcome.Succeeded;
            case SetStandingMutation setStanding:
                SetCurrentValue(student, nameof(Student.Standing), setStanding.Standing);
                ApplyStudentProvenance(student, command.Source, setStanding.SourceReference, command.AuditEvent.OccurredAtUtc);
                return AcademicProfileStoreOutcome.Succeeded;
            case AppendTranscriptAttemptMutation append:
                {
                    var term = await _dbContext.Set<AcademicTerm>()
                        .AsNoTracking()
                        .SingleOrDefaultAsync(candidate => candidate.Id == command.TermId, cancellationToken);
                    if (term is null || !string.Equals(term.Code, append.TermCode, StringComparison.Ordinal))
                    {
                        return AcademicProfileStoreOutcome.InvalidSupersession;
                    }

                    if (await _dbContext.Set<TranscriptAttempt>().AnyAsync(
                        attempt => attempt.Id == append.AttemptId,
                        cancellationToken))
                    {
                        return AcademicProfileStoreOutcome.InvalidSupersession;
                    }

                    if (append.SupersedesAttemptId is { } priorId)
                    {
                        var prior = await _dbContext.Set<TranscriptAttempt>()
                            .AsNoTracking()
                            .SingleOrDefaultAsync(attempt => attempt.Id == priorId, cancellationToken);
                        if (prior is null ||
                            prior.StudentId != student.Id ||
                            prior.TermId != command.TermId ||
                            !string.Equals(prior.CourseCode, append.CourseCode, StringComparison.Ordinal) ||
                            await _dbContext.Set<TranscriptAttempt>().AnyAsync(
                                attempt => attempt.SupersedesAttemptId == priorId,
                                cancellationToken))
                        {
                            return AcademicProfileStoreOutcome.InvalidSupersession;
                        }
                    }

                    _dbContext.Add(new TranscriptAttempt(
                        append.AttemptId,
                        student.Id,
                        command.TermId,
                        append.SupersedesAttemptId,
                        append.CourseCode,
                        append.Credits,
                        append.Grade,
                        append.Status,
                        command.Source,
                        append.SourceReference,
                        command.AuditEvent.OccurredAtUtc));
                    return AcademicProfileStoreOutcome.Succeeded;
                }
            case UpsertStudentHoldMutation upsert:
                {
                    var holdId = upsert.HoldId ?? Guid.NewGuid();
                    var hold = await _dbContext.Set<StudentHold>()
                        .SingleOrDefaultAsync(candidate => candidate.Id == holdId, cancellationToken);
                    if (hold is null)
                    {
                        _dbContext.Add(new StudentHold(
                            holdId,
                            student.Id,
                            command.TermId,
                            upsert.Code,
                            upsert.Message,
                            upsert.BlocksRegistration,
                            upsert.EffectiveFromUtc,
                            upsert.EffectiveToUtc,
                            command.Source,
                            upsert.SourceReference,
                            command.AuditEvent.OccurredAtUtc));
                        return AcademicProfileStoreOutcome.Succeeded;
                    }

                    if (hold.StudentId != student.Id || hold.TermId != command.TermId)
                    {
                        return AcademicProfileStoreOutcome.NotFound;
                    }

                    SetCurrentValue(hold, nameof(StudentHold.Code), upsert.Code);
                    SetCurrentValue(hold, nameof(StudentHold.Message), upsert.Message);
                    SetCurrentValue(hold, nameof(StudentHold.BlocksRegistration), upsert.BlocksRegistration);
                    SetCurrentValue(hold, nameof(StudentHold.EffectiveFromUtc), upsert.EffectiveFromUtc);
                    SetCurrentValue(hold, nameof(StudentHold.EffectiveToUtc), upsert.EffectiveToUtc);
                    SetCurrentValue(hold, nameof(StudentHold.Source), command.Source);
                    SetCurrentValue(hold, nameof(StudentHold.SourceReference), upsert.SourceReference);
                    SetCurrentValue(hold, nameof(StudentHold.ImportedAtUtc), command.AuditEvent.OccurredAtUtc);
                    return AcademicProfileStoreOutcome.Succeeded;
                }
            case RemoveStudentHoldMutation remove:
                {
                    var hold = await _dbContext.Set<StudentHold>()
                        .SingleOrDefaultAsync(candidate => candidate.Id == remove.HoldId, cancellationToken);
                    if (hold is null || hold.StudentId != student.Id || hold.TermId != command.TermId)
                    {
                        return AcademicProfileStoreOutcome.NotFound;
                    }

                    _dbContext.Remove(hold);
                    return AcademicProfileStoreOutcome.Succeeded;
                }
            default:
                return AcademicProfileStoreOutcome.ProfileNotReady;
        }
    }

    private IQueryable<StudentHold> ActiveHoldsQuery(
        Guid studentId,
        Guid termId,
        DateTime instantUtc) =>
        _dbContext.Set<StudentHold>().Where(hold =>
            hold.StudentId == studentId &&
            hold.TermId == termId &&
            hold.EffectiveFromUtc <= instantUtc &&
            (hold.EffectiveToUtc == null || instantUtc < hold.EffectiveToUtc));

    private async Task<AdminTermDto> ToAdminTermAsync(
        AcademicTerm term,
        CancellationToken cancellationToken)
    {
        var windows = await _dbContext.Set<RegistrationWindow>()
            .AsNoTracking()
            .Where(window => window.TermId == term.Id)
            .OrderBy(window => window.OpensAtUtc)
            .ThenBy(window => window.Id)
            .Take(MaximumWindowsPerTerm + 1)
            .ToArrayAsync(cancellationToken);
        if (windows.Length > MaximumWindowsPerTerm)
        {
            throw new InvalidOperationException(
                "ACADEMIC_TERM_WINDOW_LIMIT_EXCEEDED: A term may contain at most 20 windows.");
        }

        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        return new AdminTermDto(
            term.Id.ToString("D"),
            term.Code,
            term.DisplayName,
            term.TimeZoneId,
            term.TeachingStartsOn,
            term.TeachingEndsOn,
            term.State,
            EncodeVersion(term.Version),
            windows.Select(window => new AdminRegistrationWindowDto(
                window.Id.ToString("D"),
                ToContractScope(window.ScopeType),
                window.ScopeValue,
                window.OpensAtUtc,
                window.ClosesAtUtc,
                ToContractLifecycle(window.State),
                window.GetComputedState(nowUtc),
                EncodeVersion(window.Version))).ToArray());
    }

    private async Task<AcademicTermCreationStoreResult> ResolveCreationConflictAsync(
        CreateAcademicTermStoreCommand command,
        CancellationToken cancellationToken)
    {
        var replay = await _dbContext.Set<AcademicTerm>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                term => term.CreationClientRequestId == command.Term.CreationClientRequestId,
                cancellationToken);
        if (replay is not null)
        {
            return string.Equals(
                replay.CreationPayloadHash,
                command.Term.CreationPayloadHash,
                StringComparison.Ordinal)
                ? new AcademicTermCreationStoreResult(
                    AcademicTermCreationOutcome.Replayed,
                    await ToAdminTermAsync(replay, cancellationToken))
                : new(AcademicTermCreationOutcome.IdempotencyKeyReused);
        }

        return await _dbContext.Set<AcademicTerm>().AsNoTracking().AnyAsync(
            term => term.Code == command.Term.Code,
            cancellationToken)
            ? new(AcademicTermCreationOutcome.TermCodeExists)
            : new(AcademicTermCreationOutcome.StorageUnavailable);
    }

    private async Task EnsureSeedMatchesAsync(
        Student existing,
        DemoStudentProfileSeedCommand command,
        CancellationToken cancellationToken)
    {
        var state = await _dbContext.Set<StudentTermAcademicState>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.StudentId == existing.Id &&
                    candidate.TermId == command.StudentTermAcademicState.TermId,
                cancellationToken);
        var attempts = await _dbContext.Set<TranscriptAttempt>()
            .AsNoTracking()
            .Where(attempt => attempt.StudentId == existing.Id)
            .OrderBy(attempt => attempt.Id)
            .ToArrayAsync(cancellationToken);
        var holds = await _dbContext.Set<StudentHold>()
            .AsNoTracking()
            .Where(hold => hold.StudentId == existing.Id)
            .OrderBy(hold => hold.Id)
            .ToArrayAsync(cancellationToken);

        if (!StudentMatches(existing, command.Student) ||
            state is null || !StateMatches(state, command.StudentTermAcademicState) ||
            attempts.Length != command.TranscriptAttempts.Count ||
            command.TranscriptAttempts.Any(expected =>
                !attempts.Any(actual =>
                    actual.Id == expected.Id &&
                    TranscriptAttemptComparer.Instance.Equals(actual, expected))) ||
            holds.Length != command.Holds.Count ||
            command.Holds.Any(expected =>
                !holds.Any(actual =>
                    actual.Id == expected.Id &&
                    StudentHoldComparer.Instance.Equals(actual, expected))))
        {
            throw new InvalidOperationException(
                "ACADEMIC_SEED_VERSION_MISMATCH: Existing synthetic profile differs from the requested seed version.");
        }
    }

    private async Task UpgradeDevelopmentSeedIfSupportedAsync(
        Student existing,
        DemoStudentProfileSeedCommand command,
        CancellationToken cancellationToken)
    {
        var isSupportedVersionStep = string.Equals(
                existing.DataVersion,
                LegacySeedProfileVersion,
                StringComparison.Ordinal)
            && string.Equals(
                command.SeedProfileVersion,
                CurrentSeedProfileVersion,
                StringComparison.Ordinal);
        var state = await _dbContext.Set<StudentTermAcademicState>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.StudentId == existing.Id
                    && item.TermId == command.StudentTermAcademicState.TermId,
                cancellationToken)
            .ConfigureAwait(false);
        var attempts = await _dbContext.Set<TranscriptAttempt>()
            .AsNoTracking()
            .Where(item => item.StudentId == existing.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var holds = await _dbContext.Set<StudentHold>()
            .AsNoTracking()
            .Where(item => item.StudentId == existing.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var legacyReference = $"SeedProfileVersion={LegacySeedProfileVersion};";
        var ownsCompleteLegacyGraph = isSupportedVersionStep
            && existing.Id == command.Student.Id
            && string.Equals(existing.Source, SyntheticSeedSource, StringComparison.Ordinal)
            && existing.SourceReference.StartsWith(legacyReference, StringComparison.Ordinal)
            && state is not null
            && state.Id == command.StudentTermAcademicState.Id
            && string.Equals(state.Source, SyntheticSeedSource, StringComparison.Ordinal)
            && state.SourceReference.StartsWith(legacyReference, StringComparison.Ordinal)
            && attempts.Length == 2
            && attempts.All(item =>
                string.Equals(item.Source, SyntheticSeedSource, StringComparison.Ordinal)
                && item.SourceReference.StartsWith(legacyReference, StringComparison.Ordinal))
            && holds.Length == 2
            && holds.All(item =>
                string.Equals(item.Source, SyntheticSeedSource, StringComparison.Ordinal)
                && item.SourceReference.StartsWith(legacyReference, StringComparison.Ordinal));
        if (!ownsCompleteLegacyGraph)
        {
            throw new InvalidOperationException(
                "ACADEMIC_SEED_UPGRADE_UNSAFE: Existing academic data is not the complete v1 synthetic seed graph. Preserve it and rerun the launcher with -ResetDatabase only if deleting local demo data is intended.");
        }

        await _dbContext.Set<TranscriptAttempt>()
            .Where(item => item.StudentId == existing.Id)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
        await _dbContext.Set<StudentHold>()
            .Where(item => item.StudentId == existing.Id)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
        _dbContext.ChangeTracker.Clear();

        var trackedStudent = await _dbContext.Set<Student>()
            .SingleAsync(item => item.Id == existing.Id, cancellationToken)
            .ConfigureAwait(false);
        var trackedState = await _dbContext.Set<StudentTermAcademicState>()
            .SingleAsync(item => item.Id == state!.Id, cancellationToken)
            .ConfigureAwait(false);
        CopyStudentSeedValues(trackedStudent, command.Student);
        CopyStudentTermSeedValues(trackedState, command.StudentTermAcademicState);
        _dbContext.AddRange(command.TranscriptAttempts.OrderBy(item => item.Id));
        _dbContext.AddRange(command.Holds.OrderBy(item => item.Id));
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _dbContext.ChangeTracker.Clear();
    }

    private void CopyStudentSeedValues(Student target, Student source)
    {
        SetCurrentValue(target, nameof(Student.ProgramCode), source.ProgramCode);
        SetCurrentValue(target, nameof(Student.Cohort), source.Cohort);
        SetCurrentValue(target, nameof(Student.CurrentGpa), source.CurrentGpa);
        SetCurrentValue(target, nameof(Student.EarnedCredits), source.EarnedCredits);
        SetCurrentValue(target, nameof(Student.Standing), source.Standing);
        SetCurrentValue(target, nameof(Student.IsActive), source.IsActive);
        SetCurrentValue(target, nameof(Student.Source), source.Source);
        SetCurrentValue(target, nameof(Student.SourceReference), source.SourceReference);
        SetCurrentValue(target, nameof(Student.DataVersion), source.DataVersion);
        SetCurrentValue(target, nameof(Student.DataAsOfUtc), source.DataAsOfUtc);
        SetCurrentValue(target, nameof(Student.ImportedAtUtc), source.ImportedAtUtc);
    }

    private void CopyStudentTermSeedValues(
        StudentTermAcademicState target,
        StudentTermAcademicState source)
    {
        SetCurrentValue(
            target,
            nameof(StudentTermAcademicState.ProgramTermOrdinal),
            source.ProgramTermOrdinal);
        SetCurrentValue(target, nameof(StudentTermAcademicState.GpaAtStart), source.GpaAtStart);
        SetCurrentValue(
            target,
            nameof(StudentTermAcademicState.EarnedCreditsAtStart),
            source.EarnedCreditsAtStart);
        SetCurrentValue(
            target,
            nameof(StudentTermAcademicState.StandingAtStart),
            source.StandingAtStart);
        SetCurrentValue(target, nameof(StudentTermAcademicState.Source), source.Source);
        SetCurrentValue(
            target,
            nameof(StudentTermAcademicState.SourceReference),
            source.SourceReference);
        SetCurrentValue(target, nameof(StudentTermAcademicState.DataVersion), source.DataVersion);
        SetCurrentValue(
            target,
            nameof(StudentTermAcademicState.DataAsOfUtc),
            source.DataAsOfUtc);
    }

    private static void ValidateSeedCommand(DemoStudentProfileSeedCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.SeedProfileVersion) ||
            command.FixtureOrdinal < 1 ||
            string.IsNullOrWhiteSpace(command.UniversityId) ||
            !string.Equals(
                command.Student.DataVersion,
                command.SeedProfileVersion,
                StringComparison.Ordinal) ||
            command.StudentTermAcademicState.StudentId != command.Student.Id ||
            command.TranscriptAttempts.Any(attempt =>
                attempt.StudentId != command.Student.Id ||
                attempt.TermId != command.StudentTermAcademicState.TermId) ||
            command.Holds.Any(hold =>
                hold.StudentId != command.Student.Id ||
                hold.TermId != command.StudentTermAcademicState.TermId) ||
            command.Provenance.Count == 0)
        {
            throw new InvalidOperationException(
                "ACADEMIC_SEED_INCOMPLETE: The synthetic academic profile is incomplete.");
        }
    }

    private static bool StudentMatches(Student left, Student right) =>
        left.Id == right.Id &&
        left.ApplicationUserId == right.ApplicationUserId &&
        left.ProgramCode == right.ProgramCode &&
        left.Cohort == right.Cohort &&
        left.CurrentGpa == right.CurrentGpa &&
        left.EarnedCredits == right.EarnedCredits &&
        left.Standing == right.Standing &&
        left.IsActive == right.IsActive &&
        left.Source == right.Source &&
        left.SourceReference == right.SourceReference &&
        left.DataVersion == right.DataVersion &&
        left.DataAsOfUtc == right.DataAsOfUtc &&
        left.ImportedAtUtc == right.ImportedAtUtc;

    private static bool StateMatches(
        StudentTermAcademicState left,
        StudentTermAcademicState right) =>
        left.Id == right.Id &&
        left.StudentId == right.StudentId &&
        left.TermId == right.TermId &&
        left.ProgramTermOrdinal == right.ProgramTermOrdinal &&
        left.GpaAtStart == right.GpaAtStart &&
        left.EarnedCreditsAtStart == right.EarnedCreditsAtStart &&
        left.StandingAtStart == right.StandingAtStart &&
        left.Source == right.Source &&
        left.SourceReference == right.SourceReference &&
        left.DataVersion == right.DataVersion &&
        left.DataAsOfUtc == right.DataAsOfUtc;

    private void ApplyStudentProvenance(
        Student student,
        string source,
        string sourceReference,
        DateTime occurredAtUtc)
    {
        SetCurrentValue(student, nameof(Student.Source), source);
        SetCurrentValue(student, nameof(Student.SourceReference), sourceReference);
        SetCurrentValue(student, nameof(Student.DataAsOfUtc), occurredAtUtc);
        SetCurrentValue(student, nameof(Student.ImportedAtUtc), occurredAtUtc);
    }

    private static bool CanApplyWindowUpdate(
        RegistrationWindow window,
        TermWindowInput input)
    {
        if (window.State is DomainWindowLifecycle.Draft)
        {
            return input.LifecycleState is ContractWindowLifecycle.Draft;
        }

        if (window.ScopeType != ToDomainScope(input.ScopeType) ||
            !string.Equals(window.ScopeValue, input.ScopeValue, StringComparison.Ordinal) ||
            window.OpensAtUtc != input.OpensAtUtc ||
            window.ClosesAtUtc != input.ClosesAtUtc)
        {
            return false;
        }

        return window.State switch
        {
            DomainWindowLifecycle.Published => input.LifecycleState is
                ContractWindowLifecycle.Published or
                ContractWindowLifecycle.EmergencyClosed or
                ContractWindowLifecycle.Superseded,
            DomainWindowLifecycle.EmergencyClosed => input.LifecycleState is
                ContractWindowLifecycle.EmergencyClosed,
            DomainWindowLifecycle.Superseded => input.LifecycleState is
                ContractWindowLifecycle.Superseded,
            _ => false
        };
    }

    private void ApplyWindowInput(RegistrationWindow window, TermWindowInput input)
    {
        if (window.State is DomainWindowLifecycle.Draft)
        {
            SetCurrentValue(window, nameof(RegistrationWindow.ScopeType), ToDomainScope(input.ScopeType));
            SetCurrentValue(window, nameof(RegistrationWindow.ScopeValue), input.ScopeValue);
            SetCurrentValue(window, nameof(RegistrationWindow.OpensAtUtc), input.OpensAtUtc);
            SetCurrentValue(window, nameof(RegistrationWindow.ClosesAtUtc), input.ClosesAtUtc);
            return;
        }

        if (window.State is DomainWindowLifecycle.Published &&
            input.LifecycleState is ContractWindowLifecycle.EmergencyClosed)
        {
            window.EmergencyClose();
        }
        else if (window.State is DomainWindowLifecycle.Published &&
            input.LifecycleState is ContractWindowLifecycle.Superseded)
        {
            window.Supersede();
        }
    }

    private void ApplyTermInput(AcademicTerm term, TermInput input)
    {
        SetCurrentValue(term, nameof(AcademicTerm.Code), input.Code);
        SetCurrentValue(term, nameof(AcademicTerm.DisplayName), input.DisplayName);
        SetCurrentValue(term, nameof(AcademicTerm.TimeZoneId), input.TimeZoneId);
        SetCurrentValue(term, nameof(AcademicTerm.TeachingStartsOn), input.TeachingStartsOn);
        SetCurrentValue(term, nameof(AcademicTerm.TeachingEndsOn), input.TeachingEndsOn);
        SetCurrentValue(term, nameof(AcademicTerm.State), input.State);
    }

    private static RegistrationWindow ToDomainWindow(
        Guid id,
        Guid termId,
        TermWindowInput input) =>
        new(
            id,
            termId,
            ToDomainScope(input.ScopeType),
            input.ScopeValue,
            input.OpensAtUtc,
            input.ClosesAtUtc,
            input.LifecycleState switch
            {
                ContractWindowLifecycle.Draft => DomainWindowLifecycle.Draft,
                ContractWindowLifecycle.Published => DomainWindowLifecycle.Published,
                ContractWindowLifecycle.EmergencyClosed => DomainWindowLifecycle.EmergencyClosed,
                ContractWindowLifecycle.Superseded => DomainWindowLifecycle.Superseded,
                _ => throw new ArgumentOutOfRangeException(nameof(input.LifecycleState))
            });

    private static DomainWindowScope ToDomainScope(ContractWindowScope scope) => scope switch
    {
        ContractWindowScope.AllStudents => DomainWindowScope.AllStudents,
        ContractWindowScope.Program => DomainWindowScope.Program,
        ContractWindowScope.Cohort => DomainWindowScope.Cohort,
        _ => throw new ArgumentOutOfRangeException(nameof(scope))
    };

    private static ContractWindowScope ToContractScope(DomainWindowScope scope) => scope switch
    {
        DomainWindowScope.AllStudents => ContractWindowScope.AllStudents,
        DomainWindowScope.Program => ContractWindowScope.Program,
        DomainWindowScope.Cohort => ContractWindowScope.Cohort,
        _ => throw new ArgumentOutOfRangeException(nameof(scope))
    };

    private static ContractWindowLifecycle ToContractLifecycle(DomainWindowLifecycle state) =>
        state switch
        {
            DomainWindowLifecycle.Draft => ContractWindowLifecycle.Draft,
            DomainWindowLifecycle.Published => ContractWindowLifecycle.Published,
            DomainWindowLifecycle.EmergencyClosed => ContractWindowLifecycle.EmergencyClosed,
            DomainWindowLifecycle.Superseded => ContractWindowLifecycle.Superseded,
            _ => throw new ArgumentOutOfRangeException(nameof(state))
        };

    private static bool HasPublishedOverlap(IEnumerable<RegistrationWindow> windows)
    {
        var published = windows
            .Where(window => window.State == DomainWindowLifecycle.Published)
            .OrderBy(window => window.OpensAtUtc)
            .ThenBy(window => window.Id)
            .ToArray();
        for (var index = 1; index < published.Length; index++)
        {
            if (Overlaps(published[index - 1], published[index]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool Overlaps(RegistrationWindow left, RegistrationWindow right) =>
        left.OpensAtUtc < right.ClosesAtUtc && right.OpensAtUtc < left.ClosesAtUtc;

    private static bool IsSingletonState(TermState state) =>
        state is TermState.RegistrationOpen or TermState.Teaching;

    private static bool VersionsEqual(byte[] left, byte[] right) =>
        left.AsSpan().SequenceEqual(right);

    private static string EncodeVersion(byte[] version) => Convert.ToBase64String(version);

    private static Guid ParseId(string value) =>
        Guid.TryParse(value, out var id) && id != Guid.Empty
            ? id
            : throw new InvalidOperationException("A persisted window identifier must be a GUID.");

    private async Task<string?> CurrentTermVersionAsync(
        Guid termId,
        CancellationToken cancellationToken)
    {
        var version = await _dbContext.Set<AcademicTerm>()
            .AsNoTracking()
            .Where(term => term.Id == termId)
            .Select(term => term.Version)
            .SingleOrDefaultAsync(cancellationToken);
        return version is null ? null : EncodeVersion(version);
    }

    private async Task<string?> CurrentStudentTermVersionAsync(
        Guid studentId,
        Guid termId,
        CancellationToken cancellationToken)
    {
        var version = await _dbContext.Set<StudentTermAcademicState>()
            .AsNoTracking()
            .Where(state => state.StudentId == studentId && state.TermId == termId)
            .Select(state => state.Version)
            .SingleOrDefaultAsync(cancellationToken);
        return version is null ? null : EncodeVersion(version);
    }

    private Task<TResult> ExecuteMutationAsync<TResult>(Func<Task<TResult>> operation) =>
        _dbContext.Database.CurrentTransaction is not null
            ? operation()
            : _dbContext.Database.CreateExecutionStrategy().ExecuteAsync(operation);

    private void ForceVersionAdvance<TEntity>(TEntity entity, string propertyName)
        where TEntity : class =>
        _dbContext.Entry(entity).Property(propertyName).IsModified = true;

    private void SetCurrentValue<TEntity>(TEntity entity, string propertyName, object? value)
        where TEntity : class =>
        _dbContext.Entry(entity).Property(propertyName).CurrentValue = value;

    private static string ToStatusToken(TranscriptAttemptStatus status) => status switch
    {
        TranscriptAttemptStatus.InProgress => "in-progress",
        TranscriptAttemptStatus.Passed => "passed",
        TranscriptAttemptStatus.Failed => "failed",
        TranscriptAttemptStatus.Withdrawn => "withdrawn",
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.GetBaseException() is Microsoft.Data.SqlClient.SqlException sqlException &&
        sqlException.Number is 2601 or 2627;

    private sealed class TranscriptAttemptComparer : IEqualityComparer<TranscriptAttempt>
    {
        public static TranscriptAttemptComparer Instance { get; } = new();

        public bool Equals(TranscriptAttempt? left, TranscriptAttempt? right) =>
            left is not null && right is not null &&
            left.Id == right.Id &&
            left.StudentId == right.StudentId &&
            left.TermId == right.TermId &&
            left.SupersedesAttemptId == right.SupersedesAttemptId &&
            left.CourseCode == right.CourseCode &&
            left.Credits == right.Credits &&
            left.GradeCode == right.GradeCode &&
            left.Status == right.Status &&
            left.Source == right.Source &&
            left.SourceReference == right.SourceReference &&
            left.ImportedAtUtc == right.ImportedAtUtc;

        public int GetHashCode(TranscriptAttempt value) => value.Id.GetHashCode();
    }

    private sealed class StudentHoldComparer : IEqualityComparer<StudentHold>
    {
        public static StudentHoldComparer Instance { get; } = new();

        public bool Equals(StudentHold? left, StudentHold? right) =>
            left is not null && right is not null &&
            left.Id == right.Id &&
            left.StudentId == right.StudentId &&
            left.TermId == right.TermId &&
            left.Code == right.Code &&
            left.Message == right.Message &&
            left.BlocksRegistration == right.BlocksRegistration &&
            left.EffectiveFromUtc == right.EffectiveFromUtc &&
            left.EffectiveToUtc == right.EffectiveToUtc &&
            left.Source == right.Source &&
            left.SourceReference == right.SourceReference &&
            left.ImportedAtUtc == right.ImportedAtUtc;

        public int GetHashCode(StudentHold value) => value.Id.GetHashCode();
    }
}
