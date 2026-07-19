using Microsoft.AspNetCore.DataProtection;

namespace StudentRegistration.IntegrationTests.Specs.Spec018.EdgeCases;

public sealed class EC_2Tests
{
    [Fact]
    public void Surviving_replica_continues_authenticated_requests_after_one_instance_fails()
    {
        var keyDirectory = Path.Combine(
            Path.GetTempPath(),
            $"srs-spec018-ec2-{Guid.NewGuid():N}");
        Directory.CreateDirectory(keyDirectory);

        try
        {
            const string applicationName = "AASTMT.StudentRegistration.Spec018.EC2";
            var sharedDatabase = new SharedAccountDatabase();
            var userId = Guid.NewGuid();
            sharedDatabase.Add(userId, "synthetic-student");

            var replicaOne = Replica.Create(
                "replica-1",
                keyDirectory,
                applicationName,
                sharedDatabase);
            var replicaTwo = Replica.Create(
                "replica-2",
                keyDirectory,
                applicationName,
                sharedDatabase);
            var loadBalancer = new FaultAwareLoadBalancer(replicaOne, replicaTwo);
            var protectedSession = replicaOne.IssueSession(userId);

            replicaOne.Fail();
            var response = loadBalancer.RouteAuthenticatedRequest(protectedSession);

            Assert.Equal("replica-2", response.ReplicaId);
            Assert.Equal(userId, response.UserId);
            Assert.Equal("synthetic-student", response.UserName);
            Assert.Equal(["replica-2"], loadBalancer.ActiveReplicaIds);
            Assert.Equal(1, loadBalancer.RemovedReplicaCount);
        }
        finally
        {
            Directory.Delete(keyDirectory, recursive: true);
        }
    }

    private sealed class FaultAwareLoadBalancer(params Replica[] replicas)
    {
        private readonly List<Replica> _replicas = [.. replicas];

        public int RemovedReplicaCount { get; private set; }

        public IReadOnlyList<string> ActiveReplicaIds =>
            _replicas.Select(replica => replica.Id).ToArray();

        public AuthenticatedResponse RouteAuthenticatedRequest(string protectedSession)
        {
            for (var index = 0; index < _replicas.Count;)
            {
                try
                {
                    return _replicas[index].Handle(protectedSession);
                }
                catch (ReplicaUnavailableException)
                {
                    _replicas.RemoveAt(index);
                    RemovedReplicaCount++;
                }
            }

            throw new InvalidOperationException("No healthy replica is available.");
        }
    }

    private sealed class Replica(
        string id,
        IDataProtector sessionProtector,
        SharedAccountDatabase database)
    {
        private bool _failed;

        public string Id { get; } = id;

        public static Replica Create(
            string id,
            string keyDirectory,
            string applicationName,
            SharedAccountDatabase database)
        {
            var provider = DataProtectionProvider.Create(
                new DirectoryInfo(keyDirectory),
                builder => builder.SetApplicationName(applicationName));
            return new Replica(
                id,
                provider.CreateProtector("spec018.ec2.auth-session.v1"),
                database);
        }

        public string IssueSession(Guid userId) =>
            sessionProtector.Protect(userId.ToString("D"));

        public void Fail() => _failed = true;

        public AuthenticatedResponse Handle(string protectedSession)
        {
            if (_failed)
            {
                throw new ReplicaUnavailableException();
            }

            var userId = Guid.Parse(sessionProtector.Unprotect(protectedSession));
            return new AuthenticatedResponse(Id, userId, database.Get(userId));
        }
    }

    private sealed class SharedAccountDatabase
    {
        private readonly Dictionary<Guid, string> _accounts = [];

        public void Add(Guid userId, string userName) =>
            _accounts.Add(userId, userName);

        public string Get(Guid userId) =>
            _accounts.TryGetValue(userId, out var userName)
                ? userName
                : throw new InvalidOperationException("Shared account was not found.");
    }

    private sealed record AuthenticatedResponse(
        string ReplicaId,
        Guid UserId,
        string UserName);

    private sealed class ReplicaUnavailableException : Exception;
}
