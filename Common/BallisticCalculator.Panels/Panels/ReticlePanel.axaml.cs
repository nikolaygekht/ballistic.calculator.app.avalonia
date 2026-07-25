using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator;
using BallisticCalculator.Controls.Controllers;
using BallisticCalculator.Reticle;
using BallisticCalculator.Reticle.Data;
using BallisticCalculator.Serialization;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using System;
using System.IO;

[assembly: InternalsVisibleTo("BallisticCalculator.Panels.Tests")]

namespace BallisticCalculator.Panels.Panels;

public partial class ReticlePanel : UserControl
{
    private MeasurementSystem _measurementSystem = MeasurementSystem.Imperial;
    private AngularUnit _angularUnits = AngularUnit.Mil;
    private ReticleDefinition? _reticle;
    private ShotData? _shotData;
    private TrajectoryPoint[]? _fineTrajectory;
    private Measurement<DistanceUnit> _zeroDistance;

    public ReticlePanel()
    {
        InitializeComponent();
        InitializeControls();
        WireEvents();
    }

    #region Properties

    public IFileDialogService? FileDialogService { get; set; }

    public MeasurementSystem MeasurementSystem
    {
        get => _measurementSystem;
        set
        {
            if (_measurementSystem == value) return;
            _measurementSystem = value;
            ApplyMeasurementSystem();
            UpdateReticle();
        }
    }

    /// <summary>
    /// Angular unit used to display the computed target angular size and moving-target lead.
    /// Set by the host from the active window's View → Angular units selection.
    /// </summary>
    public AngularUnit AngularUnits
    {
        get => _angularUnits;
        set
        {
            if (_angularUnits == value) return;
            _angularUnits = value;
            UpdateReticle();
        }
    }

    public ReticleDefinition? Reticle
    {
        get => _reticle;
        set
        {
            _reticle = value;
            ReticleCanvas.Reticle = value;
            ReticleNameText.Text = value?.Name ?? "(none)";
            UpdateReticle();
        }
    }

    public ShotData? ShotData
    {
        get => _shotData;
        set
        {
            _shotData = value;
            _zeroDistance = value?.Zeroing?.Distance ?? value?.Weapon?.Zero?.Distance
                ?? new Measurement<DistanceUnit>(100, DistanceUnit.Yard);
            UpdateReticle();
        }
    }

