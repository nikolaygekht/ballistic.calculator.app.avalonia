using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Xunit;
using AwesomeAssertions;
using BallisticCalculator;
using BallisticCalculator.Controls.Models;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Panels.Tests.Mocks;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Tests.Panels;

public class DrgFromBcPanelTests
{
    private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

    private static string Sample(string name) => Path.Combine(AppContext.BaseDirectory, "TestData", name);

    private static string TempCsv(params string[] lines)
    {
        var path = Path.Combine(Path.GetTempPath(), $"drgbc-{Guid.NewGuid():N}.csv");
        File.WriteAllLines(path, lines);
        return path;
    }

    /// <summary>
    /// A panel with everything a save needs except the knots. Weight and diameter are inputs, not metadata:
    /// since BallisticCalculator 1.1.11.3 the curve is scaled by the bullet's sectional density.
    /// </summary>
    private static DrgFromBcPanel PanelWith(MockFileDialogService service)
    {
        var panel = new DrgFromBcPanel { FileDialogService = service };
        panel.NameBox.Text = "test table";
        panel.WeightControl.SetValue(new Measurement<WeightUnit>(285, WeightUnit.Grain));
        panel.DiameterControl.SetValue(new Measurement<DistanceUnit>(0.338, DistanceUnit.Inch));
        return panel;
    }

    /// <summary>Fills the entry row the way a user would before pressing Add or Change.</summary>
    private static void Enter(DrgFromBcPanel panel, double mach, double bc, DragTableId table = DragTableId.G7)
    {
        panel.MachControl.Value = (decimal)mach;
        panel.BcControl.Value = new BallisticCoefficient(bc, table);
    }

    #region Editing

    [AvaloniaFact]
    public void Panel_ShouldStartEmptyWithG7()
    {
        var panel = new DrgFromBcPanel();

        panel.Knots.Should().BeEmpty();
        (panel.BaseTableCombo.SelectedItem as DragTableInfo)!.Value.Should().Be(DragTableId.G7);
        panel.Status.Should().Contain("No knots").And.Contain("mach;bc");
    }

    [AvaloniaFact]
    public void Add_ShouldAppendTheEntryAndSelectIt()
    {
        var panel = new DrgFromBcPanel();
        Enter(panel, 1.5, 0.462);

        Click(panel.AddButton);

        panel.Knots.Should().HaveCount(1);
        panel.Knots[0].Mach.Should().BeApproximately(1.5, 1e-9);
        panel.Knots[0].Bc.Value.Should().BeApproximately(0.462, 1e-9);
        panel.KnotsGrid.SelectedItem.Should().BeSameAs(panel.Knots[0]);
        panel.Status.Should().Contain("1 knot");
    }

    [AvaloniaFact]
    public void Add_WithoutMach_ShouldReportAndAddNothing()
    {
        var panel = new DrgFromBcPanel();
        panel.BcControl.Value = new BallisticCoefficient(0.462, DragTableId.G7);

        Click(panel.AddButton);

        panel.Knots.Should().BeEmpty();
        panel.Status.Should().Contain("Mach");
    }

    [AvaloniaFact]
    public void Change_ShouldWriteTheEntryIntoTheSelectedRow()
    {
        var panel = new DrgFromBcPanel();
        Enter(panel, 1.5, 0.462);
        Click(panel.AddButton);

        Enter(panel, 1.75, 0.463);
        Click(panel.ChangeButton);

        panel.Knots.Should().HaveCount(1);
        panel.Knots[0].Mach.Should().BeApproximately(1.75, 1e-9);
        panel.Knots[0].Bc.Value.Should().BeApproximately(0.463, 1e-9);
        panel.Knots[0].MachText.Should().Contain("1.75");
    }

    [AvaloniaFact]
    public void Change_WithNothingSelected_ShouldReport()
    {
        var panel = new DrgFromBcPanel();
        Enter(panel, 1.5, 0.462);

        Click(panel.ChangeButton);

        panel.Status.Should().Contain("Select");
    }

