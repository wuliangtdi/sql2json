using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sql2Json.Core.Models;
using Sql2Json.Core.Services;

namespace Sql2Json.App.ViewModels;

/// <summary>
/// 设置 ViewModel
/// 管理应用全局设置（JSON 格式选项、并发数等）
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly IConfigService _configService;
    private readonly ITaskExecutorService _taskExecutorService;

    /// <summary>
    /// JSON 是否格式化缩进
    /// </summary>
    [ObservableProperty]
    private bool _jsonIndented = true;

    /// <summary>
    /// JSON 是否使用 UTF-8 BOM
    /// </summary>
    [ObservableProperty]
    private bool _jsonUseBom;

    /// <summary>
    /// JSON 缩进字符数
    /// </summary>
    [ObservableProperty]
    private int _jsonIndentSize = 2;

    /// <summary>
    /// JSON 属性名是否转驼峰
    /// </summary>
    [ObservableProperty]
    private bool _jsonCamelCase;

    /// <summary>
    /// 最大并发执行数
    /// </summary>
    [ObservableProperty]
    private int _maxConcurrency = 5;

    /// <summary>
    /// 结果预览最大行数
    /// </summary>
    [ObservableProperty]
    private int _previewMaxRows = 100;

    /// <summary>
    /// 保存状态提示
    /// </summary>
    [ObservableProperty]
    private string? _saveMessage;

    public SettingsViewModel(IConfigService configService, ITaskExecutorService taskExecutorService)
    {
        _configService = configService;
        _taskExecutorService = taskExecutorService;
    }

    /// <summary>
    /// 从数据库加载设置
    /// </summary>
    public async Task LoadAsync()
    {
        var settings = await _configService.GetAppSettingsAsync();
        JsonIndented = settings.JsonIndented;
        JsonUseBom = settings.JsonUseBom;
        JsonIndentSize = settings.JsonIndentSize;
        JsonCamelCase = settings.JsonCamelCase;
        MaxConcurrency = settings.MaxConcurrency;
        PreviewMaxRows = settings.PreviewMaxRows;
    }

    /// <summary>
    /// 保存设置到数据库
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        var settings = new AppSettings
        {
            JsonIndented = JsonIndented,
            JsonUseBom = JsonUseBom,
            JsonIndentSize = JsonIndentSize,
            JsonCamelCase = JsonCamelCase,
            MaxConcurrency = MaxConcurrency,
            PreviewMaxRows = PreviewMaxRows
        };

        await _configService.UpdateAppSettingsAsync(settings);

        // 同步更新任务执行器的并发数
        _taskExecutorService.UpdateMaxConcurrency(MaxConcurrency);

        SaveMessage = "设置已保存";

        // 2 秒后清除提示
        _ = Task.Delay(2000).ContinueWith(_ => SaveMessage = null);
    }
}
