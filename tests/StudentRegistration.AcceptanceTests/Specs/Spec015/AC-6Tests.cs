using StudentRegistration.TestSupport;

namespace StudentRegistration.AcceptanceTests.Specs.Spec015;
public sealed class AC_6Tests
{
    [Fact] public void Quality_gate_has_named_follow_on_evidence_tasks()
    {
        var tasks = RepositoryFiles.Read("specs/015-student-registration-records/tasks.md");
        Assert.Contains("T055 [NFR-1]", tasks);
        Assert.Contains("T057 [NFR-3]", tasks);
        Assert.Contains("T058 [NFR-4]", tasks);
        Assert.Contains("300 ms p95", tasks);
    }
}
