using System.Text.Json;
using StudentRegistration.Contracts;
using StudentRegistration.TestSupport;

namespace StudentRegistration.ContractTests.Shared;

public sealed class CommandMetadataTests
{
    [Fact]
    public void Expected_row_version_is_a_required_scalar_request_body_value()
    {
        var expectedRowVersion = new ExpectedRowVersion("AQIDBA==");
        var request = new VersionedUpdateRequest(expectedRowVersion);

        var json = JsonSerializer.Serialize(request, JsonSerializerOptions.Web);
        using var document = JsonDocument.Parse(json);
        var roundTrip = JsonSerializer.Deserialize<VersionedUpdateRequest>(
            json,
            JsonSerializerOptions.Web);

        Assert.Equal("AQIDBA==", expectedRowVersion.Value);
        Assert.Equal("AQIDBA==", expectedRowVersion.ToString());
        Assert.Equal("AQIDBA==", document.RootElement.GetProperty("expectedRowVersion").GetString());
        Assert.Equal(expectedRowVersion, roundTrip!.ExpectedRowVersion);
        Assert.Throws<ArgumentException>(() => new ExpectedRowVersion(""));
        Assert.Throws<ArgumentException>(() => new ExpectedRowVersion("   "));
        Assert.Throws<ArgumentException>(() => new ExpectedRowVersion(null!));
    }

    [Fact]
    public void Concurrency_protocol_is_request_body_only_and_has_one_stale_response()
    {
        var api = ReadApiContract();

        RepositoryFiles.ContainsAll(
            api,
            "Versioned update/delete request DTOs contain required `expectedRowVersion`.",
            "409 `STALE_VERSION`",
            "unauthorized requests return 403 without resource or version disclosure",
            "`If-Match` and 412 are outside the MVP protocol");
    }

    [Fact]
    public void Idempotency_key_is_a_required_opaque_scalar_owned_by_the_feature_request()
    {
        var idempotencyKey = new IdempotencyKey("registration-request-001");
        var request = new RetryableCommandRequest(idempotencyKey);

        var json = JsonSerializer.Serialize(request, JsonSerializerOptions.Web);
        using var document = JsonDocument.Parse(json);
        var roundTrip = JsonSerializer.Deserialize<RetryableCommandRequest>(
            json,
            JsonSerializerOptions.Web);

        Assert.Equal("registration-request-001", idempotencyKey.Value);
        Assert.Equal("registration-request-001", idempotencyKey.ToString());
        Assert.Equal(
            "registration-request-001",
            document.RootElement.GetProperty("clientRequestId").GetString());
        Assert.Equal(idempotencyKey, roundTrip!.ClientRequestId);
        Assert.Throws<ArgumentException>(() => new IdempotencyKey(""));
        Assert.Throws<ArgumentException>(() => new IdempotencyKey("\t"));
        Assert.Throws<ArgumentException>(() => new IdempotencyKey(null!));
    }

    [Fact]
    public void Idempotency_protocol_defines_owner_scope_payload_claim_replay_and_mismatch()
    {
        var api = ReadApiContract();

        RepositoryFiles.ContainsAll(
            api,
            "authenticated owner and uniqueness scope",
            "server-canonical payload",
            "atomic first claim",
            "same key, owner, scope, and canonical payload",
            "processing response without executing the command again",
            "replays the stored result",
            "409 `IDEMPOTENCY_KEY_REUSED`",
            "different canonical payload is never executed");
    }

    [Fact]
    public void Final_result_and_cancellation_policy_are_explicit_at_the_commit_boundary()
    {
        var api = ReadApiContract();

        RepositoryFiles.ContainsAll(
            api,
            "which deterministic business rejections are stored as final replayable results",
            "Transient infrastructure failures are not final replayable results",
            "Cancellation before commit rolls back the transaction, claim, and every partial effect",
            "Cancellation or response loss after commit does not undo the committed effect",
            "same-key/same-payload retry replays the stored result");
    }

    [Fact]
    public void Shared_metadata_does_not_introduce_a_generic_command_or_result_envelope()
    {
        var exportedNames = typeof(ExpectedRowVersion).Assembly.ExportedTypes
            .Select(type => type.Name)
            .ToArray();

        Assert.Contains(nameof(ExpectedRowVersion), exportedNames);
        Assert.Contains(nameof(IdempotencyKey), exportedNames);
        Assert.DoesNotContain(
            exportedNames,
            name => name.Contains("CommandResult", StringComparison.Ordinal));
        Assert.DoesNotContain(
            exportedNames,
            name => name.Contains("CommandEnvelope", StringComparison.Ordinal));
        Assert.DoesNotContain(
            exportedNames,
            name => name.Contains("DomainValue", StringComparison.Ordinal));
    }

    private sealed record VersionedUpdateRequest(ExpectedRowVersion ExpectedRowVersion);

    private sealed record RetryableCommandRequest(IdempotencyKey ClientRequestId);

    private static string ReadApiContract() =>
        string.Join(
            " ",
            RepositoryFiles.Read("specs/006-domain-class-api-contracts/contracts/api.md")
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
