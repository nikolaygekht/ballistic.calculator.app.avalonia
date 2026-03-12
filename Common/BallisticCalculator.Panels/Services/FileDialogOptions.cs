namespace BallisticCalculator.Panels.Services;

public class FileDialogOptions
{
    public string? Title { get; set; }
    public string? InitialDirectory { get; set; }
    public string? InitialFileName { get; set; }
    public string? DefaultExtension { get; set; }
    public List<FileDialogFilter> Filters { get; } = new();
}
