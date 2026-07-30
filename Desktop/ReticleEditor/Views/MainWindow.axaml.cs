using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia;
using BallisticCalculator.Reticle.Data;
using BallisticCalculator.Serialization;
using Gehtsoft.Measurements;
using ReticleEditor.Utilities;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ReticleEditor.Views;

public partial class MainWindow : Window
{
    private const double MinFontSize = 10;
    private const double MaxFontSize = 20;
    // On by default. Highlighting the selected element is how you tell which of forty lines you have
    // selected, so it is not a preference to opt into; the menu item is there to turn it off while
    // looking at the reticle as it will actually appear.
    private bool _highlightCurrentItem = true;
    private ReticleDefinition? _currentReticle = null;
    private string? _currentFileName = null;
    private Models.CombinedElementsView? _elementsView = null;
    private AngularUnit _coordinateDisplayUnit = AngularUnit.Mil;

    private const string BaseTitle = "Reticle Editor";

    private bool _isDirty;
    private bool _closeConfirmed;
    private bool _promptOpen;

    /// <summary>
    /// The parameter fields as they stood when the document was last clean. Filling those controls makes
    /// them raise Changed — partly synchronously, partly posted to the dispatcher — so the echo of a load
    /// is told from a real edit by comparing content rather than by timing.
    /// </summary>
    private string _cleanFields = "";

