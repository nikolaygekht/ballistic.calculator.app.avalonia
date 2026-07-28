using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator;
using BallisticCalculator.Types;
using System;

namespace BallisticCalculator.Panels.Panels;

/// <summary>
/// Optional single wind used only while zeroing. Enabled by a checkbox, mirroring
/// <see cref="ZeroAtmospherePanel"/>. Returns null when disabled.
/// </summary>
/// <remarks>
/// One wind, deliberately — zeroing happens at a short, controlled distance, where splitting the air into
/// zones the way <see cref="MultiWindPanel"/> does would be nonsense. The start distance is therefore
/// pinned to the muzzle and read-only: this wind blows for the whole zeroing shot.
/// </remarks>
public partial class ZeroWindPanel : UserControl
{
    private MeasurementSystem _measurementSystem = MeasurementSystem.Metric;

    public ZeroWindPanel()
    {
        InitializeComponent();
        WindSubPanel.AllowStartDistance = false;
        WireEvents();
    }

    #region Properties

    public bool ConvertOnSystemChange
    {
        get => WindSubPanel.ConvertOnSystemChange;
        set => WindSubPanel.ConvertOnSystemChange = value;
    }

    public MeasurementSystem MeasurementSystem
    {
        get => _measurementSystem;
        set
        {
            if (_measurementSystem == value) return;
            _measurementSystem = value;
            WindSubPanel.MeasurementSystem = value;
        }
    }

    public Wind? Wind
    {
        get
        {
            if (EnableCheckBox.IsChecked != true)
                return null;
            return WindSubPanel.Wind;
        }
        set
        {
            if (value == null)
            {
                EnableCheckBox.IsChecked = false;
                WindSubPanel.Clear();
                return;
            }

            EnableCheckBox.IsChecked = true;
            WindSubPanel.Wind = value;
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
        WindSubPanel.Clear();
    }

    #endregion

    #region Private Methods

    private void WireEvents()
    {
        EnableCheckBox.IsCheckedChanged += OnEnableChanged;
        WindSubPanel.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
    }

    private void OnEnableChanged(object? sender, RoutedEventArgs e)
    {
        WindSubPanel.IsEnabled = EnableCheckBox.IsChecked == true;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}
