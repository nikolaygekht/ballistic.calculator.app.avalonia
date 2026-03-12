using BallisticCalculator.Panels.Services;

namespace BallisticCalculator.Panels.Tests.Mocks;

public class MockFileDialogService : IFileDialogService
{
    public string? NextOpenResult { get; set; }
    public string? NextSaveResult { get; set; }
    public FileDialogOptions? LastOpenOptions { get; private set; }
    public FileDialogOptions? LastSaveOptions { get; private set; }

    public Task<string?> OpenFileAsync(FileDialogOptions options)
    {
        LastOpenOptions = options;
        return Task.FromResult(NextOpenResult);
    }

    public Task<string?> SaveFileAsync(FileDialogOptions options)
    {
        LastSaveOptions = options;
        return Task.FromResult(NextSaveResult);
    }
}