    public MainWindow()
    {
        InitializeComponent();

        UnsavedChangesPrompt = ShowUnsavedChangesPromptAsync;

        // Initialize measurement controls with AngularUnit type and default to Mil
        ReticleWidth.UnitType = typeof(Gehtsoft.Measurements.AngularUnit);
        ReticleHeight.UnitType = typeof(Gehtsoft.Measurements.AngularUnit);
        ZeroOffsetX.UnitType = typeof(Gehtsoft.Measurements.AngularUnit);
        ZeroOffsetY.UnitType = typeof(Gehtsoft.Measurements.AngularUnit);

        // Set default unit to Mil
        ReticleWidth.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));
        ReticleHeight.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));
        ZeroOffsetX.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));
        ZeroOffsetY.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));

        // Create empty reticle by default
        _currentReticle = new ReticleDefinition();
        UpdateElementsList();
        ShowHighlightState();

        // Hook up selection changed event
        ReticleItems.SelectionChanged += OnReticleItemsSelectionChanged;

        // Initialize coordinate unit menu checkmarks (default to Mil)
        UpdateCoordinateUnitMenuCheckmarks();

        // Initialize button states
        UpdateElementButtonStates();

        // Load and apply saved window state
        LoadWindowState();

        // Save state when closing — and, since 2026-07-29, guard the document first
        Closing += OnWindowClosing;

        // Watch the parameter fields: a typed-but-uncommitted name or size is still work to lose.
        WireDirtyTracking();
        MarkClean();
    }

    #region Unsaved changes (finding F-6)

    /// <summary>True when the document holds changes that are not in a file.</summary>
    internal bool IsDirty => _isDirty;

    /// <summary>
    /// Asks the user what to do about unsaved changes. A property so tests can answer it — the production
    /// implementation is a modal window, which a headless run cannot dismiss.
    /// </summary>
    internal Func<Task<UnsavedChangesChoice>> UnsavedChangesPrompt { get; set; }

    /// <summary>Replaces the Save As file picker in tests (a picker cannot be answered headless).</summary>
    internal Func<Task>? SaveAsOverride { get; set; }

    private void WireDirtyTracking()
    {
        // The property change rather than TextChanged: the routed event does not reach a control that is
        // not in a visual tree, which makes it untestable and is a fragile thing to depend on anyway.
        ReticleName.PropertyChanged += (_, e) =>
        {
            if (e.Property == TextBox.TextProperty)
                OnParameterFieldChanged();
        };

        ReticleWidth.Changed += (_, _) => OnParameterFieldChanged();
        ReticleHeight.Changed += (_, _) => OnParameterFieldChanged();
        ZeroOffsetX.Changed += (_, _) => OnParameterFieldChanged();
        ZeroOffsetY.Changed += (_, _) => OnParameterFieldChanged();
    }

    /// <summary>
    /// A parameter field changed. Only this path needs the content check: a load fills these controls and
    /// their notifications must not read as the user typing. Everything else that dirties the document is a
    /// direct call from a command, which can never be an echo.
    /// </summary>
    private void OnParameterFieldChanged()
    {
        if (FieldSnapshot() != _cleanFields)
            MarkDirty();
    }

    /// <summary>The parameter fields as one comparable string.</summary>
    private string FieldSnapshot() =>
        string.Join("|",
            ReticleName.Text,
            ReticleWidth.GetValue<AngularUnit>()?.ToString() ?? "",
            ReticleHeight.GetValue<AngularUnit>()?.ToString() ?? "",
            ZeroOffsetX.GetValue<AngularUnit>()?.ToString() ?? "",
            ZeroOffsetY.GetValue<AngularUnit>()?.ToString() ?? "");

    private void MarkDirty()
    {
        if (_isDirty)
            return;

        _isDirty = true;
        UpdateTitle();
    }

    private void MarkClean()
    {
        _isDirty = false;
        _cleanFields = FieldSnapshot();
        UpdateTitle();
    }

    /// <summary>The title names the file and marks unsaved changes with an asterisk.</summary>
    private void UpdateTitle()
    {
        var name = string.IsNullOrEmpty(_currentFileName)
            ? "(unsaved)"
            : System.IO.Path.GetFileName(_currentFileName);

        Title = $"{BaseTitle} — {name}{(_isDirty ? " *" : "")}";
    }

    /// <summary>
    /// True when it is safe to throw the current document away. Prompts only when there is something to
    /// lose; a Save that does not happen — a cancelled picker, a failed write — leaves the document dirty
    /// and therefore stops the operation, because proceeding would lose the work the user asked to keep.
    /// </summary>
    internal async Task<bool> ConfirmDiscardChangesAsync()
    {
        if (!_isDirty)
            return true;

        // One prompt at a time, whatever asks. Not a fix for anything observed — the prompt "vanishing and
        // coming back" on Linux under WSLg turned out to be that platform's modal handling (Windows shows
        // one dialog and merely flashes it when the owner is clicked) — but a guard that can open twice is
        // a guard whose answer is ambiguous, and key repeat or a double-clicked menu item would do it.
        if (_promptOpen)
            return false;

        _promptOpen = true;
        try
        {
            return await AskAndActAsync();
        }
        finally
        {
            _promptOpen = false;
        }
    }

    private async Task<bool> AskAndActAsync()
    {
        switch (await UnsavedChangesPrompt())
        {
            case UnsavedChangesChoice.Save:
                await SaveAsync();
                return !_isDirty;

            case UnsavedChangesChoice.Discard:
                return true;

            default:
                return false;
        }
    }

    /// <summary>The Save / Don't Save / Cancel prompt.</summary>
    private async Task<UnsavedChangesChoice> ShowUnsavedChangesPromptAsync()
    {
        var choice = UnsavedChangesChoice.Cancel;

        var name = string.IsNullOrEmpty(_currentFileName)
            ? "this reticle"
            : System.IO.Path.GetFileName(_currentFileName);

        var dialog = new Window
        {
            Title = "Unsaved changes",
            Width = 420,
            SizeToContent = SizeToContent.Height,
            MinHeight = 140,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
        };

        // MinWidth rather than Width: the labels are laid out in the app font, which the user can grow, and
        // a fixed 90 px clipped "Don't Save" as soon as they did.
        var saveButton = new Button { Content = "Save", MinWidth = 90, Padding = new Thickness(12, 4), IsDefault = true };
        var discardButton = new Button { Content = "Don't Save", MinWidth = 90, Padding = new Thickness(12, 4) };
        var cancelButton = new Button { Content = "Cancel", MinWidth = 90, Padding = new Thickness(12, 4), IsCancel = true };

        saveButton.Click += (_, _) => { choice = UnsavedChangesChoice.Save; dialog.Close(); };
        discardButton.Click += (_, _) => { choice = UnsavedChangesChoice.Discard; dialog.Close(); };
        cancelButton.Click += (_, _) => { choice = UnsavedChangesChoice.Cancel; dialog.Close(); };

        dialog.Content = new DockPanel
        {
            Children =
            {
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Spacing = 10,
                    Margin = new Thickness(0, 10),
                    [DockPanel.DockProperty] = Dock.Bottom,
                    Children = { saveButton, discardButton, cancelButton },
                },
                new TextBlock
                {
                    Text = $"{name} has changes that have not been saved.\n\nSave them?",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(15),
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                },
            },
        };

        await dialog.ShowDialog(this);
        return choice;
    }

    #endregion

    #region Window State Management

    private void LoadWindowState()
    {
        var state = WindowStateManager.Load();
        if (state != null)
        {
            // Apply window size and position
            state.ApplyToWindow(this);

            // Apply font size
            var app = Application.Current;
            if (app != null && state.FontSize >= MinFontSize && state.FontSize <= MaxFontSize)
            {
                app.Resources["AppFontSize"] = state.FontSize;
            }

            // Apply splitter position (right panel width)
            if (state.SplitterPosition > 200) // Minimum reasonable width
            {
                MainGrid.ColumnDefinitions[2].Width = new GridLength(state.SplitterPosition);
            }
        }
    }

    /// <summary>
    /// Guards the document before the window goes. The event cannot be awaited, so an unsaved document
    /// cancels the close, asks, and closes again once answered — <see cref="_closeConfirmed"/> is what
    /// stops the second pass from asking twice.
    /// </summary>
    private async void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!_closeConfirmed && _isDirty)
        {
            e.Cancel = true;

            if (await ConfirmDiscardChangesAsync())
            {
                _closeConfirmed = true;
                Close();
            }

            return;
        }

        SaveWindowState();
    }

    private void SaveWindowState()
    {
        var state = Models.WindowState.FromWindow(this);

        // Save current font size
        var app = Application.Current;
        if (app != null && app.Resources.TryGetResource("AppFontSize", null, out var fontSizeObj) &&
            fontSizeObj is double fontSize)
        {
            state.FontSize = fontSize;
        }

        // Save splitter position (right panel width)
        state.SplitterPosition = MainGrid.ColumnDefinitions[2].ActualWidth;

        WindowStateManager.Save(state);
    }

    #endregion

    #region Keyboard Shortcuts

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

        // File menu shortcuts
        if (ctrl && !shift && e.Key == Key.N)
        {
            OnFileNew(sender, new RoutedEventArgs());
            e.Handled = true;
        }
        else if (ctrl && !shift && e.Key == Key.O)
        {
            OnFileOpen(sender, new RoutedEventArgs());
            e.Handled = true;
        }
        else if (ctrl && !shift && e.Key == Key.S)
        {
            OnFileSave(sender, new RoutedEventArgs());
            e.Handled = true;
        }
        else if (ctrl && shift && e.Key == Key.S)
        {
            OnFileSaveAs(sender, new RoutedEventArgs());
            e.Handled = true;
        }
        // Font size shortcuts (both OemPlus/OemMinus and Add/Subtract for numpad)
        else if (ctrl && (e.Key == Key.OemPlus || e.Key == Key.Add))
        {
            OnFontIncrease(sender, new RoutedEventArgs());
            e.Handled = true;
        }
        else if (ctrl && (e.Key == Key.OemMinus || e.Key == Key.Subtract))
        {
            OnFontDecrease(sender, new RoutedEventArgs());
            e.Handled = true;
        }
    }

    #endregion

    #region Menu Handlers

    // File Menu
    private async void OnFileNew(object? sender, RoutedEventArgs e)
    {
        if (!await ConfirmDiscardChangesAsync())
        {
            StatusArea.Text = "New reticle cancelled";
            return;
        }

        // Create a new reticle with default values
        LoadReticle(new ReticleDefinition
        {
            Name = "New Reticle",
            Size = new ReticlePosition
            {
                X = new Measurement<AngularUnit>(10, AngularUnit.Mil),
                Y = new Measurement<AngularUnit>(10, AngularUnit.Mil)
            },
            Zero = new ReticlePosition
            {
                X = new Measurement<AngularUnit>(5, AngularUnit.Mil),
                Y = new Measurement<AngularUnit>(5, AngularUnit.Mil)
            }
        }, fileName: null);

        StatusArea.Text = "New reticle created";
    }

    /// <summary>
    /// Makes <paramref name="reticle"/> the document and refreshes everything that shows it. The document
    /// starts clean: what is on screen is what is in the file (or, for a new reticle, nothing worth
    /// keeping yet).
    /// </summary>
    internal void LoadReticle(ReticleDefinition? reticle, string? fileName)
    {
        _currentReticle = reticle;
        _currentFileName = fileName;

        UpdateReticleControls();
        UpdateElementsList();
        UpdateReticlePreview();
        UpdateElementButtonStates();

        // Last, so the snapshot it takes is of the loaded values: any later echo from those controls then
        // compares equal and does not dirty the document.
        MarkClean();
    }

    private async void OnFileOpen(object? sender, RoutedEventArgs e)
    {
        if (!await ConfirmDiscardChangesAsync())
        {
            StatusArea.Text = "Open cancelled";
            return;
        }

        var storageProvider = StorageProvider;
        if (storageProvider == null) return;

        var fileTypeFilter = new FilePickerFileType("Reticle Files")
        {
            Patterns = new[] { "*.reticle" },
            MimeTypes = new[] { "application/xml" }
        };

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Reticle File",
            AllowMultiple = false,
            FileTypeFilter = new[] { fileTypeFilter },
            SuggestedStartLocation = await GetReticleFolderAsync(),
        });

        if (files.Count > 0)
        {
            var path = files[0].TryGetLocalPath();
            if (path != null)
            {
                try
                {
                    // Load reticle from file using BallisticXmlDeserializer
                    LoadReticle(BallisticXmlDeserializer.ReadFromFile<ReticleDefinition>(path), path);

                    StatusArea.Text = $"Opened: {System.IO.Path.GetFileName(path)}";
                }
                catch (Exception ex)
                {
                    StatusArea.Text = $"Error loading file: {ex.Message}";
                }
            }
        }
    }

    private async void OnFileSave(object? sender, RoutedEventArgs e) => await SaveAsync();

    private async void OnFileSaveAs(object? sender, RoutedEventArgs e) => await SaveAsAsync();

    /// <summary>Saves to the current file, or falls back to Save As when there is no file name yet.</summary>
    private async Task SaveAsync()
    {
        if (_currentReticle == null)
            return;

        if (string.IsNullOrEmpty(_currentFileName))
        {
            await (SaveAsOverride?.Invoke() ?? SaveAsAsync());
            return;
        }

        SaveToFile(_currentFileName);
    }

    private async Task SaveAsAsync()
    {
        if (_currentReticle == null)
            return;

        var storageProvider = StorageProvider;
        if (storageProvider == null) return;

        var fileTypeFilter = new FilePickerFileType("Reticle Files")
        {
            Patterns = new[] { "*.reticle" },
            MimeTypes = new[] { "application/xml" }
        };

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Reticle File",
            FileTypeChoices = new[] { fileTypeFilter },
            SuggestedFileName = string.IsNullOrEmpty(_currentFileName)
                ? "reticle.reticle"
                : System.IO.Path.GetFileName(_currentFileName),
            SuggestedStartLocation = await GetReticleFolderAsync(),
        });

        if (file == null)
            return;

        var path = file.TryGetLocalPath();
        if (path == null)
        {
            StatusArea.Text = "Save failed: unsupported location.";
            return;
        }

        _currentFileName = path;
        SaveToFile(path);
    }

    /// <summary>The default reticle folder next to the executable (data/reticle), created if missing.</summary>
    private async Task<Avalonia.Platform.Storage.IStorageFolder?> GetReticleFolderAsync()
    {
        var provider = StorageProvider;
        if (provider == null)
            return null;
        try
        {
            var dir = BallisticCalculator.Types.DataFolders.Reticles;
            System.IO.Directory.CreateDirectory(dir);
            return await provider.TryGetFolderFromPathAsync(dir);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Writes the document. The dirty flag is cleared only on a successful write — a failed save leaves
    /// the document unsaved, which is what the close guard needs to know.
    /// </summary>
    internal void SaveToFile(string path)
    {
        if (_currentReticle == null)
            return;

        // Commit the current reticle parameter fields (name / size / zero) before writing.
        CommitReticleParameters();

        try
        {
            BallisticXmlSerializer.SerializeToFile(_currentReticle, path);
            StatusArea.Text = $"Saved: {System.IO.Path.GetFileName(path)}";
            MarkClean();
        }
        catch (System.Exception ex)
        {
            StatusArea.Text = $"Save failed: {ex.Message}";
        }
    }

    private void OnFileExit(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    // View Menu
    private void OnFontIncrease(object? sender, RoutedEventArgs e)
    {
        var app = Application.Current;
        if (app == null) return;

        if (app.Resources.TryGetResource("AppFontSize", null, out var currentSizeObj) &&
            currentSizeObj is double currentSize)
        {
            var newSize = Math.Min(currentSize + 1, MaxFontSize);
            app.Resources["AppFontSize"] = newSize;
            StatusArea.Text = $"Font size: {newSize}";
        }
    }

    private void OnFontDecrease(object? sender, RoutedEventArgs e)
    {
        var app = Application.Current;
        if (app == null) return;

        if (app.Resources.TryGetResource("AppFontSize", null, out var currentSizeObj) &&
            currentSizeObj is double currentSize)
        {
            var newSize = Math.Max(currentSize - 1, MinFontSize);
            app.Resources["AppFontSize"] = newSize;
            StatusArea.Text = $"Font size: {newSize}";
        }
    }

    /// <summary>Puts the tick on the menu item in step with the flag. Called at start-up too, or the
    /// menu would show no tick while highlighting was on.</summary>
    private void ShowHighlightState()
    {
        if (HighlightMenuItem != null)
            HighlightMenuItem.Icon = _highlightCurrentItem ? "\u2713" : null;
    }

    private void OnToggleHighlight(object? sender, RoutedEventArgs e)
    {
        _highlightCurrentItem = !_highlightCurrentItem;
        ShowHighlightState();
        StatusArea.Text = _highlightCurrentItem ? "Highlighting enabled" : "Highlighting disabled";

        // Update overlay to show/hide highlighting
        UpdateOverlay();
    }

    private void OnSelectUnitMil(object? sender, RoutedEventArgs e)
    {
        SetCoordinateDisplayUnit(AngularUnit.Mil);
    }

    private void OnSelectUnitMOA(object? sender, RoutedEventArgs e)
    {
        SetCoordinateDisplayUnit(AngularUnit.MOA);
    }

    private void OnSelectUnitInch100Yd(object? sender, RoutedEventArgs e)
    {
        SetCoordinateDisplayUnit(AngularUnit.InchesPer100Yards);
    }

    private void OnSelectUnitCm100M(object? sender, RoutedEventArgs e)
    {
        SetCoordinateDisplayUnit(AngularUnit.CmPer100Meters);
    }

    private void SetCoordinateDisplayUnit(AngularUnit unit)
    {
        _coordinateDisplayUnit = unit;
        UpdateCoordinateUnitMenuCheckmarks();

        // Get unit abbreviation for status message
        var unitNames = Measurement<AngularUnit>.GetUnitNames();
        var unitName = unitNames.FirstOrDefault(u => u.Item1.Equals(unit))?.Item2 ?? unit.ToString();
        StatusArea.Text = $"Coordinate display: {unitName}";
    }

    private void UpdateCoordinateUnitMenuCheckmarks()
    {
        MenuUnitMil.Icon = _coordinateDisplayUnit == AngularUnit.Mil ? "✓" : null;
        MenuUnitMOA.Icon = _coordinateDisplayUnit == AngularUnit.MOA ? "✓" : null;
        MenuUnitInch100Yd.Icon = _coordinateDisplayUnit == AngularUnit.InchesPer100Yards ? "✓" : null;
        MenuUnitCm100M.Icon = _coordinateDisplayUnit == AngularUnit.CmPer100Meters ? "✓" : null;
    }

    private void OnReticleItemsSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Update overlay when selection changes (for highlighting)
        UpdateOverlay();
        UpdateElementButtonStates();
    }

    // Help Menu
    private async void OnHelpAbout(object? sender, RoutedEventArgs e)
    {
        var aboutDialog = new Window
        {
            Title = "About Reticle Editor",
            Width = 300,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 10,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Reticle Editor",
                        FontSize = 16,
                        FontWeight = FontWeight.Bold
                    },
                    new TextBlock { Text = "Version 1.0" },
                    new TextBlock { Text = "Ballistic reticle design tool" },
                    new Button
                    {
                        Content = "OK",
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Margin = new Thickness(0, 10, 0, 0)
                    }
                }
            }
        };

        var okButton = (Button)((StackPanel)aboutDialog.Content).Children[3];
        okButton.Click += (s, e) => aboutDialog.Close();

        await aboutDialog.ShowDialog(this);
    }

    #endregion

    #region Canvas Event Handlers

    private void OnCanvasPointerMoved(object? sender, PointerEventArgs e)
    {
        // Get position relative to the ReticleCanvas control
        var point = e.GetPosition(ReticleCanvas);
        UpdateMouseCoordinates(point);
    }

    private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Get position relative to the ReticleCanvas control
        var point = e.GetPosition(ReticleCanvas);
        UpdateMouseCoordinates(point);
    }

    private void UpdateMouseCoordinates(Point pixelPosition)
    {
        // Try to convert to angular coordinates
        var angularPos = ReticleCanvas.PixelToAngular(pixelPosition);

        if (angularPos == null)
        {
            // No reticle or outside reticle area
            StatusX.Text = "--";
            StatusY.Text = "--";
            return;
        }

        // Convert to selected display unit
        var xInDisplayUnit = angularPos.X.To(_coordinateDisplayUnit);
        var yInDisplayUnit = angularPos.Y.To(_coordinateDisplayUnit);

        // Get unit abbreviation
        var unitNames = Measurement<AngularUnit>.GetUnitNames();
        var unitName = unitNames.FirstOrDefault(u => u.Item1.Equals(_coordinateDisplayUnit))?.Item2 ?? _coordinateDisplayUnit.ToString();

        // Display angular coordinates (relative to zero point)
        StatusX.Text = $"{xInDisplayUnit.In(_coordinateDisplayUnit):F3}{unitName}";
        StatusY.Text = $"{yInDisplayUnit.In(_coordinateDisplayUnit):F3}{unitName}";
    }

    #endregion

    #region Reticle Management

    private void OnSetReticleParameters(object? sender, RoutedEventArgs e) => SetReticleParameters();

    /// <summary>Commits the parameter fields into the document — what the <b>Set</b> button does.</summary>
    internal void SetReticleParameters()
    {
        CommitReticleParameters();
        MarkDirty();
        UpdateReticlePreview();
        UpdateElementButtonStates();
        StatusArea.Text = "Reticle parameters updated";
    }

    /// <summary>Copies the reticle parameter fields (name / size / zero) into the current reticle.</summary>
    private void CommitReticleParameters()
    {
        _currentReticle ??= new ReticleDefinition();

        // Update name
        _currentReticle.Name = ReticleName.Text;

        // Update size
        var width = ReticleWidth.GetValue<AngularUnit>();
        var height = ReticleHeight.GetValue<AngularUnit>();

        if (width.HasValue && height.HasValue)
        {
            _currentReticle.Size = new ReticlePosition
            {
                X = width.Value,
                Y = height.Value
            };
        }

        // Update zero offset
        var zeroX = ZeroOffsetX.GetValue<AngularUnit>();
        var zeroY = ZeroOffsetY.GetValue<AngularUnit>();

        if (zeroX.HasValue && zeroY.HasValue)
        {
            _currentReticle.Zero = new ReticlePosition
            {
                X = zeroX.Value,
                Y = zeroY.Value
            };
        }
    }

    private void UpdateReticleControls()
    {
        if (_currentReticle == null)
        {
            ReticleName.Text = "";
            ReticleWidth.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));
            ReticleHeight.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));
            ZeroOffsetX.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));
            ZeroOffsetY.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));

            // Set coordinate display unit to default (Mil) when no reticle
            SetCoordinateDisplayUnit(AngularUnit.Mil);
            return;
        }

        // Update name
        ReticleName.Text = _currentReticle.Name ?? "";

        // Update size - keep 0,0 if not set in loaded file
        if (_currentReticle.Size != null)
        {
            ReticleWidth.SetValue(_currentReticle.Size.X);
            ReticleHeight.SetValue(_currentReticle.Size.Y);

            // Set coordinate display unit to reticle's size unit
            SetCoordinateDisplayUnit(_currentReticle.Size.X.Unit);
        }
        else
        {
            ReticleWidth.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));
            ReticleHeight.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));

            // Set coordinate display unit to default (Mil) when size not set
            SetCoordinateDisplayUnit(AngularUnit.Mil);
        }

        // Update zero offset - keep 0,0 if not set in loaded file
        if (_currentReticle.Zero != null)
        {
            ZeroOffsetX.SetValue(_currentReticle.Zero.X);
            ZeroOffsetY.SetValue(_currentReticle.Zero.Y);
        }
        else
        {
            ZeroOffsetX.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));
            ZeroOffsetY.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));
        }
    }

    private void UpdateReticlePreview()
    {
        if (_currentReticle == null)
        {
            ReticleCanvas.Reticle = null;
            ReticleCanvas.Overlay = null;
            return;
        }

        // Set the reticle to the canvas for preview
        ReticleCanvas.Reticle = _currentReticle;

        // Update overlay (BDC + highlighting)
        UpdateOverlay();
    }

    /// <summary>
    /// The selection colour. Not blue: this overlay already draws the BDC points in blue and dark blue,
    /// and a selection that shares their colour is a selection you have to work out. Magenta appears
    /// nowhere in a reticle, reads against both a light background and black lines, and is distinct for
    /// the common forms of colour blindness in a way red-against-black is not.
    /// </summary>
    private const string HighlightColor = "magenta";

    /// <summary>
    /// How wide to draw the halo behind the selected element: several times the element's own width, but
    /// never so thin that a hairline gets a hairline halo. Sized from the reticle rather than in absolute
    /// units, so it looks the same on a 12 mrad reticle and a 350 MOA one.
    /// </summary>
    private Measurement<AngularUnit> HaloWidth(Measurement<AngularUnit>? elementWidth)
    {
        var floor = _currentReticle!.Size.X / 150;
        if (elementWidth == null)
            return floor;

        var scaled = elementWidth.Value * 4;
        return scaled.In(floor.Unit) > floor.Value ? scaled : floor;
    }

    /// <summary>Sets an element's colour, whatever concrete element type it is.</summary>
    private static void SetColor(ReticleElement element, string color)
        => element.GetType().GetProperty("Color")?.SetValue(element, color);

    private void UpdateOverlay()
    {
        if (_currentReticle == null)
        {
            ReticleCanvas.Overlay = null;
            return;
        }

        var overlayElements = new ReticleElementsCollection();

        // Add BDC visualization
        if (_currentReticle.BulletDropCompensator.Count > 0)
        {
            var selectedItem = ReticleItems.SelectedItem;

            foreach (var bdc in _currentReticle.BulletDropCompensator)
            {
                // Determine color based on selection
                bool isSelected = _highlightCurrentItem && ReferenceEquals(selectedItem, bdc);
                string color = isSelected ? HighlightColor : "darkblue";

                // Draw circle at BDC position
                overlayElements.Add(new ReticleCircle
                {
                    Center = bdc.Position,
                    Radius = _currentReticle.Size.X / 50,
                    Color = color,
                    Fill = false,
                    LineWidth = null
                });

                // Draw text label with drop value
                overlayElements.Add(new ReticleText
                {
                    Position = new ReticlePosition(
                        bdc.Position.X + bdc.TextOffset,
                        bdc.Position.Y - bdc.TextHeight / 2),
                    Color = color,
                    TextHeight = bdc.TextHeight,
                    Text = bdc.Position.Y.ToString()
                });
            }
        }

        // Highlight the selected element as a halo: the same geometry drawn far thicker in the highlight
        // colour, with the element itself redrawn on top in its own colour. Recolouring the element in
        // place -- what this did before -- is invisible in practice: a 0.01 mrad hairline restated as a
        // 0.01 mrad hairline is the same shape in a different colour, and on a dense reticle there is
        // nothing to notice. The halo shows outside the element's own footprint, so it reads at any line
        // width, and the element stays legible rather than being painted over.
        if (_highlightCurrentItem && ReticleItems.SelectedItem is ReticleElement element)
        {
            var halo = element.Clone();
            SetColor(halo, HighlightColor);

            var widthProperty = halo.GetType().GetProperty("LineWidth");
            if (widthProperty != null)
            {
                // Text has no line width and needs none: recolouring it is already conspicuous.
                var current = (Measurement<AngularUnit>?)widthProperty.GetValue(halo);
                widthProperty.SetValue(halo, HaloWidth(current));
            }

            overlayElements.Add(halo);
            overlayElements.Add(element.Clone());
        }

        ReticleCanvas.Overlay = overlayElements;
        ReticleCanvas.InvalidateVisual();
    }

    private void UpdateElementsList()
    {
        if (_currentReticle == null)
        {
            ReticleItems.ItemsSource = null;
            _elementsView = null;
            return;
        }

        // Create combined view of elements and BDC points
        _elementsView = new Models.CombinedElementsView(_currentReticle);
        ReticleItems.ItemsSource = _elementsView;
    }

    private void RefreshElementsList()
    {
        if (_currentReticle == null)
        {
            ReticleItems.ItemsSource = null;
            _elementsView = null;
            return;
        }

        // Save current selection to restore after rebuild
        var selectedIndex = ReticleItems.SelectedIndex;

        // Recreate the view to force refresh (also initializes it if not yet built)
        _elementsView = new Models.CombinedElementsView(_currentReticle);
        ReticleItems.ItemsSource = _elementsView;

        // Restore selection
        if (selectedIndex >= 0 && selectedIndex < _elementsView.Count)
        {
            ReticleItems.SelectedIndex = selectedIndex;
        }
    }

    #endregion

    #region Element Operations

    private void UpdateElementButtonStates()
    {
        var hasReticle = _currentReticle?.Size != null &&
                         _currentReticle.Size.X.In(_currentReticle.Size.X.Unit) > 0 &&
                         _currentReticle.Size.Y.In(_currentReticle.Size.Y.Unit) > 0;
        var hasSelection = ReticleItems.SelectedItem != null;

        ElementTypeCombo.IsEnabled = hasReticle;
        ButtonNew.IsEnabled = hasReticle;
        ButtonEdit.IsEnabled = hasReticle && hasSelection;
        ButtonDuplicate.IsEnabled = hasReticle && hasSelection;
        ButtonDelete.IsEnabled = hasReticle && hasSelection;
        ButtonMoveUp.IsEnabled = hasReticle && hasSelection;
        ButtonMoveDown.IsEnabled = hasReticle && hasSelection;
    }

    private async void OnNewElement(object? sender, RoutedEventArgs e)
    {
        if (_currentReticle == null) return;

        // Get selected type from combo
        var selectedIndex = ElementTypeCombo.SelectedIndex;

        object? newElement = selectedIndex switch
        {
            0 => new ReticleLine
            {
                Start = new ReticlePosition(0, 0, AngularUnit.Mil),
                End = new ReticlePosition(0, 0, AngularUnit.Mil),
                Color = "black"
            },
            1 => new ReticleCircle
            {
                Center = new ReticlePosition(0, 0, AngularUnit.Mil),
                Radius = new Measurement<AngularUnit>(1, AngularUnit.Mil),
                Color = "black"
            },
            2 => new ReticleRectangle
            {
                TopLeft = new ReticlePosition(0, 0, AngularUnit.Mil),
                Size = new ReticlePosition(1, 1, AngularUnit.Mil),
                Color = "black"
            },
            3 => new ReticlePath
            {
                Color = "black"
            },
            4 => new ReticleText
            {
                Position = new ReticlePosition(0, 0, AngularUnit.Mil),
                TextHeight = new Measurement<AngularUnit>(1, AngularUnit.Mil),
                Text = "Text",
                Color = "black"
            },
            5 => new ReticleBulletDropCompensatorPoint
            {
                Position = new ReticlePosition(0, 0, AngularUnit.Mil),
                TextOffset = new Measurement<AngularUnit>(1, AngularUnit.Mil),
                TextHeight = new Measurement<AngularUnit>(0.5, AngularUnit.Mil)
            },
            _ => null
        };

        if (newElement == null)
        {
            StatusArea.Text = "Select an element type first";
            return;
        }

        // Open appropriate editor dialog (pass reticle for path preview)
        var dialog = Utilities.DialogFactory.CreateDialogForElement(newElement, _currentReticle);
        if (dialog == null)
        {
            StatusArea.Text = $"Editor not implemented for {newElement.GetType().Name}";
            return;
        }

        var result = await dialog.ShowDialog<bool?>(this);

        if (result != true)
        {
            StatusArea.Text = "New element cancelled";
            return;
        }

        AddElement(newElement);
        StatusArea.Text = $"Added new {newElement.GetType().Name}";
    }

    /// <summary>Adds an element (or BDC point) to the document and selects it.</summary>
    internal void AddElement(object newElement)
    {
        if (_currentReticle == null) return;

        // Add to appropriate collection
        if (newElement is ReticleElement element)
            _currentReticle.Elements.Add(element);
        else if (newElement is ReticleBulletDropCompensatorPoint bdc)
            _currentReticle.BulletDropCompensator.Add(bdc);
        else
            return;

        MarkDirty();
        RefreshElementsList();
        UpdateReticlePreview();

        // Select newly added item
        if (_elementsView != null && _elementsView.Count > 0)
            ReticleItems.SelectedIndex = _elementsView.Count - 1;
    }

    private async void OnEditElement(object? sender, RoutedEventArgs e)
    {
        var selectedElement = ReticleItems.SelectedItem;
        if (selectedElement == null)
        {
            StatusArea.Text = "Select an element to edit";
            return;
        }

        // Open appropriate editor dialog (pass reticle for path preview)
        var dialog = Utilities.DialogFactory.CreateDialogForElement(selectedElement, _currentReticle);
        if (dialog == null)
        {
            StatusArea.Text = $"Editor not implemented for {selectedElement.GetType().Name}";
            return;
        }

        var result = await dialog.ShowDialog<bool?>(this);

        if (result != true)
        {
            StatusArea.Text = "Edit cancelled";
            return;
        }

        MarkDirty();
        RefreshElementsList();
        UpdateReticlePreview();

        StatusArea.Text = $"Edited {selectedElement.GetType().Name}";
    }

    private void OnDeleteElement(object? sender, RoutedEventArgs e) => DeleteSelectedElement();

    /// <summary>Removes the selected element (or BDC point) from the document.</summary>
    internal void DeleteSelectedElement()
    {
        if (_currentReticle == null) return;

        var selectedElement = ReticleItems.SelectedItem;
        var selectedIndex = ReticleItems.SelectedIndex;

        if (selectedElement == null)
        {
            StatusArea.Text = "Select an element to delete";
            return;
        }

        if (selectedElement is ReticleElement element)
        {
            for (int i = 0; i < _currentReticle.Elements.Count; i++)
            {
                if (ReferenceEquals(_currentReticle.Elements[i], element))
                {
                    _currentReticle.Elements.RemoveAt(i);
                    break;
                }
            }
        }
        else if (selectedElement is ReticleBulletDropCompensatorPoint bdc)
        {
            for (int i = 0; i < _currentReticle.BulletDropCompensator.Count; i++)
            {
                if (ReferenceEquals(_currentReticle.BulletDropCompensator[i], bdc))
                {
                    _currentReticle.BulletDropCompensator.RemoveAt(i);
                    break;
                }
            }
        }

        MarkDirty();
        RefreshElementsList();
        UpdateReticlePreview();

        // Select next item
        if (_elementsView != null && _elementsView.Count > 0)
        {
            var newIndex = Math.Min(selectedIndex, _elementsView.Count - 1);
            ReticleItems.SelectedIndex = newIndex;
        }

        StatusArea.Text = $"Deleted {selectedElement.GetType().Name}";
    }

    private void OnDuplicateElement(object? sender, RoutedEventArgs e)
    {
        if (_currentReticle == null) return;

        var selectedElement = ReticleItems.SelectedItem;
        if (selectedElement == null)
        {
            StatusArea.Text = "Select an element to duplicate";
            return;
        }

        object? clone = null;

        if (selectedElement is ReticleElement element)
        {
            clone = element.Clone();
            _currentReticle.Elements.Add((ReticleElement)clone);
        }
        else if (selectedElement is ReticleBulletDropCompensatorPoint bdc)
        {
            clone = bdc.Clone();
            _currentReticle.BulletDropCompensator.Add((ReticleBulletDropCompensatorPoint)clone);
        }

        if (clone == null) return;

        MarkDirty();
        RefreshElementsList();
        UpdateReticlePreview();

        // Select duplicated item
        if (_elementsView != null && _elementsView.Count > 0)
            ReticleItems.SelectedIndex = _elementsView.Count - 1;

        StatusArea.Text = $"Duplicated {selectedElement.GetType().Name}";
    }

    private void OnMoveElementUp(object? sender, RoutedEventArgs e) => MoveSelectedElement(-1);
    private void OnMoveElementDown(object? sender, RoutedEventArgs e) => MoveSelectedElement(1);

    /// <summary>
    /// Reorders the selected item within its own collection (elements or BDC points) by the given
    /// delta. Moves stay within a collection — the combined list shows elements first, then BDC points.
    /// </summary>
    private void MoveSelectedElement(int delta)
    {
        if (_currentReticle == null) return;

        var selected = ReticleItems.SelectedItem;
        if (selected == null) return;

        if (selected is ReticleElement element)
        {
            var coll = _currentReticle.Elements;
            int index = -1;
            for (int i = 0; i < coll.Count; i++)
                if (ReferenceEquals(coll[i], element)) { index = i; break; }

            int target = index + delta;
            if (index < 0 || target < 0 || target >= coll.Count) return;

            coll.RemoveAt(index);
            coll.Insert(target, element);
            RefreshElementsList();
            ReticleItems.SelectedIndex = target; // element combined index == element index
        }
        else if (selected is ReticleBulletDropCompensatorPoint bdc)
        {
            var coll = _currentReticle.BulletDropCompensator;
            int index = -1;
            for (int i = 0; i < coll.Count; i++)
                if (ReferenceEquals(coll[i], bdc)) { index = i; break; }

            int target = index + delta;
            if (index < 0 || target < 0 || target >= coll.Count) return;

            coll.RemoveAt(index);
            coll.Insert(target, bdc);
            RefreshElementsList();
            // BDC points follow the elements in the combined list.
            ReticleItems.SelectedIndex = _currentReticle.Elements.Count + target;
        }
        else
        {
            return;
        }

        MarkDirty();
        UpdateReticlePreview();
        StatusArea.Text = $"Moved {selected.GetType().Name} {(delta < 0 ? "up" : "down")}";
    }

    #endregion
}