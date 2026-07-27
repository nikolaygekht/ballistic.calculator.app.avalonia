using Avalonia.Controls;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Types;

namespace BallisticCalculator.Views.Dialogs;

/// <summary>
/// Tools → Approximate Drag Table → From BC Curve. A shell around <see cref="Panels.Panels.DrgFromBcPanel"/>,
/// which holds the editor and does the saving. The editor opens empty: it describes a bullet the user is
/// characterising, which is not necessarily the one in any open trajectory.
/// </summary>
public partial class ApproximateDrgFromBcDialog : Window
{
    public ApproximateDrgFromBcDialog(MeasurementSystem system, IFileDialogService fileDialogService)
    {
        InitializeComponent();

        EditorPanel.MeasurementSystem = system;
        EditorPanel.FileDialogService = fileDialogService;
        EditorPanel.CloseRequested += (_, _) => Close(true);
    }
}
