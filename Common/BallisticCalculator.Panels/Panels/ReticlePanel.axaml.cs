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
    /// <summary>
    /// Fine step size for BDC/target trajectory recalculation (same as old WinForms app).
    /// At close range, 50yd steps miss the steep drop where bullet is well below sight line.
    /// 2.5m (~2.7yd) captures the near-BDC points correctly.
    /// </summary>
    private static readonly Measurement<DistanceUnit> FineStep = new(2.5, DistanceUnit.Meter);
    private static readonly Measurement<DistanceUnit> FineMaxDistance = new(1500, DistanceUnit.Meter);

    private MeasurementSystem _measurementSystem = MeasurementSystem.Imperial;
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
            RecalculateFineTrajectory();
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
        ApplyMeasurementSystem();
    }

    private void WireEvents()
    {
        TargetDistanceControl.Changed += OnTargetParameterChangedInternal;
        TargetWidthControl.ValueChanged += (s, e) => OnTargetParameterChangedInternal(s, EventArgs.Empty);
        TargetHeightControl.ValueChanged += (s, e) => OnTargetParameterChangedInternal(s, EventArgs.Empty);
        TargetSizeUnitsCombo.SelectionChanged += (s, e) => OnTargetParameterChangedInternal(s, EventArgs.Empty);
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
            TargetDistanceControl.ChangeUnit(DistanceUnit.Meter, 0);
        else
            TargetDistanceControl.ChangeUnit(DistanceUnit.Yard, 0);
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

    #region Trajectory Calculation

    private void RecalculateFineTrajectory()
    {
        if (_shotData?.Ammunition?.Ammunition == null ||
            _shotData?.Weapon == null ||
            _shotData?.Parameters == null)
        {
            _fineTrajectory = null;
            _zeroDistance = default;
            return;
        }

        _zeroDistance = _shotData.Weapon.Zero?.Distance ??
            new Measurement<DistanceUnit>(100, DistanceUnit.Yard);

        try
        {
            var calc = new TrajectoryCalculator();
            var ammo = _shotData.Ammunition.Ammunition;
            var weapon = _shotData.Weapon;
            var atmosphere = _shotData.Atmosphere ?? new Atmosphere();
            var zeroAmmo = weapon.Zero?.Ammunition ?? ammo;
            var zeroAtmosphere = weapon.Zero?.Atmosphere ?? atmosphere;

            var shotParameters = new ShotParameters
            {
                BarrelAzimuth = _shotData.Parameters.BarrelAzimuth,
                CantAngle = _shotData.Parameters.CantAngle,
                MaximumDistance = FineMaxDistance,
                ShotAngle = _shotData.Parameters.ShotAngle,
                Step = FineStep,
                SightAngle = calc.SightAngle(zeroAmmo, weapon, zeroAtmosphere),
            };

            var trajectory = calc.Calculate(ammo, weapon, atmosphere,
                shotParameters, _shotData.Winds);

            // Trim trailing nulls
            var count = 0;
            for (var i = 0; i < trajectory.Length; i++)
            {
                if (trajectory[i] == null) break;
                count++;
            }

            if (count < trajectory.Length)
            {
                var trimmed = new TrajectoryPoint[count];
                Array.Copy(trajectory, trimmed, count);
                _fineTrajectory = trimmed;
            }
            else
            {
                _fineTrajectory = trajectory;
            }
        }
        catch
        {
            _fineTrajectory = null;
        }
    }

    #endregion

    #region Reticle Update

    private void UpdateReticle()
    {
        if (_reticle == null || _fineTrajectory == null || _fineTrajectory.Length < 3)
        {
            ReticleCanvas.Overlay = null;
            ReticleCanvas.Underlay = null;
            ReticleCanvas.InvalidateVisual();
            return;
        }

        var calculator = new TrajectoryToReticleCalculator(
            _fineTrajectory, _reticle.BulletDropCompensator, _zeroDistance);

        if (RadioFarBdc.IsChecked == true)
        {
            ReticleCanvas.Underlay = null;
            ReticleCanvas.Overlay = ReticleOverlayController.CreateBdcOverlay(
                calculator, _measurementSystem, far: true);
        }
        else if (RadioNearBdc.IsChecked == true)
        {
            ReticleCanvas.Underlay = null;
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
            }
            else
            {
                ReticleCanvas.Underlay = null;
            }
            ReticleCanvas.Overlay = null;
        }
        else
        {
            ReticleCanvas.Overlay = null;
            ReticleCanvas.Underlay = null;
        }

        ReticleCanvas.InvalidateVisual();
    }

    #endregion
}
