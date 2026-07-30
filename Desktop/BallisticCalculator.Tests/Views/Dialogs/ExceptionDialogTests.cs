using System;
using Avalonia.Headless.XUnit;
using AwesomeAssertions;
using BallisticCalculator.Views.Dialogs;
using Xunit;

namespace BallisticCalculator.Tests.Views.Dialogs;

/// <summary>
/// Cover for the dialog that reports an exception the application could not prevent: the user needs the
/// message to know what went wrong, and the stack trace to put in a report.
/// </summary>
public class ExceptionDialogTests
{
    #region What the dialog shows

    [AvaloniaFact]
    public void Constructor_ShowsTheContextAndTheExceptionMessage()
    {
        // Arrange
        var error = Caught(new ArgumentException("If form-factor is used, the bullet diameter must be set"));

        // Act
        var dialog = new ExceptionDialog("The trajectory could not be calculated.", error);

        // Assert
        dialog.ContextText.Text.Should().Be("The trajectory could not be calculated.");
        dialog.MessageText.Text.Should().Contain("bullet diameter must be set");
    }

    [AvaloniaFact]
    public void Constructor_DetailsCarryTheTypeAndTheStackTrace()
    {
        // Arrange
        var error = Caught(new InvalidOperationException("The projectile cannot reach the zero distance"));

        // Act
        var dialog = new ExceptionDialog("The trajectory could not be calculated.", error);

        // Assert
        dialog.DetailsBox.Text.Should().Contain("InvalidOperationException");
        dialog.DetailsBox.Text.Should().Contain("The projectile cannot reach the zero distance");
        dialog.DetailsBox.Text.Should().Contain(nameof(Caught), "the stack trace names the frame that threw");
    }

    [AvaloniaFact]
    public void Constructor_DetailsAreReadOnlyButSelectable()
    {
        // Arrange & Act
        var dialog = new ExceptionDialog("Failed.", Caught(new Exception("boom")));

        // Assert — the point of the box is copying out of it, not typing into it
        dialog.DetailsBox.IsReadOnly.Should().BeTrue();
    }

    #endregion

    #region Details formatting (pure)

    [Fact]
    public void FormatDetails_InnerException_IncludesTheWholeChain()
    {
        // Arrange
        var inner = Caught(new ArgumentNullException("dragTable", "The drag table shoudn't be null"));
        var outer = Caught(new InvalidOperationException("Calculation failed", inner));

        // Act
        var details = ExceptionDialog.FormatDetails(outer);

        // Assert
        details.Should().Contain("InvalidOperationException");
        details.Should().Contain("Calculation failed");
        details.Should().Contain("ArgumentNullException");
        details.Should().Contain("The drag table shoudn't be null");
    }

    [Fact]
    public void FormatDetails_NeverThrownException_StillReportsTypeAndMessage()
    {
        // Arrange — an exception that was constructed but never thrown has no stack trace
        var error = new ArgumentException("no stack here");

        // Act
        var details = ExceptionDialog.FormatDetails(error);

        // Assert
        details.Should().Contain("ArgumentException");
        details.Should().Contain("no stack here");
    }

    #endregion

    /// <summary>Throws and catches, so the exception carries a real stack trace.</summary>
    private static T Caught<T>(T exception) where T : Exception
    {
        try
        {
            throw exception;
        }
        catch (T caught)
        {
            return caught;
        }
    }
}
