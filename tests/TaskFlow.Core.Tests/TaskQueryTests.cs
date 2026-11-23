using TaskFlow.Core;
using Xunit;

namespace TaskFlow.Core.Tests;

/// <summary>Same cases as the Kotlin Multiplatform TaskListStateTest, to prove behaviour parity.</summary>
public class TaskQueryTests
{
    private static readonly DateOnly Today = new(2026, 11, 23);
    private readonly IReadOnlyList<TaskItem> _tasks = SampleTasks.Create(Today);

    [Fact]
    public void Stats_CountDoneOverdueAndToday()
    {
        var (_, stats) = TaskQuery.Apply(_tasks, TaskFilter.All, "", Today);

        Assert.Equal(8, stats.Total);
        Assert.Equal(2, stats.Done);
        Assert.Equal(1, stats.Overdue);
        Assert.Equal(1, stats.DueToday);
        Assert.Equal(0.25, stats.Progress, precision: 3);
    }

    [Fact]
    public void ActiveFilter_HidesDone_AndSortsByPriorityThenDate()
    {
        var (visible, _) = TaskQuery.Apply(_tasks, TaskFilter.Active, "", Today);

        Assert.DoesNotContain(visible, t => t.IsDone);
        Assert.Equal(new long[] { 1, 2, 3, 4, 5, 6 }, visible.Select(t => t.Id));
    }

    [Fact]
    public void Query_MatchesTitleOrTag_CaseInsensitive()
    {
        var (visible, _) = TaskQuery.Apply(_tasks, TaskFilter.All, "  maui ", Today);

        Assert.Equal(new long[] { 4, 7 }, visible.Select(t => t.Id));
    }

    [Theory]
    [InlineData(-1, DueStatus.Overdue)]
    [InlineData(0, DueStatus.Today)]
    [InlineData(2, DueStatus.Upcoming)]
    public void DueStatus_IsRelativeToToday(int offsetDays, DueStatus expected)
    {
        var task = new TaskItem(1, "x", Priority.Low, Today.AddDays(offsetDays));
        Assert.Equal(expected, task.DueStatusOn(Today));
    }

    [Fact]
    public void DoneTask_IsNeverOverdue()
    {
        var task = new TaskItem(1, "x", Priority.Low, Today.AddDays(-10), IsDone: true);
        Assert.Equal(DueStatus.None, task.DueStatusOn(Today));
    }
}
