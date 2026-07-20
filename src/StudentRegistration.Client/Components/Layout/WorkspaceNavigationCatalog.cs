namespace StudentRegistration.Client.Components.Layout;

public sealed record WorkspaceNavigationItem(string RouteId, string Label, string Href);

/// <summary>Canonical presentation destinations for each authenticated workspace.</summary>
public static class WorkspaceNavigationCatalog
{
    private static readonly IReadOnlyList<WorkspaceNavigationItem> StudentItems =
    [
        new("STU-01", "Dashboard", "/student"),
        new("STU-09", "Roadmap", "/student/roadmap"),
        new("STU-02", "Subjects", "/student/subjects"),
        new("STU-04", "Current plan", "/student/schedule"),
        new("STU-07", "Registrations", "/student/registrations"),
        new("STU-08", "Account", "/student/account")
    ];

    private static readonly IReadOnlyList<WorkspaceNavigationItem> AdminItems =
    [
        new("ADM-01", "Dashboard", "/admin"),
        new("ADM-02", "Terms", "/admin/terms"),
        new("ADM-03", "Users", "/admin/users"),
        new("ADM-04", "Students", "/admin/students"),
        new("ADM-05", "Catalogue", "/admin/catalogue"),
        new("ADM-06", "Offerings", "/admin/offerings"),
        new("ADM-10", "Approvals", "/admin/approvals"),
        new("ADM-07", "Resources", "/admin/resources"),
        new("ADM-08", "Registrations", "/admin/registrations"),
        new("ADM-09", "Audit", "/admin/audit")
    ];

    private static readonly IReadOnlyList<WorkspaceNavigationItem> StaffItems =
    [
        new("STF-01", "Staff home", "/staff"),
        new("STF-02", "Timetable", "/staff/timetable"),
        new("STF-04", "Availability", "/staff/availability"),
        new("STF-05", "Approvals", "/staff/approvals")
    ];

    public static IReadOnlyList<WorkspaceNavigationItem> For(WorkspaceKind workspace) => workspace switch
    {
        WorkspaceKind.Student => StudentItems,
        WorkspaceKind.Admin => AdminItems,
        WorkspaceKind.Staff => StaffItems,
        _ => throw new ArgumentOutOfRangeException(nameof(workspace), workspace, null)
    };
}
