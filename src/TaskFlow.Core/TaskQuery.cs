namespace TaskFlow.Core;

/// <summary>Pure filtering/sorting/stats logic, mirroring TaskListState.from() in the Kotlin Multiplatform version.</summary>
public static class TaskQuery
{
    public static (IReadOnlyList<TaskItem> Visible, TaskStats Stats) Apply(
        IEnumerable<TaskItem> tasks,
        TaskFilter filter,
        string? query,
        DateOnly today)
    {
        var all = tasks.ToList();
        var stats = new TaskStats(
            Total: all.Count,
            Done: all.Count(t => t.IsDone),
            Overdue: all.Count(t => t.DueStatusOn(today) == DueStatus.Overdue),
            DueToday: all.Count(t => t.DueStatusOn(today) == DueStatus.Today));

        var q = query?.Trim() ?? string.Empty;

        var visible = all
            .Where(t => filter switch
            {
                TaskFilter.Active => !t.IsDone,
                TaskFilter.Done => t.IsDone,
                _ => true,
            })
            .Where(t => q.Length == 0
                        || t.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                        || (t.Tag?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false))
            // Open tasks first, then by priority, then earliest due date (no date last).
            .OrderBy(t => t.IsDone)
            .ThenBy(t => (int)t.Priority)
            .ThenBy(t => t.DueDate is null)
            .ThenBy(t => t.DueDate)
            .ThenBy(t => t.Id)
            .ToList();

        return (visible, stats);
    }
}
