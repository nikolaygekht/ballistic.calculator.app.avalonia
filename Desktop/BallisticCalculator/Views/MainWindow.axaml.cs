using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using BallisticCalculator.Models;
using BallisticCalculator.Serialization;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using BallisticCalculator.Services;
using BallisticCalculator.Utilities;
using BallisticCalculator.Views.Dialogs;
using Iciclecreek.Avalonia.WindowManager;

namespace BallisticCalculator.Views;

public partial class MainWindow : Window
{
    private IAppChildWindow? _activeChild;
    private int _windowCounter;
    private int _newWindowSlot;
    private readonly List<ManagedWindow> _managedWindows = new();
    private readonly AppState _appState;
    private const int WindowOffset = 30;
    private const int MaxNewWindowSlots = 10;

    public MainWindow()
    {
        InitializeComponent();
        _appState = AppStateManager.Load();
        _fileDialogService = new FileDialogService(this);
        RestoreMainWindowState();
        SetupMenuHandlers();
        KeyDown += OnKeyDown;
        Closing += (_, _) => SaveState();
    }

    private void RestoreMainWindowState()
    {
        Width = _appState.MainWindowWidth;
        Height = _appState.MainWindowHeight;
        Position = new PixelPoint((int)_appState.MainWindowX, (int)_appState.MainWindowY);
        WindowState = _appState.MainWindowIsMaximized ? WindowState.Maximized : WindowState.Normal;
    }

