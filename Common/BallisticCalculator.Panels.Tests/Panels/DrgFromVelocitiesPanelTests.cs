using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Xunit;
using AwesomeAssertions;
using BallisticCalculator;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Panels.Tests.Mocks;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Tests.Panels;

public class DrgFromVelocitiesPanelTests
{
    private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

    private static string Sample(string name) => Path.Combine(AppContext.BaseDirectory, "TestData", name);

    private static string TempCsv(params string[] lines)
    {
        var path = Path.Combine(Path.GetTempPath(), $"drgvel-{Guid.NewGuid():N}.csv");
        File.WriteAllLines(path, lines);
        return path;
    }

    /// <summary>A panel with the two required physics inputs filled in.</summary>
    private static DrgFromVelocitiesPanel PanelWith(MockFileDialogService service)
    {
        var panel = new DrgFromVelocitiesPanel { FileDialogService = service };
        panel.NameBox.Text = "test table";
        panel.WeightControl.SetValue(new Measurement<WeightUnit>(168, WeightUnit.Grain));
        panel.DiameterControl.SetValue(new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch));
        return panel;
    }

    #region Editing

    [AvaloniaFact]
    public void Panel_ShouldStartEmpty()
    {
        var panel = new DrgFromVelocitiesPanel();

        panel.Readings.Should().BeEmpty();
        panel.DetailPanel.IsEnabled.Should().BeFalse();
        panel.Status.Should().Contain("No readings").And.Contain("3");   // states the minimum
    }

    [AvaloniaFact]
    public void Add_ShouldAppendReadingAndSelectIt()
    {
        var panel = new DrgFromVelocitiesPanel();

        Click(panel.AddButton);

        panel.Readings.Should().HaveCount(1);
        panel.ReadingsList.SelectedIndex.Should().Be(0);
        panel.DetailPanel.IsEnabled.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Add_Twice_ShouldStepDownrangeAndSlowDown()
    {
        var panel = new DrgFromVelocitiesPanel();

        Click(panel.AddButton);
        Click(panel.AddButton);

        var readings = panel.Readings;
        readings[1].Distance.In(DistanceUnit.Meter).Should().BeGreaterThan(readings[0].Distance.In(DistanceUnit.Meter));
        readings[1].Velocity.In(VelocityUnit.MetersPerSecond).Should().BeLessThan(readings[0].Velocity.In(VelocityUnit.MetersPerSecond));
    }

    [AvaloniaFact]
    public void Delete_ShouldRemoveSelectedReading()
    {
        var panel = new DrgFromVelocitiesPanel();
        Click(panel.AddButton);
        Click(panel.AddButton);

        panel.ReadingsList.SelectedIndex = 0;
        Click(panel.DeleteButton);

        panel.Readings.Should().HaveCount(1);
    }

    [AvaloniaFact]
    public void EditingDetail_ShouldBeCommittedWhenTheRowIsUsed()
    {
        var panel = new DrgFromVelocitiesPanel();
        Click(panel.AddButton);

        panel.DistanceControl.SetValue(new Measurement<DistanceUnit>(300, DistanceUnit.Yard));
        panel.VelocityControl.SetValue(new Measurement<VelocityUnit>(2847.2, VelocityUnit.FeetPerSecond));

        // Adding the next row commits the detail pane into the row it belongs to; correctness must not
        // depend on a change event, which is not raised for a programmatic set (nor for a unit switch).
        Click(panel.AddButton);

        panel.Readings[0].Distance.In(DistanceUnit.Yard).Should().BeApproximately(300, 1e-6);
        panel.Readings[0].Velocity.In(VelocityUnit.FeetPerSecond).Should().BeApproximately(2847.2, 1e-3);
        panel.Readings[0].Display.Should().Contain("300");
    }

    [AvaloniaFact]
    public void EditingDetail_ShouldBeCommittedWhenTheSelectionMoves()
    {
        var panel = new DrgFromVelocitiesPanel();
        Click(panel.AddButton);
        Click(panel.AddButton);

        panel.ReadingsList.SelectedIndex = 0;
        panel.DistanceControl.SetValue(new Measurement<DistanceUnit>(50, DistanceUnit.Yard));
        panel.ReadingsList.SelectedIndex = 1;

        panel.Readings[0].Distance.In(DistanceUnit.Yard).Should().BeApproximately(50, 1e-6);
    }

    [AvaloniaFact]
    public void EditingDetail_ShouldBeCommittedBeforeBuilding()
    {
        var service = new MockFileDialogService { NextOpenResult = Sample("velocity1.csv") };
        var panel = PanelWith(service);
        Click(panel.ImportButton);

        panel.ReadingsList.SelectedIndex = 0;
        panel.VelocityControl.SetValue(new Measurement<VelocityUnit>(3100, VelocityUnit.FeetPerSecond));
        panel.Build();

        panel.Readings[0].Velocity.In(VelocityUnit.FeetPerSecond).Should().BeApproximately(3100, 1e-3);
    }

    [AvaloniaFact]
    public void Atmosphere_ShouldRoundTripThroughTheSharedPanel()
    {
        var panel = new DrgFromVelocitiesPanel
        {
            Atmosphere = Atmosphere.CreateICAOAtmosphere(new Measurement<DistanceUnit>(5000, DistanceUnit.Foot)),
        };

        panel.Atmosphere.Should().NotBeNull();
        panel.Atmosphere!.Altitude.In(DistanceUnit.Foot).Should().BeApproximately(5000, 1);
    }

    #endregion

    #region Import

    [AvaloniaTheory]
    [InlineData("velocity1.csv", 3078.8, 1994.6)]
    [InlineData("velocity2.csv", 3121.5, 1554.0)]
    public void Import_RealSample_ShouldFillAllReadings(string file, double muzzleFps, double lastFps)
    {
        var service = new MockFileDialogService { NextOpenResult = Sample(file) };
        var panel = PanelWith(service);

        Click(panel.ImportButton);

        panel.Readings.Should().HaveCount(16);
        panel.Readings[0].Distance.In(DistanceUnit.Yard).Should().Be(0);
        panel.Readings[15].Distance.In(DistanceUnit.Yard).Should().Be(1500);
        // Inline units in the file win over the CSV unit combos.
        panel.Readings[0].Velocity.In(VelocityUnit.FeetPerSecond).Should().BeApproximately(muzzleFps, 0.01);
        panel.Readings[15].Velocity.In(VelocityUnit.FeetPerSecond).Should().BeApproximately(lastFps, 0.01);
        panel.Status.Should().Contain("16 readings");
    }

    [AvaloniaFact]
    public void Import_ShouldSortByDistance()
    {
        var service = new MockFileDialogService { NextOpenResult = TempCsv("200yd;2923.9ft/s", "0yd;3078.8ft/s", "100yd;3001.2ft/s") };
        var panel = PanelWith(service);

        Click(panel.ImportButton);

        panel.Readings.Select(r => r.Distance.In(DistanceUnit.Meter)).Should().BeInAscendingOrder();
    }

    [AvaloniaFact]
    public void Import_HeaderNamingVelocityFirst_ShouldNotTransposeColumns()
    {
        var service = new MockFileDialogService
        {
            NextOpenResult = TempCsv("velocity;distance", "3078.8ft/s;0yd", "3001.2ft/s;100yd", "2923.9ft/s;200yd"),
        };
        var panel = PanelWith(service);

        Click(panel.ImportButton);

        panel.Readings.Should().HaveCount(3);
        panel.Readings[0].Distance.In(DistanceUnit.Yard).Should().Be(0);
        panel.Readings[0].Velocity.In(VelocityUnit.FeetPerSecond).Should().BeApproximately(3078.8, 0.01);
    }

    [AvaloniaFact]
    public void Import_BareNumbers_ShouldUseTheCsvUnitCombos()
    {
        var service = new MockFileDialogService { NextOpenResult = TempCsv("0;850", "100;780.2", "200;714.9") };
        var panel = PanelWith(service);
        panel.MeasurementSystem = MeasurementSystem.Metric;      // sets the CSV combos to m and m/s

        Click(panel.ImportButton);

        panel.Readings.Should().HaveCount(3);
        panel.Readings[1].Distance.In(DistanceUnit.Meter).Should().BeApproximately(100, 1e-6);
        panel.Readings[1].Velocity.In(VelocityUnit.MetersPerSecond).Should().BeApproximately(780.2, 1e-3);
    }

    [AvaloniaFact]
    public void Import_BadLine_ShouldRejectFileAndKeepExistingReadings()
    {
        var service = new MockFileDialogService { NextOpenResult = Sample("velocity1.csv") };
        var panel = PanelWith(service);
        Click(panel.ImportButton);
        var before = panel.Readings.Select(r => (r.Distance, r.Velocity)).ToArray();

        // The typo that used to sit on line 16 of velocity2.csv.
        service.NextOpenResult = TempCsv("0yd;3078.8ft/s", "100yd;3001.2ft/s", "1400 d;1643.2ft/s");
        Click(panel.ImportButton);

        panel.Readings.Select(r => (r.Distance, r.Velocity)).Should().Equal(before);
        panel.Status.Should().Contain("3").And.Contain("Nothing was imported");
    }

    #endregion

    #region Save

    [AvaloniaFact]
    public void Save_WithTooFewReadings_ShouldReportAndNotOpenTheFileDialog()
    {
        var service = new MockFileDialogService();
        var panel = PanelWith(service);
        Click(panel.AddButton);
        Click(panel.AddButton);

        Click(panel.SaveButton);

        service.LastSaveOptions.Should().BeNull();
        panel.Status.Should().Contain("3");
    }

    [AvaloniaFact]
    public void Save_WithoutWeight_ShouldReportAndNotOpenTheFileDialog()
    {
        var service = new MockFileDialogService { NextOpenResult = Sample("velocity1.csv") };
        var panel = new DrgFromVelocitiesPanel { FileDialogService = service };
        panel.NameBox.Text = "no weight";
        panel.DiameterControl.SetValue(new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch));
        Click(panel.ImportButton);

        Click(panel.SaveButton);

        service.LastSaveOptions.Should().BeNull();
        panel.Status.Should().Contain("weight");
    }

    [AvaloniaFact]
    public void Save_WithRisingVelocity_ShouldReportWhichReading()
    {
        var service = new MockFileDialogService
        {
            NextOpenResult = TempCsv("0yd;3000ft/s", "100yd;3010ft/s", "200yd;2900ft/s"),
        };
        var panel = PanelWith(service);
        Click(panel.ImportButton);

        Click(panel.SaveButton);

        service.LastSaveOptions.Should().BeNull();
        panel.Status.Should().Contain("decrease");
    }

    [AvaloniaFact]
    public void Save_ShouldWriteADrgWithMetadataThatReloads()
    {
        var path = Path.Combine(Path.GetTempPath(), $"drgvel-{Guid.NewGuid():N}.drg");
        var service = new MockFileDialogService { NextOpenResult = Sample("velocity1.csv"), NextSaveResult = path };
        var panel = PanelWith(service);
        panel.NameBox.Text = "308 168gr radar";
        panel.SourceBox.Text = "LabRadar";
        panel.LengthControl.SetValue(new Measurement<DistanceUnit>(1.215, DistanceUnit.Inch));
        Click(panel.ImportButton);

        try
        {
            Click(panel.SaveButton);

            File.Exists(path).Should().BeTrue(panel.Status);

            var table = DrgDragTable.Open(path);
            table.TableId.Should().Be(DragTableId.GC);
            table.Ammunition!.Name.Should().Be("308 168gr radar");
            table.Ammunition.Source.Should().Be("LabRadar");
            table.Ammunition.Ammunition.Weight.In(WeightUnit.Grain).Should().BeApproximately(168, 0.01);
            table.Ammunition.Ammunition.BulletLength!.Value.In(DistanceUnit.Inch).Should().BeApproximately(1.215, 1e-4);
            panel.Status.Should().Contain("Saved");
        }
        finally
        {
            File.Delete(path);
        }
    }

    #endregion
}
