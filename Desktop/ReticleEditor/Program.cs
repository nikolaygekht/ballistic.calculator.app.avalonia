using Avalonia;
using System;
using System.Threading.Tasks;

namespace ReticleEditor;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            ShowException(ex);
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .AfterSetup(_ =>
            {
                Avalonia.Threading.Dispatcher.UIThread.UnhandledException += OnUIThreadException;
            });

    private static void OnUIThreadException(object sender, Avalonia.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        ShowException(e.Exception);
        e.Handled = true;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        ShowException(e.ExceptionObject as Exception);
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        ShowException(e.Exception);
        e.SetObserved();
    }

    private static void ShowException(Exception? ex)
    {
        var message = ex?.ToString() ?? "Unknown error";
        Console.Error.WriteLine(message);

        try
        {
            var lifetime = Avalonia.Application.Current?.ApplicationLifetime
                as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
            var mainWindow = lifetime?.MainWindow;

            if (mainWindow != null)
            {
                var dialog = new Avalonia.Controls.Window
                {
                    Title = "Unhandled Exception",
                    Width = 600,
                    Height = 400,
                    WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,
                    Content = new Avalonia.Controls.ScrollViewer
                    {
                        Content = new Avalonia.Controls.TextBlock
                        {
                            Text = message,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            Margin = new Avalonia.Thickness(10),
                        }
                    }
                };
                dialog.ShowDialog(mainWindow);
            }
        }
        catch
        {
            // Last resort - already wrote to stderr
        }
    }
}
