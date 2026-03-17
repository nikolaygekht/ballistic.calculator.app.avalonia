using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Serialization;
using BallisticCalculator.Types;
using System;

namespace BallisticCalculator.Panels.Panels;

public partial class ZeroAmmoPanel : UserControl
{
    private MeasurementSystem _measurementSystem = MeasurementSystem.Metric;

    public ZeroAmmoPanel()
    {
        InitializeComponent();
        WireEvents();
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
        }
    }

    public Ammunition? Ammunition
    {
        get
        {
            if (EnableCheckBox.IsChecked != true)
                return null;
            return AmmoSubPanel.Ammunition;
        }
        set
        {
            if (value == null)
            {
                EnableCheckBox.IsChecked = false;
                AmmoSubPanel.Clear();
                return;
            }

            EnableCheckBox.IsChecked = true;
            AmmoSubPanel.Ammunition = value;
        }
    }

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Public Methods

    public void Clear()
    {
        EnableCheckBox.IsChecked = false;
        AmmoSubPanel.Clear();
    }

    #endregion

    #region Private Methods

    private void WireEvents()
    {
        EnableCheckBox.IsCheckedChanged += OnEnableChanged;
        AmmoSubPanel.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
        LoadButton.Click += OnLoadClick;
    }

    private void OnEnableChanged(object? sender, RoutedEventArgs e)
    {
        var enabled = EnableCheckBox.IsChecked == true;
        AmmoSubPanel.IsEnabled = enabled;
        LoadButton.IsEnabled = enabled;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private async void OnLoadClick(object? sender, RoutedEventArgs e)
    {
        if (FileDialogService == null) return;

        var options = new FileDialogOptions
        {
            Title = "Load Ammunition for Zero",
            DefaultExtension = "ammox",
            Filters =
            {
                new Services.FileDialogFilter("Ammunition Files", "ammox", "ammo"),
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

            if (entry?.Ammunition != null)
                AmmoSubPanel.Ammunition = entry.Ammunition;
        }
        catch
        {
            // File load failed - silently ignore for now
        }
    }

    #endregion
}
