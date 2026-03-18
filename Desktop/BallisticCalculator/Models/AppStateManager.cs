using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace BallisticCalculator.Models;

public static class AppStateManager
{
    private static readonly string StateFilePath;
    private static AppState? _cached;

    static AppStateManager()
    {
        var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
        StateFilePath = Path.Combine(exeDir, "appstate.json");
    }

    public static AppState Load()
    {
        if (_cached != null)
            return _cached;

        try
        {
            if (File.Exists(StateFilePath))
            {
                var json = File.ReadAllText(StateFilePath);
                _cached = JsonSerializer.Deserialize<AppState>(json) ?? new AppState();
            }
            else
            {
                _cached = new AppState();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load app state: {ex.Message}");
            _cached = new AppState();
        }

        return _cached;
    }

    public static void Save(AppState state)
    {
        _cached = state;
        try
        {
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(StateFilePath, json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to save app state: {ex.Message}");
        }
    }
}
