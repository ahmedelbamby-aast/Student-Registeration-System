using StudentRegistration.Scheduling.Domain;

namespace StudentRegistration.IntegrationTests.Specs.Spec010;

public sealed class CourseOfferingModelTests
{
    [Fact]
    public void Offering_preserves_the_unique_term_course_key_and_starts_versioned()
    {
        var id = Guid.NewGuid();
        var termId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var offering = new CourseOffering(
            id,
            termId,
            courseId,
            CourseOfferingState.Draft);

        Assert.Equal(id, offering.Id);
        Assert.Equal(termId, offering.TermId);
        Assert.Equal(courseId, offering.CourseId);
        Assert.Equal(CourseOfferingState.Draft, offering.State);
        Assert.Empty(offering.Version);
        AssertPrivateSetter(nameof(CourseOffering.TermId));
        AssertPrivateSetter(nameof(CourseOffering.CourseId));
        AssertPrivateSetter(nameof(CourseOffering.Version));
    }

    [Fact]
    public void Offering_follows_the_approved_draft_published_closed_lifecycle()
    {
        var offering = Create();

        offering.Publish();
        Assert.Equal(CourseOfferingState.Published, offering.State);

        offering.Close();
        Assert.Equal(CourseOfferingState.Closed, offering.State);

        Assert.Throws<InvalidOperationException>(offering.Publish);
        Assert.Throws<InvalidOperationException>(offering.Close);
    }

    [Fact]
    public void Published_offering_can_be_cancelled_without_a_separate_open_state()
    {
        var offering = Create();

        offering.Publish();
        offering.Cancel();

        Assert.Equal(CourseOfferingState.Cancelled, offering.State);
        Assert.DoesNotContain(
            "Open",
            Enum.GetNames<CourseOfferingState>(),
            StringComparer.Ordinal);
        Assert.Throws<InvalidOperationException>(offering.Publish);
        Assert.Throws<InvalidOperationException>(offering.Cancel);
    }

    [Fact]
    public void Offering_rejects_missing_identity_key_parts_and_unknown_state()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(termId: Guid.Empty));
        Assert.Throws<ArgumentException>(() => Create(courseId: Guid.Empty));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Create(state: (CourseOfferingState)999));
    }

    private static CourseOffering Create(
        Guid? id = null,
        Guid? termId = null,
        Guid? courseId = null,
        CourseOfferingState state = CourseOfferingState.Draft) =>
        new(
            id ?? Guid.NewGuid(),
            termId ?? Guid.NewGuid(),
            courseId ?? Guid.NewGuid(),
            state);

    private static void AssertPrivateSetter(string propertyName)
    {
        var property = typeof(CourseOffering).GetProperty(propertyName);
        Assert.NotNull(property);
        Assert.False(property.SetMethod?.IsPublic ?? false);
    }
}
