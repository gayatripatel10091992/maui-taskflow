using Microsoft.Maui.Devices;
using TaskFlow.Core;
using TaskFlow.Core.ViewModels;

namespace TaskFlow.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        Func<DateOnly> today = () => DateOnly.FromDateTime(DateTime.Today);

        builder.Services.AddSingleton<ITaskRepository>(_ => new InMemoryTaskRepository(SampleTasks.Create(today())));
        builder.Services.AddSingleton(sp => new TaskListViewModel(
            sp.GetRequiredService<ITaskRepository>(),
            today,
            $".NET MAUI · {DeviceInfo.Current.Platform} {DeviceInfo.Current.VersionString}"));
        builder.Services.AddSingleton<TaskListPage>();

        return builder.Build();
    }
}
