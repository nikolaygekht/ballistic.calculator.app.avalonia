using Avalonia.Controls;
using BallisticCalculator.Types;

namespace BallisticCalculator.Views.Dialogs;

/// <summary>
/// Tools → Convert Ballistic Coefficient. A shell around <see cref="Panels.Panels.BcConverterPanel"/>, which
/// does the converting. The shell owns the atmosphere editor — the same one the measured-velocities drag table
/// editor uses — because a panel library has no business opening windows.
/// </summary>
public partial class BcConverterDialog : Window
{
    private readonly MeasurementSystem _system;

    public BcConverterDialog(MeasurementSystem system)
    {
        _system = system;
        InitializeComponent();

        ConverterPanel.MeasurementSystem = system;

        // Nothing is taken from an open trajectory: the coefficient being converted is one the user is reading
        // off a data sheet. The panel treats a null atmosphere as sea-level standard.
        ConverterPanel.CloseRequested += (_, _) => Close(true);
        ConverterPanel.AtmosphereRequested += async (_, _) => await EditAtmosphere();
    }

    private async System.Threading.Tasks.Task EditAtmosphere()
    {
        var dialog = new AtmosphereDialog(_system, ConverterPanel.Atmosphere);

        if (await dialog.ShowDialog<bool?>(this) == true && dialog.Result != null)
            ConverterPanel.Atmosphere = dialog.Result;
    }
}
