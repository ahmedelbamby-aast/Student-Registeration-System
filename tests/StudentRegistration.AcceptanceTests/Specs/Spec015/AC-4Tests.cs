using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec015;
public sealed class AC_4Tests
{
    [Fact] public void Historical_detail_uses_stored_snapshot_not_live_schedule()
    {
        var service = RepositoryFiles.Read("src/StudentRegistration.Registration/Application/RegistrationReceiptService.cs");
        Assert.Contains("ReceiptSnapshotJson", service);
        Assert.DoesNotContain("MeetingSlot", service);
        Assert.DoesNotContain("SectionGroup", service);
    }
}
