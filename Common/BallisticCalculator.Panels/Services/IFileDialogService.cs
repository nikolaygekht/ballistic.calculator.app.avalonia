namespace BallisticCalculator.Panels.Services;

public interface IFileDialogService
{
    Task<string?> OpenFileAsync(FileDialogOptions options);
    Task<string?> SaveFileAsync(FileDialogOptions options);
}
