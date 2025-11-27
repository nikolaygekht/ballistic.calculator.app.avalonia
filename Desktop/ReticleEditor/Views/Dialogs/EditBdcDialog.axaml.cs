using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Utilities;
using System;

namespace ReticleEditor.Views.Dialogs;

public partial class EditBdcDialog : Window
{
    private readonly ReticleBulletDropCompensatorPoint _element;

    public EditBdcDialog(ReticleBulletDropCompensatorPoint bdc)
    {
        _element = bdc ?? throw new ArgumentNullException(nameof(bdc));
        InitializeComponent();

        // Initialize MeasurementControls with AngularUnit type
        PositionX.UnitType = typeof(AngularUnit);
        PositionY.UnitType = typeof(AngularUnit);
        TextOffsetControl.UnitType = typeof(AngularUnit);
        TextHeightControl.UnitType = typeof(AngularUnit);

        PopulateFromElement();

        // Restore saved dialog size
        WindowStateManager.RestoreDialogSize(this, nameof(EditBdcDialog));

        // Save dialog size when closing
        Closing += (s, e) => WindowStateManager.SaveDialogSize(this, nameof(EditBdcDialog));
    }

    private void PopulateFromElement()
    {
        // Initialize position if null
        if (_element.Position == null)
            _element.Position = new ReticlePosition(0, 0, AngularUnit.Mil);

        // Populate position
        PositionX.SetValue(_element.Position.X);
        PositionY.SetValue(_element.Position.Y);

        // Populate text offset and height
        TextOffsetControl.SetValue(_element.TextOffset);
        TextHeightControl.SetValue(_element.TextHeight);
    }

    public void Save()
    {
        var posX = PositionX.GetValue<AngularUnit>();
        var posY = PositionY.GetValue<AngularUnit>();
        var textOffset = TextOffsetControl.GetValue<AngularUnit>();
        var textHeight = TextHeightControl.GetValue<AngularUnit>();

        if (posX.HasValue) _element.Position.X = posX.Value;
        if (posY.HasValue) _element.Position.Y = posY.Value;
        if (textOffset.HasValue) _element.TextOffset = textOffset.Value;
        if (textHeight.HasValue) _element.TextHeight = textHeight.Value;
    }

    private void OnOK(object? sender, RoutedEventArgs e)
    {
        Save();
        Close();
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
