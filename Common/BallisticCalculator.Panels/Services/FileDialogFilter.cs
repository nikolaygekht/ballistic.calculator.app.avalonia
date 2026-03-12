namespace BallisticCalculator.Panels.Services;

public class FileDialogFilter
{
    public string Name { get; set; }
    public string Extension { get; set; }

    public FileDialogFilter(string name, string extension)
    {
        Name = name;
        Extension = extension;
    }
}
