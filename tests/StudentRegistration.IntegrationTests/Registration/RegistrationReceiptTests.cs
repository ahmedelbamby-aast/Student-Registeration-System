using System.Text.Json;
using StudentRegistration.Registration.Application;
using StudentRegistration.Registration.Application.Ports;

namespace StudentRegistration.IntegrationTests.Registration;

public sealed class RegistrationReceiptTests
{
    [Fact]
    public void Accepted_projection_contains_the_canonical_snapshot_and_reference()
    {
        var detail = new RegistrationReceiptService().ProjectDetail(AcceptedRecord());
        Assert.Equal("accepted", detail.Status);
        Assert.Equal("REG-ABC123", detail.Receipt!.Reference);
        Assert.Equal("POLICY-1", detail.Receipt.PolicyVersion);
        Assert.Single(detail.Receipt.Groups);
        Assert.Equal("Original room", detail.Receipt.Groups[0].Meetings[0].Location);
        Assert.Equal("Lecturer", detail.Receipt.Groups[0].Meetings[0].Staff[0].Role);
        Assert.Null(detail.Rejection);
    }

    [Fact]
    public void Rejected_projection_has_no_receipt_and_says_no_partial_registration()
    {
        var record = AcceptedRecord() with
        {
            State = "rejected",
            ResultCode = "GROUP_FULL",
            Reference = null,
            ReceiptSnapshotJson = null
        };
        var detail = new RegistrationReceiptService().ProjectDetail(record);
        Assert.Null(detail.Receipt);
        Assert.True(detail.Rejection!.NoPartialRegistration);
        Assert.Contains("No subjects were partially registered", detail.Rejection.SafeMessage);
    }

    [Fact]
    public void History_uses_snapshot_term_and_stable_submission_values()
    {
        var row = new RegistrationReceiptService().ProjectHistory(AcceptedRecord());
        Assert.Equal("Original term", row.Term.DisplayName);
        Assert.Equal(3m, row.TotalCredits);
        Assert.Equal(1, row.GroupCount);
        Assert.Equal("archived", row.TermState);
    }

    [Fact]
    public void Malformed_or_incomplete_accepted_snapshot_fails_closed()
    {
        var record = AcceptedRecord() with { ReceiptSnapshotJson = "{}" };
        Assert.Throws<RegistrationRecordProjectionException>(
            () => new RegistrationReceiptService().ProjectDetail(record));
    }

    [Fact]
    public void Receipt_and_decision_policy_versions_must_match()
    {
        var record = AcceptedRecord() with
        {
            DecisionSnapshotJson = "{\"policyVersion\":\"POLICY-2\"}"
        };

        Assert.Throws<RegistrationRecordProjectionException>(
            () => new RegistrationReceiptService().ProjectDetail(record));
    }

    public static RegistrationSubmissionRecord AcceptedRecord()
    {
        var submitted = new DateTime(2026, 7, 17, 8, 0, 0, DateTimeKind.Utc);
        var termId = Guid.NewGuid();
        var snapshot = new
        {
            term = new { id = termId, code = "2026-FALL", displayName = "Original term", timeZoneId = "Africa/Cairo" },
            groups = new[]
            {
                new
                {
                    offeringId = Guid.NewGuid(), courseCode = "CS101", subjectTitle = "Foundations",
                    groupId = Guid.NewGuid(), groupCode = "L1", credits = 3m,
                    meetings = new[]
                    {
                        new
                        {
                            meetingId = Guid.NewGuid(), activityType = "Lecture", dayOfWeek = 1,
                            startLocal = "09:00", endLocal = "10:30", roomCode = "R-1",
                            location = "Original room",
                            staff = new[] { new { role = "Lecturer", displayName = "Original lecturer" } }
                        }
                    }
                }
            },
            totalCredits = 3m, policySetId = Guid.NewGuid(), policyVersion = "POLICY-1", submittedAtUtc = submitted
        };
        return new(
            Guid.NewGuid(), Guid.NewGuid(), termId, "accepted", "REGISTERED", "REG-ABC123",
            JsonSerializer.Serialize(snapshot), "{\"policyVersion\":\"POLICY-1\"}", submitted,
            submitted.AddSeconds(1),
            new(termId, "LIVE", "Live mutable term", "Africa/Cairo"));
    }
}
