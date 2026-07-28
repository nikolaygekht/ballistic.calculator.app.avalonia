using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AwesomeAssertions;
using BallisticCalculator.Views;
using Iciclecreek.Avalonia.WindowManager;
using Xunit;

namespace BallisticCalculator.Tests.Views;

/// <summary>
/// End-to-end cover for the Windows menu (D-001), exercised through a real <see cref="MainWindow"/>.
/// </summary>
/// <remarks>
/// Everything runs inside a single test on purpose: only the first <see cref="WindowsPanel"/> created
/// in a headless run resolves its control theme, so the window comes from
/// <see cref="HeadlessMainWindow"/> and the whole menu scenario is walked through in one pass. The
/// decision logic behind it is covered case by case in <see cref="WindowActivationTests"/>.
/// </remarks>
[Collection(HeadlessMainWindow.Collection)]
public class WindowsMenuTests
{
    [AvaloniaFact]
    public void WindowsMenu_SelectingEntries_AlwaysBringsThatWindowToTheFront()
    {
        // Arrange — three open child windows
        var main = HeadlessMainWindow.Instance;
        main.Show();
        Dispatcher.UIThread.RunJobs();
        for (var i = 0; i < 3; i++)
        {
            main.AddChildWindow(new CompareView(), $"Window {i + 1}");
            Dispatcher.UIThread.RunJobs();
        }

        // Assert — the menu lists them in the order they were opened, after the separator
        var entries = WindowEntries(main);
        entries.Should().HaveCount(3);
        entries.Select(e => e.Header?.ToString()).Should()
            .ContainInOrder("_1 Window 1", "_2 Window 2", "_3 Window 3");

        // Act & Assert — every entry activates and raises its own window, first entry included
        foreach (var entry in new[] { 0, 1, 2, 0 })
        {
            SelectEntry(main, entry);

            var window = main.ChildWindows[entry];
            window.IsActive.Should().BeTrue($"entry {entry + 1} was selected from the Windows menu");
            window.ZIndex.Should().Be(main.ChildWindows.Max(w => w.ZIndex),
                $"entry {entry + 1} must come to the front");
        }

        // Act & Assert — a minimized window is restored rather than silently ignored
        var minimized = main.ChildWindows[0];
        minimized.WindowState = WindowState.Minimized;
        Dispatcher.UIThread.RunJobs();
        SelectEntry(main, 1);
        SelectEntry(main, 0);

        minimized.WindowState.Should().Be(WindowState.Normal);
        minimized.IsActive.Should().BeTrue();

        // Act & Assert — an active-but-buried window is raised. Maximizing another child lifts its
        // z-order without taking the active state, which is what buries this one.
        var buried = main.ChildWindows[0];
        main.ChildWindows[1].WindowState = WindowState.Maximized;
        Dispatcher.UIThread.RunJobs();
        buried.IsActive.Should().BeTrue("precondition: it is still the active window");
        buried.ZIndex.Should().BeLessThan(main.ChildWindows[1].ZIndex, "precondition: it is buried");

        SelectEntry(main, 0);

        buried.ZIndex.Should().Be(main.ChildWindows.Max(w => w.ZIndex));
        buried.IsActive.Should().BeTrue();
    }

    private static List<MenuItem> WindowEntries(MainWindow main)
    {
        var separatorIndex = main.MenuWindows.Items.IndexOf(main.MenuWindowsSeparator);
        return main.MenuWindows.Items.Skip(separatorIndex + 1).OfType<MenuItem>().ToList();
    }

    private static void SelectEntry(MainWindow main, int entry)
    {
        WindowEntries(main)[entry].RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Dispatcher.UIThread.RunJobs();
    }
}
