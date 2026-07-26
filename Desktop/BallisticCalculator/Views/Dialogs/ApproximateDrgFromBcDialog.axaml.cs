using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Types;

namespace BallisticCalculator.Views.Dialogs;

/// <summary>
/// Tools → Approximate Drag Table → From BC Curve. A shell around <see cref="Panels.Panels.DrgFromBcPanel"/>,
/// which holds the editor and does the saving; the dialog only supplies context and closes.
/// </summary>
public partial class ApproximateDrgFromBcDialog : Window
{
    public ApproximateDrgFromBcDialog(MeasurementSystem system, IFileDialogService fileDialogService,
                                      Ammunition? prefill = null)
    {
        InitializeComponent();

        EditorPanel.MeasurementSystem = system;
        EditorPanel.FileDialogService = fileDialogService;
        EditorPanel.Prefill = prefill;
    }

    private void OnClose(object? sender, RoutedEventArgs e) => Close(true);
}
