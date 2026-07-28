using BallisticCalculator.Views;

namespace BallisticCalculator.Tests.Views;

/// <summary>
/// The one <see cref="MainWindow"/> a headless run can have, shared by every test that needs a real
/// one.
/// </summary>
/// <remarks>
/// <para>
/// <c>WindowsPanel</c> (the MDI surface) resolves its control theme only for the <b>first instance
/// created in the process</b>. Construct a second <see cref="MainWindow"/> — merely construct it, no
/// <c>Show()</c> needed — and the next one to lay out throws
/// <c>ArgumentNullException: PART_Windows</c> from <c>WindowsPanel.OnApplyTemplate</c>. So the window
/// is created once, lazily, by whichever test asks first, and shared from then on.
/// </para>
/// <para>
/// Sharing mutable state between tests is normally the wrong thing; here the framework leaves no
/// choice, and one documented instance is better than a single giant test. Test classes using it must
/// declare <c>[Collection(Collection)]</c> so they cannot interleave, and must arrange whatever state
/// they assert on rather than assuming a fresh window.
/// </para>
/// </remarks>
internal static class HeadlessMainWindow
{
    /// <summary>xUnit collection shared by every test class that touches the window.</summary>
    internal const string Collection = "MainWindow";

    private static MainWindow? _instance;

    /// <summary>URLs the window was asked to open, in order. Clear it when arranging.</summary>
    internal static List<string> OpenedUrls { get; } = new();

    /// <summary>The shared window, created on first use.</summary>
    internal static MainWindow Instance =>
        _instance ??= new MainWindow { UrlOpener = OpenedUrls.Add };
}
