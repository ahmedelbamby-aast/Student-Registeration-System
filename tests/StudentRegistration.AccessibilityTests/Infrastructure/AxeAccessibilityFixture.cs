using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using StudentRegistration.TestSupport;
using Xunit.Sdk;

namespace StudentRegistration.AccessibilityTests.Infrastructure;

[CollectionDefinition(CollectionName)]
public sealed class AxeAccessibilityCollection : ICollectionFixture<AxeAccessibilityFixture>
{
    public const string CollectionName = "SPEC-008 axe browser host";
}

public sealed class AxeAccessibilityFixture : IAsyncLifetime
{
    private const string BrowserTargetVariable = "SRS_BROWSER_TARGET";
    private const string HeadedVariable = "SRS_ACCESSIBILITY_HEADED";

    private readonly ConcurrentQueue<string> _hostOutput = new();
    private Process? _host;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private string? _runtimeUnavailable;
    private string? _hostFailure;

    public Uri BaseAddress { get; private set; } = new("http://127.0.0.1/");

    public string BrowserTarget { get; private set; } = "Playwright Chromium";

    public async Task InitializeAsync()
    {
        try
        {
            BaseAddress = new Uri($"http://127.0.0.1:{GetFreePort()}/");
            _host = StartHost();
            await WaitForHostAsync();
        }
        catch (Win32Exception exception)
        {
            _runtimeUnavailable = $"The dotnet runtime is unavailable: {exception.Message}";
            return;
        }
        catch (Exception exception)
        {
            _hostFailure = $"The Blazor host did not start: {exception.Message}. {HostTail()}";
            return;
        }

        try
        {
            _playwright = await Playwright.CreateAsync();
            BrowserTarget = Environment.GetEnvironmentVariable(BrowserTargetVariable)
                ?? "Playwright Chromium";
            var headless = !IsEnabled(HeadedVariable);
            _browser = BrowserTarget switch
            {
                "Google Chrome" => await _playwright.Chromium.LaunchAsync(
                    new BrowserTypeLaunchOptions { Headless = headless, Channel = "chrome" }),
                "Microsoft Edge" => await _playwright.Chromium.LaunchAsync(
                    new BrowserTypeLaunchOptions { Headless = headless, Channel = "msedge" }),
                "Mozilla Firefox" => await _playwright.Firefox.LaunchAsync(
                    new BrowserTypeLaunchOptions { Headless = headless }),
                "Playwright WebKit" => await _playwright.Webkit.LaunchAsync(
                    new BrowserTypeLaunchOptions { Headless = headless }),
                "Playwright Chromium" => await _playwright.Chromium.LaunchAsync(
                    new BrowserTypeLaunchOptions { Headless = headless }),
                _ => throw new InvalidOperationException(
                    $"Unsupported {BrowserTargetVariable} value '{BrowserTarget}'.")
            };
        }
        catch (Exception exception) when (
            exception is PlaywrightException or InvalidOperationException)
        {
            _runtimeUnavailable =
                $"The required {BrowserTarget} runtime is unavailable: {exception.Message}";
        }
    }

    public async Task<IBrowserContext> OpenContextAsync(
        int width,
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

    public static async Task AssertNoSeriousAxeViolationsAsync(IPage page)
    {
        var result = await page.RunAxe();
        var serious = result.Violations
            .Where(violation =>
                string.Equals(violation.Impact, "critical", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(violation.Impact, "serious", StringComparison.OrdinalIgnoreCase))
            .Select(violation => $"{violation.Id} ({violation.Impact})")
            .ToArray();
        Assert.True(
            serious.Length == 0,
            $"AUTH-01 has serious axe violations: {string.Join(", ", serious)}");
    }

    private void EnsureAvailable()
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
            throw new XunitException("SPEC-008 axe fixture was not initialized.");
        }
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();
        if (_host is { HasExited: false })
        {
            _host.Kill(entireProcessTree: true);
            await _host.WaitForExitAsync();
        }

        _host?.Dispose();
    }

    private Process StartHost()
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = RepositoryFiles.Root
        };
        foreach (var argument in new[]
                 {
                     "run", "--project",
                     RepositoryFiles.PathTo(
                         "src/StudentRegistration.Client/StudentRegistration.Client.csproj"),
                     "--configuration", "Debug", "--no-build", "--no-restore",
                     "--no-launch-profile", "--", "--urls",
                     BaseAddress.AbsoluteUri.TrimEnd('/')
                 })
        {
            startInfo.ArgumentList.Add(argument);
        }

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("The Blazor host process was not created.");
        process.OutputDataReceived += CaptureOutput;
        process.ErrorDataReceived += CaptureOutput;
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }

    private void CaptureOutput(object sender, DataReceivedEventArgs args)
    {
        if (!string.IsNullOrWhiteSpace(args.Data))
        {
            _hostOutput.Enqueue(args.Data);
            while (_hostOutput.Count > 20)
            {
                _hostOutput.TryDequeue(out _);
            }
        }
    }

    private async Task WaitForHostAsync()
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(1) };
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            if (_host!.HasExited)
            {
                throw new InvalidOperationException(
                    $"Host process exited with code {_host.ExitCode}");
            }

            try
            {
                using var response = await client.GetAsync(BaseAddress);
                if ((int)response.StatusCode < 500)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException)
            {
            }

            await Task.Delay(200);
        }

        throw new TimeoutException("Timed out waiting for the Blazor host.");
    }

    private string HostTail() => string.Join(" | ", _hostOutput.TakeLast(8));

    private static int GetFreePort()
    {
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static bool IsEnabled(string variable) =>
        string.Equals(
            Environment.GetEnvironmentVariable(variable),
            "true",
            StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Environment.GetEnvironmentVariable(variable), "1", StringComparison.Ordinal);
}
