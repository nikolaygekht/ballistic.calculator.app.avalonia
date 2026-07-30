using Avalonia.Controls;
using Avalonia.Interactivity;
using BallisticCalculator.Models;
using BallisticCalculator.Services;
using BallisticCalculator.Types;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        var (shotData, emptyPanels, incompletePanels, problems) = ShotDataPanel.Validate();

        // Everything wrong with the dialog goes into one message: fixing one problem and being told about
        // the next one is a worse experience than being told about all of them at once.
        var message = BuildProblemMessage(shotData, incompletePanels, problems);
        if (message != null)
        {
            await ShowError(message);
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

    /// <summary>
    /// The single message listing everything that stops the dialog from being accepted, or null when
    /// nothing does. Named problems come first — they say which field and why — and the partially filled
    /// tabs follow, because "Weather is incomplete" is the vaguest thing here and the least useful to read
    /// first.
    /// </summary>
    internal static string? BuildProblemMessage(ShotData? shotData,
        List<string> incompletePanels, List<string> problems)
    {
        var lines = new List<string>(problems);

        if (incompletePanels.Count > 0)
            lines.Add($"Not all required data filled in: {string.Join(", ", incompletePanels)}.");

        // The ammunition is the one thing with no default, so a missing one is always fatal. Its specific
        // reasons are already in `problems`; this only covers the case where none were reported.
        if (shotData == null && lines.Count == 0)
            lines.Add("Ammunition data is required.");

        if (lines.Count == 0)
            return null;

        if (lines.Count == 1)
            return lines[0];

        return "Please fix the following:\n\n" +
               string.Join("\n", lines.ConvertAll(l => $"• {l}"));
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private async Task ShowError(string message)
    {
        // The message is now a list rather than one line, so the window grows to fit it instead of
        // clipping at a fixed 150px.
        var dialog = new Window
        {
            Title = "Error",
            Width = 460,
            MinHeight = 140,
            MaxHeight = 600,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = true,
            Content = new DockPanel
            {
                Children =
                {
                    CreateButton("OK", true, DockPanel.DockProperty, Avalonia.Controls.Dock.Bottom),
                    new ScrollViewer
                    {
                        VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                        HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                        Content = new TextBlock
                        {
                            Text = message,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            Margin = new Avalonia.Thickness(15),
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        },
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
