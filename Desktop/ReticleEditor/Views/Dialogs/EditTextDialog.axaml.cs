using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Utilities;
using System;

namespace ReticleEditor.Views.Dialogs;

public partial class EditTextDialog : Window
{
    private readonly ReticleText _element;

    public EditTextDialog(ReticleText text)
    {
        _element = text ?? throw new ArgumentNullException(nameof(text));
        InitializeComponent();

        // Initialize MeasurementControls with AngularUnit type
        PositionX.UnitType = typeof(AngularUnit);
        PositionY.UnitType = typeof(AngularUnit);
        TextHeightControl.UnitType = typeof(AngularUnit);

        PopulateFromElement();

        // Restore saved dialog size
        WindowStateManager.RestoreDialogSize(this, nameof(EditTextDialog));

        // Save dialog size when closing
        Closing += (s, e) => WindowStateManager.SaveDialogSize(this, nameof(EditTextDialog));
    }

    private void PopulateFromElement()
    {
        // Initialize position if null
        if (_element.Position == null)
            _element.Position = new ReticlePosition(0, 0, AngularUnit.Mil);

        // Populate position
        PositionX.SetValue(_element.Position.X);
        PositionY.SetValue(_element.Position.Y);

        // Populate text content
        TextContent.Text = _element.Text ?? "";

        // Populate text height
        TextHeightControl.SetValue(_element.TextHeight);

        // Populate color combo
        ColorCombo.PopulateWithColors();
        ColorCombo.SelectedItem = _element.Color ?? "black";

        // Populate anchor combo
        AnchorCombo.Items.Clear();
        AnchorCombo.Items.Add(TextAnchor.Left);
        AnchorCombo.Items.Add(TextAnchor.Center);
        AnchorCombo.Items.Add(TextAnchor.Right);
        AnchorCombo.SelectedItem = _element.Anchor ?? TextAnchor.Left;
    }

    public void Save()
    {
        var posX = PositionX.GetValue<AngularUnit>();
        var posY = PositionY.GetValue<AngularUnit>();
        var textHeight = TextHeightControl.GetValue<AngularUnit>();

        if (posX.HasValue) _element.Position.X = posX.Value;
        if (posY.HasValue) _element.Position.Y = posY.Value;
        if (textHeight.HasValue) _element.TextHeight = textHeight.Value;

        _element.Text = TextContent.Text;
        _element.Color = ColorCombo.SelectedItem?.ToString();
        _element.Anchor = AnchorCombo.SelectedItem as TextAnchor? ?? TextAnchor.Left;
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
