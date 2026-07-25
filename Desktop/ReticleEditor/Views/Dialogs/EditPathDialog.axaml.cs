using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Utilities;
using System;
using System.Collections.ObjectModel;

namespace ReticleEditor.Views.Dialogs;

public partial class EditPathDialog : Window
{
    private readonly ReticlePath _element;
    private readonly ReticlePath _copy;
    private readonly ReticleDefinition? _reticle;
    private readonly ObservableCollection<ReticlePathElement> _elementsSource;

    public EditPathDialog(ReticlePath path, ReticleDefinition? reticle = null)
    {
        _element = path ?? throw new ArgumentNullException(nameof(path));
        _copy = ClonePath(path);
        _reticle = reticle;
        _elementsSource = new ObservableCollection<ReticlePathElement>();

        InitializeComponent();

        // Initialize MeasurementControls with AngularUnit type
        LineWidthControl.UnitType = typeof(AngularUnit);

        // Set up elements list
        ElementsList.ItemsSource = _elementsSource;

        PopulateFromElement();

        // Hook up change events for auto-preview
        LineWidthControl.Changed += OnParameterChanged;
        ColorCombo.SelectionChanged += OnParameterChanged;
        StyleCombo.SelectionChanged += OnParameterChanged;
        FillCheckBox.IsCheckedChanged += OnParameterChanged;

        UpdatePreview();

        // Restore saved dialog size
        WindowStateManager.RestoreDialogSize(this, nameof(EditPathDialog));

        // Save dialog size when closing
        Closing += (s, e) => WindowStateManager.SaveDialogSize(this, nameof(EditPathDialog));
    }

    private static ReticlePath ClonePath(ReticlePath source)
    {
        var copy = new ReticlePath
        {
            Color = source.Color,
            LineWidth = source.LineWidth,
            Fill = source.Fill,
            LineStyle = source.LineStyle
        };

        foreach (var element in source.Elements)
            copy.Elements.Add(element.Clone());

        return copy;
    }

