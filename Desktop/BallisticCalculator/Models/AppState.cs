using System.Collections.Generic;

namespace BallisticCalculator.Models;

public class AppState
{
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
