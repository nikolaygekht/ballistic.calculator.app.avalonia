using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(BallisticCalculator.Tests.TestAppBuilder))]

namespace BallisticCalculator.Tests;

/// <summary>
/// Headless tests run against the real <see cref="App"/> so that the application's styles —
/// including the MDI window manager's control themes — are available to every test, not only
/// to the first one that happens to run.
/// </summary>
public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
