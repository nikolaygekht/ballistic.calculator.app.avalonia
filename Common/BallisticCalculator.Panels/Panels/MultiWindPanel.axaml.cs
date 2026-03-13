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

    public Wind[]? Winds
    {
        get
        {
            var winds = new List<Wind>();
            foreach (var panel in GetWindPanels())
            {
                var wind = panel.Wind;
                if (wind != null)
                    winds.Add(wind);
            }
            return winds.Count > 0 ? winds.ToArray() : null;
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

            // Add a panel for each wind
            for (int i = 0; i < value.Length; i++)
            {
                var panel = CreateWindPanel(isFirst: i == 0);
                WindPanelsContainer.Children.Add(panel);
                GetWindPanelFromContainer(panel)!.Wind = value[i];
            }

            // If no winds, add one empty panel
            if (value.Length == 0)
                AddWindPanel();
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
