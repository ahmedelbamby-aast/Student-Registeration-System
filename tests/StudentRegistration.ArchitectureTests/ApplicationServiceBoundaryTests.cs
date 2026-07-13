using StudentRegistration.TestSupport;

namespace StudentRegistration.ArchitectureTests;

public sealed class ApplicationServiceBoundaryTests
{
    private const string BoundaryPath =
        "docs/architecture/application-service-boundary.md";

    [Fact]
    public void Focused_use_case_boundary_is_explicit_and_keeps_API_composition_only()
    {
        var boundary = RepositoryFiles.Read(BoundaryPath);

        RepositoryFiles.ContainsAll(
            boundary,
            "Endpoint responsibilities",
            "Application-service responsibilities",
            "Domain responsibilities",
            "StudentRegistration.Api",
            "composition only",
            "TimeProvider",
            "CancellationToken",
            "authorization",
            "transaction");
    }

    [Fact]
    public void Boundary_rejects_speculative_frameworks_and_cross_module_decisions()
    {
        var boundary = RepositoryFiles.Read(BoundaryPath);

        RepositoryFiles.ContainsAll(
            boundary,
            "No generic Application project",
            "No mediator library",
            "No generic repository",
            "module-owned use case",
            "never makes business decisions");

        Assert.False(Directory.Exists(RepositoryFiles.PathTo(
            "src/StudentRegistration.Application")));
    }
}
