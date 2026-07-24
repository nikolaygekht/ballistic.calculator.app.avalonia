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
            ZeroWindSubPanel.MeasurementSystem = value;
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

            return new ShotData()
            {
                Ammunition = ammoEntry,
                Weapon = rifle,
                Atmosphere = atmosphere,
                Winds = WindSubPanel.Winds,
                Parameters = parameters,
                Zeroing = BuildZeroing(rifle),
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

            if (value.Weapon != null)
            {
                // Zeroing is the source of truth; fall back to Weapon.Zero for older data.
                var zeroing = value.Zeroing;
                ZeroAmmoSubPanel.Ammunition = zeroing?.Ammunition ?? value.Weapon.Zero?.Ammunition;
                ZeroAtmosphereSubPanel.Atmosphere = zeroing?.Atmosphere ?? value.Weapon.Zero?.Atmosphere;
                ZeroWindSubPanel.Wind = zeroing?.Wind;
                RifleSubPanel.Rifle = value.Weapon;           // distance + V/H offsets from Weapon.Zero
                RifleSubPanel.ZeroShotAngle = zeroing?.ShotAngle;
            }
            else
            {
                RifleSubPanel.Rifle = null;
                ZeroAmmoSubPanel.Ammunition = null;
                ZeroAtmosphereSubPanel.Atmosphere = null;
                ZeroWindSubPanel.Clear();
            }

            // Parameters must be set AFTER the rifle: V/H clicks are converted to angles using
            // the sight's click sizes, which come from the RiflePanel.
            ParametersSubPanel.Parameters = value.Parameters;
        }
    }

    /// <summary>
    /// Assemble the full <see cref="ZeroingData"/> from the zeroing-related sub-panels. Distance and
    /// V/H offsets come from the rifle's zero (entered on the Rifle panel); ammo/atmosphere/wind/shot
    /// angle from their dedicated panels.
    /// </summary>
    private ZeroingData BuildZeroing(Rifle rifle) => new()
    {
        Distance = rifle.Zero.Distance,
        Ammunition = ZeroAmmoSubPanel.Ammunition,
        Atmosphere = ZeroAtmosphereSubPanel.Atmosphere,
        VerticalOffset = rifle.Zero.VerticalOffset,
        HorizontalOffset = rifle.Zero.HorizontalOffset,
        Wind = ZeroWindSubPanel.Wind,
        ShotAngle = RifleSubPanel.ZeroShotAngle,
    };

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Public Methods

    /// <summary>
    /// Validates the panel state and builds a ShotData allowing null fields for empty panels.
    /// Returns (shotData, emptyPanels, incompletePanels).
    /// shotData is null only when ammunition is not filled.
    /// emptyPanels lists panels left completely empty (defaults can be applied).
    /// incompletePanels lists panels partially filled (user error).
    /// </summary>
    public (ShotData? ShotData, List<string> EmptyPanels, List<string> IncompletePanels) Validate()
    {
        var emptyPanels = new List<string>();
        var incompletePanels = new List<string>();

        var ammoEntry = AmmoLibPanel.LibraryEntry;
        if (ammoEntry == null)
            return (null, emptyPanels, incompletePanels);

        var atmosphere = AtmosphereSubPanel.Atmosphere;
        if (atmosphere == null)
        {
            if (AtmosphereSubPanel.IsEmpty) emptyPanels.Add("Weather");
            else incompletePanels.Add("Weather");
        }

        var rifle = RifleSubPanel.Rifle;
        if (rifle == null)
        {
            if (RifleSubPanel.IsEmpty) emptyPanels.Add("Rifle");
            else incompletePanels.Add("Rifle");
        }

        var parameters = ParametersSubPanel.Parameters;
        if (parameters == null)
        {
            if (ParametersSubPanel.IsEmpty) emptyPanels.Add("Parameters");
            else incompletePanels.Add("Parameters");
        }

        var shotData = new ShotData()
        {
            Ammunition = ammoEntry,
            Weapon = rifle,
            Atmosphere = atmosphere,
            Winds = WindSubPanel.Winds,
            Parameters = parameters,
            Zeroing = rifle == null ? null : BuildZeroing(rifle),
        };

        return (shotData, emptyPanels, incompletePanels);
    }

    public void Clear()
    {
        AmmoLibPanel.Clear();
        AtmosphereSubPanel.Clear();
        WindSubPanel.Clear();
        RifleSubPanel.Clear();
        ZeroAmmoSubPanel.Clear();
        ZeroAtmosphereSubPanel.Clear();
        ZeroWindSubPanel.Clear();
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
        ZeroWindSubPanel.Changed += OnChildChanged;
        ParametersSubPanel.Changed += OnChildChanged;
    }

    private void OnChildChanged(object? sender, EventArgs e)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}
