using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Types;

namespace BallisticCalculator.Views.Dialogs;

/// <summary>
/// Tools → Approximate Drag Table → From Measured Velocities. A shell around
/// <see cref="Panels.Panels.DrgFromVelocitiesPanel"/>, which holds the editor and does the saving.
/// </summary>
public partial class ApproximateDrgFromVelocitiesDialog : Window
{
    public ApproximateDrgFromVelocitiesDialog(MeasurementSystem system, IFileDialogService fileDialogService,
                                              Ammunition? prefill = null, Atmosphere? atmosphere = null)
    {
        InitializeComponent();

        EditorPanel.MeasurementSystem = system;
        EditorPanel.FileDialogService = fileDialogService;
        EditorPanel.Prefill = prefill;

        // The readings were measured in some air; default to the active shot's conditions when known.
        if (atmosphere != null)
            EditorPanel.Atmosphere = atmosphere;
    }

    private void OnClose(object? sender, RoutedEventArgs e) => Close(true);
}