    /// <summary>
    /// The fine trajectory used for BDC placement. Provided by the host (built once via
    /// <see cref="ShotTrajectoryCalculator.CalculateFine"/> and shared with the summary analysis).
    /// </summary>
    public TrajectoryPoint[]? FineTrajectory
    {
        get => _fineTrajectory;
        set
        {
            _fineTrajectory = value;
            UpdateReticle();
        }
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Initialization

    private void InitializeControls()
    {
        TargetDistanceControl.UnitType = typeof(DistanceUnit);
        TargetDistanceControl.Minimum = 0;
        TargetDistanceControl.Increment = 10;

        MovingSpeedControl.UnitType = typeof(VelocityUnit);
        MovingSpeedControl.Minimum = 0;
        MovingSpeedControl.Increment = 1;

        MovingDirectionInput.UnitType = typeof(AngularUnit);
        MovingDirectionInput.Minimum = 0;
        MovingDirectionInput.Maximum = 360;
        MovingDirectionInput.Increment = 1;
        MovingDirectionInput.ChangeUnit(AngularUnit.Degree, 0, false);
        MovingDirectionInput.SetValue(new Measurement<AngularUnit>(0, AngularUnit.Degree));

        ApplyMeasurementSystem();
    }

    private void WireEvents()
    {
        TargetDistanceControl.Changed += OnTargetParameterChangedInternal;
        TargetWidthControl.ValueChanged += (s, e) => OnTargetParameterChangedInternal(s, EventArgs.Empty);
        TargetHeightControl.ValueChanged += (s, e) => OnTargetParameterChangedInternal(s, EventArgs.Empty);
        TargetSizeUnitsCombo.SelectionChanged += (s, e) => OnTargetParameterChangedInternal(s, EventArgs.Empty);

        MovingTargetCheck.IsCheckedChanged += OnMovingTargetToggled;
        MovingSpeedControl.Changed += OnTargetParameterChangedInternal;

        // Two-way sync between the numeric direction input (degrees) and the compass dial;
        // the dial's Direction is the source of truth used by the overlay/lead calculation.
        MovingDirectionInput.Changed += (s, e) =>
        {
            if (_syncingDirection) return;
            var value = MovingDirectionInput.IsEmpty ? null : MovingDirectionInput.GetValue<AngularUnit>();
            if (value != null)
            {
                _syncingDirection = true;
                MovingDirectionControl.Direction = value.Value.In(AngularUnit.Degree);
                _syncingDirection = false;
            }
            OnTargetParameterChangedInternal(s, EventArgs.Empty);
        };
        MovingDirectionControl.Changed += (s, e) =>
        {
            if (_syncingDirection) return;
            _syncingDirection = true;
            MovingDirectionInput.SetValue(new Measurement<AngularUnit>(
                MovingDirectionControl.Direction, AngularUnit.Degree));
            _syncingDirection = false;
            OnTargetParameterChangedInternal(s, EventArgs.Empty);
        };
    }

    private bool _syncingDirection;

    private void OnMovingTargetToggled(object? sender, RoutedEventArgs e)
    {
        MovingTargetControlsPanel.IsEnabled = MovingTargetCheck.IsChecked == true;
        OnTargetParameterChangedInternal(sender, EventArgs.Empty);
    }

    private void OnTargetParameterChangedInternal(object? sender, EventArgs e)
    {
        if (RadioTarget.IsChecked == true)
            UpdateReticle();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyMeasurementSystem()
    {
        if (_measurementSystem == MeasurementSystem.Metric)
        {
            TargetDistanceControl.ChangeUnit(DistanceUnit.Meter, 0);
            MovingSpeedControl.ChangeUnit(VelocityUnit.KilometersPerHour, 1);
        }
        else
        {
            TargetDistanceControl.ChangeUnit(DistanceUnit.Yard, 0);
            MovingSpeedControl.ChangeUnit(VelocityUnit.MilesPerHour, 1);
        }
    }

    #endregion

    #region Event Handlers

    private async void OnLoadReticle(object? sender, RoutedEventArgs e)
    {
        if (FileDialogService == null)
            return;

        var path = await FileDialogService.OpenFileAsync(new FileDialogOptions
        {
            Title = "Open Reticle",
            DefaultExtension = ".reticle",
            Filters = { new Services.FileDialogFilter("Reticles", "reticle") },
        });

        if (path == null)
            return;

        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            Reticle = fs.BallisticXmlDeserialize<ReticleDefinition>();
        }
        catch (Exception ex)
        {
            ReticleNameText.Text = $"Error: {ex.Message}";
        }
    }

    private void OnMilDot(object? sender, RoutedEventArgs e)
    {
        Reticle = new MilDotReticle();
    }

    private void OnDisplayModeChanged(object? sender, RoutedEventArgs e)
    {
        TargetControlsPanel.IsEnabled = RadioTarget.IsChecked == true;
        UpdateReticle();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Reticle Update

    private void UpdateReticle()
    {
        if (_reticle == null || _fineTrajectory == null || _fineTrajectory.Length < 3)
        {
            ReticleCanvas.Overlay = null;
            ReticleCanvas.Underlay = null;
            ReticleCanvas.MovingTargetBox = null;
            ClearInfoTexts();
            ReticleCanvas.InvalidateVisual();
            return;
        }

        // Info texts apply only to Target mode; clear them here and set them in that branch.
        ClearInfoTexts();

        var calculator = new TrajectoryToReticleCalculator(
            _fineTrajectory, _reticle.BulletDropCompensator, _zeroDistance);

        if (RadioFarBdc.IsChecked == true)
        {
            ReticleCanvas.Underlay = null;
            ReticleCanvas.MovingTargetBox = null;
            ReticleCanvas.Overlay = ReticleOverlayController.CreateBdcOverlay(
                calculator, _measurementSystem, far: true);
        }
        else if (RadioNearBdc.IsChecked == true)
        {
            ReticleCanvas.Underlay = null;
            ReticleCanvas.MovingTargetBox = null;
            ReticleCanvas.Overlay = ReticleOverlayController.CreateBdcOverlay(
                calculator, _measurementSystem, far: false);
        }
        else if (RadioTarget.IsChecked == true)
        {
            var sizeUnits = TargetSizeUnitsCombo.SelectedIndex == 0
                ? DistanceUnit.Inch
                : DistanceUnit.Centimeter;
            var width = ((double)(TargetWidthControl.Value ?? 6)).As(sizeUnits);
            var height = ((double)(TargetHeightControl.Value ?? 6)).As(sizeUnits);
            var distance = TargetDistanceControl.GetValue<DistanceUnit>();

            if (distance != null)
            {
                var target = ReticleOverlayController.CreateTargetOverlay(
                    calculator, width, height, distance.Value);

                if (target != null)
                {
                    var underlay = new ReticleElementsCollection();
                    underlay.Add(target);
                    ReticleCanvas.Underlay = underlay;
                }
                else
                {
                    ReticleCanvas.Underlay = null;
                }

                ReticleCanvas.MovingTargetBox = CalculateMovingTargetBox(
                    calculator, width, height, distance.Value);

                UpdateInfoTexts(calculator, width, height, distance.Value);
            }
            else
            {
                ReticleCanvas.Underlay = null;
                ReticleCanvas.MovingTargetBox = null;
            }
            ReticleCanvas.Overlay = null;
        }
        else
        {
            ReticleCanvas.Overlay = null;
            ReticleCanvas.Underlay = null;
            ReticleCanvas.MovingTargetBox = null;
        }

        ReticleCanvas.InvalidateVisual();
    }

    /// <summary>
    /// Builds the dashed moving-target "aim here" box when the moving-target option is enabled
    /// and a crossing speed is set; otherwise returns null. Uses the same target size/distance
    /// as the static target box.
    /// </summary>
    private ReticleRectangle? CalculateMovingTargetBox(
        TrajectoryToReticleCalculator calculator,
        Measurement<DistanceUnit> width,
        Measurement<DistanceUnit> height,
        Measurement<DistanceUnit> distance)
    {
        if (MovingTargetCheck.IsChecked != true)
            return null;

        var speed = MovingSpeedControl.GetValue<VelocityUnit>();
        if (speed == null || speed.Value.Value <= 0)
            return null;

        var direction = new Measurement<AngularUnit>(
            MovingDirectionControl.Direction, AngularUnit.Degree);

        return ReticleOverlayController.CreateMovingTargetOverlay(
            calculator, width, height, distance, speed.Value, direction);
    }

    private void ClearInfoTexts()
    {
        TargetAngularSizeText.Text = "";
        LeadCorrectionText.Text = "";
    }

    /// <summary>
    /// Updates the read-only info lines under the Target inputs: the target's angular size and,
    /// when the moving-target option is on, the computed lead hold-off — both in the current
    /// angular unit.
    /// </summary>
    private void UpdateInfoTexts(
        TrajectoryToReticleCalculator calculator,
        Measurement<DistanceUnit> width,
        Measurement<DistanceUnit> height,
        Measurement<DistanceUnit> distance)
    {
        var units = new MeasurementSystemController(_measurementSystem, _angularUnits);

        var angularWidth = ReticleOverlayController.CalculateAngularSize(width, distance);
        var angularHeight = ReticleOverlayController.CalculateAngularSize(height, distance);
        TargetAngularSizeText.Text =
            $"Angular: {units.FormatAngular(angularWidth)} × {units.FormatAngular(angularHeight)} {units.AngularUnitName}";

        if (MovingTargetCheck.IsChecked == true)
        {
            var speed = MovingSpeedControl.GetValue<VelocityUnit>();
            if (speed != null && speed.Value.Value > 0)
            {
                var direction = new Measurement<AngularUnit>(
                    MovingDirectionControl.Direction, AngularUnit.Degree);
                var lead = ReticleOverlayController.CalculateMovingTargetLead(
                    calculator, distance, speed.Value, direction);
                if (lead != null)
                {
                    // Lead sign: left +, right -. Show magnitude plus the hold direction.
                    var hold = lead.Value.Value >= 0 ? "left" : "right";
                    LeadCorrectionText.Text =
                        $"Lead: {units.FormatAngular(lead.Value.Abs())} {units.AngularUnitName} {hold}";
                }
            }
        }
    }

    #endregion
}
