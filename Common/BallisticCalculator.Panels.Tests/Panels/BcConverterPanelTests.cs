using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Xunit;
using AwesomeAssertions;
using BallisticCalculator;
using BallisticCalculator.Controls.Models;
using BallisticCalculator.Panels.Panels;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;

namespace BallisticCalculator.Panels.Tests.Panels;

public class BcConverterPanelTests
{
    private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

    /// <summary>
    /// Fills the three inputs and recomputes. <c>Recalculate</c> is explicit because a programmatic
    /// <c>SetValue</c> raises no change event in headless Avalonia — in the app the events do the calling.
    /// </summary>
    private static BcConverterPanel PanelWith(double bc, DragTableId sourceTable, DragTableId targetTable,
                                              double fps)
    {
        var panel = new BcConverterPanel();
        panel.SourceBcControl.Value = new BallisticCoefficient(bc, sourceTable);
        panel.SelectTargetTable(targetTable);
        panel.VelocityControl.SetValue(new Measurement<VelocityUnit>(fps, VelocityUnit.FeetPerSecond));
        panel.Recalculate();
        return panel;
    }

    #region Layout and defaults

    [AvaloniaFact]
    public void Panel_ShouldStartWithNothingConvertedAndSayWhatIsNeeded()
    {
        var panel = new BcConverterPanel();

        panel.Conversion.Should().BeNull();
        panel.TargetText.Should().BeEmpty();
        panel.Status.Should().Contain("source ballistic coefficient");
    }

    [AvaloniaFact]
    public void Panel_ShouldDefaultToConvertingIntoG7()
    {
        // Arrange & Act: the source control defaults to G1, so G7 is the useful other side
        var panel = new BcConverterPanel();

        // Assert
        panel.TargetTable.Should().Be(DragTableId.G7);
    }

    [AvaloniaFact]
    public void TargetTableList_ShouldNotOfferTheCustomTable()
    {
        var panel = new BcConverterPanel();

        panel.TableCombo.ItemCount.Should().Be(BcConversionCalculator.StandardTables.Count);
    }

    [AvaloniaFact]
    public void Panel_ShouldLayOutInsideAWindow()
    {
        var panel = new BcConverterPanel();
        var window = new Window { Content = panel, Width = 420, Height = 260 };

        window.Show();

        panel.Bounds.Height.Should().BeGreaterThan(0);
        panel.TargetBox.IsReadOnly.Should().BeTrue("the converted value is an output, not an input");
    }

    #endregion

    #region Conversion

    [AvaloniaFact]
    public void Recalculate_WithCompleteInput_ShouldShowTheConvertedCoefficient()
    {
        // Arrange & Act
        var panel = PanelWith(0.462, DragTableId.G1, DragTableId.G7, 2700);

        // Assert
        panel.Conversion.Should().NotBeNull();
        panel.Conversion!.Converted.Table.Should().Be(DragTableId.G7);
        panel.TargetText.Should().EndWith("G7");
        panel.TargetText.Should().StartWith("0.2", "0.462 G1 is about 0.23 G7 at supersonic velocity");
    }

    [AvaloniaFact]
    public void Recalculate_ShouldReportTheReferenceMachInTheStatus()
    {
        var panel = PanelWith(0.365, DragTableId.G1, DragTableId.G7, 2700);

        panel.Status.Should().Contain("Mach 2.4");
    }

    [AvaloniaFact]
    public void Recalculate_IntoTheSameTable_ShouldReturnTheSourceAndSayNothingToConvert()
    {
        var panel = PanelWith(0.365, DragTableId.G1, DragTableId.G1, 2700);

        panel.Conversion!.Converted.Value.Should().Be(0.365);
        panel.Status.Should().Contain("same table");
    }

    [AvaloniaFact]
    public void Recalculate_AfterChangingTheReference_ShouldGiveADifferentAnswer()
    {
        // Arrange
        var panel = PanelWith(0.365, DragTableId.G1, DragTableId.G7, 2700);
        var supersonic = panel.Conversion!.Converted.Value;

        // Act
        panel.VelocityControl.SetValue(new Measurement<VelocityUnit>(1450, VelocityUnit.FeetPerSecond));
        panel.Recalculate();

        // Assert
        panel.Conversion!.Converted.Value.Should().NotBe(supersonic);
    }