    [AvaloniaFact]
    public void SelectingARow_ShouldLoadItIntoTheEntryFields()
    {
        // A DataGrid only raises SelectionChanged once its rows are materialized, so this behaviour needs a
        // shown window rather than a bare panel.
        var panel = new DrgFromBcPanel();
        var window = new Window { Content = panel, Width = 600, Height = 620 };
        window.Show();
        Enter(panel, 1.5, 0.462);
        Click(panel.AddButton);
        Enter(panel, 2.25, 0.480, DragTableId.G1);
        Click(panel.AddButton);

        panel.KnotsGrid.SelectedItem = panel.Knots[0];

        panel.MachControl.Value.Should().Be(1.5m);
        panel.BcControl.Value!.Value.Value.Should().BeApproximately(0.462, 1e-9);
        panel.BcControl.Value!.Value.Table.Should().Be(DragTableId.G7);
    }

    [AvaloniaFact]
    public void Delete_ShouldRemoveSelectedRow()
    {
        var panel = new DrgFromBcPanel();
        Enter(panel, 1.5, 0.462);
        Click(panel.AddButton);
        Enter(panel, 2.0, 0.470);
        Click(panel.AddButton);

        panel.KnotsGrid.SelectedItem = panel.Knots[0];
        Click(panel.DeleteButton);

        panel.Knots.Should().HaveCount(1);
        panel.Knots[0].Mach.Should().BeApproximately(2.0, 1e-9);
    }

    [AvaloniaFact]
    public void Sort_ShouldOrderByMach()
    {
        var panel = new DrgFromBcPanel();
        Enter(panel, 2.5, 0.484);
        Click(panel.AddButton);
        Enter(panel, 1.5, 0.462);
        Click(panel.AddButton);

        Click(panel.SortButton);

        panel.Knots.Select(k => k.Mach).Should().BeInAscendingOrder();
    }

    #endregion

    #region Import

    [AvaloniaTheory]
    [InlineData("mbc1.csv", 5, DragTableId.G7)]
    [InlineData("mbc2.csv", 8, DragTableId.G1)]
    public void Import_RealSample_ShouldFillKnotsAndTakeBaseTableFromFile(string file, int knots, DragTableId table)
    {
        var service = new MockFileDialogService { NextOpenResult = Sample(file) };
        var panel = PanelWith(service);

        Click(panel.ImportButton);

        panel.Knots.Should().HaveCount(knots);
        panel.Knots.Select(k => k.Bc.Table).Should().AllBeEquivalentTo(table);
        (panel.BaseTableCombo.SelectedItem as DragTableInfo)!.Value.Should().Be(table);
        panel.Status.Should().Contain($"{knots} knots").And.Contain(table.ToString());
    }

    [AvaloniaFact]
    public void Import_ShouldSortKnotsByMach()
    {
        var service = new MockFileDialogService { NextOpenResult = TempCsv("2.25;0.318", "1.20;0.307", "1.65;0.301") };
        var panel = PanelWith(service);

        Click(panel.ImportButton);

        panel.Knots.Select(k => k.Mach).Should().BeInAscendingOrder();
    }

    [AvaloniaFact]
    public void Import_HeaderNamingBcFirst_ShouldNotTransposeColumns()
    {
        var service = new MockFileDialogService { NextOpenResult = TempCsv("bc;mach", "0.307;1.20", "0.318;2.25") };
        var panel = PanelWith(service);

        Click(panel.ImportButton);

        panel.Knots.Should().HaveCount(2);
        panel.Knots[0].Mach.Should().BeApproximately(1.20, 1e-9);
        panel.Knots[0].Bc.Value.Should().BeApproximately(0.307, 1e-9);
    }

