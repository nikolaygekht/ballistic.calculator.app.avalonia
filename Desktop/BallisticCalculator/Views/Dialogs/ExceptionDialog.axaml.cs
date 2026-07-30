using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Text;
using System.Threading.Tasks;
using System;

namespace BallisticCalculator.Views.Dialogs;

/// <summary>
/// Reports an exception the application could not prevent: what it was doing, what went wrong, and the
/// full exception chain with stack traces for a bug report. Shown by the guards around the calculation
/// (see <see cref="Utilities.ShotCalculator.TryCalculate"/>) and around file I/O.
/// </summary>
public partial class ExceptionDialog : Window
{
    /// <param name="context">What the application was doing, in a sentence the user recognises.</param>
    /// <param name="exception">The exception to report.</param>
    public ExceptionDialog(string context, Exception exception)
    {
        InitializeComponent();

        ContextText.Text = context;
        MessageText.Text = exception?.Message ?? "";
        DetailsBox.Text = exception == null ? "" : FormatDetails(exception);
    }

    /// <summary>Shows the dialog over <paramref name="owner"/> and waits for it to be closed.</summary>
    public static async Task ShowAsync(Window owner, string context, Exception exception)
    {
        var dialog = new ExceptionDialog(context, exception);
        await dialog.ShowDialog(owner);
    }

    /// <summary>
    /// The whole exception chain as text: type, message and stack trace for the exception and each of its
    /// inner exceptions. A never-thrown exception has no stack trace, which is not worth a special case
    /// beyond leaving the section out.
    /// </summary>
    public static string FormatDetails(Exception exception)
    {
        var text = new StringBuilder();
        var current = exception;
        var depth = 0;

        while (current != null)
        {
            if (depth > 0)
                text.AppendLine().AppendLine("Caused by:");

            text.Append(current.GetType().FullName).Append(": ").AppendLine(current.Message);

            if (!string.IsNullOrWhiteSpace(current.StackTrace))
                text.AppendLine(current.StackTrace);

            current = current.InnerException;
            depth++;
        }

        return text.ToString();
    }

    private async void OnCopy(object? sender, RoutedEventArgs e)
    {
        var clipboard = GetTopLevel(this)?.Clipboard;
        if (clipboard == null)
            return;

        var text = $"{ContextText.Text}\n{MessageText.Text}\n\n{DetailsBox.Text}";
        await clipboard.SetTextAsync(text);
    }

    private void OnClose(object? sender, RoutedEventArgs e) => Close();
}