    private void SaveState()
    {
        if (WindowState == WindowState.Normal)
        {
            _appState.MainWindowWidth = Width;
            _appState.MainWindowHeight = Height;
            _appState.MainWindowX = Position.X;
            _appState.MainWindowY = Position.Y;
        }
        _appState.MainWindowIsMaximized = WindowState == WindowState.Maximized;
        AppStateManager.Save(_appState);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var alt = e.KeyModifiers.HasFlag(KeyModifiers.Alt);

        if (!ctrl) return;

        if (!shift && !alt)
        {
            switch (e.Key)
            {
                case Key.I: OpenNewTrajectory(MeasurementSystem.Imperial); e.Handled = true; break;
                case Key.M: OpenNewTrajectory(MeasurementSystem.Metric); e.Handled = true; break;
                case Key.O: MenuFileOpen.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); e.Handled = true; break;
                case Key.S: if (MenuFileSave.IsEnabled) MenuFileSave.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); e.Handled = true; break;
                case Key.E: if (MenuViewEditParameters.IsEnabled) EditParameters(); e.Handled = true; break;
                case Key.X: Close(); e.Handled = true; break;
                case Key.T: if (MenuViewShowTable.IsEnabled) (_activeChild as ITrajectoryChildWindow)?.ShowTable(); e.Handled = true; break;
                case Key.C: if (MenuViewShowChart.IsEnabled) (_activeChild as ITrajectoryChildWindow)?.ShowChart(); e.Handled = true; break;
                case Key.R: if (MenuViewShowReticle.IsEnabled) (_activeChild as ITrajectoryChildWindow)?.ShowReticle(); e.Handled = true; break;
                case Key.F1: MenuHelpAbout.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); e.Handled = true; break;
            }
        }
        else if (shift && !alt)
        {
            switch (e.Key)
            {
                case Key.I: if (_activeChild != null) SetAndUpdate(w => w.MeasurementSystem = MeasurementSystem.Imperial); e.Handled = true; break;
                case Key.M: if (_activeChild != null) SetAndUpdate(w => w.MeasurementSystem = MeasurementSystem.Metric); e.Handled = true; break;
                case Key.Z: if (_activeChild is ITrajectoryChildWindow t) t.ZoomYToVisibleRange(); e.Handled = true; break;
            }
        }
        else if (alt && !shift)
        {
            switch (e.Key)
            {
                case Key.A: if (_activeChild != null) SetAndUpdate(w => w.AngularUnits = AngularUnit.MOA); e.Handled = true; break;
                case Key.M: if (_activeChild != null) SetAndUpdate(w => w.AngularUnits = AngularUnit.Mil); e.Handled = true; break;
                case Key.T: if (_activeChild != null) SetAndUpdate(w => w.AngularUnits = AngularUnit.Thousand); e.Handled = true; break;
                case Key.R: if (_activeChild != null) SetAndUpdate(w => w.AngularUnits = AngularUnit.MRad); e.Handled = true; break;
                case Key.V: if (_activeChild != null) SetAndUpdate(w => w.ChartMode = TrajectoryChartMode.Velocity); e.Handled = true; break;
                case Key.D: if (_activeChild != null) SetAndUpdate(w => w.ChartMode = TrajectoryChartMode.Drop); e.Handled = true; break;
                case Key.W: if (_activeChild != null) SetAndUpdate(w => w.ChartMode = TrajectoryChartMode.Windage); e.Handled = true; break;
            }
        }
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

    private async void OpenNewTrajectory(MeasurementSystem system)
    {
        var dialog = new ShotParametersDialog(system);
        var result = await dialog.ShowDialog<bool?>(this);
        if (result != true || dialog.Result == null)
            return;

        _windowCounter++;
        var shotData = dialog.Result;
        var trajectory = ShotCalculator.Calculate(shotData, system);

        var view = new TrajectoryView
        {
            FileDialogService = _fileDialogService,
            MeasurementSystem = system,
            ShotData = shotData,
            Trajectory = trajectory,
        };

        if (_appState.TableColumnWidths != null)
            view.SetColumnWidths(_appState.TableColumnWidths);

        var title = shotData.Ammunition?.Name ?? $"Trajectory {_windowCounter}";
        AddChildWindow(view, title);
    }

    private void AddChildWindow(IAppChildWindow content, string title)
    {
        var window = new ManagedWindow
        {
            Title = title,
            Content = content as Avalonia.Controls.Control,
            Width = _appState.ChildWindowWidth,
            Height = _appState.ChildWindowHeight,
            SizeToContent = SizeToContent.Manual,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Position = new PixelPoint(WindowOffset * _newWindowSlot, WindowOffset * _newWindowSlot),
        };

        window.Activated += (_, _) => ActiveChild = window.Content as IAppChildWindow;
        window.Deactivated += (_, _) =>
        {
            if (_activeChild == window.Content as IAppChildWindow)
                ActiveChild = null;
        };
        window.Closed += (_, _) =>
        {
            _appState.ChildWindowWidth = window.Width;
            _appState.ChildWindowHeight = window.Height;
            if (window.Content is TrajectoryView tv)
                _appState.TableColumnWidths = tv.GetColumnWidths();
            _managedWindows.Remove(window);
            UpdateWindowsMenu();
            if (_activeChild == window.Content as IAppChildWindow)
                ActiveChild = null;
        };

        _managedWindows.Add(window);
        _newWindowSlot = (_newWindowSlot + 1) % MaxNewWindowSlots;
        UpdateWindowsMenu();
        WindowsPanel.Show(window);
    }

    private async void EditParameters()
    {
        if (_activeChild is not ITrajectoryChildWindow trajectoryChild)
            return;

        var dialog = new ShotParametersDialog(trajectoryChild.MeasurementSystem, trajectoryChild.ShotData);
        var result = await dialog.ShowDialog<bool?>(this);
        if (result != true || dialog.Result == null)
            return;

        trajectoryChild.ShotData = dialog.Result;
        trajectoryChild.Trajectory = ShotCalculator.Calculate(dialog.Result, trajectoryChild.MeasurementSystem);

        // Update the ManagedWindow title
        var title = dialog.Result.Ammunition?.Name ?? "Trajectory";
        var managedWindow = _managedWindows.FirstOrDefault(w => w.Content == trajectoryChild);
        if (managedWindow != null)
        {
            managedWindow.Title = title;
            UpdateWindowsMenu();
        }
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

        var panelWidth = WindowsPanel.Bounds.Width;
        var panelHeight = WindowsPanel.Bounds.Height;

        var totalOffset = _managedWindows.Count - 1;
        var windowWidth = Math.Max(200, panelWidth - WindowOffset * totalOffset);
        var windowHeight = Math.Max(150, panelHeight - WindowOffset * totalOffset);

        for (var i = 0; i < _managedWindows.Count; i++)
        {
            var w = _managedWindows[i];
            w.WindowState = WindowState.Normal;
            w.Width = windowWidth;
            w.Height = windowHeight;
            w.Position = new PixelPoint(WindowOffset * i, WindowOffset * i);
        }

        _managedWindows[^1].Activate();
    }

    private void SetupMenuHandlers()
    {
        // File menu
        MenuFileNewImperial.Click += (_, _) => OpenNewTrajectory(MeasurementSystem.Imperial);
        MenuFileNewMetric.Click += (_, _) => OpenNewTrajectory(MeasurementSystem.Metric);
        MenuFileOpen.Click += async (_, _) => await Open();
        MenuFileSave.Click += async (_, _) => await Save();
        MenuFileSaveAs.Click += async (_, _) => await SaveAs();
        MenuFileExportCsvLocal.Click += async (_, _) => await ExportCsv(useLocalCulture: true);
        MenuFileExportCsvInvariant.Click += async (_, _) => await ExportCsv(useLocalCulture: false);
        MenuFileExit.Click += (_, _) => Close();

        // View menu
        MenuViewEditParameters.Click += (_, _) => EditParameters();

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

    #region File I/O

    private readonly FileDialogService _fileDialogService;

    private async Task Open()
    {
        var path = await _fileDialogService.OpenFileAsync(new Panels.Services.FileDialogOptions
        {
            Title = "Open Trajectory",
            DefaultExtension = ".trajectory",
            Filters = { new Panels.Services.FileDialogFilter("Ballistic Calculator files", "trajectory") },
        });

        if (path == null)
            return;

        try
        {
            var document = new XmlDocument();
            document.Load(path);
            var serializer = new BallisticXmlDeserializer();
            var data = serializer.Deserialize<TrajectoryFormState>(document.DocumentElement!);

            var shotData = data.ShotData?.ToShotData();
            if (shotData == null)
                return;

            var system = data.MeasurementSystem;
            var trajectory = ShotCalculator.Calculate(shotData, system);

            _windowCounter++;
            var view = new TrajectoryView
            {
                FileDialogService = new FileDialogService(this),
                MeasurementSystem = system,
                AngularUnits = data.AngularUnits,
                ShotData = shotData,
                Trajectory = trajectory,
                FileName = path,
            };

            if (data.ChartMode.HasValue)
                view.ChartMode = data.ChartMode.Value;

            if (_appState.TableColumnWidths != null)
                view.SetColumnWidths(_appState.TableColumnWidths);

            var title = shotData.Ammunition?.Name ?? Path.GetFileNameWithoutExtension(path);
            AddChildWindow(view, title);
        }
        catch (Exception ex)
        {
            // Show error in a simple way — no MessageBox in Avalonia without TopLevel
            Console.Error.WriteLine($"Error opening file: {ex.Message}");
        }
    }

    private async Task Save()
    {
        if (_activeChild is not ITrajectoryChildWindow trajectoryChild)
            return;

        if (string.IsNullOrEmpty(trajectoryChild.FileName))
        {
            await SaveAs();
            return;
        }

        DoSave(trajectoryChild);
    }

    private async Task SaveAs()
    {
        if (_activeChild is not ITrajectoryChildWindow trajectoryChild)
            return;

        var path = await _fileDialogService.SaveFileAsync(new Panels.Services.FileDialogOptions
        {
            Title = "Save Trajectory",
            DefaultExtension = ".trajectory",
            InitialFileName = trajectoryChild.FileName,
            Filters = { new Panels.Services.FileDialogFilter("Ballistic Calculator files", "trajectory") },
        });

        if (path == null)
            return;

        trajectoryChild.FileName = path;
        DoSave(trajectoryChild);

        // Update title with file name
        var managedWindow = _managedWindows.FirstOrDefault(w => w.Content == trajectoryChild);
        if (managedWindow != null)
        {
            managedWindow.Title = Path.GetFileNameWithoutExtension(path);
            UpdateWindowsMenu();
        }
    }

    private void DoSave(ITrajectoryChildWindow child)
    {
        if (child.ShotData == null || string.IsNullOrEmpty(child.FileName))
            return;

        try
        {
            var state = new TrajectoryFormState
            {
                ShotData = TrajectoryFormShotData.FromShotData(child.ShotData),
                MeasurementSystem = child.MeasurementSystem,
                AngularUnits = child.AngularUnits,
                ChartMode = child.ChartMode,
            };

            var serializer = new BallisticXmlSerializer();
            var root = serializer.Serialize(state);
            var document = root.OwnerDocument!;
            document.AppendChild(root);
            document.Save(child.FileName);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error saving file: {ex.Message}");
        }
    }

    private async Task ExportCsv(bool useLocalCulture)
    {
        if (_activeChild is not ITrajectoryChildWindow trajectoryChild)
            return;

        if (trajectoryChild.Trajectory == null)
            return;

        var path = await _fileDialogService.SaveFileAsync(new Panels.Services.FileDialogOptions
        {
            Title = "Export CSV",
            DefaultExtension = ".csv",
            Filters = { new Panels.Services.FileDialogFilter("CSV files", "csv") },
        });

        if (path == null)
            return;

        try
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            using var writer = new StreamWriter(fs);
            var controller = new CsvExportController(
                trajectoryChild.Trajectory,
                trajectoryChild.MeasurementSystem,
                trajectoryChild.ShotData?.Weapon?.Sight,
                trajectoryChild.AngularUnits,
                useLocalCulture);

            foreach (var line in controller.Prepare())
                writer.WriteLine(line);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error exporting CSV: {ex.Message}");
        }
    }

    #endregion
}
