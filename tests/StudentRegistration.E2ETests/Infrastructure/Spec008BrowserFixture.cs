using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.Playwright;
using StudentRegistration.TestSupport;
using Xunit.Sdk;

namespace StudentRegistration.E2ETests.Infrastructure;

[CollectionDefinition(CollectionName)]
public sealed class Spec008BrowserCollection : ICollectionFixture<Spec008BrowserFixture>
{
    public const string CollectionName = "SPEC-008 browser host";
}

/// <summary>
/// Runs the real Blazor WebAssembly development host and a pinned headless
/// Chromium instance. A missing local browser/runtime is reported as a skip;
/// an application startup or journey failure remains a failing test.
/// </summary>
public sealed class Spec008BrowserFixture : IAsyncLifetime
{
    private readonly ConcurrentQueue<string> _hostOutput = new();
    private Process? _host;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private string? _runtimeUnavailable;
    private string? _hostFailure;

    public Uri BaseAddress { get; private set; } = new("http://127.0.0.1/");

    public async Task InitializeAsync()
    {
        try
        {
            BaseAddress = new Uri($"http://127.0.0.1:{GetFreePort()}/");
            _host = StartClientHost(BaseAddress);
            await WaitForHostAsync(_host, BaseAddress);
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
            _browser = await _playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions { Headless = true });
        }
        catch (PlaywrightException exception)
        {
            _runtimeUnavailable =
                $"The pinned Playwright Chromium runtime is unavailable: {exception.Message}";
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
            throw SkipException.ForSkip(_runtimeUnavailable);
        }

        if (_browser is null)
        {
            throw new XunitException("SPEC-008 browser fixture was not initialized.");
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

    private Process StartClientHost(Uri address)
    {
        var configuration = Environment.GetEnvironmentVariable(
            "STUDENTREGISTRATION_BROWSER_CONFIGURATION");
        configuration = string.Equals(configuration, "Debug", StringComparison.OrdinalIgnoreCase)
            ? "Debug"
            : "Release";
        var startInfo = new ProcessStartInfo("dotnet")
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = RepositoryFiles.Root
        };
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(RepositoryFiles.PathTo(
            "src/StudentRegistration.Client/StudentRegistration.Client.csproj"));
        startInfo.ArgumentList.Add("--configuration");
        startInfo.ArgumentList.Add(configuration);
        startInfo.ArgumentList.Add("--no-build");
        startInfo.ArgumentList.Add("--no-restore");
        startInfo.ArgumentList.Add("--no-launch-profile");
        startInfo.ArgumentList.Add("--");
        startInfo.ArgumentList.Add("--urls");
        startInfo.ArgumentList.Add(address.AbsoluteUri.TrimEnd('/'));

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

    private async Task WaitForHostAsync(Process process, Uri address)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(1) };
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            if (process.HasExited)
            {
                throw new InvalidOperationException(
                    $"Host process exited with code {process.ExitCode}");
            }

            try
            {
                using var response = await client.GetAsync(address);
                if ((int)response.StatusCode < 500)
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
