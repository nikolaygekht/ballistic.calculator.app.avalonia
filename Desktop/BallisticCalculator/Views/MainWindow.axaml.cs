using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using Iciclecreek.Avalonia.WindowManager;

namespace BallisticCalculator.Views;

public partial class MainWindow : Window
{
    private IAppChildWindow? _activeChild;
    private int _windowCounter;
    private readonly List<ManagedWindow> _managedWindows = new();

    public MainWindow()
    {
        InitializeComponent();
        SetupMenuHandlers();
    }

    internal IAppChildWindow? ActiveChild
    {
        get => _activeChild;
        set
        {
            _activeChild = value;
            UpdateMenus();
        }
    }

    private void OpenNewTrajectory(MeasurementSystem system)
    {
        _windowCounter++;
        var view = new TestTrajectoryView
        {
            MeasurementSystem = system,
        };
        view.UpdateDisplay();

        var window = new ManagedWindow
        {
            Title = $"Trajectory {_windowCounter} ({system})",
            Content = view,
            Width = 400,
            Height = 300,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
        };

        window.Activated += (_, _) => ActiveChild = window.Content as IAppChildWindow;
        window.Deactivated += (_, _) =>
        {
            if (_activeChild == window.Content as IAppChildWindow)
                ActiveChild = null;
        };
        window.Closed += (_, _) =>
        {
            _managedWindows.Remove(window);
            UpdateWindowsMenu();
            if (_activeChild == window.Content as IAppChildWindow)
                ActiveChild = null;
        };

        _managedWindows.Add(window);
        UpdateWindowsMenu();
        WindowsPanel.Show(window);
    }

    private void UpdateWindowsMenu()
    {
        // Remove old dynamic window items (everything after the separator)
        var separatorIndex = MenuWindows.Items.IndexOf(MenuWindowsSeparator);
        while (MenuWindows.Items.Count > separatorIndex + 1)
            MenuWindows.Items.RemoveAt(MenuWindows.Items.Count - 1);

        MenuWindowsSeparator.IsVisible = _managedWindows.Count > 0;

        for (var i = 0; i < _managedWindows.Count; i++)
        {
            var w = _managedWindows[i];
            var item = new MenuItem { Header = $"_{i + 1} {w.Title}" };
            item.Click += (_, _) => w.Activate();
            MenuWindows.Items.Add(item);
        }
    }

    private void UpdateWindowsMenuCheckmarks()
    {
        var separatorIndex = MenuWindows.Items.IndexOf(MenuWindowsSeparator);
        for (var i = separatorIndex + 1; i < MenuWindows.Items.Count; i++)
        {
            if (MenuWindows.Items[i] is MenuItem item)
            {
                var windowIndex = i - separatorIndex - 1;
                var isActive = windowIndex < _managedWindows.Count
                    && _managedWindows[windowIndex].Content as IAppChildWindow == _activeChild
                    && _activeChild != null;
                SetChecked(item, isActive);
            }
        }
    }

    private void CascadeWindows()
    {
        if (_managedWindows.Count == 0)
            return;

        const int offsetX = 30;
        const int offsetY = 30;

        var panelWidth = WindowsPanel.Bounds.Width;
        var panelHeight = WindowsPanel.Bounds.Height;

        // The last window's origin is at (offset * (count-1), offset * (count-1))
        // and it must fit within the panel, so:
        var totalOffset = (_managedWindows.Count - 1);
        var windowWidth = Math.Max(200, panelWidth - offsetX * totalOffset);
        var windowHeight = Math.Max(150, panelHeight - offsetY * totalOffset);

        for (var i = 0; i < _managedWindows.Count; i++)
        {
            var w = _managedWindows[i];
            w.WindowState = WindowState.Normal;
            w.Width = windowWidth;
            w.Height = windowHeight;
            w.Position = new PixelPoint(offsetX * i, offsetY * i);
        }

        _managedWindows[^1].Activate();
    }

