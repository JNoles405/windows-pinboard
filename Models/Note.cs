using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WindowsPinboard.Models;

public class Note : INotifyPropertyChanged
{
    private string _title = "";
    private string _body = "";
    private DateTime _updatedAt = DateTime.UtcNow;

    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Title
    {
        get => _title;
        set { if (_title != value) { _title = value; _updatedAt = DateTime.UtcNow; OnChanged(); OnChanged(nameof(UpdatedAt)); OnChanged(nameof(DisplayTitle)); } }
    }

    public string Body
    {
        get => _body;
        set { if (_body != value) { _body = value; _updatedAt = DateTime.UtcNow; OnChanged(); OnChanged(nameof(UpdatedAt)); OnChanged(nameof(DisplayTitle)); OnChanged(nameof(Preview)); } }
    }

    public DateTime UpdatedAt
    {
        get => _updatedAt;
        set { if (_updatedAt != value) { _updatedAt = value; OnChanged(); } }
    }

    public string DisplayTitle => string.IsNullOrWhiteSpace(_title)
        ? (FirstLine(_body) is { Length: > 0 } s ? s : "Untitled")
        : _title;

    public string Preview
    {
        get
        {
            var text = _body?.Replace("\r", "").Replace("\n", "  ") ?? "";
            return text.Length > 120 ? text[..120] + "…" : text;
        }
    }

    private static string FirstLine(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var idx = s.IndexOfAny(['\r', '\n']);
        var line = idx >= 0 ? s[..idx] : s;
        return line.Trim();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
