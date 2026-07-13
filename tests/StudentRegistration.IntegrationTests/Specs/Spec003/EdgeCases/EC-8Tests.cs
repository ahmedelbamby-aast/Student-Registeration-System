using StudentRegistration.TestSupport;

namespace StudentRegistration.IntegrationTests.Specs.Spec003.EdgeCases;

public sealed class EC_8Tests
{
    [Fact]
    public void Shared_table_contract_exposes_labelled_keyboard_overflow_and_a_narrow_alternative()
    {
        var responsiveContract = RepositoryFiles.Read(
            "specs/003-ux-storyboard-accessibility/design/responsive-layout-contract.md");
        var dataTable = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Data/DataTable.razor");
        var dataTableStyles = RepositoryFiles.Read(
            "src/StudentRegistration.Client/Components/Data/DataTable.razor.css");

        RepositoryFiles.ContainsAll(
            responsiveContract,
            "Label/value cards or chronological rows",
            "semantic region that has a label and keyboard access",
            "Data tables retain captions, header associations, sorting state, pagination",
            "the same actions in their narrow alternative");
        RepositoryFiles.ContainsAll(
            dataTable,
            "role=\"region\"",
            "aria-label=\"@ViewportAccessibleName\"",
            "tabindex=\"0\"",
            "<caption>@Caption</caption>",
            "<thead>",
            "<th scope=\"col\"",
            "data-table-alternative",
            "@NarrowAlternative");
        Assert.Contains("overflow-x: auto", dataTableStyles, StringComparison.Ordinal);
    }
}
