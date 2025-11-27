using Avalonia.Controls;
using BallisticCalculator.Reticle.Data;
using ReticleEditor.Views.Dialogs;

namespace ReticleEditor.Utilities;

/// <summary>
/// Factory class for creating element editor dialogs based on element type
/// </summary>
public static class DialogFactory
{
    /// <summary>
    /// Creates the appropriate editor dialog for the given element
    /// </summary>
    /// <param name="element">The element to edit</param>
    /// <returns>A dialog window configured to edit the element, or null if no editor exists</returns>
    public static Window? CreateDialogForElement(object element)
    {
        return CreateDialogForElement(element, null);
    }

    /// <summary>
    /// Creates the appropriate editor dialog for the given element with reticle context
    /// </summary>
    /// <param name="element">The element to edit</param>
    /// <param name="reticle">The reticle definition for context (used for preview in path editor)</param>
    /// <returns>A dialog window configured to edit the element, or null if no editor exists</returns>
    public static Window? CreateDialogForElement(object element, ReticleDefinition? reticle)
    {
        return element switch
        {
            // Main reticle elements
            ReticleLine line => new EditLineDialog(line),
            ReticleCircle circle => new EditCircleDialog(circle),
            ReticleRectangle rect => new EditRectangleDialog(rect),
            ReticleText text => new EditTextDialog(text),
            ReticleBulletDropCompensatorPoint bdc => new EditBdcDialog(bdc),
            ReticlePath path => new EditPathDialog(path, reticle),

            // Path sub-elements
            ReticlePathElementMoveTo moveTo => new EditMoveToDialog(moveTo),
            ReticlePathElementLineTo lineTo => new EditLineToDialog(lineTo),
            ReticlePathElementArc arc => new EditArcDialog(arc),

            _ => null
        };
    }
}
