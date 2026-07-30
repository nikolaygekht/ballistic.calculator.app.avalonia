using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AwesomeAssertions;
using BallisticCalculator.Reticle.Data;
using Gehtsoft.Measurements;
using ReticleEditor.Views;
using Xunit;

namespace ReticleEditor.Tests.Views;

/// <summary>
/// Cover for the unsaved-changes guard (finding F-6). Nothing in the editor used to track a dirty
/// document: <c>File → New</c> and <c>File → Open</c> built straight over the current reticle and closing
/// the window saved the *window state* and returned, so an hour of element-by-element work — with no undo
/// and no way to recover the drawing from the preview — was a stray Ctrl+N away.
/// </summary>
/// <remarks>
/// <see cref="MainWindow.UnsavedChangesPrompt"/> is the seam these tests need: the production prompt is a
/// modal window, which a headless run cannot answer.
/// </remarks>
public class UnsavedChangesTests
{
    #region What makes a document dirty

    [AvaloniaFact]
    public void NewWindow_IsNotDirty()
    {
        // Arrange & Act
        var window = new MainWindow();

        // Assert — an untouched editor has nothing to lose
        window.IsDirty.Should().BeFalse();
    }

    /// <summary>
    /// The measurement controls finish setting themselves up during layout and notify again, which used to
    /// dirty a document nobody had touched: the editor came up titled "(unsaved) *".
    /// </summary>
    [AvaloniaFact]
    public void NewWindow_AfterTheUiSettles_IsStillNotDirty()
    {
        // Arrange
        var window = new MainWindow();

        // Act — let every posted notification run
        window.Show();
        Dispatcher.UIThread.RunJobs();

        // Assert
        window.IsDirty.Should().BeFalse();
        window.Title.Should().NotContain("*");
    }

    [AvaloniaFact]
    public void LoadedReticle_AfterTheUiSettles_IsStillNotDirty()
    {
        // Arrange
        var window = new MainWindow();
        window.Show();

        // Act
        window.LoadReticle(CreateReticle(), "mildot.reticle");
        Dispatcher.UIThread.RunJobs();

        // Assert
        window.IsDirty.Should().BeFalse();
        window.Title.Should().NotContain("*");
    }

    [AvaloniaFact]
    public void SetReticleParameters_MarksTheDocumentDirty()
    {
        // Arrange
        var window = new MainWindow();

        // Act — the Set button commits the name/size/zero fields into the reticle
        window.ReticleName.Text = "My Reticle";
        window.SetReticleParameters();

        // Assert
        window.IsDirty.Should().BeTrue();
    }

    [AvaloniaFact]
    public void EditingAParameterField_MarksTheDocumentDirty()
    {
        // Arrange — typing is a change even before Set: the value is in the field, not in the reticle,
        // and closing the window would lose it
        var window = new MainWindow();

        // Act
        window.ReticleName.Text = "Half-typed name";

        // Assert
        window.IsDirty.Should().BeTrue();
    }

    [AvaloniaFact]
    public void AddingAnElement_MarksTheDocumentDirty()
    {
        // Arrange
        var window = new MainWindow();
        window.LoadReticle(CreateReticle(), fileName: null);

        // Act
        window.AddElement(new ReticleLine
        {
            Start = new ReticlePosition(0, 0, AngularUnit.Mil),
            End = new ReticlePosition(1, 1, AngularUnit.Mil),
        });

        // Assert
        window.IsDirty.Should().BeTrue();
    }

    [AvaloniaFact]
    public void DeletingAnElement_MarksTheDocumentDirty()
    {
        // Arrange
        var window = new MainWindow();
        var reticle = CreateReticle();
        reticle.Elements.Add(new ReticleLine
        {
            Start = new ReticlePosition(0, 0, AngularUnit.Mil),
            End = new ReticlePosition(1, 1, AngularUnit.Mil),
        });
        window.LoadReticle(reticle, fileName: null);
        window.ReticleItems.SelectedIndex = 0;

        // Act
        window.DeleteSelectedElement();

        // Assert
        window.IsDirty.Should().BeTrue();
    }

