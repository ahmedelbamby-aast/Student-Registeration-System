using System.Text.Json;
using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec005;

public sealed class NFR_1EvidenceTests
{
    private const string InventoryPath = "docs/data/critical-query-inventory.md";
    private const string EvidencePath =
        "docs/release-evidence/SPEC-005-NFR-1-plans.json";

    [Fact]
    public void Inventory_covers_every_critical_query_family_and_threshold_rule()
    {
        var inventory = RepositoryFiles.Read(InventoryPath);
        RepositoryFiles.ContainsAll(
            inventory,
            "SPEC-005 Critical Query and Row-Count Inventory",
            "CQ-01",
            "CQ-02",
            "CQ-03",
            "CQ-04",
            "CQ-05",
            "CQ-06",
            "CQ-07",
            "Student discovery/detail",
            "Offering discovery/detail",
            "Plan read/write/validation",
            "Submission claim/replay",
            "Authorized roster",
            "Audit review",
            "Operational enrollment metric",
            "above 10,000 rows",
            "SPEC005-SCAN-20260719",
            "2026-08-02",
            "(OfferingId, GroupId, State, StudentId)");
    }

    [Fact]
    public void Recorded_actual_plans_meet_latency_index_and_bounded_scan_gate()
    {
        using var document = JsonDocument.Parse(RepositoryFiles.Read(EvidencePath));
        var root = document.RootElement;
        Assert.Equal("spec005-query-plans/1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("SPEC-005", root.GetProperty("ownerSpec").GetString());
        Assert.Equal("SQL Server 2022 Developer", root.GetProperty("sqlEdition").GetString());
        Assert.Equal(160, root.GetProperty("compatibilityLevel").GetInt32());
        Assert.Equal("synthetic-only", root.GetProperty("dataClassification").GetString());
        Assert.False(root.GetProperty("productionAuthorized").GetBoolean());

        var counts = root.GetProperty("rowCounts");
        Assert.Equal(25_000, counts.GetProperty("applicationUsers").GetInt32());
        Assert.Equal(25_000, counts.GetProperty("students").GetInt32());
        Assert.Equal(25_000, counts.GetProperty("registrationSubmissions").GetInt32());
        Assert.Equal(75_000, counts.GetProperty("enrollments").GetInt32());
        Assert.Equal(100_000, counts.GetProperty("auditEvents").GetInt32());

        var plans = root.GetProperty("plans").EnumerateArray().ToArray();
        Assert.Equal(7, plans.Length);
        Assert.Equal(
            ["CQ-01", "CQ-02", "CQ-03", "CQ-04", "CQ-05", "CQ-06", "CQ-07"],
            plans.Select(plan => plan.GetProperty("queryId").GetString()!).ToArray());
        foreach (var plan in plans)
        {
            Assert.True(plan.GetProperty("actualPlanCaptured").GetBoolean());
            Assert.Matches("^[A-F0-9]{64}$", plan.GetProperty("actualPlanSha256").GetString());
            Assert.InRange(plan.GetProperty("p95Milliseconds").GetDouble(), 0, 300);
            Assert.NotEmpty(plan.GetProperty("physicalOperators").EnumerateArray());
            Assert.NotEmpty(plan.GetProperty("indexesUsed").EnumerateArray());

            var scans = plan.GetProperty("scanOperators").EnumerateArray().ToArray();
            var exception = plan.GetProperty("exceptionId").ValueKind is JsonValueKind.Null
                ? null
                : plan.GetProperty("exceptionId").GetString();
            if (scans.Length == 0)
            {
                Assert.Null(exception);
                continue;
            }

            Assert.Contains(plan.GetProperty("queryId").GetString(), new[] { "CQ-05", "CQ-07" });
            Assert.Equal("SPEC005-SCAN-20260719", exception);
            Assert.Equal("Ahmed ELbamby", plan.GetProperty("exceptionOwner").GetString());
            Assert.Equal("2026-08-02", plan.GetProperty("exceptionExpiresOn").GetString());
        }

        Assert.Equal("pass", root.GetProperty("status").GetString());
    }
}
