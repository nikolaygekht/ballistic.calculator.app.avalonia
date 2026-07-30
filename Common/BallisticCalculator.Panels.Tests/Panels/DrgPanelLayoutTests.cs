using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Avalonia.Threading;
using Xunit;
using AwesomeAssertions;
using BallisticCalculator.Panels.Panels;

namespace BallisticCalculator.Panels.Tests.Panels;

/// <summary>
/// Both "Approximate Drag Table" panels are taller than their dialog at its minimum height, so the form
/// scrolls. The action row must not scroll with it: it used to be the last child of one big scrolled
/// StackPanel, and shrinking the dialog pushed Save and Close past the scroll extent, where no amount of
/// scrolling reached them. These tests pin the buttons down at a height no bigger than the dialogs'
/// MinHeight of 300.
/// </summary>
public class DrgPanelLayoutTests
{
    /// <summary>The dialogs are 600 x 620/640 and clamp at MinWidth 480 / MinHeight 300.</summary>
    private const double ShrunkWidth = 480;
    private const double ShrunkHeight = 300;

    private static Window Shrunk(Control panel)
    {
        var window = new Window { Content = panel, Width = ShrunkWidth, Height = ShrunkHeight };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.Measure(new Size(ShrunkWidth, ShrunkHeight));
        window.Arrange(new Rect(0, 0, ShrunkWidth, ShrunkHeight));
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    /// <summary>
    /// Asserts the control was laid out somewhere a user can actually see and click it — inside the
    /// window's client area, not below its bottom edge.
    /// </summary>
    private static void ShouldBeOnScreenIn(Control control, Window window, string what)
    {
        control.Bounds.Height.Should().BeGreaterThan(0, $"{what} should have been laid out");

        var origin = control.TranslatePoint(new Point(0, 0), window);
        origin.Should().NotBeNull($"{what} should be in the same visual tree as the window");

        origin!.Value.Y.Should().BeGreaterThanOrEqualTo(0,
            $"{what} should not be above the top of a {ShrunkHeight}-high dialog");
        (origin.Value.Y + control.Bounds.Height).Should().BeLessThanOrEqualTo(ShrunkHeight,
            $"{what} should still be inside a {ShrunkHeight}-high dialog, not pushed off the bottom");
    }

    #region From BC Curve

    [AvaloniaFact]
    public void SaveAndClose_InAShortDialog_ShouldStayOnScreen()
    {
        var panel = new DrgFromBcPanel();

        var window = Shrunk(panel);

        ShouldBeOnScreenIn(panel.SaveButton, window, "Save Drg");
        ShouldBeOnScreenIn(panel.CloseButton, window, "Close");
    }

    [AvaloniaFact]
    public void StatusText_InAShortDialog_ShouldStayOnScreenWithTheButtons()
    {
        // The status line reports the result of a Save, so it is useless if pressing the pinned Save
        // button leaves the answer somewhere off the bottom of the dialog.
        var panel = new DrgFromBcPanel();

        var window = Shrunk(panel);

        ShouldBeOnScreenIn(panel.StatusText, window, "the status line");
    }

    [AvaloniaFact]
    public void TheForm_ShouldScrollWhileTheActionRowStaysOutOfIt()
    {
        // The structural half of the same fix, and the part worth pinning: the fields scroll, the
        // buttons are not in the scrolled content at all. Asserting the scroll extent instead would
        // prove nothing here — headless has no font metrics, so the form measures short enough to fit
        // and never overflows (see BcConverterPanelTests on headless layout).
        var panel = new DrgFromBcPanel();

        Shrunk(panel);

        IsInsideAScrollViewer(panel.NameBox, panel).Should().BeTrue("the form fields should scroll");
        IsInsideAScrollViewer(panel.SaveButton, panel).Should().BeFalse(
            "Save must not sit in the scrolled content — that is what let it be pushed out of reach");
        IsInsideAScrollViewer(panel.CloseButton, panel).Should().BeFalse("nor must Close");
    }

    #endregion

    #region From Measured Velocities

    [AvaloniaFact]
    public void Velocities_SaveAndClose_InAShortDialog_ShouldStayOnScreen()
    {
        var panel = new DrgFromVelocitiesPanel();

        var window = Shrunk(panel);

        ShouldBeOnScreenIn(panel.SaveButton, window, "Save Drg");
        ShouldBeOnScreenIn(panel.CloseButton, window, "Close");
        ShouldBeOnScreenIn(panel.AtmosphereButton, window, "Set Atmosphere");
    }

    [AvaloniaFact]
    public void Velocities_StatusText_InAShortDialog_ShouldStayOnScreenWithTheButtons()
    {
        var panel = new DrgFromVelocitiesPanel();

        var window = Shrunk(panel);

        ShouldBeOnScreenIn(panel.StatusText, window, "the status line");
    }

    [AvaloniaFact]
    public void Velocities_TheForm_ShouldScrollWhileTheActionRowStaysOutOfIt()
    {
        var panel = new DrgFromVelocitiesPanel();

        Shrunk(panel);

        IsInsideAScrollViewer(panel.NameBox, panel).Should().BeTrue("the form fields should scroll");
        IsInsideAScrollViewer(panel.SaveButton, panel).Should().BeFalse(
            "Save must not sit in the scrolled content — that is what let it be pushed out of reach");
        IsInsideAScrollViewer(panel.CloseButton, panel).Should().BeFalse("nor must Close");
    }

    #endregion

    /// <summary>Whether <paramref name="control"/> sits in scrolled content somewhere inside the panel.</summary>
    private static bool IsInsideAScrollViewer(Control control, Control root) =>
        control.GetVisualAncestors()
               .TakeWhile(ancestor => !ReferenceEquals(ancestor, root))
               .OfType<ScrollViewer>()
               .Any();
}