    [AvaloniaFact]
    public void Import_MixedTables_ShouldKeepThemAndSayTheyWillBeConverted()
    {
        var service = new MockFileDialogService { NextOpenResult = TempCsv("1.5;0.883G1", "2.0;0.470G7") };
        var panel = PanelWith(service);

        Click(panel.ImportButton);

        panel.Knots.Select(k => k.Bc.Table).Should().BeEquivalentTo(new[] { DragTableId.G1, DragTableId.G7 });
        panel.Status.Should().Contain("converted");
    }

    [AvaloniaFact]
    public void Import_BadLine_ShouldRejectFileAndKeepExistingKnots()
    {
        var service = new MockFileDialogService { NextOpenResult = Sample("mbc1.csv") };
        var panel = PanelWith(service);
        Click(panel.ImportButton);
        var before = panel.Knots.Select(k => (k.Mach, k.Bc.Value)).ToArray();

        service.NextOpenResult = TempCsv("1.5;0.462", "oops;0.463", "2;0.470");
        Click(panel.ImportButton);

        panel.Knots.Select(k => (k.Mach, k.Bc.Value)).Should().Equal(before);
        panel.Status.Should().Contain("2").And.Contain("Nothing was imported");
    }

    [AvaloniaFact]
    public void Import_VelocityInsteadOfMach_ShouldRejectRatherThanTreatItAsMach()
    {
        // 2700 is not a plausible Mach number; reading it as one would produce a nonsense curve.
        var service = new MockFileDialogService { NextOpenResult = TempCsv("2700;0.307", "1800;0.318") };
        var panel = PanelWith(service);

        Click(panel.ImportButton);

        panel.Knots.Should().BeEmpty();
        panel.Status.Should().Contain("Mach");
    }

    #endregion

    #region Save

    [AvaloniaFact]
    public void Save_WithoutName_ShouldReportAndNotOpenTheFileDialog()
    {
        var service = new MockFileDialogService { NextOpenResult = Sample("mbc1.csv") };
        var panel = PanelWith(service);
        panel.NameBox.Text = "";
        Click(panel.ImportButton);

        Click(panel.SaveButton);

        service.LastSaveOptions.Should().BeNull();
        panel.Status.Should().Contain("name");
    }

    [AvaloniaFact]
    public void Save_WithoutBullet_ShouldReportAndNotOpenTheFileDialog()
    {
        // Weight and diameter set the curve's scale, so a save without them cannot be right.
        var service = new MockFileDialogService { NextOpenResult = Sample("mbc1.csv") };
        var panel = new DrgFromBcPanel { FileDialogService = service };
        panel.NameBox.Text = "no bullet";
        Click(panel.ImportButton);

        Click(panel.SaveButton);

        service.LastSaveOptions.Should().BeNull();
        panel.Status.Should().Contain("weight").And.Contain("sectional density");
    }

    [AvaloniaFact]
    public void Save_WithoutKnots_ShouldReportAndNotOpenTheFileDialog()
    {
        var service = new MockFileDialogService();
        var panel = PanelWith(service);

        Click(panel.SaveButton);

        service.LastSaveOptions.Should().BeNull();
        panel.Status.Should().Contain("knot");
    }

    [AvaloniaFact]
    public void Save_ShouldWriteADrgWithMetadataThatReloads()
    {
        var path = Path.Combine(Path.GetTempPath(), $"drgbc-{Guid.NewGuid():N}.drg");
        var service = new MockFileDialogService { NextOpenResult = Sample("mbc1.csv"), NextSaveResult = path };
        var panel = PanelWith(service);
        panel.NameBox.Text = "308 220gr custom";
        panel.SourceBox.Text = "Litz";
        panel.WeightControl.SetValue(new Measurement<WeightUnit>(220, WeightUnit.Grain));
        panel.DiameterControl.SetValue(new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch));
        panel.LengthControl.SetValue(new Measurement<DistanceUnit>(1.226, DistanceUnit.Inch));
        Click(panel.ImportButton);

