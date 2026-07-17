using System.Text.Json;
using StudentRegistration.Registration.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec014;

public sealed class EnrollmentModelTests
{
    [Fact]
    public void Enrollment_is_the_registration_owned_student_offering_group_record()
    {
        var id = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var offeringId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var registeredAtUtc = new DateTime(2026, 7, 17, 9, 0, 0, DateTimeKind.Utc);

        var enrollment = new Enrollment(
            id,
            studentId,
            offeringId,
            groupId,
            submissionId,
            EnrollmentState.Active,
            registeredAtUtc);

        Assert.Equal(typeof(RegistrationPlan).Assembly, typeof(Enrollment).Assembly);
        Assert.Equal("StudentRegistration.Registration.Domain", typeof(Enrollment).Namespace);
        Assert.Equal(id, enrollment.Id);
        Assert.Equal(studentId, enrollment.StudentId);
        Assert.Equal(offeringId, enrollment.OfferingId);
        Assert.Equal(groupId, enrollment.GroupId);
        Assert.Equal(submissionId, enrollment.SubmissionId);
        Assert.Equal(EnrollmentState.Active, enrollment.State);
        Assert.Equal(registeredAtUtc, enrollment.RegisteredAtUtc);
        Assert.Empty(enrollment.Version);
    }

    [Theory]
    [InlineData(nameof(Enrollment.Id))]
    [InlineData(nameof(Enrollment.StudentId))]
    [InlineData(nameof(Enrollment.OfferingId))]
    [InlineData(nameof(Enrollment.GroupId))]
    [InlineData(nameof(Enrollment.SubmissionId))]
    [InlineData(nameof(Enrollment.State))]
    [InlineData(nameof(Enrollment.RegisteredAtUtc))]
    [InlineData(nameof(Enrollment.Version))]
    public void Enrollment_schema_fields_are_not_publicly_mutable(string propertyName)
    {
        var property = typeof(Enrollment).GetProperty(propertyName);

        Assert.NotNull(property);
        Assert.False(property.SetMethod?.IsPublic ?? false);
    }

    [Fact]
    public void Enrollment_rejects_invalid_identity_state_or_registration_time()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(studentId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(offeringId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(groupId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(submissionId: Guid.Empty));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(state: (EnrollmentState)999));
        Assert.Throws<ArgumentException>(() => Create(
            registeredAtUtc: DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Local)));
    }

    [Fact]
    public void Enrollment_json_contract_round_trips_the_canonical_schema()
    {
        var enrollment = Create();

        var json = JsonSerializer.Serialize(enrollment, JsonSerializerOptions.Web);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(
            [
                "groupId", "id", "offeringId", "registeredAtUtc", "state",
                "studentId", "submissionId", "version"
            ],
            document.RootElement.EnumerateObject()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));

        var roundTrip = JsonSerializer.Deserialize<Enrollment>(
            json,
            JsonSerializerOptions.Web);
        Assert.NotNull(roundTrip);
        Assert.Equal(enrollment.Id, roundTrip.Id);
        Assert.Equal(enrollment.StudentId, roundTrip.StudentId);
        Assert.Equal(enrollment.OfferingId, roundTrip.OfferingId);
        Assert.Equal(enrollment.GroupId, roundTrip.GroupId);
        Assert.Equal(enrollment.SubmissionId, roundTrip.SubmissionId);
        Assert.Equal(enrollment.State, roundTrip.State);
        Assert.Equal(enrollment.RegisteredAtUtc, roundTrip.RegisteredAtUtc);
    }

    private static Enrollment Create(
        Guid? id = null,
        Guid? studentId = null,
        Guid? offeringId = null,
        Guid? groupId = null,
        Guid? submissionId = null,
        EnrollmentState state = EnrollmentState.Active,
        DateTime? registeredAtUtc = null) =>
        new(
            id ?? Guid.NewGuid(),
            studentId ?? Guid.NewGuid(),
            offeringId ?? Guid.NewGuid(),
            groupId ?? Guid.NewGuid(),
            submissionId ?? Guid.NewGuid(),
            state,
            registeredAtUtc ?? new DateTime(2026, 7, 17, 9, 0, 0, DateTimeKind.Utc));
}
