using System.Text.Json;
using StudentRegistration.Client.Features.Frontend.Models;

namespace StudentRegistration.IntegrationTests.Specs.Spec003;

public sealed class ConflictViewModelTests
{
    [Fact]
    public void Serializes_every_conflicting_group_overlap_alternative_and_resolution_link()
    {
        var model = new ConflictView(
            [
                new ConflictSubjectGroupView("AI301", "Machine Learning", "G1"),
                new ConflictSubjectGroupView("AI302", "Computer Vision", "G2")
            ],
            [
                new ConflictOverlapSlotView(
                    DayOfWeek.Monday,
                    new TimeOnly(10, 0),
                    new TimeOnly(11, 30),
                    ["AI301:G1", "AI302:G2"])
            ],
            [
                new ConflictAlternativeView(
                    "ALT-01",
                    "Move Computer Vision to G3",
                    ["AI302:G3"])
            ],
            [new ConflictResolutionLinkView("Resolve manually", "/student/schedule")]);

        using var document = JsonDocument.Parse(
            JsonSerializer.Serialize(model, JsonSerializerOptions.Web));
        var root = document.RootElement;

        Assert.Equal(2, root.GetProperty("subjectGroups").GetArrayLength());
        Assert.Single(root.GetProperty("overlapSlots").EnumerateArray());
        Assert.Equal("ALT-01", root.GetProperty("alternatives")[0].GetProperty("alternativeId").GetString());
        Assert.Equal("/student/schedule", root.GetProperty("resolutionLinks")[0].GetProperty("href").GetString());
    }

    [Fact]
    public void Rejects_an_overlap_that_does_not_end_after_it_starts()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ConflictOverlapSlotView(
                DayOfWeek.Monday,
                new TimeOnly(11, 30),
                new TimeOnly(10, 0),
                ["AI301:G1", "AI302:G2"]));

        Assert.Equal("endsAt", exception.ParamName);
    }
}
