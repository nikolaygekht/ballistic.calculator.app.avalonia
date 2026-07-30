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
            ZeroSubPanel.FileDialogService = value;
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
            ZeroSubPanel.MeasurementSystem = value;
            ParametersSubPanel.MeasurementSystem = value;
        }
    }

    public ShotData? ShotData
    {
        get
        {
            var ammoEntry = AmmoLibPanel.LibraryEntry;
            var atmosphere = AtmosphereSubPanel.Atmosphere;
            var rifle = BuildRifle();
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
                Zeroing = ZeroSubPanel.Zeroing,
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
                RifleSubPanel.Sight = value.Weapon.Sight;
                RifleSubPanel.Rifling = value.Weapon.Rifling;

                // Zeroing is the source of truth; fall back to Weapon.Zero for older data. The fallback
                // has to be per-field as well as whole-object: a <zeroing> block that exists but carries
                // no distance used to blank the Zero tab even though the rifle's own zero knew it, and OK
                // then refused the shot. The distance is the one field mirrored in both places.
                var zeroing = value.Zeroing ?? ZeroingFromWeapon(value.Weapon);
                if (zeroing.Distance == null && value.Weapon.Zero?.Distance != null)
                    zeroing = WithDistance(zeroing, value.Weapon.Zero.Distance);

                ZeroSubPanel.Zeroing = zeroing;
            }
            else
            {
                RifleSubPanel.Sight = null;
                RifleSubPanel.Rifling = null;
                ZeroSubPanel.Zeroing = value.Zeroing;
            }

            // Parameters must be set AFTER the rifle: V/H clicks are converted to angles using
            // the sight's click sizes, which come from the RiflePanel.
            ParametersSubPanel.Parameters = value.Parameters;
        }
    }

    /// <summary>
    /// Build the library <see cref="Rifle"/> from the sight (Rifle panel), the rifling (Rifle panel)
    /// and the zero distance + impact offsets (Zero panel). Returns null when the sight or the zero
    /// distance is missing.
    /// </summary>
    private Rifle? BuildRifle()
    {
        var sight = RifleSubPanel.Sight;
        var zeroDistance = ZeroSubPanel.ZeroDistance;
        if (sight == null || zeroDistance == null)
            return null;

        var zeroing = ZeroSubPanel.Zeroing;
        var zero = new ZeroingParameters(zeroDistance.Value, null, null)
        {
            VerticalOffset = zeroing?.VerticalOffset,
            HorizontalOffset = zeroing?.HorizontalOffset,
        };

        return new Rifle(sight, zero, RifleSubPanel.Rifling);
    }

    /// <summary>
    /// A copy of <paramref name="zeroing"/> carrying <paramref name="distance"/>. Copied rather than
    /// mutated: the object belongs to the caller's <see cref="ShotData"/>, and filling a gap in the view
    /// is no reason to edit the model behind it.
    /// </summary>
    private static ZeroingData WithDistance(ZeroingData zeroing, Measurement<DistanceUnit>? distance) => new()
    {
        Distance = distance,
        Ammunition = zeroing.Ammunition,
        Atmosphere = zeroing.Atmosphere,
        VerticalOffset = zeroing.VerticalOffset,
        HorizontalOffset = zeroing.HorizontalOffset,
        Wind = zeroing.Wind,
        ShotAngle = zeroing.ShotAngle,
    };

    /// <summary>Fallback for older data that has no &lt;zeroing&gt; block: reconstruct it from the rifle's zero.</summary>
    private static ZeroingData ZeroingFromWeapon(Rifle weapon) => new()
    {
        Distance = weapon.Zero?.Distance,
        Ammunition = weapon.Zero?.Ammunition,
        Atmosphere = weapon.Zero?.Atmosphere,
        VerticalOffset = weapon.Zero?.VerticalOffset,
        HorizontalOffset = weapon.Zero?.HorizontalOffset,
    };

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Public Methods

    /// <summary>
    /// Validates the panel state and builds a ShotData allowing null fields for empty panels.
    /// Returns (shotData, emptyPanels, incompletePanels, problems).
    /// shotData is null only when ammunition is not filled.
    /// emptyPanels lists panels left completely empty (defaults can be applied).
    /// incompletePanels lists panels partially filled (user error).
    /// problems lists specific, named faults — a field that is missing, a combination the engine cannot
    /// compute, or a ticked group whose value would be silently discarded. Every problem found anywhere on
    /// the dialog is collected in one pass, so the user sees all of them at once rather than one per
    /// attempt.
    /// </summary>
    public (ShotData? ShotData, List<string> EmptyPanels, List<string> IncompletePanels,
            List<string> Problems) Validate()
    {
        var emptyPanels = new List<string>();
        var incompletePanels = new List<string>();

        // Collected regardless of whether the ammunition builds — the point is to report everything.
        var problems = new List<string>();
        problems.AddRange(AmmoLibPanel.Problems());
        problems.AddRange(ZeroSubPanel.Problems());
        problems.AddRange(ParametersSubPanel.Problems());

        var ammoEntry = AmmoLibPanel.LibraryEntry;
        if (ammoEntry == null)
            return (null, emptyPanels, incompletePanels, problems);

        var atmosphere = AtmosphereSubPanel.Atmosphere;
        if (atmosphere == null)
        {
            if (AtmosphereSubPanel.IsEmpty) emptyPanels.Add("Weather");
            else incompletePanels.Add("Weather");
        }

        var rifle = BuildRifle();
        if (rifle == null)
        {
            // A missing rifle has two possible causes — the sight (Rifle tab) or the zero distance
            // (Zero tab) — and each must be blamed only for its own. Reporting "Rifle" whenever the
            // rifle failed to build named the Rifle tab for a missing zero distance, which sent the
            // user to stare at a tab whose every field was filled in.
            if (RifleSubPanel.Sight == null)
            {
                if (RifleSubPanel.IsEmpty) emptyPanels.Add("Rifle");
                else incompletePanels.Add("Rifle");
            }

            // The zero distance has no default, so an empty Zero tab cannot be waved through with
            // "use default values?" the way Weather and Parameters can — there is nothing to default it
            // to, and the shot would fail to calculate a moment later. It blocks either way.
            if (ZeroSubPanel.ZeroDistance == null)
                incompletePanels.Add("Zero");
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
            Zeroing = ZeroSubPanel.Zeroing,
        };

        return (shotData, emptyPanels, incompletePanels, problems);
    }

    public void Clear()
    {
        AmmoLibPanel.Clear();
        AtmosphereSubPanel.Clear();
        WindSubPanel.Clear();
        RifleSubPanel.Clear();
        ZeroSubPanel.Clear();
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
        ZeroSubPanel.Changed += OnChildChanged;
        ParametersSubPanel.Changed += OnChildChanged;

        // A sight preset can carry a default zero distance; forward it to the Zero panel.
        RifleSubPanel.ZeroDistanceSuggested += (_, distance) => ZeroSubPanel.SetZeroDistance(distance);
    }

    private void OnChildChanged(object? sender, EventArgs e)
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}