    private void PopulateFromElement()
    {
        // Populate line width (optional)
        if (_element.LineWidth.HasValue)
            LineWidthControl.SetValue(_element.LineWidth.Value);
        else
            LineWidthControl.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Mil));

        // Populate color combo
        ColorCombo.PopulateWithColors();
        ColorCombo.SelectedItem = _element.Color ?? "black";

        // Populate line style (legacy null => Solid)
        StyleCombo.PopulateWithLineStyles();
        StyleCombo.SelectLineStyle(_element.LineStyle);

        // Populate fill checkbox
        FillCheckBox.IsChecked = _element.Fill == true;

        // Populate elements list
        RefreshElementsList();
    }

    private void RefreshElementsList()
    {
        _elementsSource.Clear();
        foreach (var element in _element.Elements)
            _elementsSource.Add(element);
    }

    public void Save()
    {
        // Handle optional LineWidth
        var lineWidth = LineWidthControl.GetValue<AngularUnit>();
        if (lineWidth.HasValue && lineWidth.Value.In(lineWidth.Value.Unit) > 0)
            _element.LineWidth = lineWidth.Value;
        else
            _element.LineWidth = null;

        _element.Color = ColorCombo.SelectedItem?.ToString();
        _element.Fill = FillCheckBox.IsChecked == true;
        _element.LineStyle = StyleCombo.SelectedLineStyle();
    }

    public void Revert()
    {
        _element.Color = _copy.Color;
        _element.LineWidth = _copy.LineWidth;
        _element.Fill = _copy.Fill;
        _element.LineStyle = _copy.LineStyle;

        _element.Elements.Clear();
        foreach (var element in _copy.Elements)
            _element.Elements.Add(element.Clone());

        PopulateFromElement();
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (_reticle == null || _reticle.Size == null)
        {
            PreviewCanvas.Reticle = null;
            return;
        }

        // Create a temporary path with current control values for preview
        // (don't modify the actual element until Save is called)
        var previewPath = new ReticlePath();

        // Copy current control values
        var lineWidth = LineWidthControl.GetValue<AngularUnit>();
        if (lineWidth.HasValue && lineWidth.Value.In(lineWidth.Value.Unit) > 0)
            previewPath.LineWidth = lineWidth.Value;
        else
            previewPath.LineWidth = null;

        previewPath.Color = ColorCombo.SelectedItem?.ToString();
        previewPath.Fill = FillCheckBox.IsChecked == true;
        previewPath.LineStyle = StyleCombo.SelectedLineStyle();

        // Copy elements from the actual path
        foreach (var element in _element.Elements)
            previewPath.Elements.Add(element);

        // Create a temporary reticle with the preview path
        var previewReticle = new ReticleDefinition
        {
            Size = _reticle.Size,
            Zero = _reticle.Zero
        };
        previewReticle.Elements.Add(previewPath);

        PreviewCanvas.Reticle = previewReticle;
        PreviewCanvas.InvalidateVisual();
    }

    private AngularUnit GetDefaultUnit()
    {
        if (_reticle?.Size != null)
            return _reticle.Size.X.Unit;
        return AngularUnit.Mil;
    }

    public void AddMoveTo()
    {
        var element = new ReticlePathElementMoveTo
        {
            Position = new ReticlePosition(0, 0, GetDefaultUnit())
        };

        _element.Elements.Add(element);
        _elementsSource.Add(element);
        ElementsList.SelectedItem = element;
        UpdatePreview();
    }

    public void AddLineTo()
    {
        var element = new ReticlePathElementLineTo
        {
            Position = new ReticlePosition(0, 0, GetDefaultUnit())
        };

        _element.Elements.Add(element);
        _elementsSource.Add(element);
        ElementsList.SelectedItem = element;
        UpdatePreview();
    }

    public void AddArc()
    {
        var element = new ReticlePathElementArc
        {
            Position = new ReticlePosition(0, 0, GetDefaultUnit()),
            Radius = new Measurement<AngularUnit>(1, GetDefaultUnit()),
            ClockwiseDirection = false,
            MajorArc = false
        };

        _element.Elements.Add(element);
        _elementsSource.Add(element);
        ElementsList.SelectedItem = element;
        UpdatePreview();
    }

    public void DeleteSelectedElement()
    {
        var selectedElement = ElementsList.SelectedItem as ReticlePathElement;
        if (selectedElement == null)
            return;

        var selectedIndex = ElementsList.SelectedIndex;

        // Remove from path
        for (int i = _element.Elements.Count - 1; i >= 0; i--)
        {
            if (ReferenceEquals(_element.Elements[i], selectedElement))
            {
                _element.Elements.RemoveAt(i);
                break;
            }
        }

        // Remove from observable collection
        _elementsSource.Remove(selectedElement);

        // Select next item
        if (_elementsSource.Count > 0)
        {
            var newIndex = Math.Min(selectedIndex, _elementsSource.Count - 1);
            ElementsList.SelectedIndex = newIndex;
        }

        UpdatePreview();
    }

    private async void EditSelectedElement()
    {
        var selectedElement = ElementsList.SelectedItem;
        if (selectedElement == null)
            return;

        var dialog = DialogFactory.CreateDialogForElement(selectedElement);
        if (dialog == null)
            return;

        await dialog.ShowDialog(this);

        // Refresh the list to update display
        var selectedIndex = ElementsList.SelectedIndex;
        RefreshElementsList();
        if (selectedIndex >= 0 && selectedIndex < _elementsSource.Count)
            ElementsList.SelectedIndex = selectedIndex;

        UpdatePreview();
    }

    /// <summary>Moves the selected path element up (-1) or down (+1) within the path.</summary>
    public void MoveSelectedElement(int delta)
    {
        if (ElementsList.SelectedItem is not ReticlePathElement selected)
            return;

        int index = -1;
        for (int i = 0; i < _element.Elements.Count; i++)
        {
            if (ReferenceEquals(_element.Elements[i], selected))
            {
                index = i;
                break;
            }
        }

        int target = index + delta;
        if (index < 0 || target < 0 || target >= _element.Elements.Count)
            return;

        _element.Elements.RemoveAt(index);
        _element.Elements.Insert(target, selected);

        RefreshElementsList();
        ElementsList.SelectedIndex = target;
        UpdatePreview();
    }

    // Event handlers
    private void OnMoveUp(object? sender, RoutedEventArgs e) => MoveSelectedElement(-1);
    private void OnMoveDown(object? sender, RoutedEventArgs e) => MoveSelectedElement(1);
    private void OnAddMoveTo(object? sender, RoutedEventArgs e) => AddMoveTo();
    private void OnAddLineTo(object? sender, RoutedEventArgs e) => AddLineTo();
    private void OnAddArc(object? sender, RoutedEventArgs e) => AddArc();
    private void OnEditElement(object? sender, RoutedEventArgs e) => EditSelectedElement();
    private void OnDeleteElement(object? sender, RoutedEventArgs e) => DeleteSelectedElement();

    private void OnParameterChanged(object? sender, EventArgs e) => UpdatePreview();

    private void OnRevert(object? sender, RoutedEventArgs e)
    {
        Revert();
    }

    private void OnOK(object? sender, RoutedEventArgs e)
    {
        Save();
        Close(true);
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Revert();
        Close(false);
    }
}
