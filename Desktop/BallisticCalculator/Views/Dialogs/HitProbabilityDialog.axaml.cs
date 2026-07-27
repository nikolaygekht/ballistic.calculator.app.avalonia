using Avalonia.Controls;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Views.Dialogs;

/// <summary>
/// Tools → Hit Probability. A shell around <see cref="Panels.Panels.HitProbabilityPanel"/>, which does the
/// estimating. Unlike the other Tools windows this one is about a specific shot, so the title names it.
/// </summary>
public partial class HitProbabilityDialog : Window
{
    public HitProbabilityDialog(MeasurementSystem system, AngularUnit angularUnits, ShotData shotData,
                                string? shotName = null)
    {
        InitializeComponent();

        EstimatorPanel.MeasurementSystem = system;
        EstimatorPanel.AngularUnits = angularUnits;
        EstimatorPanel.ShotData = shotData;

        if (!string.IsNullOrWhiteSpace(shotName))
            Title = $"Hit Probability — {shotName}";

        EstimatorPanel.CloseRequested += (_, _) => Close(true);
    }
}
