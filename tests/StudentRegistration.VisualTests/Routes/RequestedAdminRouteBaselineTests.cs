using StudentRegistration.VisualTests.Infrastructure;

namespace StudentRegistration.VisualTests.Routes;

[Collection(VisualRegressionCollection.CollectionName)]
public sealed class RequestedAdminRouteBaselineTests(VisualRegressionFixture fixture)
{
    public static TheoryData<string, string, string, int> AdminRoutes()
    {
        var routes = new[]
        {
            (Id: "ADM-01", Path: "/admin"),
            (Id: "ADM-08", Path: "/admin/registrations"),
            (Id: "ADM-09", Path: "/admin/audit")
        };
        var browsers = new[] { "chrome", "edge", "firefox", "webkit" };
        var widths = new[] { 375, 768, 1280, 1920 };
        var data = new TheoryData<string, string, string, int>();
        foreach (var route in routes)
            foreach (var browser in browsers)
                foreach (var width in widths)
                {
                    data.Add(route.Id, route.Path, browser, width);
                }
        return data;
    }

    [Theory]
    [MemberData(nameof(AdminRoutes))]
    public Task Requested_admin_route_matches_approved_baseline(
        string routeId,
        string route,
        string browser,
        int width) =>
        Spec003RouteVisualAssertions.AssertApprovedBaselineAsync(
            fixture, routeId, route, browser, width);
}
