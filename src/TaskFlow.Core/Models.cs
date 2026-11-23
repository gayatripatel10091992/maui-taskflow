using System.Globalization;

namespace TaskFlow.Core;

public enum Priority { High, Medium, Low }

public enum TaskFilter { All, Active, Done }

public enum DueStatus { None, Overdue, Today, Upcoming }

/// <summary>Named TaskItem to avoid clashing with System.Threading.Tasks.Task.</summary>
public sealed record TaskItem(
    long Id,
    string Title,
    Priority Priority,
    DateOnly? DueDate = null,
    string? Tag = null,
    bool IsDone = false)
{
    public DueStatus DueStatusOn(DateOnly today) => this switch
    {
        { DueDate: null } or { IsDone: true } => DueStatus.None,
        { DueDate: var d } when d < today => DueStatus.Overdue,
        { DueDate: var d } when d == today => DueStatus.Today,
        _ => DueStatus.Upcoming,
    };
}

public sealed record TaskStats(int Total, int Done, int Overdue, int DueToday)
{
    public static readonly TaskStats Empty = new(0, 0, 0, 0);

    public int Active => Total - Done;
    public double Progress => Total == 0 ? 0 : (double)Done / Total;
    public string Summary => $"{Done} of {Total} done";
    public string ActiveText => $"{Active} active";
    public string DueTodayText => $"{DueToday} due today";
    public string OverdueText => $"{Overdue} overdue";
}

/// <summary>Display-ready row. Colours are hex strings so this library stays UI-framework agnostic.</summary>
public sealed record TaskRow(
    long Id,
    string Title,
    bool IsDone,
    Priority Priority,
    string? Tag,
    string DueText,
    DueStatus DueStatus)
{
    public string PriorityLabel => Priority.ToString();

    public string PriorityColor => Priority switch
    {
        Priority.High => "#E5484D",
        Priority.Medium => "#F5A524",
        _ => "#30A46C",
    };

    public string DueColor => DueStatus switch
    {
        DueStatus.Overdue => "#E5484D",
        DueStatus.Today => "#F5A524",
        _ => "#6B7280",
    };

    public bool HasTag => !string.IsNullOrWhiteSpace(Tag);
    public string TagText => HasTag ? $"#{Tag}" : string.Empty;

    public static TaskRow From(TaskItem item, DateOnly today)
    {
        var status = item.DueStatusOn(today);
        var date = item.DueDate?.ToString("MMM d", CultureInfo.InvariantCulture);
        var dueText = status switch
        {
            DueStatus.Overdue => $"Overdue · {date}",
            DueStatus.Today => "Today",
            _ => date ?? string.Empty,
        };
        return new TaskRow(item.Id, item.Title, item.IsDone, item.Priority, item.Tag, dueText, status);
    }
}
