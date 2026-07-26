using Avalonia.Controls;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Types;

namespace BallisticCalculator.Views.Dialogs;

/// <summary>
/// Tools → Approximate Drag Table → From Measured Velocities. A shell around
/// <see cref="Panels.Panels.DrgFromVelocitiesPanel"/>, which holds the editor and does the saving. The shell
/// owns the atmosphere editor, because a panel library has no business opening windows.
/// </summary>
public partial class ApproximateDrgFromVelocitiesDialog : Window
{
    private readonly MeasurementSystem _system;

    public ApproximateDrgFromVelocitiesDialog(MeasurementSystem system, IFileDialogService fileDialogService,
                                              Ammunition? prefill = null, Atmosphere? atmosphere = null)
    {
        _system = system;
        InitializeComponent();

        EditorPanel.MeasurementSystem = system;
        EditorPanel.FileDialogService = fileDialogService;
        EditorPanel.Prefill = prefill;

        // Default to the active shot's conditions when there are any; the panel treats null as standard.
        if (atmosphere != null)
            EditorPanel.Atmosphere = atmosphere;

        EditorPanel.CloseRequested += (_, _) => Close(true);
        EditorPanel.AtmosphereRequested += async (_, _) => await EditAtmosphere();
    }

    private async System.Threading.Tasks.Task EditAtmosphere()
    {
        var dialog = new AtmosphereDialog(_system, EditorPanel.Atmosphere);

        if (await dialog.ShowDialog<bool?>(this) == true && dialog.Result != null)
            EditorPanel.Atmosphere = dialog.Result;
    }
}