    private void SetupMenuHandlers()
    {
        // File menu
        MenuFileNewImperial.Click += (_, _) => OpenNewTrajectory(MeasurementSystem.Imperial);
        MenuFileNewMetric.Click += (_, _) => OpenNewTrajectory(MeasurementSystem.Metric);
        MenuFileOpen.Click += (_, _) => { /* TODO: Open() */ };
        MenuFileSave.Click += (_, _) => { /* TODO: Save() */ };
        MenuFileSaveAs.Click += (_, _) => { /* TODO: SaveAs() */ };
        MenuFileExportCsv.Click += (_, _) => { /* TODO: ExportCsv() */ };
        MenuFileExit.Click += (_, _) => Close();

        // View menu
        MenuViewEditParameters.Click += (_, _) => { /* TODO: EditParams() */ };

        // Measurement system
        MenuViewSystemImperial.Click += (_, _) => SetAndUpdate(w => w.MeasurementSystem = MeasurementSystem.Imperial);
        MenuViewSystemMetric.Click += (_, _) => SetAndUpdate(w => w.MeasurementSystem = MeasurementSystem.Metric);

        // Angular units
        MenuViewAngularMOA.Click += (_, _) => SetAndUpdate(w => w.AngularUnits = AngularUnit.MOA);
        MenuViewAngularMils.Click += (_, _) => SetAndUpdate(w => w.AngularUnits = AngularUnit.Mil);
        MenuViewAngularThousands.Click += (_, _) => SetAndUpdate(w => w.AngularUnits = AngularUnit.Thousand);
        MenuViewAngularMRads.Click += (_, _) => SetAndUpdate(w => w.AngularUnits = AngularUnit.MRad);
        MenuViewAngularInches.Click += (_, _) => SetAndUpdate(w => w.AngularUnits = AngularUnit.InchesPer100Yards);
        MenuViewAngularCentimeters.Click += (_, _) => SetAndUpdate(w => w.AngularUnits = AngularUnit.CmPer100Meters);

        // Drop base
        MenuViewDropLineOfSight.Click += (_, _) => SetAndUpdate(w => w.DropBase = DropBase.SightLine);
        MenuViewDropMuzzleLevel.Click += (_, _) => SetAndUpdate(w => w.DropBase = DropBase.MuzzlePoint);

        // Chart mode
        MenuViewChartVelocity.Click += (_, _) => SetAndUpdate(w => w.ChartMode = TrajectoryChartMode.Velocity);
        MenuViewChartMach.Click += (_, _) => SetAndUpdate(w => w.ChartMode = TrajectoryChartMode.Mach);
        MenuViewChartDrop.Click += (_, _) => SetAndUpdate(w => w.ChartMode = TrajectoryChartMode.Drop);
        MenuViewChartWindage.Click += (_, _) => SetAndUpdate(w => w.ChartMode = TrajectoryChartMode.Windage);
        MenuViewChartEnergy.Click += (_, _) => SetAndUpdate(w => w.ChartMode = TrajectoryChartMode.Energy);
        MenuViewChartZoomY.Click += (_, _) => (_activeChild as ITrajectoryChildWindow)?.ZoomYToVisibleRange();

        // Show
        MenuViewShowTable.Click += (_, _) => (_activeChild as ITrajectoryChildWindow)?.ShowTable();
        MenuViewShowChart.Click += (_, _) => (_activeChild as ITrajectoryChildWindow)?.ShowChart();
        MenuViewShowReticle.Click += (_, _) => (_activeChild as ITrajectoryChildWindow)?.ShowReticle();

        // Compare
        MenuViewCompareAdd.Click += (_, _) => { /* TODO: AddToCompare() */ };
        MenuViewCompareRemoveLast.Click += (_, _) => (_activeChild as IComparisonChartChildWindow)?.RemoveLastTrajectory();

        // Windows
        MenuWindowsCascade.Click += (_, _) => CascadeWindows();

        // Help
        MenuHelpAbout.Click += (_, _) => { /* TODO: show About dialog */ };
    }

    private void SetAndUpdate(Action<IAppChildWindow> action)
    {
        if (_activeChild != null)
        {
            action(_activeChild);
            UpdateMenus();
        }
    }