    [AvaloniaFact]
    public void Recalculate_WithAnAtmosphere_ShouldUseItsSpeedOfSound()
    {
        // Arrange: same velocity, colder air, so a higher Mach and a different converted value
        var standard = PanelWith(0.365, DragTableId.G1, DragTableId.G7, 2700);

        var panel = new BcConverterPanel
        {
            Atmosphere = new Atmosphere(new Measurement<DistanceUnit>(10000, DistanceUnit.Foot),
                                        new Measurement<PressureUnit>(20.6, PressureUnit.InchesOfMercury),
                                        new Measurement<TemperatureUnit>(-5, TemperatureUnit.Fahrenheit),
                                        0.2),
        };
        panel.SourceBcControl.Value = new BallisticCoefficient(0.365, DragTableId.G1);
        panel.SelectTargetTable(DragTableId.G7);
        panel.VelocityControl.SetValue(new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond));

        // Act
        panel.Recalculate();

        // Assert
        panel.Conversion!.ReferenceMach.Should().BeGreaterThan(standard.Conversion!.ReferenceMach);
        panel.Status.Should().NotContain("standard atmosphere");
    }

    [AvaloniaFact]
    public void Recalculate_WithNoAtmosphere_ShouldSaySoInTheStatus()
    {
        var panel = PanelWith(0.365, DragTableId.G1, DragTableId.G7, 2700);

        panel.Status.Should().Contain("standard atmosphere");
    }

    #endregion

    #region Honesty about the transonic band

    [AvaloniaFact]
    public void Recalculate_WithATransonicReference_ShouldWarn()
    {
        var panel = PanelWith(0.365, DragTableId.G1, DragTableId.G7, 1450);

        panel.Conversion!.IsTransonic.Should().BeTrue();
        panel.WarningText.IsVisible.Should().BeTrue();
        panel.WarningText.Text.Should().Contain("accuracy");
        panel.NoteText.Text.Should().Contain("only exact at", "the caveat is stated whatever the reference");
    }

    [AvaloniaFact]
    public void Recalculate_WithASupersonicReference_ShouldNotWarn()
    {
        var panel = PanelWith(0.365, DragTableId.G1, DragTableId.G7, 2700);

        panel.WarningText.IsVisible.Should().BeFalse();
    }

    #endregion

    #region Refused input

    [AvaloniaFact]
    public void Recalculate_WithNoVelocity_ShouldClearTheResultAndSayWhy()
    {
        // Arrange: the panel starts with a supersonic default, so emptying the field is the way to have none
        var panel = new BcConverterPanel();
        panel.SourceBcControl.Value = new BallisticCoefficient(0.365, DragTableId.G1);
        panel.VelocityControl.NumericPart.Text = "";

        // Act
        panel.Recalculate();

        // Assert
        panel.Conversion.Should().BeNull();
        panel.TargetText.Should().BeEmpty();
        panel.Status.Should().Contain("reference velocity");
    }

    [AvaloniaFact]
    public void Recalculate_WithAFormFactorSource_ShouldClearTheResultAndSayWhy()
    {
        // Arrange
        var panel = new BcConverterPanel();
        panel.SourceBcControl.Value =
            new BallisticCoefficient(1.0, DragTableId.G1, BallisticCoefficientValueType.FormFactor);
        panel.VelocityControl.SetValue(new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond));

        // Act
        panel.Recalculate();

        // Assert
        panel.Conversion.Should().BeNull();
        panel.Status.Should().Contain("form factor");
    }

    [AvaloniaFact]
    public void SourceControl_ShouldNotOfferTheCustomTable()
    {
        // Arrange & Act: GC has no fixed curve, so it is not on offer on either side of the conversion
        var panel = new BcConverterPanel();

        // Assert
        panel.SourceBcControl.AllowCustomTable.Should().BeFalse();
        panel.SourceBcControl.TablePart.Items.Cast<DragTableInfo>()
             .Should().NotContain(t => t.Value == DragTableId.GC);
    }

    [AvaloniaFact]
    public void SourceControl_GivenACustomTableCoefficient_ShouldHoldNothing()
    {
        // Arrange: nothing in the UI can produce this, but the panel must not convert a mislabelled value
        var panel = new BcConverterPanel();

        // Act
        panel.SourceBcControl.Value = new BallisticCoefficient(0.5, DragTableId.GC);
        panel.Recalculate();

        // Assert
        panel.Conversion.Should().BeNull();
        panel.Status.Should().Contain("source ballistic coefficient");
    }

    [AvaloniaFact]
    public void Recalculate_AfterAGoodValueGoesBad_ShouldNotKeepTheStaleAnswer()
    {
        // Arrange
        var panel = PanelWith(0.365, DragTableId.G1, DragTableId.G7, 2700);
        panel.TargetText.Should().NotBeEmpty();

        // Act — clear the reference velocity the way a user would
        panel.VelocityControl.NumericPart.Text = "";
        panel.Recalculate();

        // Assert
        panel.TargetText.Should().BeEmpty();
        panel.Conversion.Should().BeNull();
    }

    #endregion

    #region Prefill and units

    [AvaloniaFact]
    public void Prefill_ShouldTakeTheSourceCoefficientFromTheAmmunition()
    {
        // Arrange
        var ammo = new Ammunition(new Measurement<WeightUnit>(168, WeightUnit.Grain),
                                  new BallisticCoefficient(0.223, DragTableId.G7),
                                  new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond));
        var panel = new BcConverterPanel();

        // Act
        panel.Prefill = ammo;

        // Assert
        panel.SourceBcControl.Value!.Value.Value.Should().BeApproximately(0.223, 1e-9);
        panel.SourceBcControl.Value!.Value.Table.Should().Be(DragTableId.G7);
        panel.TargetTable.Should().Be(DragTableId.G1, "a G7 source is asking to be read as G1");
    }

    [AvaloniaFact]
    public void Prefill_WithACustomTableAmmunition_ShouldLeaveTheSourceAlone()
    {
        // Arrange: a GC coefficient cannot be converted, so prefilling it would only produce an error
        var ammo = new Ammunition(new Measurement<WeightUnit>(168, WeightUnit.Grain),
                                  new BallisticCoefficient(1.0, DragTableId.GC,
                                                           BallisticCoefficientValueType.FormFactor),
                                  new Measurement<VelocityUnit>(2700, VelocityUnit.FeetPerSecond));
        var panel = new BcConverterPanel();

        // Act
        panel.Prefill = ammo;

        // Assert
        panel.SourceBcControl.IsEmpty.Should().BeTrue();
    }

    [AvaloniaFact]
    public void MeasurementSystem_Metric_ShouldSwitchTheReferenceVelocityToMetresPerSecond()
    {
        var panel = new BcConverterPanel { MeasurementSystem = MeasurementSystem.Metric };

        panel.VelocityControl.GetValue<VelocityUnit>()!.Value.Unit.Should().Be(VelocityUnit.MetersPerSecond);
    }

    [AvaloniaFact]
    public void Panel_ShouldStartWithASupersonicReferenceVelocity()
    {
        // Arrange & Act: the default must sit in the band where the conversion is trustworthy
        var panel = new BcConverterPanel();

        // Assert
        var velocity = panel.VelocityControl.GetValue<VelocityUnit>();
        velocity.Should().NotBeNull();
        DragTableBuilder.VelocityToMach(velocity!.Value).Should().BeGreaterThan(BcConversionCalculator.TransonicMach);
    }

    #endregion

    #region Buttons

    [AvaloniaFact]
    public void CloseButton_ShouldAskTheHostToClose()
    {
        var panel = new BcConverterPanel();
        var asked = false;
        panel.CloseRequested += (_, _) => asked = true;

        Click(panel.CloseButton);

        asked.Should().BeTrue();
    }

    [AvaloniaFact]
    public void AtmosphereButton_ShouldAskTheHostForTheAtmosphereEditor()
    {
        var panel = new BcConverterPanel();
        var asked = false;
        panel.AtmosphereRequested += (_, _) => asked = true;

        Click(panel.AtmosphereButton);

        asked.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Atmosphere_SetAfterAConversion_ShouldRecomputeImmediately()
    {
        // Arrange
        var panel = PanelWith(0.365, DragTableId.G1, DragTableId.G7, 2700);
        var before = panel.Conversion!.ReferenceMach;

        // Act — this is what the host does when the atmosphere dialog returns
        panel.Atmosphere = new Atmosphere(new Measurement<DistanceUnit>(10000, DistanceUnit.Foot),
                                          new Measurement<PressureUnit>(20.6, PressureUnit.InchesOfMercury),
                                          new Measurement<TemperatureUnit>(-5, TemperatureUnit.Fahrenheit),
                                          0.2);

        // Assert
        panel.Conversion!.ReferenceMach.Should().NotBe(before);
    }

    #endregion
}
