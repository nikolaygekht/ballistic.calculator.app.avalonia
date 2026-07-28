using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BallisticCalculator.Panels.Panels;

public partial class MultiWindPanel : UserControl
{
    private MeasurementSystem _measurementSystem = MeasurementSystem.Metric;

    public MultiWindPanel()
    {
        InitializeComponent();
        AddWindPanel();
    }

    #region Properties

    public bool ConvertOnSystemChange { get; set; }

    public MeasurementSystem MeasurementSystem
    {
        get => _measurementSystem;
        set
        {
            if (_measurementSystem == value) return;
            _measurementSystem = value;
            ApplyMeasurementSystemToAll();
        }
    }

    public int WindPanelCount => GetWindPanels().Count;

    /// <summary>
    /// The wind zones along the flight path.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Each row's distance is the range at which its wind starts</b> — the natural thing for a shooter
    /// to enter. So a row reads "this wind, from here on", and it holds until the next row's distance:
    /// </para>
    /// <list type="bullet">
    /// <item><c>0, wind</c> — that wind over the whole trajectory.</item>
    /// <item><c>250 m, wind</c> — no wind at all until 250 m, then that wind for the rest of it.</item>
    /// </list>
    /// <para>
    /// The library counts the other way round: <see cref="Wind.MaximumRange"/> is where a wind <i>ends</i>,
    /// zones must arrive in ascending order, and — the trap — a <b>lone</b> wind ignores its range and
    /// blows for the whole flight. This property converts in both directions: zone <i>i</i> is handed over
    /// ending where zone <i>i+1</i> begins, the last zone is left open-ended, and a first zone that starts
    /// downrange gets an explicit still-air zone put in front of it, which is the only way to say
    /// "calm until here".
    /// </para>
    /// <para>
    /// Getting this backwards is not cosmetic. When a row's own start was handed over as its maximum range,
    /// the first zone never blew at all — the engine leaves a zone as soon as the bullet passes that range
    /// — so the wind typed in row one was silently discarded and every later row acted over the previous
    /// row's stretch of the trajectory.
    /// </para>
    /// </remarks>
    public Wind[]? Winds
    {
        get
        {
            // WindPanel packs its row's distance into MaximumRange; here that value is the zone's start.
            var rows = new List<(Measurement<DistanceUnit> Start, Wind Wind)>();
            foreach (var panel in GetWindPanels())
            {
                var wind = panel.Wind;
                if (wind != null)
                    rows.Add((wind.MaximumRange ?? Measurement<DistanceUnit>.ZERO, wind));
            }

            if (rows.Count == 0)
                return null;

            // The library requires ascending zones; the user is under no obligation to type them so.
            var ordered = rows.OrderBy(r => r.Start.In(DistanceUnit.Meter)).ToList();

            // A first zone that starts downrange means still air in front of it. One wind on its own would
            // ignore its range and blow from the muzzle, so the calm stretch has to be an actual zone.
            if (ordered[0].Start.In(DistanceUnit.Meter) > 0)
            {
                var calm = new Wind
                {
                    Direction = new Measurement<AngularUnit>(0, AngularUnit.Degree),
                    Velocity = new Measurement<VelocityUnit>(0, ordered[0].Wind.Velocity.Unit),
                };
                ordered.Insert(0, (Measurement<DistanceUnit>.ZERO, calm));
            }

            var winds = new Wind[ordered.Count];
            for (var i = 0; i < ordered.Count; i++)
            {
                winds[i] = new Wind
                {
                    Direction = ordered[i].Wind.Direction,
                    Velocity = ordered[i].Wind.Velocity,
                    // Ends where the next zone begins; the last one holds to the end of the trajectory.
                    MaximumRange = i + 1 < ordered.Count ? ordered[i + 1].Start : null,
                };
            }
            return winds;
        }
        set
        {
            if (value == null)
            {
                Clear();
                return;
            }

            // Remove all existing panels
            WindPanelsContainer.Children.Clear();

            if (value.Length == 0)
            {
                AddWindPanel();
                return;
            }

            // A leading still-air zone is this panel's own doing (see the getter), so fold it back into
            // the first real zone's start rather than showing the user a row of nothing.
            var first = value[0].Velocity.Value == 0 && value.Length > 1 && value[0].MaximumRange != null ? 1 : 0;
            var zeroUnit = _measurementSystem == MeasurementSystem.Metric ? DistanceUnit.Meter : DistanceUnit.Yard;

            // One row per remaining zone, showing where that wind starts rather than where it ends: the
            // previous zone's maximum range, or the muzzle when there is no previous zone.
            for (int i = first; i < value.Length; i++)
            {
                var panel = CreateWindPanel(isFirst: i == first);
                WindPanelsContainer.Children.Add(panel);
                GetWindPanelFromContainer(panel)!.Wind = new Wind
                {
                    Direction = value[i].Direction,
                    Velocity = value[i].Velocity,
                    MaximumRange = i == 0
                        ? (value.Length > 1 ? new Measurement<DistanceUnit>(0, zeroUnit) : null)
                        : value[i - 1].MaximumRange,
                };
            }
        }
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Panel Management

    private Control CreateWindPanel(bool isFirst)
    {
        var windPanel = new WindPanel
        {
            MeasurementSystem = _measurementSystem,
            ConvertOnSystemChange = ConvertOnSystemChange,
        };
        windPanel.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);

        var container = new StackPanel { Spacing = 0 };

        // Separator line above non-first wind panels
        if (!isFirst)
        {
            container.Children.Add(new Border
            {
                Height = 1,
                Background = Avalonia.Media.Brushes.Gray,
                Margin = new Avalonia.Thickness(0, 4, 0, 4),
            });
        }

        // Always wrap in Grid with remove button (disabled for first)
        var innerGrid = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("*,Auto"),
        };

