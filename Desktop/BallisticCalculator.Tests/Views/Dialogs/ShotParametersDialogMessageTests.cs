using System.Collections.Generic;
using AwesomeAssertions;
using BallisticCalculator.Types;
using BallisticCalculator.Views.Dialogs;
using Xunit;

namespace BallisticCalculator.Tests.Views.Dialogs;

/// <summary>
/// Cover for the message the OK button shows: one message listing everything that is wrong, rather than
/// the first fault only. Fixing one problem and being told about the next is the behaviour these tests
/// exist to prevent.
/// </summary>
public class ShotParametersDialogMessageTests
{
    private static readonly ShotData AnyShotData = new();

    #region Nothing wrong

    [Fact]
    public void BuildProblemMessage_NothingWrong_ReturnsNull()
    {
        ShotParametersDialog.BuildProblemMessage(AnyShotData, new List<string>(), new List<string>())
            .Should().BeNull();
    }

    #endregion

    #region One fault reads as a plain sentence

    [Fact]
    public void BuildProblemMessage_SingleProblem_IsNotDressedUpAsAList()
    {
        // Arrange
        var problems = new List<string> { "Muzzle velocity is not specified." };

        // Act
        var message = ShotParametersDialog.BuildProblemMessage(AnyShotData, new List<string>(), problems);

        // Assert
        message.Should().Be("Muzzle velocity is not specified.");
    }

    #endregion

    #region Several faults are all reported together

    [Fact]
    public void BuildProblemMessage_SeveralProblems_ListsEveryOne()
    {
        // Arrange
        var problems = new List<string>
        {
            "Bullet diameter is required when the ballistic coefficient is a form factor.",
            "Wind at zero is ticked, but not all of its fields are filled in.",
            "V-Clicks are dialled, but the sight has no vertical click size.",
        };

        // Act
        var message = ShotParametersDialog.BuildProblemMessage(AnyShotData, new List<string>(), problems);

        // Assert
        message.Should().NotBeNull();
        foreach (var problem in problems)
            message.Should().Contain(problem);
        message.Should().Contain("•", "a list of faults is easier to read as a list");
    }

    [Fact]
    public void BuildProblemMessage_ProblemsAndIncompletePanels_ReportsBoth()
    {
        // Arrange
        var problems = new List<string> { "Muzzle velocity is not specified." };
        var incomplete = new List<string> { "Weather", "Rifle" };

        // Act
        var message = ShotParametersDialog.BuildProblemMessage(null, incomplete, problems);

        // Assert
        message.Should().Contain("Muzzle velocity is not specified.");
        message.Should().Contain("Weather, Rifle");
    }

    #endregion

    #region A missing ammunition is always fatal

    /// <summary>
    /// The specific reasons normally come from the panel; this is the fallback for a null shot with no
    /// reported reason, which must still refuse rather than accept a null.
    /// </summary>
    [Fact]
    public void BuildProblemMessage_NoShotDataAndNoReportedReason_StillRefuses()
    {
        ShotParametersDialog.BuildProblemMessage(null, new List<string>(), new List<string>())
            .Should().Be("Ammunition data is required.");
    }

    [Fact]
    public void BuildProblemMessage_NoShotDataWithReportedReasons_DoesNotAddTheGenericLine()
    {
        // Arrange
        var problems = new List<string> { "Bullet weight is not specified." };

        // Act
        var message = ShotParametersDialog.BuildProblemMessage(null, new List<string>(), problems);

        // Assert — the generic line said nothing the specific one does not
        message.Should().Be("Bullet weight is not specified.");
        message.Should().NotContain("Ammunition data is required");
    }

    #endregion
}
