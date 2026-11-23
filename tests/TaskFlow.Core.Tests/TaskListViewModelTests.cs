using TaskFlow.Core;
using TaskFlow.Core.ViewModels;
using Xunit;

namespace TaskFlow.Core.Tests;

public class TaskListViewModelTests
{
    private static readonly DateOnly Today = new(2026, 11, 23);

    private static TaskListViewModel CreateViewModel(IEnumerable<TaskItem>? seed = null) =>
        new(new InMemoryTaskRepository(seed ?? SampleTasks.Create(Today)), () => Today);

    [Fact]
    public void LoadsSortedRowsAndStats()
    {
        var vm = CreateViewModel();

        Assert.Equal(8, vm.Tasks.Count);
        Assert.Equal(1, vm.Tasks[0].Id);
        Assert.Equal("Overdue · Nov 22", vm.Tasks[0].DueText);
        Assert.Equal("2 of 8 done", vm.Stats.Summary);
    }

    [Fact]
    public void SetFilterCommand_UpdatesListAndSelectionFlags()
    {
        var vm = CreateViewModel();

        vm.SetFilterCommand.Execute("Done");

        Assert.True(vm.IsDoneSelected);
        Assert.False(vm.IsAllSelected);
        Assert.All(vm.Tasks, row => Assert.True(row.IsDone));
    }

    [Fact]
    public void AddCommand_IsDisabledForBlankTitle_AndAddsTrimmedTask()
    {
        var vm = CreateViewModel(Array.Empty<TaskItem>());

        vm.NewTitle = "   ";
        Assert.False(vm.AddCommand.CanExecute(null));

        vm.NewTitle = "  Ship release ";
        vm.NewPriority = Priority.High;
        vm.AddCommand.Execute(null);

        var row = Assert.Single(vm.Tasks);
        Assert.Equal("Ship release", row.Title);
        Assert.Equal(Priority.High, row.Priority);
        Assert.Equal(string.Empty, vm.NewTitle);
    }

    [Fact]
    public void ToggleAndDelete_UpdateStats()
    {
        var vm = CreateViewModel();

        vm.ToggleCommand.Execute(1L);
        Assert.Equal(3, vm.Stats.Done);

        vm.DeleteCommand.Execute(1L);
        Assert.Equal(7, vm.Stats.Total);
    }

    [Fact]
    public void Query_FiltersRows()
    {
        var vm = CreateViewModel();

        vm.Query = "billing";

        Assert.Equal(2, Assert.Single(vm.Tasks).Id);
    }
}
