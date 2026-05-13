using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sql2Json.Core.Enums;
using Sql2Json.Core.Models;
using Sql2Json.Core.Services;

namespace Sql2Json.App.ViewModels;

/// <summary>
/// 主窗口 ViewModel，管理左侧导航栏切换和子 ViewModel 的协调
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IConfigService _configService;
    private readonly ITaskExecutorService _taskExecutorService;

    /// <summary>
    /// 查询面板 ViewModel
    /// </summary>
    public QueryViewModel QueryViewModel { get; }

    /// <summary>
    /// 任务列表 ViewModel
    /// </summary>
    public TaskListViewModel TaskListViewModel { get; }

    /// <summary>
    /// 数据库配置 ViewModel
    /// </summary>
    public DatabaseConfigViewModel DatabaseConfigViewModel { get; }

    /// <summary>
    /// 文件夹配置 ViewModel
    /// </summary>
    public FolderConfigViewModel FolderConfigViewModel { get; }

    /// <summary>
    /// 设置 ViewModel
    /// </summary>
    public SettingsViewModel SettingsViewModel { get; }

    /// <summary>
    /// 当前选中的导航页索引（0=查询, 1=数据库, 2=文件夹, 3=设置）
    /// </summary>
    [ObservableProperty]
    private int _selectedNavIndex;

    /// <summary>
    /// 通知消息（任务完成时显示）
    /// </summary>
    [ObservableProperty]
    private string? _notificationMessage;

    /// <summary>
    /// 是否显示通知
    /// </summary>
    [ObservableProperty]
    private bool _isNotificationVisible;

    public MainWindowViewModel(
        IConfigService configService,
        ITaskExecutorService taskExecutorService,
        QueryViewModel queryViewModel,
        TaskListViewModel taskListViewModel,
        DatabaseConfigViewModel databaseConfigViewModel,
        FolderConfigViewModel folderConfigViewModel,
        SettingsViewModel settingsViewModel)
    {
        _configService = configService;
        _taskExecutorService = taskExecutorService;
        QueryViewModel = queryViewModel;
        TaskListViewModel = taskListViewModel;
        DatabaseConfigViewModel = databaseConfigViewModel;
        FolderConfigViewModel = folderConfigViewModel;
        SettingsViewModel = settingsViewModel;

        // 订阅任务完成事件，显示通知
        _taskExecutorService.TaskCompleted += OnTaskCompleted;
    }

    /// <summary>
    /// 任务完成时显示通知（3 秒后自动消失）
    /// </summary>
    private void OnTaskCompleted(QueryTask task)
    {
        var statusText = task.Status == QueryTaskStatus.Completed ? "✓ 完成" : "✕ 失败";
        NotificationMessage = $"{statusText}: {task.FileName}.json";
        IsNotificationVisible = true;

        // 3 秒后自动隐藏
        _ = Task.Delay(3000).ContinueWith(_ =>
        {
            IsNotificationVisible = false;
            NotificationMessage = null;
        });
    }

    /// <summary>
    /// 切换到查询页
    /// </summary>
    [RelayCommand]
    private void NavToQuery() => SelectedNavIndex = 0;

    /// <summary>
    /// 切换到数据库配置页
    /// </summary>
    [RelayCommand]
    private void NavToDatabase() => SelectedNavIndex = 1;

    /// <summary>
    /// 切换到文件夹配置页
    /// </summary>
    [RelayCommand]
    private void NavToFolder() => SelectedNavIndex = 2;

    /// <summary>
    /// 切换到设置页
    /// </summary>
    [RelayCommand]
    private void NavToSettings() => SelectedNavIndex = 3;

    /// <summary>
    /// 应用启动时加载初始数据
    /// </summary>
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        await QueryViewModel.LoadInitialDataAsync();
        await DatabaseConfigViewModel.LoadAsync();
        await FolderConfigViewModel.LoadAsync();
        await SettingsViewModel.LoadAsync();
    }
}
