namespace ReticleEditor.Views;

/// <summary>The answer to the unsaved-changes prompt.</summary>
internal enum UnsavedChangesChoice
{
    /// <summary>Save the document, then carry on with what was asked.</summary>
    Save,

    /// <summary>Throw the changes away and carry on.</summary>
    Discard,

    /// <summary>Do not carry on; keep the document as it is.</summary>
    Cancel,
}
