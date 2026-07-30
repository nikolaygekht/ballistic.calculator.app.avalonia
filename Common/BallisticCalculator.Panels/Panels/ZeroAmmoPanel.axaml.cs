using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator;
using BallisticCalculator.Panels.Services;
using BallisticCalculator.Serialization;
using BallisticCalculator.Types;
using System;
using System.Collections.Generic;
using System.Linq;

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

    private IFileDialogService? _fileDialogService;
    public IFileDialogService? FileDialogService
    {
        get => _fileDialogService;
        set
        {
            _fileDialogService = value;
            AmmoSubPanel.FileDialogService = value;
        }
    }

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

    /// <summary>
    /// Why the override cannot be used, or empty when it is off or usable. A ticked-but-incomplete
    /// override reads as null to <see cref="ZeroingCalculator"/>, which means "same as the shot" — the shot
    /// then computes with the wrong zero and nothing says so (finding F-4). The inner panel's own problems
    /// (form factor without diameter, missing <c>.drg</c>) apply to the zero ammunition too.
    /// </summary>
    public List<string> Problems()
    {
        if (EnableCheckBox.IsChecked != true)
            return new List<string>();

        return AmmoSubPanel.Problems()
            .Select(p => $"Other ammunition for zero: {p}")
            .ToList();
    }

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
            InitialDirectory = DataFolders.Ammo,
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
