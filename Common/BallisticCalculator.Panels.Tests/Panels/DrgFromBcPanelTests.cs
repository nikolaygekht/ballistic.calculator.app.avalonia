using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
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

    private static DrgFromBcPanel PanelWith(MockFileDialogService service)
    {
        var panel = new DrgFromBcPanel { FileDialogService = service };
        panel.NameBox.Text = "test table";
        return panel;
    }

    #region Editing

    [AvaloniaFact]
    public void Panel_ShouldStartEmptyWithG7()
    {
        var panel = new DrgFromBcPanel();

        panel.Knots.Should().BeEmpty();
        (panel.BaseTableCombo.SelectedItem as DragTableInfo)!.Value.Should().Be(DragTableId.G7);
        panel.DetailPanel.IsEnabled.Should().BeFalse();
        panel.Status.Should().Contain("No knots");
    }

    [AvaloniaFact]
    public void Add_ShouldAppendKnotAndSelectIt()
    {
        var panel = new DrgFromBcPanel();

        Click(panel.AddButton);

        panel.Knots.Should().HaveCount(1);
        panel.KnotsList.SelectedIndex.Should().Be(0);
        panel.DetailPanel.IsEnabled.Should().BeTrue();
        panel.Status.Should().Contain("1 knot");
    }

    [AvaloniaFact]
    public void Add_Twice_ShouldContinueTheCurve()
    {
        var panel = new DrgFromBcPanel();

        Click(panel.AddButton);
        Click(panel.AddButton);

        panel.Knots.Should().HaveCount(2);
        panel.Knots[1].Mach.Should().BeGreaterThan(panel.Knots[0].Mach);
    }

    [AvaloniaFact]
    public void Delete_ShouldRemoveSelectedKnot()
    {
        var panel = new DrgFromBcPanel();
        Click(panel.AddButton);
        Click(panel.AddButton);

        panel.KnotsList.SelectedIndex = 0;
        Click(panel.DeleteButton);

        panel.Knots.Should().HaveCount(1);
    }

    [AvaloniaFact]
    public void EditingDetail_ShouldWriteBackToTheModelAndList()
    {
        var panel = new DrgFromBcPanel();
        Click(panel.AddButton);

        panel.XValueBox.Text = "1.75";
        panel.BcBox.Text = "0.463";

        panel.Knots[0].Mach.Should().BeApproximately(1.75, 1e-9);
        panel.Knots[0].Bc.Should().BeApproximately(0.463, 1e-9);
        panel.Knots[0].Display.Should().Contain("1.75").And.Contain("0.463");
    }

    [AvaloniaFact]
    public void SwitchingToVelocityMode_ShouldPreserveMachExactly()
    {
        var panel = new DrgFromBcPanel();
        Click(panel.AddButton);
        panel.XValueBox.Text = "2.25";
        var mach = panel.Knots[0].Mach;

        panel.KnotModeCombo.SelectedIndex = 1;      // Velocity

        panel.Knots[0].Mach.Should().Be(mach);
        panel.XValueLabel.Text.Should().Be("Velocity:");
        panel.VelocityUnitCombo.IsVisible.Should().BeTrue();
        panel.XValueBox.Text.Should().NotBe("2.25");   // now shown as a velocity

        panel.KnotModeCombo.SelectedIndex = 0;      // back to Mach
        panel.Knots[0].Mach.Should().Be(mach);
    }

    [AvaloniaFact]
    public void VelocityMode_TypedVelocity_ShouldConvertToMach()
    {
        var panel = new DrgFromBcPanel();
        Click(panel.AddButton);
        panel.KnotModeCombo.SelectedIndex = 1;
        panel.VelocityUnitCombo.SelectedItem = panel.VelocityUnitCombo.Items
            .OfType<UnitItem>().First(i => (VelocityUnit)i.Unit == VelocityUnit.FeetPerSecond);

        panel.XValueBox.Text = "2700";

        var expected = DragTableBuilder.VelocityToMach(new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond));
        panel.Knots[0].Mach.Should().BeApproximately(expected, 1e-9);
    }

    [AvaloniaFact]
    public void Prefill_ShouldFillMetadataFields()
    {
        var panel = new DrgFromBcPanel
        {
            Prefill = new Ammunition
            {
                Weight = new Measurement<WeightUnit>(220, WeightUnit.Grain),
                BallisticCoefficient = new BallisticCoefficient(0.5, DragTableId.G7),
                MuzzleVelocity = new Measurement<VelocityUnit>(2600, VelocityUnit.FeetPerSecond),
                BulletDiameter = new Measurement<DistanceUnit>(0.308, DistanceUnit.Inch),
                BulletLength = new Measurement<DistanceUnit>(1.226, DistanceUnit.Inch),
            },
        };

        panel.WeightControl.GetValue<WeightUnit>()!.Value.In(WeightUnit.Grain).Should().BeApproximately(220, 0.01);
        panel.DiameterControl.GetValue<DistanceUnit>()!.Value.In(DistanceUnit.Inch).Should().BeApproximately(0.308, 1e-6);
        panel.LengthControl.GetValue<DistanceUnit>()!.Value.In(DistanceUnit.Inch).Should().BeApproximately(1.226, 1e-6);
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
        panel.Knots[0].Bc.Should().BeApproximately(0.307, 1e-9);
    }

    [AvaloniaFact]
    public void Import_BadLine_ShouldRejectFileAndKeepExistingKnots()
    {
        var service = new MockFileDialogService { NextOpenResult = Sample("mbc1.csv") };
        var panel = PanelWith(service);
        Click(panel.ImportButton);
        var before = panel.Knots.Select(k => (k.Mach, k.Bc)).ToArray();

        service.NextOpenResult = TempCsv("1.5;0.462", "oops;0.463", "2;0.470");
        Click(panel.ImportButton);

        panel.Knots.Select(k => (k.Mach, k.Bc)).Should().Equal(before);
        panel.Status.Should().Contain("2").And.Contain("Nothing was imported");
    }

    [AvaloniaFact]
    public void Import_VelocityKeyedFile_ShouldConvertToMach()
    {
        // Inline units win over the display mode, which is still Mach here.
        var service = new MockFileDialogService { NextOpenResult = TempCsv("velocity;bc", "2700ft/s;0.307", "1800ft/s;0.318") };
        var panel = PanelWith(service);

        Click(panel.ImportButton);

        panel.Knots.Should().HaveCount(2);
        var expected = DragTableBuilder.VelocityToMach(new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond));
        panel.Knots.Select(k => k.Mach).Should().Contain(m => Math.Abs(m - expected) < 1e-9);
    }

    #endregion

    #region Save

    [AvaloniaFact]
    public void Save_WithoutName_ShouldReportAndNotOpenTheFileDialog()
    {
        var service = new MockFileDialogService { NextOpenResult = Sample("mbc1.csv") };
        var panel = new DrgFromBcPanel { FileDialogService = service };
        Click(panel.ImportButton);

        Click(panel.SaveButton);

        service.LastSaveOptions.Should().BeNull();
        panel.Status.Should().Contain("name");
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
            File.Exists(path).Should().BeTrue();

            var table = DrgDragTable.Open(path);
            table.TableId.Should().Be(DragTableId.GC);
            table.Ammunition!.Name.Should().Be("308 220gr custom");
            table.Ammunition.Source.Should().Be("Litz");
            table.Ammunition.Ammunition.BulletLength!.Value.In(DistanceUnit.Inch).Should().BeApproximately(1.226, 1e-4);
            panel.Status.Should().Contain("Saved").And.Contain("GC");
        }
        finally
        {
            File.Delete(path);
        }
    }

    #endregion
}
