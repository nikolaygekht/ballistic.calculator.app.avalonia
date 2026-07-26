using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Xunit;
using AwesomeAssertions;
using BallisticCalculator.Panels.Panels;

namespace BallisticCalculator.Panels.Tests.Panels;

/// <summary>Both editors must load and lay out; a XAML mistake here only shows at runtime otherwise.</summary>
public class DialogSmokeTests
{
    [AvaloniaFact]
    public void BothEditors_ShouldLoadAndLayOutInsideAScrollViewer()
    {
        var bc = new DrgFromBcPanel();
        var vel = new DrgFromVelocitiesPanel();

        var stack = new StackPanel();
        stack.Children.Add(new ScrollViewer { Content = bc, Height = 300 });
        stack.Children.Add(new ScrollViewer { Content = vel, Height = 300 });
        var window = new Window { Content = stack, Width = 600, Height = 640 };
        window.Show();

        bc.Bounds.Height.Should().BeGreaterThan(0, "the BC editor must lay out");
        vel.Bounds.Height.Should().BeGreaterThan(0, "the velocities editor must lay out");
        bc.KnotsGrid.Bounds.Height.Should().BeGreaterThan(0);
        vel.ReadingsGrid.Bounds.Height.Should().BeGreaterThan(0);
    }
}
