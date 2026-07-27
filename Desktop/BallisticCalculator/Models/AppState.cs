using System.Collections.Generic;
using BallisticCalculator.Types;

namespace BallisticCalculator.Models;

public class AppState
{
    /// <summary>
    /// The units of the last trajectory the user created. The standalone Tools dialogs open in this
    /// system, since they belong to no trajectory — see <c>claude/units.md</c>. Imperial until the user
    /// creates their first trajectory; deliberately not changed by opening a file or by
    /// View → Measurement System.
    /// </summary>
    public MeasurementSystem LastMeasurementSystem { get; set; } = MeasurementSystem.Imperial;

    public double MainWindowWidth { get; set; } = 900;
    public double MainWindowHeight { get; set; } = 650;
    public double MainWindowX { get; set; } = 100;
    public double MainWindowY { get; set; } = 100;
    public bool MainWindowIsMaximized { get; set; }

    public double ChildWindowWidth { get; set; } = 400;
    public double ChildWindowHeight { get; set; } = 300;

    public double ShotDialogWidth { get; set; }
    public double ShotDialogHeight { get; set; }

    public double[]? TableColumnWidths { get; set; }
}
