using Microsoft.Data.SqlClient;
using StudentRegistration.LoadTests.Infrastructure;

namespace StudentRegistration.LoadTests.Specs.Spec018;

public sealed class SharedDataProtectionKeyTests
{
    [Fact]
    [Trait("Dependency", "Docker")]
    public async Task Two_replicas_share_certificate_protected_sql_keys()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        await using var fixture = new Spec008TwoReplicaSharedSqlFixture(studentCount: 1);

        await fixture.InitializeAsync(timeout.Token);

        Assert.True(fixture.IsReady);
        Assert.True(fixture.CrossReplicaTicketVerified);
        Assert.Equal(2, fixture.ReplicaClients.Count);
        Assert.NotSame(fixture.ReplicaClients[0], fixture.ReplicaClients[1]);

        await using var connection = new SqlConnection(fixture.ConnectionString);
        await connection.OpenAsync(timeout.Token);
        await using var command = new SqlCommand(
            "SELECT [Xml] FROM [DataProtectionKeys]",
            connection);
        await using var reader = await command.ExecuteReaderAsync(timeout.Token);
        var protectedKeys = new List<string>();
        while (await reader.ReadAsync(timeout.Token))
        {
            protectedKeys.Add(reader.GetString(0));
        }

        Assert.NotEmpty(protectedKeys);
        Assert.All(protectedKeys, keyXml =>
        {
            Assert.Contains("encryptedSecret", keyXml, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("EncryptedData", keyXml, StringComparison.OrdinalIgnoreCase);
        });
    }
}
