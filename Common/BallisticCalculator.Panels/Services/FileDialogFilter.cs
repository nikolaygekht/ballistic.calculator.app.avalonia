namespace BallisticCalculator.Panels.Services;

public class FileDialogFilter
{
    public string Name { get; set; }
    public string[] Extensions { get; set; }

    public FileDialogFilter(string name, params string[] extensions)
    {
        Name = name;
        Extensions = extensions;
    }
}
