using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using BallisticCalculator.Reticle.Data;
using BallisticCalculator.Serialization;
using Gehtsoft.Measurements;
using ReticleEditor.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ReticleEditor.Views;

public partial class MainWindow : Window
{
    private const double MinFontSize = 10;
    private const double MaxFontSize = 20;
    private bool _highlightCurrentItem = false;
    private ReticleDefinition? _currentReticle = null;
    private string? _currentFileName = null;
    private Models.CombinedElementsView? _elementsView = null;
    private AngularUnit _coordinateDisplayUnit = AngularUnit.Mil;

    public MainWindow()
    {
        InitializeComponent();

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

        // Hook up selection changed event
        ReticleItems.SelectionChanged += OnReticleItemsSelectionChanged;

        // Initialize coordinate unit menu checkmarks (default to Mil)
        UpdateCoordinateUnitMenuCheckmarks();

        // Load and apply saved window state
        LoadWindowState();

        // Save state when closing
        Closing += OnWindowClosing;
    }

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

    private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
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
        // Create a new reticle with default values
        _currentReticle = new ReticleDefinition
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
        };

        _currentFileName = null;
        UpdateReticleControls();
        UpdateElementsList();
        UpdateReticlePreview();
        StatusArea.Text = "New reticle created";
        await Task.CompletedTask;
    }

    private async void OnFileOpen(object? sender, RoutedEventArgs e)
    {
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
            FileTypeFilter = new[] { fileTypeFilter }
        });

        if (files.Count > 0)
        {
            var path = files[0].TryGetLocalPath();
            if (path != null)
            {
                try
                {
                    // Load reticle from file using BallisticXmlDeserializer
                    _currentReticle = BallisticXmlDeserializer.ReadFromFile<ReticleDefinition>(path);
                    _currentFileName = path;

                    UpdateReticleControls();
                    UpdateElementsList();
                    UpdateReticlePreview();

                    StatusArea.Text = $"Opened: {System.IO.Path.GetFileName(path)}";
                }
                catch (Exception ex)
                {
                    StatusArea.Text = $"Error loading file: {ex.Message}";
                }
            }
        }
    }

    private async void OnFileSave(object? sender, RoutedEventArgs e)
    {
        // TODO: Implement save
        StatusArea.Text = "Save (not implemented yet)";
        await Task.CompletedTask;
    }

    private async void OnFileSaveAs(object? sender, RoutedEventArgs e)
    {
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
            SuggestedFileName = "reticle.reticle"
        });

        if (file != null)
        {
            // TODO: Save reticle to file
            var path = file.TryGetLocalPath();
            StatusArea.Text = $"Saved: {System.IO.Path.GetFileName(path)}";
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

    private void OnToggleHighlight(object? sender, RoutedEventArgs e)
    {
        _highlightCurrentItem = !_highlightCurrentItem;
        if (HighlightMenuItem != null)
        {
            HighlightMenuItem.Icon = _highlightCurrentItem ? "✓" : null;
        }
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
                string color = isSelected ? "blue" : "darkblue";

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

        // Add highlighted selected element
        if (_highlightCurrentItem && ReticleItems.SelectedItem != null)
        {
            var selectedItem = ReticleItems.SelectedItem;

            if (selectedItem is ReticleElement element)
            {
                // Clone the element and change its color to blue
                var clone = element.Clone();
                var colorProperty = clone.GetType().GetProperty("Color");
                if (colorProperty != null)
                {
                    colorProperty.SetValue(clone, "blue");
                    overlayElements.Add(clone);
                }
            }
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
        if (_currentReticle == null || _elementsView == null)
            return;

        // Save current selection and scroll position
        var selectedItem = ReticleItems.SelectedItem;
        var selectedIndex = ReticleItems.SelectedIndex;

        // Recreate the view to force refresh
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

        // Open appropriate editor dialog
        Window? dialog = newElement switch
        {
            ReticleLine line => new Views.Dialogs.EditLineDialog(line),
            // TODO: Add other dialog types as they are implemented
            _ => null
        };

        if (dialog == null)
        {
            StatusArea.Text = $"Editor not implemented for {newElement.GetType().Name}";
            return;
        }

        await dialog.ShowDialog(this);

        // Add to appropriate collection
        if (newElement is ReticleElement element)
            _currentReticle.Elements.Add(element);
        else if (newElement is ReticleBulletDropCompensatorPoint bdc)
            _currentReticle.BulletDropCompensator.Add(bdc);

        RefreshElementsList();
        UpdateReticlePreview();

        // Select newly added item
        if (_elementsView != null && _elementsView.Count > 0)
            ReticleItems.SelectedIndex = _elementsView.Count - 1;

        StatusArea.Text = $"Added new {newElement.GetType().Name}";
    }

    private async void OnEditElement(object? sender, RoutedEventArgs e)
    {
        var selectedElement = ReticleItems.SelectedItem;
        if (selectedElement == null)
        {
            StatusArea.Text = "Select an element to edit";
            return;
        }

        // Open appropriate editor dialog
        Window? dialog = selectedElement switch
        {
            ReticleLine line => new Views.Dialogs.EditLineDialog(line),
            // TODO: Add other dialog types as they are implemented
            _ => null
        };

        if (dialog == null)
        {
            StatusArea.Text = $"Editor not implemented for {selectedElement.GetType().Name}";
            return;
        }

        await dialog.ShowDialog(this);

        RefreshElementsList();
        UpdateReticlePreview();

        StatusArea.Text = $"Edited {selectedElement.GetType().Name}";
    }

    private void OnDeleteElement(object? sender, RoutedEventArgs e)
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

        RefreshElementsList();
        UpdateReticlePreview();

        // Select duplicated item
        if (_elementsView != null && _elementsView.Count > 0)
            ReticleItems.SelectedIndex = _elementsView.Count - 1;

        StatusArea.Text = $"Duplicated {selectedElement.GetType().Name}";
    }

    #endregion
}