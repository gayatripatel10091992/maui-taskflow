namespace TaskFlow.Maui;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;
    }

    // Resolve the page only after App.xaml resources are loaded; injecting the page into the
    // constructor would build it first and its StaticResource lookups would fail at startup.
    protected override Window CreateWindow(IActivationState? activationState) =>
        new(_services.GetRequiredService<TaskListPage>()) { Title = "TaskFlow", Width = 440, Height = 860 };
}
