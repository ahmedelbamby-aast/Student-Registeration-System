using StudentRegistration.Scheduling.Application;

namespace StudentRegistration.AcceptanceTests.Specs.Spec010;

public sealed class AC_1Tests
{
    [Fact]
    public async Task Complete_lecturer_and_ta_bundle_publishes_with_student_detail()
    {
        var offeringStore = new OfferingStoreFake();
        var offeringService = new OfferingService(offeringStore);
        var validation = await offeringService.ValidateForPublicationAsync(
            offeringStore.Snapshot.Id);

        var publicationStore = new PublicationStoreFake();
        var transaction = new PublicationTransactionFake(publicationStore);
        var publication = await new OfferingPublicationService(
                new OfferingPublicationValidator(),
                transaction)
            .PublishAsync(
                Spec010Scenario.PublishCommand(publicationStore.Snapshot));
        var detail = await offeringService.GetStudentDetailAsync(
            offeringStore.Snapshot.Id);

        Assert.True(validation.Valid);
        Assert.Equal(
            OfferingPublicationOutcome.Published,
            publication.Outcome);
        Assert.Equal("published", transaction.OfferingState);
        var group = Assert.Single(detail!.Groups);
        Assert.Equal(30, group.Capacity);
        Assert.Contains(group.Meetings, meeting =>
            meeting.ActivityType == "Lecture"
            && meeting.Staff.Any(staff => staff.Role == "Lecturer"));
        Assert.Contains(group.Meetings, meeting =>
            meeting.ActivityType == "Tutorial"
            && meeting.Staff.Any(
                staff => staff.Role == "TeachingAssistant"));
        Assert.All(group.Meetings, meeting =>
        {
            Assert.False(string.IsNullOrWhiteSpace(meeting.RoomCode));
            Assert.False(string.IsNullOrWhiteSpace(meeting.Location));
            Assert.True(meeting.EndLocal > meeting.StartLocal);
        });
    }
}
