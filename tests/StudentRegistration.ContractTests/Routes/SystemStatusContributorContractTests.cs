using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Routes;

public sealed class SystemStatusContributorContractTests
{
    private const string ContributorContractPath =
        "specs/006-domain-class-api-contracts/contracts/routes/SYS-01.md";

    [Fact]
    public void Contributor_preserves_canonical_sys01_ownership_and_design_only_pins()
    {
        using var routeManifest = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/route-manifest.json"));
        var route = routeManifest.RootElement.GetProperty("routes")
            .EnumerateArray()
            .Single(item => item.GetProperty("id").GetString() == "SYS-01");

        Assert.Equal("/status/{code}", route.GetProperty("template").GetString());
        Assert.Equal("SystemStatusPage", route.GetProperty("page").GetString());
        Assert.Equal("003", route.GetProperty("designOwner").GetString());
        Assert.Equal("003", route.GetProperty("implementationOwner").GetString());
        Assert.Equal(
            ["003", "006", "007", "008", "018"],
            route.GetProperty("owners").EnumerateArray()
                .Select(item => item.GetString()!)
                .ToArray());

        var contract = RepositoryFiles.Read(ContributorContractPath);
        RepositoryFiles.ContainsAll(
            contract,
            "**Contributor contract:** `SPEC-006/SYS-01/1.0`",
            "**Canonical Page Design Record:** `SPEC-003/SYS-01/1.0`",
            "**SPEC-003 governance commit:** `8ee8f724af3b60bbbc464532bc68de6f173247e5`",
            "**SPEC-003 design contract pin:** `frontend-design-index/1.1`",
            "**Page Design Record governance pin:** `page-design-record/1.1`",
            "**Route manifest pin:** `2.1.0`",
            "**Page/API manifest pin:** `1.1.0`",
            "**Route readiness:** `design-only`",
            "| SPEC-006 runtime error contribution | `not-pinned` |",
            "| SPEC-006 component/Razor consumption | `not-pinned` |",
            "SPEC-003 remains the sole design and implementation owner",
            "SPEC-006 does not own or edit `SystemStatusPage.razor`",
            "No Razor, component, browser, accessibility, visual, or E2E evidence is claimed");
    }

    [Fact]
    public void Contributor_maps_stable_reasons_to_safe_status_data_and_actions()
    {
        using var pageApiManifest = JsonDocument.Parse(
            RepositoryFiles.Read(".specify/page-api-manifest.json"));
        Assert.Equal(
            ["GET /api/public/context", "GET /api/health"],
            pageApiManifest.RootElement.GetProperty("pages")
                .GetProperty("SYS-01")
                .EnumerateArray()
                .Select(item => item.GetString()!)
                .ToArray());

        var contract = RepositoryFiles.Read(ContributorContractPath);
        RepositoryFiles.ContainsAll(
            contract,
            "## Safe data contract",
            "`ApiError.code`",
            "`ApiError.message`",
            "`ApiError.correlationId`",
            "| `healthy` | `success` |",
            "| `degraded`, `unhealthy`, `SERVICE_UNAVAILABLE`, `MAINTENANCE` | `service-error` |",
            "| `UNAUTHORIZED`, `FORBIDDEN` | `unauthorized` |",
            "| `SESSION_EXPIRED` | `session-expired` |",
            "| `offline` | `offline` |",
            "| Unknown or unexpected reason | `service-error` |",
            "Retry",
            "Open public gateway",
            "Open appropriate sign-in",
            "Return to authorized home",
            "Open safe support path");
    }

    [Fact]
    public void Contributor_fails_safe_for_privacy_stale_and_concurrent_changes()
    {
        var contract = RepositoryFiles.Read(ContributorContractPath);
        RepositoryFiles.ContainsAll(
            contract,
            "## Authorization and privacy",
            "stack trace",
            "SQL text",
            "connection information",
            "secret or credential",
            "raw payload",
            "protected resource identifier",
            "internal health detail",
            "topology",
            "401/403",
            "`currentVersion`",
            "## Stale and concurrent behavior",
            "The canonical `stale` page state is `not-applicable`",
            "refetches and renders the latest complete safe response",
            "never merges diagnostics from different responses",
            "never claims that an offline or failed write was queued or accepted");
    }
}
