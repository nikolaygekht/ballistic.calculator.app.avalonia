using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BallisticCalculator.Views.Dialogs;

/// <summary>
/// A plain message with an OK button, for a condition the user can act on. Deliberately separate from
/// <see cref="ExceptionDialog"/>: a bad input dressed up with a stack trace reads as a crash, and an
/// unusable shot is not a crash.
/// </summary>
public partial class MessageDialog : Window
{
    public MessageDialog(string title, string message)
    {
        InitializeComponent();

        Title = title;
        MessageText.Text = message;
    }

    /// <summary>Shows the message over <paramref name="owner"/> and waits for it to be dismissed.</summary>
    public static async Task ShowAsync(Window owner, string title, string message)
    {
        var dialog = new MessageDialog(title, message);
        await dialog.ShowDialog(owner);
    }

    private void OnOk(object? sender, RoutedEventArgs e) => Close();
}
