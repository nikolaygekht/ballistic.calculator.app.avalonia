using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator;
using BallisticCalculator.Types;
using System;

namespace BallisticCalculator.Panels.Panels;

public partial class ZeroAtmospherePanel : UserControl
{
    private MeasurementSystem _measurementSystem = MeasurementSystem.Metric;

    public ZeroAtmospherePanel()
    {
        InitializeComponent();
        WireEvents();
    }

    #region Properties

    public bool ConvertOnSystemChange
    {
        get => AtmoSubPanel.ConvertOnSystemChange;
        set => AtmoSubPanel.ConvertOnSystemChange = value;
    }

    public MeasurementSystem MeasurementSystem
    {
        get => _measurementSystem;
        set
        {
            if (_measurementSystem == value) return;
            _measurementSystem = value;
            AtmoSubPanel.MeasurementSystem = value;
        }
    }

    public Atmosphere? Atmosphere
    {
        get
        {
            if (EnableCheckBox.IsChecked != true)
                return null;
            return AtmoSubPanel.Atmosphere;
        }
        set
        {
            if (value == null)
            {
                EnableCheckBox.IsChecked = false;
                AtmoSubPanel.Clear();
                return;
            }

            EnableCheckBox.IsChecked = true;
            AtmoSubPanel.Atmosphere = value;
        }
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Public Methods

    public void Clear()
    {
        EnableCheckBox.IsChecked = false;
        AtmoSubPanel.Clear();
    }

    #endregion

    #region Private Methods

    private void WireEvents()
    {
        EnableCheckBox.IsCheckedChanged += OnEnableChanged;
        AtmoSubPanel.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
    }

    private void OnEnableChanged(object? sender, RoutedEventArgs e)
    {
        AtmoSubPanel.IsEnabled = EnableCheckBox.IsChecked == true;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}
