using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sql2Json.Core.Services;

namespace Sql2Json.App.ViewModels;

/// <summary>
/// 主窗口 ViewModel，管理左侧导航栏切换和子 ViewModel 的协调
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IConfigService _configService;

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

    public MainWindowViewModel(
        IConfigService configService,
        QueryViewModel queryViewModel,
        TaskListViewModel taskListViewModel,
        DatabaseConfigViewModel databaseConfigViewModel,
        FolderConfigViewModel folderConfigViewModel,
        SettingsViewModel settingsViewModel)
    {
        _configService = configService;
        QueryViewModel = queryViewModel;
        TaskListViewModel = taskListViewModel;
        DatabaseConfigViewModel = databaseConfigViewModel;
        FolderConfigViewModel = folderConfigViewModel;
        SettingsViewModel = settingsViewModel;
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
