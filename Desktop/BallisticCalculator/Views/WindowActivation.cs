using Avalonia.Controls;

namespace BallisticCalculator.Views;

/// <summary>
/// Works out how to bring a child window to the front.
/// </summary>
/// <remarks>
/// <c>ManagedWindow.Activate()</c> returns without doing anything in two states, which is why a
/// plain call to it is not enough for the Windows menu (D-001):
/// <list type="bullet">
/// <item>the window is <b>minimized</b> — it must be restored first;</item>
/// <item>the window is already <b>IsActive</b> — yet it can still sit behind another window,
/// because raising a window's z-order (maximizing another child, for instance) does not transfer
/// the active state. Such a window only needs raising, and <c>Activate()</c> will not do it.</item>
/// </list>
/// </remarks>
internal static class WindowActivation
{
    /// <summary>What has to be done to a window to bring it forward.</summary>
    /// <param name="Restore">Leave the minimized state before anything else.</param>
    /// <param name="Activate">Call <c>Activate()</c> — it also raises the window.</param>
    /// <param name="BringToTop">Raise the window explicitly, because <c>Activate()</c> would no-op.</param>
    internal readonly record struct Plan(bool Restore, bool Activate, bool BringToTop);

    internal static Plan For(WindowState state, bool isActive)
        => new(Restore: state == WindowState.Minimized,
               Activate: !isActive,
               BringToTop: isActive);
}
