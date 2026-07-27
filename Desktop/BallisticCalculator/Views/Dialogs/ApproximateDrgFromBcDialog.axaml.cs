using Avalonia.Controls;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Types;

namespace BallisticCalculator.Views.Dialogs;

/// <summary>
/// Tools → Approximate Drag Table → From BC Curve. A shell around <see cref="Panels.Panels.DrgFromBcPanel"/>,
/// which holds the editor and does the saving. The editor opens empty: it describes a bullet the user is
/// characterising, which is not necessarily the one in any open trajectory. The shell owns the atmosphere
/// editor, because a panel library has no business opening windows.
/// </summary>
public partial class ApproximateDrgFromBcDialog : Window
{
    private readonly MeasurementSystem _system;

    public ApproximateDrgFromBcDialog(MeasurementSystem system, IFileDialogService fileDialogService)
    {
        _system = system;
        InitializeComponent();

        EditorPanel.MeasurementSystem = system;
        EditorPanel.FileDialogService = fileDialogService;
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
