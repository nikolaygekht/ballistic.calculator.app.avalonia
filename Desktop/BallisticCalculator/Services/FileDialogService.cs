using Avalonia.Controls;
using Avalonia.Platform.Storage;
using BallisticCalculator.Panels.Services;
using System.Linq;
using System.Threading.Tasks;

namespace BallisticCalculator.Services;

public class FileDialogService : IFileDialogService
{
    private readonly Window _owner;

    public FileDialogService(Window owner)
    {
        _owner = owner;
    }

    public async Task<string?> OpenFileAsync(FileDialogOptions options)
    {
        var storageProvider = _owner.StorageProvider;

        var fileTypes = options.Filters.Select(f => new FilePickerFileType(f.Name)
        {
            Patterns = f.Extensions.Select(ext => $"*.{ext}").ToArray()
        }).ToArray();

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = options.Title ?? "Open",
            AllowMultiple = false,
            FileTypeFilter = fileTypes.Length > 0 ? fileTypes : null,
        });

        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }

    public async Task<string?> SaveFileAsync(FileDialogOptions options)
    {
        var storageProvider = _owner.StorageProvider;

        var fileTypes = options.Filters.Select(f => new FilePickerFileType(f.Name)
        {
            Patterns = f.Extensions.Select(ext => $"*.{ext}").ToArray()
        }).ToArray();

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = options.Title ?? "Save",
            SuggestedFileName = options.InitialFileName,
            DefaultExtension = options.DefaultExtension,
            FileTypeChoices = fileTypes.Length > 0 ? fileTypes : null,
        });

        return file?.Path.LocalPath;
    }
}
