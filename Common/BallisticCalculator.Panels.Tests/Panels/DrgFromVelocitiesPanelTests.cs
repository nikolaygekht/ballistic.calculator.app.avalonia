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

    private static void Enter(DrgFromVelocitiesPanel panel, double yards, double fps)
    {
        panel.DistanceControl.SetValue(new Measurement<DistanceUnit>(yards, DistanceUnit.Yard));
        panel.VelocityControl.SetValue(new Measurement<VelocityUnit>(fps, VelocityUnit.FeetPerSecond));
    }

    #region Editing

    [AvaloniaFact]
    public void Panel_ShouldStartEmpty()
    {
        var panel = new DrgFromVelocitiesPanel();

        panel.Readings.Should().BeEmpty();
        panel.Status.Should().Contain("No readings")
             .And.Contain("3")                       // states the minimum
             .And.Contain("standard atmosphere");
    }

    [AvaloniaFact]
    public void Add_ShouldAppendTheEntryAndSelectIt()
    {
        var panel = new DrgFromVelocitiesPanel();
        Enter(panel, 100, 3001.2);

        Click(panel.AddButton);

        panel.Readings.Should().HaveCount(1);
        panel.Readings[0].Distance.In(DistanceUnit.Yard).Should().BeApproximately(100, 1e-6);
        panel.Readings[0].Velocity.In(VelocityUnit.FeetPerSecond).Should().BeApproximately(3001.2, 1e-3);
        panel.ReadingsGrid.SelectedItem.Should().BeSameAs(panel.Readings[0]);
    }

    [AvaloniaFact]
    public void Add_WithoutVelocity_ShouldReportAndAddNothing()
    {
        var panel = new DrgFromVelocitiesPanel();
        panel.DistanceControl.SetValue(new Measurement<DistanceUnit>(100, DistanceUnit.Yard));

        Click(panel.AddButton);

        panel.Readings.Should().BeEmpty();
        panel.Status.Should().Contain("velocity");
    }

    [AvaloniaFact]
    public void Change_ShouldWriteTheEntryIntoTheSelectedRow()
    {
        var panel = new DrgFromVelocitiesPanel();
        Enter(panel, 100, 3001.2);
        Click(panel.AddButton);

        Enter(panel, 300, 2847.2);
        Click(panel.ChangeButton);

        panel.Readings.Should().HaveCount(1);
        panel.Readings[0].Distance.In(DistanceUnit.Yard).Should().BeApproximately(300, 1e-6);
        panel.Readings[0].Velocity.In(VelocityUnit.FeetPerSecond).Should().BeApproximately(2847.2, 1e-3);
        panel.Readings[0].DistanceText.Should().Contain("300");
    }

    [AvaloniaFact]
    public void SelectingARow_ShouldLoadItIntoTheEntryFields()
    {
        // A DataGrid only raises SelectionChanged once its rows are materialized, so this behaviour needs a
        // shown window rather than a bare panel.
        var panel = new DrgFromVelocitiesPanel();
        var window = new Window { Content = panel, Width = 600, Height = 640 };
        window.Show();
        Enter(panel, 0, 3078.8);
        Click(panel.AddButton);
        Enter(panel, 100, 3001.2);
        Click(panel.AddButton);

        panel.ReadingsGrid.SelectedItem = panel.Readings[0];

        panel.DistanceControl.GetValue<DistanceUnit>()!.Value.In(DistanceUnit.Yard).Should().Be(0);
        panel.VelocityControl.GetValue<VelocityUnit>()!.Value.In(VelocityUnit.FeetPerSecond).Should().BeApproximately(3078.8, 1e-3);
    }

    [AvaloniaFact]
    public void Delete_ShouldRemoveSelectedRow()
    {
        var panel = new DrgFromVelocitiesPanel();
        Enter(panel, 0, 3078.8);
        Click(panel.AddButton);
        Enter(panel, 100, 3001.2);
        Click(panel.AddButton);

        panel.ReadingsGrid.SelectedItem = panel.Readings[0];
        Click(panel.DeleteButton);

        panel.Readings.Should().HaveCount(1);
        panel.Readings[0].Distance.In(DistanceUnit.Yard).Should().BeApproximately(100, 1e-6);
    }

    [AvaloniaFact]
    public void Sort_ShouldOrderByDistance()
    {
        var panel = new DrgFromVelocitiesPanel();
        Enter(panel, 200, 2923.9);
        Click(panel.AddButton);
        Enter(panel, 0, 3078.8);
        Click(panel.AddButton);

        Click(panel.SortButton);

        panel.Readings.Select(r => r.Distance.In(DistanceUnit.Meter)).Should().BeInAscendingOrder();
    }

    [AvaloniaFact]
    public void Atmosphere_ShouldBeReportedInTheStatus()
    {
        var panel = new DrgFromVelocitiesPanel
        {
            Atmosphere = Atmosphere.CreateICAOAtmosphere(new Measurement<DistanceUnit>(5000, DistanceUnit.Foot)),
        };

        panel.Atmosphere.Should().NotBeNull();
        panel.Status.Should().NotContain("standard atmosphere");
    }

    [AvaloniaFact]
    public void SetAtmosphereButton_ShouldAskTheHost()
    {
        var panel = new DrgFromVelocitiesPanel();
        var asked = false;
        panel.AtmosphereRequested += (_, _) => asked = true;

        Click(panel.AtmosphereButton);

        asked.Should().BeTrue();
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

    // Units are required in the file: reading a yards file as metres yields a plausible curve that is
    // quietly wrong, and only the file knows which it is.
    [AvaloniaFact]
    public void Import_BareNumbers_ShouldRejectAndSayWhatIsMissing()
    {
        var service = new MockFileDialogService { NextOpenResult = TempCsv("0;850", "100;780.2", "200;714.9") };
        var panel = PanelWith(service);

        Click(panel.ImportButton);

        panel.Readings.Should().BeEmpty();
        panel.Status.Should().Contain("unit").And.Contain("Nothing was imported");
    }

    [AvaloniaFact]
    public void Import_MetricUnitsInFile_ShouldBeTakenFromTheFile()
    {
        var service = new MockFileDialogService { NextOpenResult = TempCsv("0m;850m/s", "100m;780.2m/s", "200m;714.9m/s") };
        var panel = PanelWith(service);

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
        Enter(panel, 0, 3078.8);
        Click(panel.AddButton);
        Enter(panel, 100, 3001.2);
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

    [AvaloniaFact]
    public void CloseButton_ShouldRaiseCloseRequested()
    {
        var panel = new DrgFromVelocitiesPanel();
        var raised = false;
        panel.CloseRequested += (_, _) => raised = true;

        Click(panel.CloseButton);

        raised.Should().BeTrue();
    }

    #endregion
}
