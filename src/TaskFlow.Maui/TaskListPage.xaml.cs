using TaskFlow.Core;
using TaskFlow.Core.ViewModels;

namespace TaskFlow.Maui;

public partial class TaskListPage : ContentPage
{
    private readonly TaskListViewModel _viewModel;

    public TaskListPage(TaskListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    // CheckBox has no Command, so the view forwards the event to the view model.
    // The guard ignores events raised when a recycled cell is re-bound.
    private void OnTaskCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: TaskRow row } && row.IsDone != e.Value)
        {
            _viewModel.ToggleCommand.Execute(row.Id);
        }
    }

    private void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: TaskRow row })
        {
            _viewModel.DeleteCommand.Execute(row.Id);
        }
    }
}
