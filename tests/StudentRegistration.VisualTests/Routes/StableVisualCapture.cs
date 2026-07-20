using System.Security.Cryptography;
using Microsoft.Playwright;
using Xunit.Sdk;

namespace StudentRegistration.VisualTests.Routes;

internal static class StableVisualCapture
{
    internal static async Task<byte[]> CaptureAsync(
        IPage page,
        string routeId,
        string browser,
        int width)
    {
        await page.AddStyleTagAsync(new()
        {
            Content = """
                *, *::before, *::after {
                    animation: none !important;
                    transition: none !important;
                    caret-color: transparent !important;
                }
                """
        });
        await page.EvaluateAsync("""
            async () => {
                await document.fonts.ready;
                await new Promise(resolve => {
                    let timer;
                    const observer = new MutationObserver(() => {
                        clearTimeout(timer);
                        timer = setTimeout(finish, 250);
                    });
                    const finish = () => {
                        observer.disconnect();
                        resolve();
                    };
                    observer.observe(document.documentElement, {
                        subtree: true,
                        childList: true,
                        attributes: true,
                        characterData: true
                    });
                    timer = setTimeout(finish, 250);
                });
                await new Promise(resolve =>
                    requestAnimationFrame(() => requestAnimationFrame(resolve)));
            }
            """);

        byte[]? previous = null;
        string? previousHash = null;
        var observedHashes = new List<string>();
        for (var attempt = 1; attempt <= 5; attempt++)
        {
            var current = await page.ScreenshotAsync(new() { FullPage = true });
            var currentHash = Hash(current);
            observedHashes.Add(currentHash);
            if (string.Equals(previousHash, currentHash, StringComparison.Ordinal))
            {
                return current;
            }

            previous = current;
            previousHash = currentHash;
            await page.WaitForTimeoutAsync(150);
        }

        var diagnosticDirectory = Path.Combine(
            Path.GetTempPath(),
            "StudentRegistration.VisualTests",
            routeId);
        Directory.CreateDirectory(diagnosticDirectory);
        var diagnosticPath = Path.Combine(
            diagnosticDirectory,
            $"{browser}-{width}-unstable.png");
        await File.WriteAllBytesAsync(diagnosticPath, previous!);
        throw new XunitException(
            $"Visual capture did not settle for {routeId} {browser} {width}px after five frames. " +
            $"Observed hashes: {string.Join(", ", observedHashes)}. " +
            $"Last screenshot: {diagnosticPath}");
    }

    private static string Hash(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
