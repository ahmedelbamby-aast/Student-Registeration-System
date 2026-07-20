using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Academics.Domain;
using StudentRegistration.IdentityAccess.Domain;
using StudentRegistration.Infrastructure.SqlServer.Persistence;
using StudentRegistration.Scheduling.Application;
using StudentRegistration.Scheduling.Application.Ports;
using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.Infrastructure.SqlServer.Scheduling;

public sealed class SqlOfferingStore(StudentRegistrationDbContext dbContext)
    : IOfferingStore
{
    public async Task<OfferingSnapshot?> LoadAsync(
        Guid offeringId,
        CancellationToken cancellationToken)
    {
        var offering = await dbContext.Set<CourseOffering>()
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == offeringId, cancellationToken)
            .ConfigureAwait(false);
        return offering is null
            ? null
            : await BuildSnapshotAsync(offering, cancellationToken).ConfigureAwait(false);
    }

    public async Task<OfferingGroupSnapshot?> LoadGroupAsync(
        Guid groupId,
        CancellationToken cancellationToken)
    {
        var offeringId = await dbContext.Set<SectionGroup>()
            .AsNoTracking()
            .Where(group => group.Id == groupId)
            .Select(group => (Guid?)group.OfferingId)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (offeringId is null)
        {
            return null;
        }

        var offering = await LoadAsync(offeringId.Value, cancellationToken)
            .ConfigureAwait(false);
        return offering?.Groups.Single(group => group.Id == groupId);
    }

    public async Task<OfferingSnapshot> CreateAsync(
        CreateOfferingStoreCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (!await dbContext.Set<AcademicTerm>()
                .AsNoTracking()
                .AnyAsync(term => term.Id == command.TermId, cancellationToken)
                .ConfigureAwait(false))
        {
            throw NotFound("TERM_NOT_FOUND", "The academic term was not found.");
        }

        if (!await dbContext.Set<Course>()
                .AsNoTracking()
                .AnyAsync(course => course.Id == command.CourseId, cancellationToken)
                .ConfigureAwait(false))
        {
            throw NotFound("COURSE_NOT_FOUND", "The catalogue course was not found.");
        }

        if (await dbContext.Set<CourseOffering>()
                .AsNoTracking()
                .AnyAsync(
                    offering => offering.TermId == command.TermId
                        && offering.CourseId == command.CourseId,
                    cancellationToken)
                .ConfigureAwait(false))
        {
            throw Conflict("OFFERING_EXISTS", "The term already has an offering for this course.");
        }

        var normalizedCodes = command.Groups
            .Select(group => NormalizeCode(group.Code))
            .ToArray();
        if (normalizedCodes.Distinct(StringComparer.Ordinal).Count() != normalizedCodes.Length)
        {
            throw Conflict("GROUP_CODE_EXISTS", "Group codes must be unique within an offering.");
        }

        var offering = new CourseOffering(
            Guid.NewGuid(),
            command.TermId,
            command.CourseId,
            CourseOfferingState.Draft);
        dbContext.Add(offering);
        foreach (var group in command.Groups)
        {
            dbContext.Add(new SectionGroup(
                Guid.NewGuid(),
                offering.Id,
                group.Code,
                group.Capacity,
                enrolledCount: 0,
                SectionGroupState.Draft,
                registrationPaused: false));
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw Conflict("OFFERING_EXISTS", "The offering or group code already exists.");
        }
        finally
        {
            dbContext.ChangeTracker.Clear();
        }

        return await LoadAsync(offering.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The created offering could not be reloaded.");
    }

    public async Task<OfferingGroupSnapshot> UpdateGroupAsync(
        UpdateGroupStoreCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var group = await dbContext.Set<SectionGroup>()
            .Include(item => item.Meetings)
            .Include(item => item.StaffAssignments)
            .SingleOrDefaultAsync(item => item.Id == command.GroupId, cancellationToken)
            .ConfigureAwait(false);
        if (group is null)
        {
            throw NotFound("GROUP_NOT_FOUND", "The group was not found.");
        }

        var offering = await dbContext.Set<CourseOffering>()
            .SingleOrDefaultAsync(item => item.Id == group.OfferingId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw NotFound("OFFERING_NOT_FOUND", "The owning offering was not found.");
        if (!offering.Version.AsSpan().SequenceEqual(command.ExpectedOfferingRowVersion)
            || !group.Version.AsSpan().SequenceEqual(command.ExpectedGroupRowVersion))
        {
            throw Conflict("STALE_VERSION", "The offering or group changed.");
        }

        var duplicateCode = await dbContext.Set<SectionGroup>()
            .AsNoTracking()
            .AnyAsync(
                item => item.OfferingId == group.OfferingId
                    && item.Id != group.Id
                    && item.GroupCode == NormalizeCode(command.GroupCode),
                cancellationToken)
            .ConfigureAwait(false);
        if (duplicateCode)
        {
            throw Conflict("GROUP_CODE_EXISTS", "The group code already exists.");
        }

        var roomIds = command.Meetings.Select(meeting => meeting.RoomId).Distinct().ToArray();
        var existingRoomCount = await dbContext.Set<Room>()
            .AsNoTracking()
            .CountAsync(room => roomIds.Contains(room.Id), cancellationToken)
            .ConfigureAwait(false);
        if (existingRoomCount != roomIds.Length)
        {
            throw NotFound("ROOM_NOT_FOUND", "One or more rooms were not found.");
        }

        var staffIds = command.StaffAssignments.Select(item => item.StaffId).Distinct().ToArray();
        var existingStaffCount = await dbContext.Set<Staff>()
            .AsNoTracking()
            .CountAsync(staff => staffIds.Contains(staff.Id), cancellationToken)
            .ConfigureAwait(false);
        if (existingStaffCount != staffIds.Length)
        {
            throw NotFound("STAFF_NOT_FOUND", "One or more staff members were not found.");
        }

        group.Rename(command.GroupCode);
        try
        {
            group.ChangeCapacity(command.Capacity);
        }
        catch (ArgumentException exception)
        {
            throw Conflict("CAPACITY_BELOW_ENROLLED", exception.Message);
        }

        group.SetRegistrationPaused(command.RegistrationPaused);
        var meetings = command.Meetings.Select(input => new MeetingSlot(
            input.Id,
            group.Id,
            input.RoomId,
            ParseActivity(input.ActivityType),
            (DayOfWeek)input.DayOfWeek,
            input.StartLocal,
            input.EndLocal)).ToArray();
        var meetingTypes = meetings.ToDictionary(item => item.Id, item => item.ActivityType);
        var assignments = command.StaffAssignments.Select(input =>
            new GroupStaffAssignment(
                group.Id,
                input.MeetingSlotId,
                meetingTypes.GetValueOrDefault(input.MeetingSlotId),
                input.StaffId,
                ParseTeachingRole(input.Role))).ToArray();
        group.ReplaceSchedule(meetings, assignments);

        // Child changes advance the owning offering concurrency boundary.
        dbContext.Entry(offering).Property(item => item.State).IsModified = true;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw Conflict("STALE_VERSION", "The offering or group changed.");
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw Conflict("GROUP_CODE_EXISTS", "The group code already exists.");
        }
        finally
        {
            dbContext.ChangeTracker.Clear();
        }

        return await LoadGroupAsync(group.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The updated group could not be reloaded.");
    }

    public async Task<AdminOfferingPage> ListAsync(
        AdminOfferingQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var source =
            from offering in dbContext.Set<CourseOffering>().AsNoTracking()
            join course in dbContext.Set<Course>().AsNoTracking()
                on offering.CourseId equals course.Id
            select new
            {
                offering.Id,
                offering.TermId,
                offering.CourseId,
                CourseCode = course.Code,
                CourseTitle = course.Title,
                offering.State,
                offering.Version,
                GroupCount = dbContext.Set<SectionGroup>()
                    .Count(item => item.OfferingId == offering.Id)
            };

        if (query.TermId is { } termId)
        {
            source = source.Where(item => item.TermId == termId);
        }
        if (!string.IsNullOrWhiteSpace(query.State))
        {
            var state = ParseOfferingState(query.State);
            source = source.Where(item => item.State == state);
        }
        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var pattern = $"%{EscapeLike(query.Query)}%";
            source = source.Where(item =>
                EF.Functions.Like(item.CourseCode, pattern, "\\")
                || EF.Functions.Like(item.CourseTitle, pattern, "\\"));
        }

        var total = await source.CountAsync(cancellationToken).ConfigureAwait(false);
        source = query.Sort switch
        {
            "courseCode-desc,id" => source.OrderByDescending(item => item.CourseCode).ThenBy(item => item.Id),
            "state,id" => source.OrderBy(item => item.State).ThenBy(item => item.Id),
            "state-desc,id" => source.OrderByDescending(item => item.State).ThenBy(item => item.Id),
            _ => source.OrderBy(item => item.CourseCode).ThenBy(item => item.Id),
        };
        var rows = await source
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var items = rows.Select(row => new OfferingSnapshot(row.Id, Token(row.State), [])
        {
            TermId = row.TermId,
            CourseId = row.CourseId,
            CourseCode = row.CourseCode,
            CourseTitle = row.CourseTitle,
            RowVersion = row.Version.ToArray(),
            GroupCount = row.GroupCount,
        }).ToArray();
        return new(items, query.Page, query.PageSize, total, query.Sort ?? "courseCode,id");
    }

    private async Task<OfferingSnapshot> BuildSnapshotAsync(
        CourseOffering offering,
        CancellationToken cancellationToken)
    {
        var course = await dbContext.Set<Course>()
            .AsNoTracking()
            .SingleAsync(item => item.Id == offering.CourseId, cancellationToken)
            .ConfigureAwait(false);
        var groups = await dbContext.Set<SectionGroup>()
            .AsNoTracking()
            .Where(item => item.OfferingId == offering.Id)
            .OrderBy(item => item.GroupCode)
            .ThenBy(item => item.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var groupIds = groups.Select(item => item.Id).ToArray();
        var meetings = await dbContext.Set<MeetingSlot>()
            .AsNoTracking()
            .Where(item => groupIds.Contains(item.GroupId))
            .OrderBy(item => item.DayOfWeek)
            .ThenBy(item => item.StartLocal)
            .ThenBy(item => item.Id)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var roomIds = meetings.Select(item => item.RoomId).Distinct().ToArray();
        var rooms = await dbContext.Set<Room>()
            .AsNoTracking()
            .Where(item => roomIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, cancellationToken)
            .ConfigureAwait(false);
        var assignments = await dbContext.Set<GroupStaffAssignment>()
            .AsNoTracking()
            .Where(item => groupIds.Contains(item.GroupId))
            .OrderBy(item => item.MeetingSlotId)
            .ThenBy(item => item.StaffId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
        var staffIds = assignments.Select(item => item.StaffId).Distinct().ToArray();
        var staff = await dbContext.Set<Staff>()
            .AsNoTracking()
            .Where(item => staffIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, cancellationToken)
            .ConfigureAwait(false);

        var snapshots = groups.Select(group =>
        {
            var heldSeatCount = ReadHeldSeatCount(group);
            return new OfferingGroupSnapshot(
                group.Id,
                group.GroupCode,
                group.Capacity,
                group.EnrolledCount,
                group.RegistrationPaused,
                Token(group.State),
                group.Version.ToArray(),
                meetings.Where(meeting => meeting.GroupId == group.Id).Select(meeting =>
                    new OfferingMeetingSnapshot(
                        meeting.Id,
                        Token(meeting.ActivityType),
                        (int)meeting.DayOfWeek,
                        meeting.StartLocal,
                        meeting.EndLocal,
                        rooms.GetValueOrDefault(meeting.RoomId)?.Code ?? "Unavailable",
                        rooms.GetValueOrDefault(meeting.RoomId)?.Location ?? "Unavailable",
                        assignments.Where(item => item.MeetingSlotId == meeting.Id).Select(item =>
                            new OfferingStaffSnapshot(
                                item.StaffId,
                                Token(item.TeachingRole),
                                staff.GetValueOrDefault(item.StaffId)?.DisplayName ?? "Assigned staff"))
                            .ToArray())
                    {
                        RoomId = meeting.RoomId,
                    }).ToArray())
            {
                OfferingId = group.OfferingId,
                HeldSeatCount = heldSeatCount,
            };
        }).ToArray();

        return new OfferingSnapshot(offering.Id, Token(offering.State), snapshots)
        {
            TermId = offering.TermId,
            CourseId = offering.CourseId,
            CourseCode = course.Code,
            CourseTitle = course.Title,
            RowVersion = offering.Version.ToArray(),
            GroupCount = snapshots.Length,
        };
    }

    private static int ReadHeldSeatCount(SectionGroup group)
    {
        var property = group.GetType().GetProperty("HeldSeatCount")
            ?? group.GetType().GetProperty("HeldCount");
        return property?.GetValue(group) is int value ? value : 0;
    }

    private static string NormalizeCode(string value) => value.Trim().Normalize().ToUpperInvariant();

    private static ActivityType ParseActivity(string value) =>
        Enum.TryParse<ActivityType>(value, ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed)
                ? parsed
                : throw new ArgumentException("The activity type is invalid.", nameof(value));

    private static TeachingRole ParseTeachingRole(string value) =>
        Enum.TryParse<TeachingRole>(value.Replace("-", string.Empty), ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed)
                ? parsed
                : throw new ArgumentException("The teaching role is invalid.", nameof(value));

    private static CourseOfferingState ParseOfferingState(string value) => value switch
    {
        "draft" => CourseOfferingState.Draft,
        "published" => CourseOfferingState.Published,
        "closed" => CourseOfferingState.Closed,
        "cancelled" => CourseOfferingState.Cancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };

    private static OfferingStoreException NotFound(string code, string message) =>
        new(OfferingStoreFailure.NotFound, code, message);

    private static OfferingStoreException Conflict(string code, string message) =>
        new(OfferingStoreFailure.Conflict, code, message);

    private static string Token(CourseOfferingState value) => value.ToString().ToLowerInvariant();

    private static string Token(SectionGroupState value) => value.ToString().ToLowerInvariant();

    private static string Token(ActivityType value) => value.ToString();

    private static string Token(TeachingRole value) => value.ToString();

}
