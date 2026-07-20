using System.ComponentModel;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using StudentRegistration.TestSupport;
using Xunit.Sdk;

namespace StudentRegistration.E2ETests.Infrastructure;

[CollectionDefinition(CollectionName)]
public sealed class Spec003PublishedBrowserCollection :
    ICollectionFixture<Spec003PublishedBrowserFixture>
{
    public const string CollectionName = "SPEC-003 published Release browser host";
}

/// <summary>
/// Serves the output of <c>dotnet publish -c Release</c> without starting the
/// API's database, authentication, or background-worker runtime. The host
/// selects the publish-generated Brotli/gzip sidecars exactly as a production
/// static-file edge would, while route APIs remain deterministic Playwright
/// fixtures owned by the performance test.
/// </summary>
public sealed class Spec003PublishedBrowserFixture : IAsyncLifetime
{
    private static readonly FileExtensionContentTypeProvider ContentTypes = new();
    private readonly string _publishRoot = RepositoryFiles.PathTo(
        "src/StudentRegistration.Client/bin/Release/net10.0/publish/wwwroot");
    private WebApplication? _host;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private string? _runtimeUnavailable;
    private string? _hostFailure;

    public Uri BaseAddress { get; private set; } = new("http://127.0.0.1/");

    public string BuildConfiguration => "Release";

    public string BrowserTarget => "Google Chrome Stable";

    public async Task InitializeAsync()
    {
        try
        {
            ValidatePublishedRelease();
            BaseAddress = new Uri($"http://127.0.0.1:{GetFreePort()}/");
            await StartHostAsync();
            await WaitForHostAsync();
        }
        catch (Win32Exception exception)
        {
            _runtimeUnavailable = $"The dotnet runtime is unavailable: {exception.Message}";
            return;
        }
        catch (Exception exception)
        {
            _hostFailure = $"The published Release host did not start: {exception.Message}";
            return;
        }

        try
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Channel = "chrome",
                    Headless = true
                });
        }
        catch (PlaywrightException exception)
        {
            _runtimeUnavailable =
                $"Google Chrome Stable is unavailable: {exception.Message}";
        }
    }

    public async Task<IBrowserContext> OpenContextAsync(
        int width = 1280,
        int height = 900,
        float deviceScaleFactor = 1)
    {
        EnsureAvailable();
        return await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseAddress.AbsoluteUri,
            DeviceScaleFactor = deviceScaleFactor,
            ViewportSize = new ViewportSize { Width = width, Height = height }
        });
    }

    public void EnsureAvailable()
    {
        if (_hostFailure is not null)
        {
            throw new XunitException(_hostFailure);
        }

        if (_runtimeUnavailable is not null)
        {
            throw new XunitException(_runtimeUnavailable);
        }

        if (_browser is null)
        {
            throw new XunitException("SPEC-003 published browser fixture was not initialized.");
        }
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();
        if (_host is not null)
        {
            await _host.StopAsync();
            await _host.DisposeAsync();
        }
    }

    private void ValidatePublishedRelease()
    {
        var framework = Path.Combine(_publishRoot, "_framework");
        if (!Directory.Exists(framework))
        {
            throw new DirectoryNotFoundException(
                "Run `dotnet publish src/StudentRegistration.Client/" +
                "StudentRegistration.Client.csproj -c Release` before the NFR-6 gate.");
        }

        if (!Directory.EnumerateFiles(framework, "*.wasm.br").Any())
        {
            throw new InvalidOperationException(
                "The Release publish output has no Brotli-compressed WebAssembly assets.");
        }
    }

    private async Task StartHostAsync()
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Production,
            ContentRootPath = _publishRoot
        });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls(BaseAddress.AbsoluteUri.TrimEnd('/'));
        _host = builder.Build();
        _host.Run(ServePublishedAssetAsync);
        await _host.StartAsync();
    }

    private async Task ServePublishedAssetAsync(HttpContext context)
    {
        context.Response.Headers["X-SRS-Published-Release"] = "true";
        if (!HttpMethods.IsGet(context.Request.Method)
            && !HttpMethods.IsHead(context.Request.Method))
        {
            context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            return;
        }

        var requestPath = Uri.UnescapeDataString(context.Request.Path.Value ?? "/");
        if (requestPath.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var relativePath = requestPath.TrimStart('/').Replace(
            '/',
            Path.DirectorySeparatorChar);
        if (string.IsNullOrWhiteSpace(relativePath) || !Path.HasExtension(relativePath))
        {
            relativePath = "index.html";
        }

        var rawPath = Path.GetFullPath(Path.Combine(_publishRoot, relativePath));
        var rootPrefix = _publishRoot.TrimEnd(Path.DirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!rawPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase)
            || !File.Exists(rawPath))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var acceptEncoding = context.Request.Headers.AcceptEncoding.ToString();
        var servedPath = rawPath;
        string? contentEncoding = null;
        if (acceptEncoding.Contains("br", StringComparison.OrdinalIgnoreCase)
            && File.Exists(rawPath + ".br"))
        {
            servedPath = rawPath + ".br";
            contentEncoding = "br";
        }
        else if (acceptEncoding.Contains("gzip", StringComparison.OrdinalIgnoreCase)
                 && File.Exists(rawPath + ".gz"))
        {
            servedPath = rawPath + ".gz";
            contentEncoding = "gzip";
        }

        if (!ContentTypes.TryGetContentType(rawPath, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        var info = new FileInfo(servedPath);
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = contentType;
        context.Response.ContentLength = info.Length;
        context.Response.Headers.CacheControl = "no-cache";
        context.Response.Headers.Vary = "Content-Encoding";
        if (contentEncoding is not null)
        {
            context.Response.Headers.ContentEncoding = contentEncoding;
        }

        if (HttpMethods.IsGet(context.Request.Method))
        {
            await context.Response.SendFileAsync(servedPath);
        }
    }

    private async Task WaitForHostAsync()
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(1) };
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var response = await client.GetAsync(BaseAddress);
                if (response.IsSuccessStatusCode
                    && response.Headers.TryGetValues(
                        "X-SRS-Published-Release",
                        out var values)
                    && values.Contains("true", StringComparer.Ordinal))
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // The listener is still starting.
            }
            catch (TaskCanceledException)
            {
                // The listener is still starting.
            }

            await Task.Delay(200);
        }

        throw new TimeoutException("Timed out waiting for the published Release host.");
    }

    private static int GetFreePort()
    {
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    }
}
