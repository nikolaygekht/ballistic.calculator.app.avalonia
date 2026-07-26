using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Types;

namespace BallisticCalculator.Views.Dialogs;

/// <summary>
/// Edits a standalone <see cref="Atmosphere"/> — used by the drag table editor, where the conditions the
/// velocities were measured in are an input to the drag recovery rather than part of a shot.
/// </summary>
public partial class AtmosphereDialog : Window
{
    public AtmosphereDialog(MeasurementSystem system, Atmosphere? atmosphere)
    {
        InitializeComponent();

        AtmospherePanel.MeasurementSystem = system;
        AtmospherePanel.Atmosphere = atmosphere ?? new Atmosphere();
    }

    /// <summary>The edited atmosphere; null when the panel could not produce one.</summary>
    public Atmosphere? Result { get; private set; }

    private void OnOK(object? sender, RoutedEventArgs e)
    {
        Result = AtmospherePanel.Atmosphere;
        Close(true);
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(false);
}
