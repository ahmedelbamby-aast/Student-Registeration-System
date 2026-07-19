using StudentRegistration.VisualTests.Infrastructure;
namespace StudentRegistration.VisualTests.Routes;
public sealed class CatalogueAdministrationPageVisualContractTests { [Fact] public void Adm_05_visual_contract_is_frozen() => Spec003RouteVisualAssertions.AssertFrozenContract("ADM-05", "T211", "CatalogueAdministrationPage"); }
[Collection(VisualRegressionCollection.CollectionName)] public sealed class CatalogueAdministrationPageVisualTests(VisualRegressionFixture fixture) { [Theory] [MemberData(nameof(Spec003RouteVisualAssertions.BrowserWidths), MemberType=typeof(Spec003RouteVisualAssertions))] public Task Adm_05_matches_approved_baseline(string browser,int width) => Spec003RouteVisualAssertions.AssertApprovedBaselineAsync(fixture,"ADM-05","/admin/catalogue",browser,width); }
