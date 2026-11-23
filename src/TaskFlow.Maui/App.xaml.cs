namespace TaskFlow.Maui;

public partial class App : Application
{
    private readonly TaskListPage _page;

    public App(TaskListPage page)
    {
        InitializeComponent();
        _page = page;
    }

    protected override Window CreateWindow(IActivationState? activationState) =>
        new(_page) { Title = "TaskFlow", Width = 440, Height = 860 };
}
