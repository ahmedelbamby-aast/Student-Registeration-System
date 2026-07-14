using System.Security.Cryptography;
using StudentRegistration.IdentityAccess.Application.Ports;

namespace StudentRegistration.Api.Composition;

internal sealed class DevelopmentIdentityAbuseKeyProvider : IIdentityAbuseKeyProvider
{
    private const int KeyLength = 32;
    private readonly byte[] _key;

    public DevelopmentIdentityAbuseKeyProvider(IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "The local abuse-subject key is available only in Development.");
        }

        var root = Path.GetFullPath(environment.ContentRootPath);
        var directory = ContainedPath(root, ".local", "security");
        Directory.CreateDirectory(directory);
        var path = ContainedPath(directory, "identity-abuse-hmac.key");
        _key = LoadOrCreate(path);
    }

    public ReadOnlyMemory<byte> GetKey() => _key;

    private static byte[] LoadOrCreate(string path)
    {
        if (!File.Exists(path))
        {
            var generated = RandomNumberGenerator.GetBytes(KeyLength);
            try
            {
                using var stream = new FileStream(
                    path,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: KeyLength,
                    FileOptions.WriteThrough);
                if (!OperatingSystem.IsWindows())
                {
                    File.SetUnixFileMode(
                        path,
                        UnixFileMode.UserRead | UnixFileMode.UserWrite);
                }

                stream.Write(generated);
                stream.Flush(flushToDisk: true);
                return generated;
            }
            catch (IOException) when (File.Exists(path))
            {
                CryptographicOperations.ZeroMemory(generated);
            }
        }

        var key = File.ReadAllBytes(path);
        if (key.Length != KeyLength)
        {
            throw new InvalidOperationException(
                "IDENTITY_ABUSE_KEY_INVALID: The Development key must contain exactly 256 bits.");
        }

        return key;
    }

    private static string ContainedPath(string root, params string[] segments)
    {
        var path = Path.GetFullPath(Path.Combine([root, .. segments]));
        var relative = Path.GetRelativePath(root, path);
        if (Path.IsPathRooted(relative)
            || relative.Equals("..", StringComparison.Ordinal)
            || relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            || relative.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The Development abuse-subject key must stay in the local repository boundary.");
        }

        return path;
    }
}

internal sealed class TestingIdentityAbuseKeyProvider : IIdentityAbuseKeyProvider
{
    private static readonly byte[] SharedProcessKey = RandomNumberGenerator.GetBytes(32);

    public TestingIdentityAbuseKeyProvider(IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        if (!environment.IsEnvironment("Testing"))
        {
            throw new InvalidOperationException(
                "The in-memory abuse-subject key is available only in Testing.");
        }
    }

    public ReadOnlyMemory<byte> GetKey() => SharedProcessKey;
}

internal sealed class ConfiguredIdentityAbuseKeyProvider(
    IConfiguration configuration,
    IHostEnvironment environment) : IIdentityAbuseKeyProvider
{
    private const string ConfigurationKey = "Identity:Security:AbuseSubjectHmacKey";
    private readonly Lazy<byte[]> _key = new(() => Load(configuration, environment));

    public ReadOnlyMemory<byte> GetKey() => _key.Value;

    private static byte[] Load(IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);
        if (!environment.IsProduction())
        {
            throw new InvalidOperationException(
                "The externally configured abuse-subject key is reserved for Production.");
        }

        var encoded = configuration[ConfigurationKey];
        try
        {
            var key = Convert.FromBase64String(encoded ?? string.Empty);
            if (key.Length is < 32 or > 64)
            {
                throw new FormatException();
            }

            return key;
        }
        catch (FormatException)
        {
            throw new InvalidOperationException(
                "IDENTITY_ABUSE_KEY_REQUIRED: Production requires a 256-512 bit external HMAC key.");
        }
    }
}