        Grid.SetColumn(windPanel, 0);
        innerGrid.Children.Add(windPanel);

        var removeButton = new Button
        {
            Content = "X",
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Margin = new Avalonia.Thickness(4, 0, 0, 0),
            IsEnabled = !isFirst,
        };
        removeButton.Click += (s, e) => RemoveWindPanel(container);
        Grid.SetColumn(removeButton, 1);
        innerGrid.Children.Add(removeButton);

        container.Children.Add(innerGrid);
        return container;
    }

    private void AddWindPanel()
    {
        bool isFirst = WindPanelsContainer.Children.Count == 0;
        var container = CreateWindPanel(isFirst);
        WindPanelsContainer.Children.Add(container);
    }

    private void OnAddClicked()
    {
        var panels = GetWindPanels();
        var distanceUnit = _measurementSystem == MeasurementSystem.Metric ? DistanceUnit.Meter : DistanceUnit.Yard;

        // If first panel has distance disabled, enable it and set to 0
        if (panels.Count > 0 && panels[0].MaxDistanceCheckBox.IsChecked != true)
        {
            panels[0].MaxDistanceCheckBox.IsChecked = true;
            panels[0].MaxDistanceControl.SetValue(new Measurement<DistanceUnit>(0, distanceUnit));
        }

        // Get last panel for copying values and calculating distance
        var lastPanel = panels[panels.Count - 1];

        // Determine default distance for the new panel: previous + 100
        double defaultDistance = 100;
        var lastRange = lastPanel.MaxDistanceControl.GetValue<DistanceUnit>();
        if (lastRange != null)
            defaultDistance = lastRange.Value.Value + 100;
        else
            defaultDistance = panels.Count * 100;

        // Copy direction and velocity from last panel
        var lastDirection = lastPanel.DirectionControl.GetValue<AngularUnit>();
        var lastVelocity = lastPanel.VelocityControl.GetValue<VelocityUnit>();

        // Add new panel
        AddWindPanel();

        // Configure the new panel
        var newPanels = GetWindPanels();
        var newPanel = newPanels[newPanels.Count - 1];
        newPanel.MaxDistanceCheckBox.IsChecked = true;
        newPanel.MaxDistanceControl.SetValue(new Measurement<DistanceUnit>(defaultDistance, distanceUnit));

        if (lastDirection != null)
            newPanel.DirectionControl.SetValue(lastDirection.Value);
        if (lastVelocity != null)
            newPanel.VelocityControl.SetValue(lastVelocity.Value);
    }

    private void RemoveWindPanel(Control container)
    {
        WindPanelsContainer.Children.Remove(container);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private List<WindPanel> GetWindPanels()
    {
        var panels = new List<WindPanel>();
        foreach (var child in WindPanelsContainer.Children)
        {
            var panel = GetWindPanelFromContainer(child);
            if (panel != null)
                panels.Add(panel);
        }
        return panels;
    }

    private static WindPanel? GetWindPanelFromContainer(Control container)
    {
        // All panels are wrapped in StackPanel > Grid > WindPanel
        if (container is StackPanel sp)
        {
            foreach (var child in sp.Children)
            {
                if (child is Grid g)
                {
                    foreach (var gridChild in g.Children)
                    {
                        if (gridChild is WindPanel found)
                            return found;
                    }
                }
            }
        }

        return null;
    }

    #endregion

    #region Unit Switching

    private void ApplyMeasurementSystemToAll()
    {
        foreach (var panel in GetWindPanels())
        {
            panel.ConvertOnSystemChange = ConvertOnSystemChange;
            panel.MeasurementSystem = _measurementSystem;
        }
    }

    #endregion

    #region Event Handlers

    private void OnAdd(object? sender, RoutedEventArgs e)
    {
        OnAddClicked();
    }

    private void OnClear(object? sender, RoutedEventArgs e)
    {
        Clear();
    }

    #endregion

    #region Public Methods

    public void Clear()
    {
        WindPanelsContainer.Children.Clear();
        AddWindPanel();
    }

    #endregion
}
