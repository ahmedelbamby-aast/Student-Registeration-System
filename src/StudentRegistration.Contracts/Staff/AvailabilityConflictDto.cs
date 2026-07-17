namespace StudentRegistration.Contracts.Staff;

public sealed record AvailabilityConflictDto(
    ApiError Error,
    StaffTermAvailabilityDto CurrentAvailability,
    DateTime ServerTimeUtc,
    DateTime DeadlineUtc);
