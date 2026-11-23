namespace TaskFlow.Core;

public interface ITaskRepository
{
    event EventHandler? Changed;
    IReadOnlyList<TaskItem> GetAll();
    TaskItem Add(TaskItem item);
    void Toggle(long id);
    void Delete(long id);
}

/// <summary>Thread-safe in-memory store. Swap for SQLite without touching the view model.</summary>
public sealed class InMemoryTaskRepository : ITaskRepository
{
    private readonly object _gate = new();
    private readonly List<TaskItem> _items;

    public InMemoryTaskRepository(IEnumerable<TaskItem>? seed = null)
    {
        _items = seed?.ToList() ?? new List<TaskItem>();
    }

    public event EventHandler? Changed;

    public IReadOnlyList<TaskItem> GetAll()
    {
        lock (_gate) return _items.ToList();
    }

    public TaskItem Add(TaskItem item)
    {
        TaskItem created;
        lock (_gate)
        {
            var nextId = _items.Count == 0 ? 1 : _items.Max(t => t.Id) + 1;
            created = item with { Id = nextId };
            _items.Add(created);
        }
        Changed?.Invoke(this, EventArgs.Empty);
        return created;
    }

    public void Toggle(long id) => Update(id, t => t with { IsDone = !t.IsDone });

    public void Delete(long id)
    {
        bool removed;
        lock (_gate) removed = _items.RemoveAll(t => t.Id == id) > 0;
        if (removed) Changed?.Invoke(this, EventArgs.Empty);
    }

    private void Update(long id, Func<TaskItem, TaskItem> change)
    {
        var updated = false;
        lock (_gate)
        {
            var index = _items.FindIndex(t => t.Id == id);
            if (index >= 0)
            {
                _items[index] = change(_items[index]);
                updated = true;
            }
        }
        if (updated) Changed?.Invoke(this, EventArgs.Empty);
    }
}

public static class SampleTasks
{
    /// <summary>Same demo data as the Kotlin Multiplatform version, relative to <paramref name="today"/>.</summary>
    public static IReadOnlyList<TaskItem> Create(DateOnly today) => new[]
    {
        new TaskItem(1, "Fix ANR in call screen", Priority.High, today.AddDays(-1), "Android"),
        new TaskItem(2, "Review Play Billing upgrade PR", Priority.High, today, "Review"),
        new TaskItem(3, "Target API 35 behaviour changes", Priority.Medium, today.AddDays(3), "Android"),
        new TaskItem(4, "Write MAUI view model tests", Priority.Medium, today.AddDays(5), "MAUI"),
        new TaskItem(5, "Plan sprint with client", Priority.Low, today.AddDays(7), "Planning"),
        new TaskItem(6, "Update Appium UI tests", Priority.Low, null, "Tooling"),
        new TaskItem(7, "Settings & update management", Priority.Medium, today.AddDays(-2), "MAUI", IsDone: true),
        new TaskItem(8, "Set up GitHub Actions CI", Priority.High, today.AddDays(-4), "CI", IsDone: true),
    };
}