    private void UpdateMenus()
    {
        var isTrajectory = _activeChild is ITrajectoryChildWindow;
        var isComparison = _activeChild is IComparisonChartChildWindow;
        var isAnyChild = _activeChild != null;

        // File menu
        MenuFileSave.IsEnabled = isTrajectory;
        MenuFileSaveAs.IsEnabled = isTrajectory;
        MenuFileExportCsv.IsEnabled = isTrajectory;

        // Edit parameters
        MenuViewEditParameters.IsEnabled = isTrajectory;

        // Measurement system
        MenuViewSystemImperial.IsEnabled = isAnyChild;
        MenuViewSystemMetric.IsEnabled = isAnyChild;

        // Angular units
        MenuViewAngularMOA.IsEnabled = isAnyChild;
        MenuViewAngularMils.IsEnabled = isAnyChild;
        MenuViewAngularThousands.IsEnabled = isAnyChild;
        MenuViewAngularMRads.IsEnabled = isAnyChild;
        MenuViewAngularInches.IsEnabled = isAnyChild;
        MenuViewAngularCentimeters.IsEnabled = isAnyChild;

        // Drop base
        MenuViewDropLineOfSight.IsEnabled = isAnyChild;
        MenuViewDropMuzzleLevel.IsEnabled = isAnyChild;

        // Chart mode
        MenuViewChartVelocity.IsEnabled = isAnyChild;
        MenuViewChartMach.IsEnabled = isAnyChild;
        MenuViewChartDrop.IsEnabled = isAnyChild;
        MenuViewChartWindage.IsEnabled = isAnyChild;
        MenuViewChartEnergy.IsEnabled = isAnyChild;
        MenuViewChartZoomY.IsEnabled = isAnyChild;

        // Show (trajectory only)
        MenuViewShowTable.IsEnabled = isTrajectory;
        MenuViewShowChart.IsEnabled = isTrajectory;
        MenuViewShowReticle.IsEnabled = isTrajectory;

        // Compare
        MenuViewCompareAdd.IsEnabled = isTrajectory;
        MenuViewCompareRemoveLast.IsEnabled = isComparison;

        // Windows
        MenuWindowsCascade.IsEnabled = _managedWindows.Count > 0;

        // Checkmarks
        UpdateCheckmarks();
        UpdateWindowsMenuCheckmarks();
    }

    private void UpdateCheckmarks()
    {
        if (_activeChild == null)
        {
            ClearCheckmarks();
            return;
        }

        var w = _activeChild;

        // Measurement system
        SetChecked(MenuViewSystemImperial, w.MeasurementSystem == MeasurementSystem.Imperial);
        SetChecked(MenuViewSystemMetric, w.MeasurementSystem == MeasurementSystem.Metric);

        // Angular units
        SetChecked(MenuViewAngularMOA, w.AngularUnits == AngularUnit.MOA);
        SetChecked(MenuViewAngularMils, w.AngularUnits == AngularUnit.Mil);
        SetChecked(MenuViewAngularThousands, w.AngularUnits == AngularUnit.Thousand);
        SetChecked(MenuViewAngularMRads, w.AngularUnits == AngularUnit.MRad);
        SetChecked(MenuViewAngularInches, w.AngularUnits == AngularUnit.InchesPer100Yards);
        SetChecked(MenuViewAngularCentimeters, w.AngularUnits == AngularUnit.CmPer100Meters);

        // Drop base
        SetChecked(MenuViewDropLineOfSight, w.DropBase == DropBase.SightLine);
        SetChecked(MenuViewDropMuzzleLevel, w.DropBase == DropBase.MuzzlePoint);

        // Chart mode
        SetChecked(MenuViewChartVelocity, w.ChartMode == TrajectoryChartMode.Velocity);
        SetChecked(MenuViewChartMach, w.ChartMode == TrajectoryChartMode.Mach);
        SetChecked(MenuViewChartDrop, w.ChartMode == TrajectoryChartMode.Drop);
        SetChecked(MenuViewChartWindage, w.ChartMode == TrajectoryChartMode.Windage);
        SetChecked(MenuViewChartEnergy, w.ChartMode == TrajectoryChartMode.Energy);
    }

    private void ClearCheckmarks()
    {
        SetChecked(MenuViewSystemImperial, false);
        SetChecked(MenuViewSystemMetric, false);

        SetChecked(MenuViewAngularMOA, false);
        SetChecked(MenuViewAngularMils, false);
        SetChecked(MenuViewAngularThousands, false);
        SetChecked(MenuViewAngularMRads, false);
        SetChecked(MenuViewAngularInches, false);
        SetChecked(MenuViewAngularCentimeters, false);

        SetChecked(MenuViewDropLineOfSight, false);
        SetChecked(MenuViewDropMuzzleLevel, false);

        SetChecked(MenuViewChartVelocity, false);
        SetChecked(MenuViewChartMach, false);
        SetChecked(MenuViewChartDrop, false);
        SetChecked(MenuViewChartWindage, false);
        SetChecked(MenuViewChartEnergy, false);
    }

    private static void SetChecked(MenuItem item, bool isChecked)
    {
        item.Icon = isChecked ? "\u2713" : null;
    }
}
