using StudentRegistration.TestSupport;

namespace StudentRegistration.QualityTests.Specs.Spec003;

public sealed class ScopeEvidenceTests
{
    [Fact]
    public void Frontend_scope_exclusions_and_gate_order_are_recorded()
    {
        var evidence = string.Join(
            " ",
            RepositoryFiles.Read("docs/release-evidence/SPEC-003-scope-review.md")
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        RepositoryFiles.ContainsAll(
            evidence,
            "**Result: PASS.**",
            "official AASTMT logo",
            "Arabic translation and RTL delivery",
            "Native mobile applications",
            "Drag-and-drop",
            "Client-side authorization, eligibility, capacity, or commit decisions",
            "025479c100b83e726c777b2311015470481a7515",
            "4b37c176228475850a84b8351aba1fc91f3c2102");
        Assert.True(RepositoryFiles.Exists(
            "src/StudentRegistration.Client/wwwroot/brand/aastmt-logo.png"));
    }

    [Fact]
    public void Delivered_client_has_no_rtl_or_drag_only_control_path()
    {
        var clientRoot = RepositoryFiles.PathTo("src/StudentRegistration.Client");
        var source = string.Join(
            Environment.NewLine,
            Directory.GetFiles(clientRoot, "*", SearchOption.AllDirectories)
                .Where(path => path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                .Select(File.ReadAllText));

        Assert.DoesNotContain("dir=\"rtl\"", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("draggable=\"true\"", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("@ondrop", source, StringComparison.OrdinalIgnoreCase);
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "specs/003-ux-storyboard-accessibility/design/pages/ADM-07.md"),
            "keyboard text entry",
            "requiring drag-and-drop");
    }

    [Fact]
    public void Client_authority_contract_keeps_server_decisions_canonical()
    {
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "specs/003-ux-storyboard-accessibility/design/states/reason-map.md"),
            "server",
            "capacity",
            "cached policy",
            "local role state never changes this rule");
        RepositoryFiles.ContainsAll(
            RepositoryFiles.Read(
                "tests/StudentRegistration.Client.UnitTests/UX/UiStateMapperTests.cs"),
            "server",
            "reason",
            "success");
    }
}
