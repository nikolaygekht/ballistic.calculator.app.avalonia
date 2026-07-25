using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator;
using BallisticCalculator.Models;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Views.Dialogs;

/// <summary>
/// Tools → Edit Barrels. Master/detail editor over the barrel presets in <c>data/dictionaries.xml</c>.
/// Loads the whole dictionary, edits the barrel list, and on OK saves the merged file (sights kept).
/// </summary>
public partial class BarrelListEditorDialog : Window
{
    private readonly MeasurementSystem _system;
    private readonly IReadOnlyList<SightDictionaryEntry> _sights;
    private readonly ObservableCollection<BarrelEditModel> _barrels = new();
    private bool _loading;

    public BarrelListEditorDialog(MeasurementSystem system)
    {
        _system = system;
        InitializeComponent();

        StepControl.UnitType = typeof(DistanceUnit);
        StepControl.Minimum = 0;
        ApplyUnits();

        var dictionary = BallisticDictionary.LoadDefault();
        _sights = dictionary.Sights;
        foreach (var b in dictionary.Barrels)
            _barrels.Add(new BarrelEditModel { Name = b.Name, Step = b.Step, Direction = b.Direction });

        EntriesList.ItemsSource = _barrels;
        WireDetailEvents();
        if (_barrels.Count > 0)
            EntriesList.SelectedIndex = 0;
    }

    private void ApplyUnits()
        => StepControl.ChangeUnit(
            _system == MeasurementSystem.Metric ? DistanceUnit.Millimeter : DistanceUnit.Inch, 1, false);

    private void WireDetailEvents()
    {
        NameBox.TextChanged += (_, _) =>
        {
            if (_loading || Current == null) return;
            Current.Name = NameBox.Text ?? "";
        };
        StepControl.Changed += (_, _) =>
        {
            if (_loading || Current is not { } m) return;
            m.Step = StepControl.IsEmpty ? null : StepControl.GetValue<DistanceUnit>();
        };
        DirectionCombo.SelectionChanged += (_, _) =>
        {
            if (_loading || Current is not { } m) return;
            m.Direction = DirectionCombo.SelectedIndex == 1 ? TwistDirection.Left : TwistDirection.Right;
        };
    }

    private BarrelEditModel? Current => EntriesList.SelectedItem as BarrelEditModel;

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var m = Current;
        DetailPanel.IsEnabled = m != null;
        if (m == null)
            return;

        _loading = true;
        NameBox.Text = m.Name;
        if (m.Step.HasValue)
            StepControl.SetValue(m.Step.Value);
        else
            StepControl.Value = null;
        DirectionCombo.SelectedIndex = m.Direction == TwistDirection.Left ? 1 : 0;
        _loading = false;
    }

    private void OnAdd(object? sender, RoutedEventArgs e)
    {
        var model = new BarrelEditModel
        {
            Name = "New barrel",
            Step = _system == MeasurementSystem.Metric
                ? new Measurement<DistanceUnit>(250, DistanceUnit.Millimeter)
                : new Measurement<DistanceUnit>(10, DistanceUnit.Inch),
            Direction = TwistDirection.Right,
        };
        _barrels.Add(model);
        EntriesList.SelectedItem = model;
        NameBox.Focus();
        NameBox.SelectAll();
    }

    private void OnDelete(object? sender, RoutedEventArgs e)
    {
        if (Current is not { } m) return;
        var index = _barrels.IndexOf(m);
        _barrels.Remove(m);
        if (_barrels.Count == 0)
            DetailPanel.IsEnabled = false;
        else
            EntriesList.SelectedIndex = System.Math.Min(index, _barrels.Count - 1);
    }

    private async void OnOK(object? sender, RoutedEventArgs e)
    {
        var invalid = _barrels.Where(b => string.IsNullOrWhiteSpace(b.Name) || b.Step == null).ToList();
        if (invalid.Count > 0)
        {
            await ShowError("Every barrel needs a name and a twist rate.");
            return;
        }

        var entries = _barrels
            .Select(b => new BarrelDictionaryEntry
            {
                Name = b.Name.Trim(),
                Step = b.Step!.Value,
                Direction = b.Direction,
            })
            .ToList();

        try
        {
            new BallisticDictionary(_sights, entries).SaveDefault();
        }
        catch (System.Exception ex)
        {
            await ShowError($"Could not save the dictionary:\n{ex.Message}");
            return;
        }

        Close(true);
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(false);

    private async Task ShowError(string message)
    {
        var okButton = new Button { Content = "OK", Width = 80, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
        var dialog = new Window
        {
            Title = "Error",
            Width = 360,
            Height = 160,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Content = new DockPanel
            {
                Margin = new Avalonia.Thickness(15),
                Children =
                {
                    new StackPanel
                    {
                        [DockPanel.DockProperty] = Dock.Bottom,
                        Children = { okButton },
                    },
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    },
                },
            },
        };
        okButton.Click += (_, _) => dialog.Close();
        await dialog.ShowDialog(this);
    }
}
