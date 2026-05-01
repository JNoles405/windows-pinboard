using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WindowsPinboard.Models;
using WindowsPinboard.Services;

namespace WindowsPinboard.ViewModels;

public class SidebarViewModel : INotifyPropertyChanged
{
    private readonly StorageService _storage;
    private Note? _selected;
    private string _searchText = "";

    public ObservableCollection<Note> Notes { get; }
    public ICollectionView NotesView { get; }

    public SidebarViewModel(StorageService storage)
    {
        _storage = storage;
        Notes = _storage.LoadNotes();
        Notes.CollectionChanged += (_, __) => Save();
        foreach (var n in Notes) HookNote(n);

        NotesView = System.Windows.Data.CollectionViewSource.GetDefaultView(Notes);
        NotesView.Filter = NoteMatchesFilter;
        NotesView.SortDescriptions.Add(new SortDescription(nameof(Note.UpdatedAt), ListSortDirection.Descending));

        AddNoteCommand = new RelayCommand(AddNote);
        DeleteNoteCommand = new RelayCommand(p =>
        {
            if (p is Note n) DeleteNote(n);
            else if (Selected is { } s) DeleteNote(s);
        });
    }

    public Note? Selected
    {
        get => _selected;
        set { if (_selected != value) { _selected = value; OnChanged(); OnChanged(nameof(HasSelection)); } }
    }

    public bool HasSelection => _selected is not null;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnChanged();
                NotesView.Refresh();
            }
        }
    }

    public ICommand AddNoteCommand { get; }
    public ICommand DeleteNoteCommand { get; }

    private bool NoteMatchesFilter(object obj)
    {
        if (string.IsNullOrWhiteSpace(_searchText)) return true;
        if (obj is not Note n) return false;
        var q = _searchText.Trim();
        return (n.Title?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
            || (n.Body?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public void AddNote()
    {
        var n = new Note { Title = "", Body = "" };
        HookNote(n);
        Notes.Insert(0, n);
        Selected = n;
    }

    public void DeleteNote(Note n)
    {
        UnhookNote(n);
        Notes.Remove(n);
        if (ReferenceEquals(Selected, n)) Selected = null;
    }

    private void HookNote(Note n) => n.PropertyChanged += OnNoteChanged;
    private void UnhookNote(Note n) => n.PropertyChanged -= OnNoteChanged;

    private void OnNoteChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Note.Title) or nameof(Note.Body) or nameof(Note.UpdatedAt))
        {
            Save();
            // Re-sort so freshly edited note moves to top
            NotesView.Refresh();
        }
    }

    private void Save() => _storage.SaveNotes(Notes);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
