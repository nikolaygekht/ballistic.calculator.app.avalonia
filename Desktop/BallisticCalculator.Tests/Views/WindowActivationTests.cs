using Avalonia.Controls;
using AwesomeAssertions;
using BallisticCalculator.Views;
using Xunit;

namespace BallisticCalculator.Tests.Views;

/// <summary>
/// D-001: selecting an entry in the Windows menu could do nothing at all, because
/// <c>ManagedWindow.Activate()</c> returns early both while a window is minimized and while it is
/// already <c>IsActive</c> — and a window can be active yet buried behind another one.
/// </summary>
public class WindowActivationTests
{
    #region Plan Tests

    [Fact]
    public void For_InactiveNormalWindow_ActivatesIt()
    {
        // Arrange & Act
        var plan = WindowActivation.For(WindowState.Normal, isActive: false);

        // Assert
        plan.Restore.Should().BeFalse();
        plan.Activate.Should().BeTrue();
        plan.BringToTop.Should().BeFalse("Activate() raises the window itself");
    }

    [Fact]
    public void For_ActiveWindow_RaisesItWithoutActivating()
    {
        // Arrange & Act — an active window can still sit behind another one
        var plan = WindowActivation.For(WindowState.Normal, isActive: true);

        // Assert
        plan.Restore.Should().BeFalse();
        plan.Activate.Should().BeFalse("Activate() returns early when the window is already active");
        plan.BringToTop.Should().BeTrue();
    }

    [Fact]
    public void For_MinimizedInactiveWindow_RestoresThenActivates()
    {
        // Arrange & Act
        var plan = WindowActivation.For(WindowState.Minimized, isActive: false);

        // Assert
        plan.Restore.Should().BeTrue("Activate() does nothing while the window is minimized");
        plan.Activate.Should().BeTrue();
    }

    [Fact]
    public void For_MinimizedActiveWindow_RestoresThenRaises()
    {
        // Arrange & Act
        var plan = WindowActivation.For(WindowState.Minimized, isActive: true);

        // Assert
        plan.Restore.Should().BeTrue();
        plan.Activate.Should().BeFalse();
        plan.BringToTop.Should().BeTrue();
    }

    [Theory]
    [InlineData(WindowState.Normal)]
    [InlineData(WindowState.Maximized)]
    [InlineData(WindowState.FullScreen)]
    public void For_WindowThatIsNotMinimized_IsNeverRestored(WindowState state)
    {
        // Arrange & Act & Assert
        WindowActivation.For(state, isActive: false).Restore.Should().BeFalse();
        WindowActivation.For(state, isActive: true).Restore.Should().BeFalse();
    }

    [Theory]
    [InlineData(WindowState.Normal, false)]
    [InlineData(WindowState.Normal, true)]
    [InlineData(WindowState.Minimized, false)]
    [InlineData(WindowState.Minimized, true)]
    [InlineData(WindowState.Maximized, false)]
    [InlineData(WindowState.Maximized, true)]
    public void For_AnyState_AlwaysRaisesTheWindowOneWayOrTheOther(WindowState state, bool isActive)
    {
        // Arrange & Act
        var plan = WindowActivation.For(state, isActive);

        // Assert — the window must end up in front, whichever route is taken
        (plan.Activate || plan.BringToTop).Should().BeTrue();
    }

    #endregion
}
