using Avalonia.Controls;
using BallisticCalculator;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using System;

namespace BallisticCalculator.Panels.Panels;

public partial class ShotDataPanel : UserControl
{
    private MeasurementSystem _measurementSystem = MeasurementSystem.Metric;

    public ShotDataPanel()
    {
        InitializeComponent();
        WireInterPanelReferences();
        WireEvents();
    }

    #region Properties

    public IFileDialogService? FileDialogService
    {
        get => AmmoLibPanel.FileDialogService;
        set
        {
            AmmoLibPanel.FileDialogService = value;
            ZeroAmmoSubPanel.FileDialogService = value;
        }
    }

    public MeasurementSystem MeasurementSystem
    {
        get => _measurementSystem;
        set
        {
            if (_measurementSystem == value) return;
            _measurementSystem = value;
            AmmoLibPanel.MeasurementSystem = value;
            AtmosphereSubPanel.MeasurementSystem = value;
            WindSubPanel.MeasurementSystem = value;
            RifleSubPanel.MeasurementSystem = value;
            ZeroAmmoSubPanel.MeasurementSystem = value;
            ZeroAtmosphereSubPanel.MeasurementSystem = value;
            ParametersSubPanel.MeasurementSystem = value;
        }
    }

    public ShotData? ShotData
    {
        get
        {
            var ammoEntry = AmmoLibPanel.LibraryEntry;
            var atmosphere = AtmosphereSubPanel.Atmosphere;
            var rifle = RifleSubPanel.Rifle;
            var parameters = ParametersSubPanel.Parameters;

            if (ammoEntry == null || atmosphere == null || rifle == null || parameters == null)
                return null;

            // Combine zero ammo/atmosphere into rifle's zeroing parameters
            rifle.Zero.Ammunition = ZeroAmmoSubPanel.Ammunition;
            rifle.Zero.Atmosphere = ZeroAtmosphereSubPanel.Atmosphere;

            return new ShotData()
            {
                Ammunition = ammoEntry,
                Weapon = rifle,
                Atmosphere = atmosphere,
                Winds = WindSubPanel.Winds,
                Parameters = parameters,
            };
        }
        set
        {
            if (value == null)
            {
                Clear();
                return;
            }

            AmmoLibPanel.LibraryEntry = value.Ammunition;
            AtmosphereSubPanel.Atmosphere = value.Atmosphere;
            WindSubPanel.Winds = value.Winds;
            ParametersSubPanel.Parameters = value.Parameters;

            if (value.Weapon != null)
            {
                // Extract zero ammo/atmosphere before setting rifle
                ZeroAmmoSubPanel.Ammunition = value.Weapon.Zero?.Ammunition;
                ZeroAtmosphereSubPanel.Atmosphere = value.Weapon.Zero?.Atmosphere;
                RifleSubPanel.Rifle = value.Weapon;
            }
            else
            {
                RifleSubPanel.Rifle = null;
                ZeroAmmoSubPanel.Ammunition = null;
                ZeroAtmosphereSubPanel.Atmosphere = null;
            }
        }
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Public Methods

    public void Clear()
    {
        AmmoLibPanel.Clear();
        AtmosphereSubPanel.Clear();
        WindSubPanel.Clear();
        RifleSubPanel.Clear();
        ZeroAmmoSubPanel.Clear();
        ZeroAtmosphereSubPanel.Clear();
        ParametersSubPanel.Clear();
    }

    #endregion

    #region Private Methods

    private void WireInterPanelReferences()
    {
        ParametersSubPanel.RiflePanel = RifleSubPanel;
    }

    private void WireEvents()
    {
        AmmoLibPanel.Changed += OnChildChanged;
        AtmosphereSubPanel.Changed += OnChildChanged;
        WindSubPanel.Changed += OnChildChanged;
        RifleSubPanel.Changed += OnChildChanged;
        ZeroAmmoSubPanel.Changed += OnChildChanged;
        ZeroAtmosphereSubPanel.Changed += OnChildChanged;
        ParametersSubPanel.Changed += OnChildChanged;
    }

    private void OnChildChanged(object? sender, EventArgs e)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}
