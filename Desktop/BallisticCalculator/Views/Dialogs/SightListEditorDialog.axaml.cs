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
/// Tools → Edit Sights. Master/detail editor over the sight presets in <c>data/dictionaries.xml</c>.
/// Loads the whole dictionary, edits the sight list, and on OK saves the merged file (barrels kept).
/// </summary>
public partial class SightListEditorDialog : Window
{
    private readonly MeasurementSystem _system;
    private readonly IReadOnlyList<BarrelDictionaryEntry> _barrels;
    private readonly ObservableCollection<SightEditModel> _sights = new();
    private bool _loading;

    public SightListEditorDialog(MeasurementSystem system)
    {
        _system = system;
        InitializeComponent();

        SightHeightControl.UnitType = typeof(DistanceUnit);
        SightHeightControl.Minimum = 0;
        DefaultZeroControl.UnitType = typeof(DistanceUnit);
        DefaultZeroControl.Minimum = 0;
        HClickControl.UnitType = typeof(AngularUnit);
        HClickControl.Minimum = 0;
        VClickControl.UnitType = typeof(AngularUnit);
        VClickControl.Minimum = 0;
        ApplyUnits();

        var dictionary = BallisticDictionary.LoadDefault();
        _barrels = dictionary.Barrels;
        foreach (var s in dictionary.Sights)
            _sights.Add(new SightEditModel
            {
                Name = s.Name,
                SightHeight = s.SightHeight,
                DefaultZero = s.DefaultZero,
                HorizontalClick = s.HorizontalClick,
                VerticalClick = s.VerticalClick,
            });

        EntriesList.ItemsSource = _sights;
        WireDetailEvents();
        if (_sights.Count > 0)
            EntriesList.SelectedIndex = 0;
    }

    private void ApplyUnits()
    {
        if (_system == MeasurementSystem.Metric)
        {
            SightHeightControl.ChangeUnit(DistanceUnit.Millimeter, 0, false);
            DefaultZeroControl.ChangeUnit(DistanceUnit.Meter, 0, false);
        }
        else
        {
            SightHeightControl.ChangeUnit(DistanceUnit.Inch, 1, false);
            DefaultZeroControl.ChangeUnit(DistanceUnit.Yard, 0, false);
        }
        HClickControl.ChangeUnit(AngularUnit.Mil, 2, false);
        VClickControl.ChangeUnit(AngularUnit.Mil, 2, false);
    }

    private void WireDetailEvents()
    {
        NameBox.TextChanged += (_, _) =>
        {
            if (_loading || Current == null) return;
            Current.Name = NameBox.Text ?? "";
        };
        SightHeightControl.Changed += (_, _) => WriteBack();
        DefaultZeroControl.Changed += (_, _) => WriteBack();
        HClickControl.Changed += (_, _) => WriteBack();
        VClickControl.Changed += (_, _) => WriteBack();
    }

    private SightEditModel? Current => EntriesList.SelectedItem as SightEditModel;

    private void WriteBack()
    {
        if (_loading || Current is not { } m) return;
        m.SightHeight = SightHeightControl.IsEmpty ? null : SightHeightControl.GetValue<DistanceUnit>();
        m.DefaultZero = DefaultZeroControl.IsEmpty ? null : DefaultZeroControl.GetValue<DistanceUnit>();
        m.HorizontalClick = HClickControl.IsEmpty ? null : HClickControl.GetValue<AngularUnit>();
        m.VerticalClick = VClickControl.IsEmpty ? null : VClickControl.GetValue<AngularUnit>();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var m = Current;
        DetailPanel.IsEnabled = m != null;
        if (m == null)
            return;

        _loading = true;
        NameBox.Text = m.Name;
        SetOrClear(SightHeightControl, m.SightHeight);
        SetOrClear(DefaultZeroControl, m.DefaultZero);
        SetOrClear(HClickControl, m.HorizontalClick);
        SetOrClear(VClickControl, m.VerticalClick);
        _loading = false;
    }

    private static void SetOrClear<T>(Controls.Controls.MeasurementControl control, Measurement<T>? value)
        where T : System.Enum
    {
        if (value.HasValue)
            control.SetValue(value.Value);
        else
            control.Value = null;
    }

    private void OnAdd(object? sender, RoutedEventArgs e)
    {
        var model = new SightEditModel
        {
            Name = "New sight",
            SightHeight = _system == MeasurementSystem.Metric
                ? new Measurement<DistanceUnit>(40, DistanceUnit.Millimeter)
                : new Measurement<DistanceUnit>(1.5, DistanceUnit.Inch),
        };
        _sights.Add(model);
        EntriesList.SelectedItem = model;
        NameBox.Focus();
        NameBox.SelectAll();
    }

    private void OnDelete(object? sender, RoutedEventArgs e)
    {
        if (Current is not { } m) return;
        var index = _sights.IndexOf(m);
        _sights.Remove(m);
        if (_sights.Count == 0)
            DetailPanel.IsEnabled = false;
        else
            EntriesList.SelectedIndex = System.Math.Min(index, _sights.Count - 1);
    }

    private async void OnOK(object? sender, RoutedEventArgs e)
    {
        var invalid = _sights.Where(s => string.IsNullOrWhiteSpace(s.Name) || s.SightHeight == null).ToList();
        if (invalid.Count > 0)
        {
            await ShowError("Every sight needs a name and a sight height.");
            return;
        }

        var entries = _sights
            .Select(s => new SightDictionaryEntry
            {
                Name = s.Name.Trim(),
                SightHeight = s.SightHeight!.Value,
                DefaultZero = s.DefaultZero,
                HorizontalClick = s.HorizontalClick,
                VerticalClick = s.VerticalClick,
            })
            .ToList();

        try
        {
            new BallisticDictionary(entries, _barrels).SaveDefault();
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
