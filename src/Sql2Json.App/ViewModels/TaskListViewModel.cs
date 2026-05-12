using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sql2Json.Core.Enums;
using Sql2Json.Core.Models;
using Sql2Json.Core.Services;

namespace Sql2Json.App.ViewModels;

/// <summary>
/// 任务列表 ViewModel
/// 管理所有查询任务的显示、状态跟踪、结果预览和批量导出
/// </summary>
public partial class TaskListViewModel : ObservableObject
{
    private readonly IJsonExportService _jsonExportService;
    private readonly IConfigService _configService;

    /// <summary>
    /// 所有查询任务列表（按创建时间倒序）
    /// </summary>
    public ObservableCollection<QueryTask> Tasks { get; } = [];

    /// <summary>
    /// 当前选中的任务（用于预览结果）
    /// </summary>
    [ObservableProperty]
    private QueryTask? _selectedTask;

    /// <summary>
    /// 结果预览的 JSON 文本
    /// </summary>
    [ObservableProperty]
    private string _previewJson = string.Empty;

    /// <summary>
    /// 状态栏信息
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "就绪";

    public TaskListViewModel(IJsonExportService jsonExportService, IConfigService configService)
    {
        _jsonExportService = jsonExportService;
        _configService = configService;
    }

    /// <summary>
    /// 添加新任务到列表顶部
    /// </summary>
    /// <param name="task">查询任务</param>
    public void AddTask(QueryTask task)
    {
        Tasks.Insert(0, task);
        StatusMessage = $"已添加任务: {task.FileName}";
    }

    /// <summary>
    /// 选中任务变更时，自动加载结果预览
    /// </summary>
    partial void OnSelectedTaskChanged(QueryTask? value)
    {
        if (value?.Result != null)
        {
            PreviewJson = _jsonExportService.Serialize(value.Result, new JsonExportOptions { Indented = true });
        }
        else
        {
            PreviewJson = string.Empty;
        }
    }

    /// <summary>
    /// 导出选中任务的结果为 JSON 文件
    /// </summary>
    [RelayCommand]
    private async Task ExportSelectedAsync()
    {
        if (SelectedTask?.Result == null)
        {
            StatusMessage = "请选择一个已完成的任务";
            return;
        }

        var settings = await _configService.GetAppSettingsAsync();
        var options = new JsonExportOptions
        {
            Indented = settings.JsonIndented,
            UseBom = settings.JsonUseBom,
            IndentSize = settings.JsonIndentSize,
            CamelCasePropertyNames = settings.JsonCamelCase
        };

        var filePath = await _jsonExportService.ExportAsync(
            SelectedTask.Result,
            SelectedTask.FolderPath,
            SelectedTask.FileName,
            options);

        StatusMessage = $"已导出: {filePath}";
    }

    /// <summary>
    /// 批量导出所有已完成任务的结果
    /// </summary>
    [RelayCommand]
    private async Task BatchExportAsync()
    {
        var completedTasks = Tasks
            .Where(t => t.Status == QueryTaskStatus.Completed && t.Result != null)
            .ToList();

        if (completedTasks.Count == 0)
        {
            StatusMessage = "没有已完成的任务可导出";
            return;
        }

        var settings = await _configService.GetAppSettingsAsync();
        var options = new JsonExportOptions
        {
            Indented = settings.JsonIndented,
            UseBom = settings.JsonUseBom,
            IndentSize = settings.JsonIndentSize,
            CamelCasePropertyNames = settings.JsonCamelCase
        };

        var files = await _jsonExportService.BatchExportAsync(completedTasks, options);
        StatusMessage = $"批量导出完成，共 {files.Count} 个文件";
    }

    /// <summary>
    /// 取消选中的任务
    /// </summary>
    [RelayCommand]
    private void CancelSelected()
    {
        if (SelectedTask == null) return;

        if (SelectedTask.Status is QueryTaskStatus.Pending or QueryTaskStatus.Running)
        {
            SelectedTask.CancellationTokenSource.Cancel();
            StatusMessage = $"已取消任务: {SelectedTask.FileName}";
        }
    }

    /// <summary>
    /// 从列表中移除选中的任务
    /// </summary>
    [RelayCommand]
    private void RemoveSelected()
    {
        if (SelectedTask == null) return;
        Tasks.Remove(SelectedTask);
        SelectedTask = null;
    }

    /// <summary>
    /// 清空所有已完成/失败/取消的任务
    /// </summary>
    [RelayCommand]
    private void ClearCompleted()
    {
        var toRemove = Tasks
            .Where(t => t.Status is QueryTaskStatus.Completed
                or QueryTaskStatus.Failed
                or QueryTaskStatus.Cancelled)
            .ToList();

        foreach (var task in toRemove)
            Tasks.Remove(task);

        StatusMessage = $"已清除 {toRemove.Count} 个任务";
    }
}
