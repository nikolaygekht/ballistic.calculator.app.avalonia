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

    public BcConverterDialog(MeasurementSystem system, Ammunition? prefill = null, Atmosphere? atmosphere = null)
    {
        _system = system;
        InitializeComponent();

        ConverterPanel.MeasurementSystem = system;

        // Default to the active shot's air when there is one; the panel treats null as standard.
        if (atmosphere != null)
            ConverterPanel.Atmosphere = atmosphere;

        ConverterPanel.Prefill = prefill;

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
