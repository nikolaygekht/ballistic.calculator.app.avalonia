using Avalonia.Controls;
using Avalonia.Interactivity;

namespace DebugApp1.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // Font Size Control
    private void OnSetFontSize10(object? sender, RoutedEventArgs e) => SetApplicationFontSize(10);
    private void OnSetFontSize12(object? sender, RoutedEventArgs e) => SetApplicationFontSize(12);
    private void OnSetFontSize13(object? sender, RoutedEventArgs e) => SetApplicationFontSize(13);
    private void OnSetFontSize14(object? sender, RoutedEventArgs e) => SetApplicationFontSize(14);
    private void OnSetFontSize16(object? sender, RoutedEventArgs e) => SetApplicationFontSize(16);

    private void SetApplicationFontSize(double fontSize)
    {
        var app = Avalonia.Application.Current;
        if (app == null) return;

        app.Resources["AppFontSize"] = fontSize;

        FontSizeDisplay.Text = $"Current font size: {fontSize}";
    }
}
