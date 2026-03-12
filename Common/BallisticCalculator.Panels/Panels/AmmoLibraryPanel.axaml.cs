using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator;
using BallisticCalculator.Controls.Controls;
using BallisticCalculator.Data.Dictionary;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Serialization;
using BallisticCalculator.Types;
using Gehtsoft.Measurements;
using System;
using System.Linq;

namespace BallisticCalculator.Panels.Panels;

public partial class AmmoLibraryPanel : UserControl
{
    private MeasurementSystem _measurementSystem = MeasurementSystem.Metric;

    public AmmoLibraryPanel()
    {
        InitializeComponent();
        InitializeControls();
        WireEvents();
        ApplyMeasurementSystem();
    }

    #region Properties

    public IFileDialogService? FileDialogService { get; set; }

    public bool ConvertOnSystemChange
    {
        get => AmmoSubPanel.ConvertOnSystemChange;
        set => AmmoSubPanel.ConvertOnSystemChange = value;
    }

    public MeasurementSystem MeasurementSystem
    {
        get => _measurementSystem;
        set
        {
            if (_measurementSystem == value) return;
            _measurementSystem = value;
            AmmoSubPanel.MeasurementSystem = value;
            ApplyMeasurementSystem();
        }
    }

    public AmmunitionLibraryEntry? LibraryEntry
    {
        get
        {
            var ammo = AmmoSubPanel.Ammunition;
            if (ammo == null)
                return null;

            return new AmmunitionLibraryEntry()
            {
                Name = NameTextBox.Text ?? "",
                Ammunition = ammo,
                Caliber = string.IsNullOrWhiteSpace(CaliberTextBox.Text) ? null : CaliberTextBox.Text,
                AmmunitionType = (BulletTypeCombo.SelectedItem as AmmunitionType)?.Abbreviation,
                BarrelLength = BarrelLengthControl.IsEmpty ? null : BarrelLengthControl.GetValue<DistanceUnit>(),
                Source = string.IsNullOrWhiteSpace(SourceTextBox.Text) ? null : SourceTextBox.Text,
            };
        }
        set
        {
            if (value == null)
            {
                Clear();
                return;
            }

            NameTextBox.Text = value.Name ?? "";
            AmmoSubPanel.Ammunition = value.Ammunition;
            CaliberTextBox.Text = value.Caliber ?? "";

            SelectBulletType(value.AmmunitionType);

            if (value.BarrelLength.HasValue)
                BarrelLengthControl.SetValue(ConvertBarrelLength(value.BarrelLength.Value));
            else
                BarrelLengthControl.Value = null;

            SourceTextBox.Text = value.Source ?? "";
        }
    }

    public Ammunition? Ammunition
    {
        get => AmmoSubPanel.Ammunition;
        set => AmmoSubPanel.Ammunition = value;
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Initialization

    private void InitializeControls()
    {
        BarrelLengthControl.UnitType = typeof(DistanceUnit);
        BarrelLengthControl.Minimum = 0;
        BarrelLengthControl.Increment = 1;

        PopulateBulletTypes();
    }

    private void PopulateBulletTypes()
    {
        BulletTypeCombo.Items.Clear();

        // Add empty entry
        BulletTypeCombo.Items.Add("");

        var types = AmmunitionTypeFactory.Create();
        foreach (var type in types)
            BulletTypeCombo.Items.Add(type);

        BulletTypeCombo.SelectedIndex = 0;
    }

    private void WireEvents()
    {
        AmmoSubPanel.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        NameTextBox.TextChanged += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        CaliberTextBox.TextChanged += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        BulletTypeCombo.SelectionChanged += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        BarrelLengthControl.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        SourceTextBox.TextChanged += (s, e) => Changed?.Invoke(this, EventArgs.Empty);

        LoadButton.Click += OnLoadClick;
        SaveButton.Click += OnSaveClick;
    }

    #endregion

    #region Unit Switching

    private void ApplyMeasurementSystem()
    {
        var convert = ConvertOnSystemChange;
        if (_measurementSystem == MeasurementSystem.Metric)
        {
            BarrelLengthControl.ChangeUnit(DistanceUnit.Millimeter, 0, convert);
        }
        else
        {
            BarrelLengthControl.ChangeUnit(DistanceUnit.Inch, 1, convert);
        }
    }

    #endregion

    #region File Operations

    private async void OnLoadClick(object? sender, RoutedEventArgs e)
    {
        if (FileDialogService == null) return;

        var options = new FileDialogOptions
        {
            Title = "Load Ammunition",
            DefaultExtension = "ammox",
            Filters =
            {
                new Services.FileDialogFilter("Ammunition Files", "ammox"),
                new Services.FileDialogFilter("Legacy Ammunition Files", "ammo"),
            }
        };

        var fileName = await FileDialogService.OpenFileAsync(options);
        if (fileName == null) return;

        try
        {
            AmmunitionLibraryEntry? entry;
            if (fileName.EndsWith(".ammo", StringComparison.OrdinalIgnoreCase))
                entry = BallisticXmlDeserializer.ReadLegacyAmmunitionLibraryEntryFromFile(fileName);
            else
                entry = BallisticXmlDeserializer.ReadFromFile<AmmunitionLibraryEntry>(fileName);

            if (entry != null)
                LibraryEntry = entry;
        }
        catch
        {
            // File load failed - silently ignore for now
        }
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        if (FileDialogService == null) return;

        var entry = LibraryEntry;
        if (entry == null) return;

        var options = new FileDialogOptions
        {
            Title = "Save Ammunition",
            DefaultExtension = "ammox",
            InitialFileName = string.IsNullOrWhiteSpace(entry.Name) ? "ammunition" : entry.Name,
            Filters = { new Services.FileDialogFilter("Ammunition Files", "ammox") }
        };

        var fileName = await FileDialogService.SaveFileAsync(options);
        if (fileName == null) return;

        try
        {
            BallisticXmlSerializer.SerializeToFile(entry, fileName);
        }
        catch
        {
            // File save failed - silently ignore for now
        }
    }

    #endregion

    #region Unit Conversion Helpers

    private Measurement<DistanceUnit> ConvertBarrelLength(Measurement<DistanceUnit> value)
    {
        var targetUnit = _measurementSystem == MeasurementSystem.Metric ? DistanceUnit.Millimeter : DistanceUnit.Inch;
        return value.To(targetUnit);
    }

    #endregion

    #region Helper Methods

    private void SelectBulletType(string? abbreviation)
    {
        if (string.IsNullOrWhiteSpace(abbreviation))
        {
            BulletTypeCombo.SelectedIndex = 0;
            return;
        }

        foreach (var item in BulletTypeCombo.Items)
        {
            if (item is AmmunitionType type && type.Abbreviation == abbreviation)
            {
                BulletTypeCombo.SelectedItem = item;
                return;
            }
        }

        BulletTypeCombo.SelectedIndex = 0;
    }

    public void Clear()
    {
        NameTextBox.Text = "";
        AmmoSubPanel.Clear();
        CaliberTextBox.Text = "";
        BulletTypeCombo.SelectedIndex = 0;
        BarrelLengthControl.Value = null;
        SourceTextBox.Text = "";
    }

    #endregion
}
