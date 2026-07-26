using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Xunit;
using AwesomeAssertions;
using BallisticCalculator.Controls.Controls;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Controls.Tests.UI;

/// <summary>
/// Keyboard navigation through a <see cref="MeasurementControl"/>: value → unit → next field.
/// <para>
/// Regression cover for a defect where the control forwarded every bubbling GotFocus to its numeric part,
/// so focus could never move to the unit combo or out of the control — Tab died at the first measurement
/// field of any panel.
/// </para>
/// </summary>
public class MeasurementControlFocusTests
{
    private sealed class Harness
    {
        public Window Window { get; }
        public TextBox Before { get; }
        public MeasurementControl Measurement { get; }
        public TextBox After { get; }

        public Harness()
        {
            Before = new TextBox { Name = "Before" };
            Measurement = new MeasurementControl { Name = "Measurement", UnitType = typeof(DistanceUnit) };
            After = new TextBox { Name = "After" };

            var stack = new StackPanel();
            stack.Children.Add(Before);
            stack.Children.Add(Measurement);
            stack.Children.Add(After);

            Window = new Window { Content = stack, Width = 400, Height = 300 };
            Window.Show();
            Measurement.SetValue(new Measurement<DistanceUnit>(100, DistanceUnit.Meter));
            Before.Focus();
        }

        public object? Focused => Window.FocusManager?.GetFocusedElement();

        public void Tab() => Window.KeyPress(Key.Tab, RawInputModifiers.None);
    }

    [AvaloniaFact]
    public void Tab_FromNumericPart_ShouldReachTheUnitCombo()
    {
        var h = new Harness();
        h.Tab();                                   // Before -> the control's numeric part
        h.Focused.Should().BeSameAs(h.Measurement.NumericPart);

        h.Tab();                                   // numeric part -> unit combo

        h.Focused.Should().BeSameAs(h.Measurement.UnitPart,
            "Tab must be able to reach the unit selector, otherwise units are mouse-only");
    }

    [AvaloniaFact]
    public void Tab_ShouldLeaveTheControlAndReachTheNextField()
    {
        var h = new Harness();
        h.Tab();    // -> numeric
        h.Tab();    // -> unit
        h.Tab();    // -> out of the control

        h.Focused.Should().BeSameAs(h.After,
            "Tab must continue past a measurement field; it used to be trapped there");
    }

    [AvaloniaFact]
    public void ShiftTab_ShouldWalkBackOutOfTheControl()
    {
        var h = new Harness();
        h.Tab();    // -> numeric
        h.Tab();    // -> unit

        h.Window.KeyPress(Key.Tab, RawInputModifiers.Shift);
        h.Focused.Should().BeSameAs(h.Measurement.NumericPart);

        h.Window.KeyPress(Key.Tab, RawInputModifiers.Shift);
        h.Focused.Should().BeSameAs(h.Before);
    }

    [AvaloniaFact]
    public void BallisticCoefficientControl_TabShouldReachTheTableComboAndMoveOn()
    {
        // Same defect, same fix — this control had the identical GotFocus forwarding.
        var before = new TextBox();
        var bc = new BallisticCoefficientControl();
        var after = new TextBox();
        var stack = new StackPanel();
        stack.Children.Add(before);
        stack.Children.Add(bc);
        stack.Children.Add(after);
        var window = new Window { Content = stack, Width = 400, Height = 300 };
        window.Show();
        before.Focus();

        window.KeyPress(Key.Tab, RawInputModifiers.None);
        window.FocusManager?.GetFocusedElement().Should().BeSameAs(bc.NumericPart);

        window.KeyPress(Key.Tab, RawInputModifiers.None);
        window.FocusManager?.GetFocusedElement().Should().BeSameAs(bc.TablePart);

        window.KeyPress(Key.Tab, RawInputModifiers.None);
        window.FocusManager?.GetFocusedElement().Should().BeSameAs(after);
    }
}
