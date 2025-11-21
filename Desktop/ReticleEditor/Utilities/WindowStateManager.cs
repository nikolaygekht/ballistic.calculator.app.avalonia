using Avalonia.Controls;
using ReticleEditor.Models;
using System;
using System.IO;
using System.Text.Json;

namespace ReticleEditor.Utilities;

/// <summary>
/// Manages saving and loading window state
/// </summary>
public static class WindowStateManager
{
    private static readonly string StateFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ReticleEditor",
        "windowState.json");

    private static Models.WindowState? _cachedState;

    /// <summary>
    /// Save window state to file
    /// </summary>
    public static void Save(Models.WindowState state)
    {
        try
        {
            var directory = Path.GetDirectoryName(StateFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(StateFilePath, json);
            _cachedState = state;
        }
        catch (Exception ex)
        {
            // Log error but don't crash the application
            Console.WriteLine($"Failed to save window state: {ex.Message}");
        }
    }

    /// <summary>
    /// Load window state from file
    /// </summary>
    public static Models.WindowState? Load()
    {
        if (_cachedState != null)
            return _cachedState;

        try
        {
            if (!File.Exists(StateFilePath))
            {
                _cachedState = new Models.WindowState();
                return _cachedState;
            }

            var json = File.ReadAllText(StateFilePath);
            _cachedState = JsonSerializer.Deserialize<Models.WindowState>(json) ?? new Models.WindowState();
            return _cachedState;
        }
        catch (Exception ex)
        {
            // Log error but don't crash the application
            Console.WriteLine($"Failed to load window state: {ex.Message}");
            _cachedState = new Models.WindowState();
            return _cachedState;
        }
    }

    /// <summary>
    /// Restore dialog size from saved state
    /// </summary>
    /// <param name="window">Window to restore size for</param>
    /// <param name="dialogTypeName">Name of the dialog type (e.g., "EditLineDialog")</param>
    public static void RestoreDialogSize(Window window, string dialogTypeName)
    {
        var state = Load();
        if (state != null && state.DialogSizes.TryGetValue(dialogTypeName, out var size))
        {
            window.Width = size.Width;
            window.Height = size.Height;
        }
    }

    /// <summary>
    /// Save dialog size to state
    /// </summary>
    /// <param name="window">Window to save size from</param>
    /// <param name="dialogTypeName">Name of the dialog type (e.g., "EditLineDialog")</param>
    public static void SaveDialogSize(Window window, string dialogTypeName)
    {
        var state = Load() ?? new Models.WindowState();

        var size = new DialogSize
        {
            Width = window.Width,
            Height = window.Height
        };

        state.DialogSizes[dialogTypeName] = size;
        Save(state);
    }
}