        try
        {
            Click(panel.SaveButton);

            service.LastSaveOptions!.DefaultExtension.Should().Be("drg");
            File.Exists(path).Should().BeTrue(panel.Status);

            var table = DrgDragTable.Open(path);
            table.TableId.Should().Be(DragTableId.GC);
            table.Ammunition!.Name.Should().Be("308 220gr custom");
            table.Ammunition.Source.Should().Be("Litz");
            table.Ammunition.Ammunition.BulletLength!.Value.In(DistanceUnit.Inch).Should().BeApproximately(1.226, 1e-4);
            panel.Status.Should().Contain("Saved");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [AvaloniaFact]
    public void Save_MixedTables_ShouldConvertAndSayHowMany()
    {
        var path = Path.Combine(Path.GetTempPath(), $"drgbc-{Guid.NewGuid():N}.drg");
        var service = new MockFileDialogService
        {
            NextOpenResult = TempCsv("1.5;0.883G1", "2.0;0.470G7", "2.5;0.484G7"),
            NextSaveResult = path,
        };
        var panel = PanelWith(service);
        Click(panel.ImportButton);

        try
        {
            Click(panel.SaveButton);

            File.Exists(path).Should().BeTrue(panel.Status);
            panel.Status.Should().Contain("1 knot converted");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [AvaloniaFact]
    public void CloseButton_ShouldRaiseCloseRequested()
    {
        var panel = new DrgFromBcPanel();
        var raised = false;
        panel.CloseRequested += (_, _) => raised = true;

        Click(panel.CloseButton);

        raised.Should().BeTrue();
    }

    #endregion

    #region Mach and velocity

    private static double SoundFps(Atmosphere? air) =>
        (air ?? new Atmosphere()).SoundVelocity.In(VelocityUnit.FeetPerSecond);

    [AvaloniaFact]
    public void Mach_WhenEntered_ShouldShowTheVelocityForTheCurrentAir()
    {
        // Arrange
        var panel = new DrgFromBcPanel();

        // Act
        panel.MachControl.Value = 2.0m;

        // Assert — standard air unless told otherwise
        panel.VelocityControl.GetValue<VelocityUnit>()!.Value.In(VelocityUnit.FeetPerSecond)
             .Should().BeApproximately(2.0 * SoundFps(null), 0.5);
    }

    [AvaloniaFact]
    public void Velocity_WhenTyped_ShouldComputeTheMachForTheCurrentAir()
    {
        // Arrange — typing is the user-driven path; SetValue is the programmatic one and deliberately
        // stays silent, which is what keeps the two fields from echoing each other.
        var panel = new DrgFromBcPanel();
        var fps = 1.5 * SoundFps(null);

        // Act — TextChanged is dispatched rather than raised inline, so the queue has to run
        panel.VelocityControl.NumericPart.Text = fps.ToString("0.0", CultureInfo.InvariantCulture);
        Dispatcher.UIThread.RunJobs();

        // Assert
        ((double)panel.MachControl.Value!).Should().BeApproximately(1.5, 1e-3);
    }

    [AvaloniaFact]
    public void Mach_TypedFinerThanTheVelocityShows_ShouldNotBeRewrittenByTheMirror()
    {
        // Arrange & Act — writing the velocity raises a dispatched change that comes back to the Mach
        var panel = new DrgFromBcPanel();
        panel.MachControl.Value = 1.2345m;
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        // Assert — what the user typed still stands
        panel.MachControl.Value.Should().Be(1.2345m);
    }

    [AvaloniaFact]
    public void Velocity_ChangedByMoreThanRounding_ShouldStillUpdateTheMach()
    {
        // Arrange — a Mach the mirror must not be too shy to change
        var panel = new DrgFromBcPanel();
        panel.MachControl.Value = 2.0m;
        Dispatcher.UIThread.RunJobs();

        // Act — a real edit, several ft/s away
        var fps = 2.01 * SoundFps(null);
        panel.VelocityControl.NumericPart.Text = fps.ToString("0.0", CultureInfo.InvariantCulture);
        Dispatcher.UIThread.RunJobs();

        // Assert
        ((double)panel.MachControl.Value!).Should().BeApproximately(2.01, 1e-3);
    }

    [AvaloniaFact]
    public void Velocity_WhenCleared_ShouldClearTheMach()
    {
        // Arrange
        var panel = new DrgFromBcPanel();
        panel.MachControl.Value = 2.0m;

        // Act
        panel.VelocityControl.NumericPart.Text = "";
        Dispatcher.UIThread.RunJobs();

        // Assert
        panel.MachControl.Value.Should().BeNull();
    }

    [AvaloniaFact]
    public void Atmosphere_WhenChanged_ShouldRestateTheVelocityForTheNewAir()
    {
        // Arrange — Mach 2 in standard air
        var panel = new DrgFromBcPanel();
        panel.MachControl.Value = 2.0m;
        var standard = panel.VelocityControl.GetValue<VelocityUnit>()!.Value.In(VelocityUnit.FeetPerSecond);

        // Act — thin, cold air at 10,000 ft: sound is slower, so Mach 2 is a lower velocity
        var air = Atmosphere.CreateICAOAtmosphere(new Measurement<DistanceUnit>(10000, DistanceUnit.Foot));
        panel.Atmosphere = air;

        // Assert — the Mach the user typed is what stands; the velocity restates it
        panel.MachControl.Value.Should().Be(2.0m);
        var restated = panel.VelocityControl.GetValue<VelocityUnit>()!.Value.In(VelocityUnit.FeetPerSecond);
        restated.Should().BeApproximately(2.0 * SoundFps(air), 0.5);
        restated.Should().BeLessThan(standard);
    }

    [AvaloniaFact]
    public void Atmosphere_WhenNotSet_ShouldSayStandard()
    {
        // Arrange & Act
        var panel = new DrgFromBcPanel();

        // Assert
        panel.AtmosphereDescription.Should().Contain("standard");
    }

    [AvaloniaFact]
    public void Atmosphere_WhenSet_ShouldBeDescribedToTheUser()
    {
        // Arrange
        var panel = new DrgFromBcPanel();

        // Act
        panel.Atmosphere = Atmosphere.CreateICAOAtmosphere(new Measurement<DistanceUnit>(10000, DistanceUnit.Foot));

        // Assert — the conversion depends on it, so it has to be visible
        panel.AtmosphereDescription.Should().NotContain("standard");
        panel.AtmosphereDescription.Should().Contain("10,000").And.Contain("ft");
    }

    [AvaloniaFact]
    public void SetAtmosphereButton_ShouldAskTheHost()
    {
        // Arrange
        var panel = new DrgFromBcPanel();
        var asked = false;
        panel.AtmosphereRequested += (_, _) => asked = true;

        // Act
        Click(panel.AtmosphereButton);

        // Assert
        asked.Should().BeTrue();
    }

    [AvaloniaFact]
    public void SelectingARow_ShouldRestateTheVelocityForItsMach()
    {
        // Arrange
        var panel = new DrgFromBcPanel();
        var window = new Window { Content = panel, Width = 600, Height = 700 };
        window.Show();
        Enter(panel, 1.5, 0.462);
        Click(panel.AddButton);
        Enter(panel, 2.5, 0.484);
        Click(panel.AddButton);

        // Act
        panel.KnotsGrid.SelectedItem = panel.Knots[0];

        // Assert
        panel.VelocityControl.GetValue<VelocityUnit>()!.Value.In(VelocityUnit.FeetPerSecond)
             .Should().BeApproximately(1.5 * SoundFps(null), 0.5);
    }

    #endregion

    #region Layout

    [AvaloniaFact]
    public void KnotsGrid_Columns_AreNotStarSized()
    {
        // Arrange & Act
        var panel = new DrgFromBcPanel();

        // Assert — see DrgFromVelocitiesPanelTests: star columns starve the vertical scroll bar (D-002).
        panel.KnotsGrid.Columns.Should().AllSatisfy(column =>
            column.Width.IsStar.Should().BeFalse("a star column starves the vertical scroll bar"));
    }

    #endregion
}
