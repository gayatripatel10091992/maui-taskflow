using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TaskFlow.Core.ViewModels;

/// <summary>
/// MVVM view model built with CommunityToolkit.Mvvm source generators.
/// It lives in a plain net10.0 library, so it is unit-tested with xUnit without any device or emulator.
/// </summary>
public partial class TaskListViewModel : ObservableObject
{
    private readonly ITaskRepository _repository;
    private readonly Func<DateOnly> _today;

    public TaskListViewModel(ITaskRepository repository, Func<DateOnly> today, string platformLabel = ".NET MAUI")
    {
        _repository = repository;
        _today = today;
        PlatformLabel = platformLabel;
        _repository.Changed += (_, _) => Refresh();
        Refresh();
    }

    public string PlatformLabel { get; }

    public ObservableCollection<TaskRow> Tasks { get; } = new();

    /// <summary>An array (not IReadOnlyList) because MAUI's Picker.ItemsSource is a non-generic IList.</summary>
    public Priority[] Priorities { get; } = Enum.GetValues<Priority>();

    [ObservableProperty]
    private TaskStats _stats = TaskStats.Empty;

    [ObservableProperty]
    private string _query = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAllSelected))]
    [NotifyPropertyChangedFor(nameof(IsActiveSelected))]
    [NotifyPropertyChangedFor(nameof(IsDoneSelected))]
    private TaskFilter _filter = TaskFilter.All;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private string _newTitle = string.Empty;

    [ObservableProperty]
    private Priority _newPriority = Priority.Medium;

    public bool IsAllSelected => Filter == TaskFilter.All;
    public bool IsActiveSelected => Filter == TaskFilter.Active;
    public bool IsDoneSelected => Filter == TaskFilter.Done;

    partial void OnQueryChanged(string value) => Refresh();

    partial void OnFilterChanged(TaskFilter value) => Refresh();

    [RelayCommand]
    private void SetFilter(string filter)
    {
        if (Enum.TryParse<TaskFilter>(filter, ignoreCase: true, out var parsed)) Filter = parsed;
    }

    private bool CanAdd() => !string.IsNullOrWhiteSpace(NewTitle);

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        var title = NewTitle.Trim();
        if (title.Length == 0) return;
        _repository.Add(new TaskItem(0, title, NewPriority, _today().AddDays(1)));
        NewTitle = string.Empty;
    }

    [RelayCommand]
    private void Toggle(long id) => _repository.Toggle(id);

    [RelayCommand]
    private void Delete(long id) => _repository.Delete(id);

    private void Refresh()
    {
        var today = _today();
        var (visible, stats) = TaskQuery.Apply(_repository.GetAll(), Filter, Query, today);
        Tasks.Clear();
        foreach (var task in visible) Tasks.Add(TaskRow.From(task, today));
        Stats = stats;
    }
}
