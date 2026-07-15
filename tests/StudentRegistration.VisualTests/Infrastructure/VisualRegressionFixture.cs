using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.Playwright;
using StudentRegistration.TestSupport;
using Xunit.Sdk;

namespace StudentRegistration.VisualTests.Infrastructure;

[CollectionDefinition(CollectionName)]
public sealed class VisualRegressionCollection : ICollectionFixture<VisualRegressionFixture>
{
    public const string CollectionName = "SPEC-008 visual browser host";
}

public sealed class VisualRegressionFixture : IAsyncLifetime
{
    private readonly ConcurrentQueue<string> _hostOutput = new();
    private readonly Dictionary<string, IBrowser> _browsers =
        new(StringComparer.OrdinalIgnoreCase);
    private Process? _host;
    private IPlaywright? _playwright;
    private string? _runtimeUnavailable;
    private string? _hostFailure;

    public Uri BaseAddress { get; private set; } = new("http://127.0.0.1/");

    public async Task InitializeAsync()
    {
        try
        {
            BaseAddress = new Uri($"http://127.0.0.1:{GetFreePort()}/");
            _host = StartHost();
            await WaitForHostAsync();
            _playwright = await Playwright.CreateAsync();
        }
        catch (Win32Exception exception)
        {
            _runtimeUnavailable = $"The dotnet runtime is unavailable: {exception.Message}";
        }
        catch (PlaywrightException exception)
        {
            _runtimeUnavailable = $"The Playwright runtime is unavailable: {exception.Message}";
        }
        catch (Exception exception)
        {
            _hostFailure = $"The Blazor host did not start: {exception.Message}. {HostTail()}";
        }
    }

    public async Task<IBrowserContext> OpenContextAsync(
        string browserName,
        int width,
        int height = 1000)
    {
        EnsureHostAvailable();
        var browser = await GetBrowserAsync(browserName);
        return await browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseAddress.AbsoluteUri,
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce,
            ViewportSize = new ViewportSize { Width = width, Height = height }
        });
    }

    public async Task DisposeAsync()
    {
        foreach (var browser in _browsers.Values)
        {
            await browser.CloseAsync();
        }

        _playwright?.Dispose();
        if (_host is { HasExited: false })
        {
            _host.Kill(entireProcessTree: true);
            await _host.WaitForExitAsync();
        }

        _host?.Dispose();
    }

    private async Task<IBrowser> GetBrowserAsync(string browserName)
    {
        if (_browsers.TryGetValue(browserName, out var existing))
        {
            return existing;
        }

        try
        {
            var browser = browserName.ToLowerInvariant() switch
            {
                "chrome" => await _playwright!.Chromium.LaunchAsync(
                    new BrowserTypeLaunchOptions { Channel = "chrome", Headless = true }),
                "edge" => await _playwright!.Chromium.LaunchAsync(
                    new BrowserTypeLaunchOptions { Channel = "msedge", Headless = true }),
                "firefox" => await _playwright!.Firefox.LaunchAsync(
                    new BrowserTypeLaunchOptions { Headless = true }),
                "webkit" => await _playwright!.Webkit.LaunchAsync(
                    new BrowserTypeLaunchOptions { Headless = true }),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(browserName), browserName, "Unknown governed browser target.")
            };
            _browsers.Add(browserName, browser);
            return browser;
        }
        catch (PlaywrightException exception)
        {
            throw SkipException.ForSkip(
                $"The governed {browserName} browser runtime is unavailable: {exception.Message}");
        }
    }

    private void EnsureHostAvailable()
    {
        if (_hostFailure is not null)
        {
            throw new XunitException(_hostFailure);
        }

        if (_runtimeUnavailable is not null)
        {
            throw SkipException.ForSkip(_runtimeUnavailable);
        }

        if (_playwright is null)
        {
            throw new XunitException("SPEC-008 visual fixture was not initialized.");
        }
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
}
