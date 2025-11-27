using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Utilities;
using System;

namespace ReticleEditor.Views.Dialogs;

public partial class EditMoveToDialog : Window
{
    private readonly ReticlePathElementMoveTo _element;

    public EditMoveToDialog(ReticlePathElementMoveTo moveTo)
    {
        _element = moveTo ?? throw new ArgumentNullException(nameof(moveTo));
        InitializeComponent();

        // Initialize MeasurementControls with AngularUnit type
        PositionX.UnitType = typeof(AngularUnit);
        PositionY.UnitType = typeof(AngularUnit);

        PopulateFromElement();

        // Restore saved dialog size
        WindowStateManager.RestoreDialogSize(this, nameof(EditMoveToDialog));

        // Save dialog size when closing
        Closing += (s, e) => WindowStateManager.SaveDialogSize(this, nameof(EditMoveToDialog));
    }

    private void PopulateFromElement()
    {
        // Initialize position if null
        if (_element.Position == null)
            _element.Position = new ReticlePosition(0, 0, AngularUnit.Mil);

        // Populate position
        PositionX.SetValue(_element.Position.X);
        PositionY.SetValue(_element.Position.Y);
    }

    public void Save()
    {
        var posX = PositionX.GetValue<AngularUnit>();
        var posY = PositionY.GetValue<AngularUnit>();

        if (posX.HasValue) _element.Position.X = posX.Value;
        if (posY.HasValue) _element.Position.Y = posY.Value;
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
