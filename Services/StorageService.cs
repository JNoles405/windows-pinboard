using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using WindowsPinboard.Models;

namespace WindowsPinboard.Services;

public class StorageService
{
    private static readonly string AppDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WindowsPinboard");

    private static readonly string NotesFile = Path.Combine(AppDir, "notes.json");
    private static readonly string SettingsFile = Path.Combine(AppDir, "settings.json");

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public StorageService()
    {
        Directory.CreateDirectory(AppDir);
    }

    public ObservableCollection<Note> LoadNotes()
    {
        if (!File.Exists(NotesFile)) return new ObservableCollection<Note>();
        try
        {
            var json = File.ReadAllText(NotesFile);
            var list = JsonSerializer.Deserialize<List<Note>>(json, JsonOpts) ?? new();
            return new ObservableCollection<Note>(list.OrderByDescending(n => n.UpdatedAt));
        }
        catch
        {
            return new ObservableCollection<Note>();
        }
    }

    public void SaveNotes(IEnumerable<Note> notes)
    {
        var json = JsonSerializer.Serialize(notes.ToList(), JsonOpts);
        var tmp = NotesFile + ".tmp";
        File.WriteAllText(tmp, json);
        if (File.Exists(NotesFile)) File.Replace(tmp, NotesFile, null);
        else File.Move(tmp, NotesFile);
    }

    public AppSettings LoadSettings()
    {
        if (!File.Exists(SettingsFile)) return new AppSettings();
        try
        {
            var json = File.ReadAllText(SettingsFile);
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOpts) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void SaveSettings(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, JsonOpts);
        var tmp = SettingsFile + ".tmp";
        File.WriteAllText(tmp, json);
        if (File.Exists(SettingsFile)) File.Replace(tmp, SettingsFile, null);
        else File.Move(tmp, SettingsFile);
    }
}
