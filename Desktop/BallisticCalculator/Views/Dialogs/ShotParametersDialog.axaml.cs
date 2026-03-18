using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Models;
using BallisticCalculator.Services;
using BallisticCalculator.Types;

namespace BallisticCalculator.Views.Dialogs;

public partial class ShotParametersDialog : Window
{
    public ShotParametersDialog(MeasurementSystem measurementSystem, ShotData? shotData = null)
    {
        InitializeComponent();

        ShotDataPanel.FileDialogService = new FileDialogService(this);
        ShotDataPanel.MeasurementSystem = measurementSystem;

        if (shotData != null)
            ShotDataPanel.ShotData = shotData;

        var state = AppStateManager.Load();
        if (state.ShotDialogWidth > 0 && state.ShotDialogHeight > 0)
        {
            Width = state.ShotDialogWidth;
            Height = state.ShotDialogHeight;
        }

        Closing += (_, _) =>
        {
            state.ShotDialogWidth = Width;
            state.ShotDialogHeight = Height;
            AppStateManager.Save(state);
        };
    }

    public ShotData? Result { get; private set; }

    private async void OnOK(object? sender, RoutedEventArgs e)
    {
        var (shotData, emptyPanels, incompletePanels) = ShotDataPanel.Validate();

        // Ammunition is required
        if (shotData == null)
        {
            await ShowError("Ammunition data is required.");
            return;
        }

        // Partially filled panels are a user error
        if (incompletePanels.Count > 0)
        {
            await ShowError($"Not all required data filled in: {string.Join(", ", incompletePanels)}");
            return;
        }

        // Completely empty panels — ask whether to use defaults
        if (emptyPanels.Count > 0)
        {
            var useDefaults = await ShowConfirm(
                $"{string.Join(", ", emptyPanels)} not filled.\nUse default values?");
            if (!useDefaults)
                return;
        }

        Result = shotData;
        Close(true);
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private async Task ShowError(string message)
    {
        var dialog = new Window
        {
            Title = "Error",
            Width = 350,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Content = new DockPanel
            {
                Children =
                {
                    CreateButton("OK", true, DockPanel.DockProperty, Avalonia.Controls.Dock.Bottom),
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        Margin = new Avalonia.Thickness(15),
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    }
                }
            }
        };
        await dialog.ShowDialog(this);
    }

    private async Task<bool> ShowConfirm(string message)
    {
        var result = false;
        var dialog = new Window
        {
            Title = "Confirm",
            Width = 380,
            Height = 160,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
        };

        var yesButton = new Button { Content = "Yes", Width = 80 };
        var noButton = new Button { Content = "No", Width = 80 };

        yesButton.Click += (_, _) => { result = true; dialog.Close(); };
        noButton.Click += (_, _) => { result = false; dialog.Close(); };

        dialog.Content = new DockPanel
        {
            Children =
            {
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Spacing = 10,
                    Margin = new Avalonia.Thickness(0, 10),
                    [DockPanel.DockProperty] = Avalonia.Controls.Dock.Bottom,
                    Children = { yesButton, noButton }
                },
                new TextBlock
                {
                    Text = message,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Margin = new Avalonia.Thickness(15),
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                }
            }
        };

        await dialog.ShowDialog(this);
        return result;
    }

    private static Button CreateButton(string text, bool isDefault, Avalonia.AvaloniaProperty dockProp, Avalonia.Controls.Dock dock)
    {
        var btn = new Button
        {
            Content = text,
            Width = 80,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 10),
        };
        btn.SetValue(dockProp, dock);
        btn.Click += (_, _) => (btn.Parent?.Parent as Window ?? (btn.Parent as DockPanel)?.Parent as Window)?.Close();
        return btn;
    }
}
