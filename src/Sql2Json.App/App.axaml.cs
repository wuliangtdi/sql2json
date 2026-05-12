using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Sql2Json.App.ViewModels;
using Sql2Json.App.Views;
using Sql2Json.Core.Data;
using Sql2Json.Core.Services;

namespace Sql2Json.App;

/// <summary>
/// Avalonia 应用主类，负责初始化主题、DI 容器和主窗口
/// </summary>
public class App : Application
{
    /// <summary>
    /// 全局服务提供者，用于解析依赖
    /// </summary>
    public static ServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 配置 DI 容器
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        // 确保本地数据库已创建
        var dbContext = Services.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureCreated();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// 注册所有服务和 ViewModel 到 DI 容器
    /// </summary>
    private static void ConfigureServices(IServiceCollection services)
    {
        // 数据层：使用 DbContextFactory 支持多线程并发访问
        services.AddDbContext<AppDbContext>();
        services.AddDbContextFactory<AppDbContext>();

        // 核心服务
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<IDatabaseService, DatabaseService>();
        services.AddSingleton<IJsonExportService, JsonExportService>();
        services.AddSingleton<ITaskExecutorService, TaskExecutorService>();
        services.AddSingleton<IQueryHistoryService, QueryHistoryService>();

        // ViewModels（需要跨 VM 共享状态的用 Singleton）
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<QueryViewModel>();
        services.AddSingleton<TaskListViewModel>();
        services.AddSingleton<DatabaseConfigViewModel>();
        services.AddSingleton<FolderConfigViewModel>();
        services.AddSingleton<SettingsViewModel>();
    }
}
