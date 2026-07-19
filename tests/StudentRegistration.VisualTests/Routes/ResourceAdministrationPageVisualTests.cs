using StudentRegistration.VisualTests.Infrastructure;
namespace StudentRegistration.VisualTests.Routes;
public sealed class ResourceAdministrationPageVisualContractTests { [Fact] public void Adm_07_visual_contract_is_frozen() => Spec003RouteVisualAssertions.AssertFrozenContract("ADM-07", "T221", "ResourceAdministrationPage"); }
[Collection(VisualRegressionCollection.CollectionName)] public sealed class ResourceAdministrationPageVisualTests(VisualRegressionFixture fixture) { [Theory] [MemberData(nameof(Spec003RouteVisualAssertions.BrowserWidths), MemberType=typeof(Spec003RouteVisualAssertions))] public Task Adm_07_matches_approved_baseline(string browser,int width) => Spec003RouteVisualAssertions.AssertApprovedBaselineAsync(fixture,"ADM-07","/admin/resources",browser,width); }
