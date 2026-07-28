using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AwesomeAssertions;
using BallisticCalculator.Views;
using Xunit;

namespace BallisticCalculator.Tests.Views;

/// <summary>
/// Cover for <c>Help → User Guide</c>: the menu entry and F1 both open the published documentation
/// site, and About keeps its place below a separator.
/// </summary>
/// <remarks>
/// <see cref="MainWindow.UrlOpener"/> is the seam these tests need — the production handler calls
/// <c>Process.Start</c> with <c>UseShellExecute</c>, which would launch a real browser on the machine
/// running the suite. The window itself comes from <see cref="HeadlessMainWindow"/>; see there for why
/// it is shared rather than constructed per test.
/// </remarks>
[Collection(HeadlessMainWindow.Collection)]
public class HelpMenuTests
{
    private const string PublishedUrl = "https://nikolaygekht.github.io/ballistic.calculator.app.avalonia/";

    #region The menu entry

    [AvaloniaFact]
    public void MenuHelpUserGuide_WhenSelected_OpensThePublishedUserGuide()
    {
        // Arrange
        var main = HeadlessMainWindow.Instance;
        HeadlessMainWindow.OpenedUrls.Clear();

        // Act
        main.MenuHelpUserGuide.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        // Assert
        HeadlessMainWindow.OpenedUrls.Should().ContainSingle().Which.Should().Be(PublishedUrl);
    }

    [AvaloniaFact]
    public void HelpMenu_Contents_ListTheUserGuideAboveAboutWithASeparatorBetween()
    {
        // Arrange
        var main = HeadlessMainWindow.Instance;

        // Act
        var items = main.MenuHelp.Items.ToList();

        // Assert
        items.Should().HaveCount(3);
        items[0].Should().BeSameAs(main.MenuHelpUserGuide);
        items[1].Should().BeOfType<Separator>();
        items[2].Should().BeSameAs(main.MenuHelpAbout);

        main.MenuHelpUserGuide.InputGesture!.ToString().Should().Be("F1");
        main.MenuHelpAbout.InputGesture!.ToString().Should().Be("Ctrl+F1");
    }

    #endregion

    #region The keyboard

    [AvaloniaFact]
    public void OnKeyDown_F1WithoutModifiers_OpensTheUserGuide()
    {
        // Arrange
        var main = HeadlessMainWindow.Instance;
        HeadlessMainWindow.OpenedUrls.Clear();

        // Act
        PressF1(main, KeyModifiers.None);

        // Assert
        HeadlessMainWindow.OpenedUrls.Should().ContainSingle().Which.Should().Be(PublishedUrl);
    }

    [AvaloniaFact]
    public void OnKeyDown_F1WithAModifier_DoesNotOpenTheUserGuide()
    {
        // Arrange — Ctrl+F1 belongs to About; Shift+F1 and Alt+F1 are unassigned
        var main = HeadlessMainWindow.Instance;
        HeadlessMainWindow.OpenedUrls.Clear();

        // Act
        PressF1(main, KeyModifiers.Shift);
        PressF1(main, KeyModifiers.Alt);

        // Assert
        HeadlessMainWindow.OpenedUrls.Should().BeEmpty();
    }

    #endregion

    /// <summary>
    /// The URL the application ships must be the one that is actually published — a typo here is a
    /// broken Help menu that no other test would notice.
    /// </summary>
    [AvaloniaFact]
    public void HelpUrl_IsTheGitHubPagesSiteBuiltFromTheDocsFolder()
    {
        MainWindow.HelpUrl.Should().Be(PublishedUrl);
    }

    private static void PressF1(MainWindow main, KeyModifiers modifiers)
    {
        main.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.F1,
            KeyModifiers = modifiers,
        });
        Dispatcher.UIThread.RunJobs();
    }
}
