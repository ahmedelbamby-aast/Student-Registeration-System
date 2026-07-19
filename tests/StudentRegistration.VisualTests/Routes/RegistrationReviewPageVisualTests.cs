using StudentRegistration.VisualTests.Infrastructure;
namespace StudentRegistration.VisualTests.Routes;
public sealed class RegistrationReviewPageVisualContractTests { [Fact] public void Stu_05_visual_contract_is_frozen() => Spec003RouteVisualAssertions.AssertFrozenContract("STU-05", "T171", "RegistrationReviewPage"); }
[Collection(VisualRegressionCollection.CollectionName)] public sealed class RegistrationReviewPageVisualTests(VisualRegressionFixture fixture) { [Theory] [MemberData(nameof(Spec003RouteVisualAssertions.BrowserWidths), MemberType=typeof(Spec003RouteVisualAssertions))] public Task Stu_05_matches_approved_baseline(string browser,int width) => Spec003RouteVisualAssertions.AssertApprovedBaselineAsync(fixture,"STU-05","/student/review",browser,width); }