    [AvaloniaFact]
    public void LoadingAReticle_ClearsTheDirtyFlag()
    {
        // Arrange
        var window = new MainWindow();
        window.ReticleName.Text = "changed";
        window.IsDirty.Should().BeTrue();

        // Act — what Open does once the guard has let it through
        window.LoadReticle(CreateReticle(), "some.reticle");

        // Assert
        window.IsDirty.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Saving_ClearsTheDirtyFlag()
    {
        // Arrange
        var window = new MainWindow();
        var path = Path.Combine(Path.GetTempPath(), $"dirty-{System.Guid.NewGuid():N}.reticle");
        window.LoadReticle(CreateReticle(), path);
        window.ReticleName.Text = "Saved Reticle";
        window.IsDirty.Should().BeTrue();

        try
        {
            // Act
            window.SaveToFile(path);

            // Assert
            window.IsDirty.Should().BeFalse();
            File.Exists(path).Should().BeTrue();
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    #endregion

    #region What the guard does with each answer

    [AvaloniaFact]
    public async Task ConfirmDiscardChanges_CleanDocument_DoesNotPrompt()
    {
        // Arrange
        var window = new MainWindow();
        var asked = 0;
        window.UnsavedChangesPrompt = () => { asked++; return Task.FromResult(UnsavedChangesChoice.Cancel); };

        // Act
        var mayProceed = await window.ConfirmDiscardChangesAsync();

        // Assert — prompting when there is nothing to lose is its own kind of bug
        mayProceed.Should().BeTrue();
        asked.Should().Be(0);
    }

    [AvaloniaFact]
    public async Task ConfirmDiscardChanges_Cancel_StopsTheOperationAndKeepsTheChanges()
    {
        // Arrange
        var window = new MainWindow();
        window.ReticleName.Text = "changed";
        window.UnsavedChangesPrompt = () => Task.FromResult(UnsavedChangesChoice.Cancel);

        // Act
        var mayProceed = await window.ConfirmDiscardChangesAsync();

        // Assert
        mayProceed.Should().BeFalse();
        window.IsDirty.Should().BeTrue();
    }

    [AvaloniaFact]
    public async Task ConfirmDiscardChanges_Discard_LetsTheOperationThrough()
    {
        // Arrange
        var window = new MainWindow();
        window.ReticleName.Text = "changed";
        window.UnsavedChangesPrompt = () => Task.FromResult(UnsavedChangesChoice.Discard);

        // Act
        var mayProceed = await window.ConfirmDiscardChangesAsync();

        // Assert
        mayProceed.Should().BeTrue();
    }

    [AvaloniaFact]
    public async Task ConfirmDiscardChanges_Save_SavesAndLetsTheOperationThrough()
    {
        // Arrange — a document with a file name saves without a picker
        var window = new MainWindow();
        var path = Path.Combine(Path.GetTempPath(), $"dirty-{System.Guid.NewGuid():N}.reticle");
        window.LoadReticle(CreateReticle(), path);
        window.ReticleName.Text = "changed";
        window.UnsavedChangesPrompt = () => Task.FromResult(UnsavedChangesChoice.Save);

        try
        {
            // Act
            var mayProceed = await window.ConfirmDiscardChangesAsync();

            // Assert
            mayProceed.Should().BeTrue();
            window.IsDirty.Should().BeFalse();
            File.Exists(path).Should().BeTrue();
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    /// <summary>
    /// The case that would otherwise lose the work anyway: the user picks Save, then cancels the Save As
    /// picker (or the write fails). The document is still unsaved, so the operation must not proceed.
    /// </summary>
    [AvaloniaFact]
    public async Task ConfirmDiscardChanges_SaveThatDoesNotHappen_StopsTheOperation()
    {
        // Arrange — no file name, and a Save As that reports no file chosen
        var window = new MainWindow();
        window.ReticleName.Text = "changed";
        window.UnsavedChangesPrompt = () => Task.FromResult(UnsavedChangesChoice.Save);
        window.SaveAsOverride = () => Task.CompletedTask; // picker cancelled: nothing written

        // Act
        var mayProceed = await window.ConfirmDiscardChangesAsync();

        // Assert
        mayProceed.Should().BeFalse();
        window.IsDirty.Should().BeTrue();
    }

    #endregion

    #region File → New, the stray-Ctrl+N case

    [AvaloniaFact]
    public void FileNew_DirtyDocumentAndCancel_KeepsTheDocument()
    {
        // Arrange
        var window = new MainWindow();
        window.LoadReticle(CreateReticle(), "work-in-progress.reticle");
        window.ReticleName.Text = "An hour of work";
        window.UnsavedChangesPrompt = () => Task.FromResult(UnsavedChangesChoice.Cancel);

        // Act
        window.MenuFileNew.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        // Assert — nothing was replaced
        window.ReticleName.Text.Should().Be("An hour of work");
        window.IsDirty.Should().BeTrue();
        window.Title.Should().Contain("work-in-progress.reticle");
    }

    [AvaloniaFact]
    public void FileNew_DirtyDocumentAndDiscard_ReplacesIt()
    {
        // Arrange
        var window = new MainWindow();
        window.LoadReticle(CreateReticle(), "work-in-progress.reticle");
        window.ReticleName.Text = "An hour of work";
        window.UnsavedChangesPrompt = () => Task.FromResult(UnsavedChangesChoice.Discard);

        // Act
        window.MenuFileNew.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        // Assert — the default 10 x 10 mil reticle, and clean
        window.ReticleName.Text.Should().Be("New Reticle");
        window.IsDirty.Should().BeFalse();
    }

    [AvaloniaFact]
    public void FileNew_CleanDocument_DoesNotPrompt()
    {
        // Arrange
        var window = new MainWindow();
        window.LoadReticle(CreateReticle(), "saved.reticle");
        var asked = 0;
        window.UnsavedChangesPrompt = () => { asked++; return Task.FromResult(UnsavedChangesChoice.Cancel); };

        // Act
        window.MenuFileNew.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        // Assert
        asked.Should().Be(0);
        window.ReticleName.Text.Should().Be("New Reticle");
    }

    #endregion

    #region The keyboard shortcut asks once

    /// <summary>
    /// One keystroke, one prompt. <c>Ctrl+N</c> has two possible routes to the handler — the window's own
    /// key handling and the menu item's <c>InputGesture</c> — so this pins the count. (The prompt
    /// "vanishing and coming back" that prompted the check was Linux/WSLg modal handling, not a second
    /// dialog: Windows shows one.)
    /// </summary>
    [AvaloniaFact]
    public void CtrlN_DirtyDocument_PromptsOnce()
    {
        // Arrange
        var window = new MainWindow();
        window.Show();
        window.LoadReticle(CreateReticle(), "work-in-progress.reticle");
        window.ReticleName.Text = "An hour of work";
        var asked = 0;
        window.UnsavedChangesPrompt = () => { asked++; return Task.FromResult(UnsavedChangesChoice.Cancel); };

        // Act
        window.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Key = Key.N,
            KeyModifiers = KeyModifiers.Control,
        });
        Dispatcher.UIThread.RunJobs();

        // Assert
        asked.Should().Be(1);
    }

    #endregion

    #region Closing the window

    /// <summary>
    /// The worst of the three paths before this existed: closing saved the *window state* and returned
    /// without ever looking at the document.
    /// </summary>
    [AvaloniaFact]
    public void Close_DirtyDocumentAndCancel_KeepsTheWindowOpen()
    {
        // Arrange
        var window = new MainWindow();
        window.Show();
        window.LoadReticle(CreateReticle(), "work-in-progress.reticle");
        window.ReticleName.Text = "An hour of work";
        var asked = 0;
        window.UnsavedChangesPrompt = () => { asked++; return Task.FromResult(UnsavedChangesChoice.Cancel); };
        var closed = false;
        window.Closed += (_, _) => closed = true;

        // Act
        window.Close();
        Dispatcher.UIThread.RunJobs();

        // Assert
        asked.Should().Be(1);
        closed.Should().BeFalse();
        window.IsDirty.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Close_DirtyDocumentAndDiscard_Closes()
    {
        // Arrange
        var window = new MainWindow();
        window.Show();
        window.LoadReticle(CreateReticle(), "work-in-progress.reticle");
        window.ReticleName.Text = "An hour of work";
        window.UnsavedChangesPrompt = () => Task.FromResult(UnsavedChangesChoice.Discard);
        var closed = false;
        window.Closed += (_, _) => closed = true;

        // Act
        window.Close();
        Dispatcher.UIThread.RunJobs();

        // Assert
        closed.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Close_CleanDocument_ClosesWithoutAsking()
    {
        // Arrange
        var window = new MainWindow();
        window.Show();
        window.LoadReticle(CreateReticle(), "saved.reticle");
        var asked = 0;
        window.UnsavedChangesPrompt = () => { asked++; return Task.FromResult(UnsavedChangesChoice.Cancel); };
        var closed = false;
        window.Closed += (_, _) => closed = true;

        // Act
        window.Close();
        Dispatcher.UIThread.RunJobs();

        // Assert
        asked.Should().Be(0);
        closed.Should().BeTrue();
    }

    #endregion

    #region The title says so

    [AvaloniaFact]
    public void Title_DirtyDocument_IsMarked()
    {
        // Arrange
        var window = new MainWindow();
        var clean = window.Title;

        // Act
        window.ReticleName.Text = "changed";

        // Assert
        window.Title.Should().NotBe(clean);
        window.Title.Should().Contain("*");
    }

    [AvaloniaFact]
    public void Title_NamesTheFile()
    {
        // Arrange
        var window = new MainWindow();

        // Act
        window.LoadReticle(CreateReticle(), "/somewhere/mildot.reticle");

        // Assert
        window.Title.Should().Contain("mildot.reticle");
        window.Title.Should().NotContain("*");
    }

    #endregion

    private static ReticleDefinition CreateReticle() => new()
    {
        Name = "Test",
        Size = new ReticlePosition(10, 10, AngularUnit.Mil),
        Zero = new ReticlePosition(5, 5, AngularUnit.Mil),
    };
}
