namespace StudentRegistration.ContractTests.Specs.Spec014;

public sealed class Endpoint02ContractTests
{
    [Fact]
    public void Get_requires_exact_permission_and_authorizes_before_disclosure()
    {
        var get = Spec014ContractAssertions.Section(
            "### GET /api/student/terms/{termId}/registrations/by-request/{clientRequestId}",
            "## Idempotency Semantics");
        Spec014ContractAssertions.ContainsAll(
            get,
            "authenticated `Student`",
            "`Registration.SubmitOwn` permission",
            "Authorization occurs before result, owner, term, submission, or version disclosure",
            "200 durable pending or final result",
            "bounded non-durable 202",
            "404 `REQUEST_NOT_FOUND`");
    }

    [Fact]
    public void Get_uses_one_privacy_safe_not_found_shape_for_every_hidden_case()
    {
        var get = Spec014ContractAssertions.Section(
            "### GET /api/student/terms/{termId}/registrations/by-request/{clientRequestId}",
            "## Idempotency Semantics");
        Spec014ContractAssertions.ContainsAll(
            get,
            "A nonexistent key, rolled-back claim, another student's key, and a key outside the authorized term are indistinguishable",
            "disclose no current version or submission identifier");
        Spec014ContractAssertions.Excludes(
            get,
            "OWNER_MISMATCH",
            "STUDENT_NOT_FOUND",
            "CURRENT_VERSION");
    }

    [Fact]
    public void Lookup_and_replay_keep_the_exact_student_term_request_scope()
    {
        Spec014ContractAssertions.ContractContains(
            "The scope is exactly `(authenticated StudentId, route TermId, ClientRequestId)`",
            "The same UUID in a different term is allowed and independent",
            "A 202 is retry guidance, not proof of a committed Processing row",
            "Every replay returns the same reference/snapshot");
    }
}
