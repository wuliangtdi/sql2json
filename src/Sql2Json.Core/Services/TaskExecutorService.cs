using Sql2Json.Core.Enums;
using Sql2Json.Core.Models;

namespace Sql2Json.Core.Services;

/// <summary>
/// 任务执行器服务实现
/// 使用 SemaphoreSlim 控制并发数，每个任务在独立线程中执行
/// 支持动态调整并发数和单任务取消
/// </summary>
public class TaskExecutorService : ITaskExecutorService
{
    private readonly IDatabaseService _databaseService;
    private readonly IConfigService _configService;
    private readonly IJsonExportService _jsonExportService;
    private volatile SemaphoreSlim _semaphore;
    private int _maxConcurrency;

    public TaskExecutorService(IDatabaseService databaseService, IConfigService configService, IJsonExportService jsonExportService)
    {
        _databaseService = databaseService;
        _configService = configService;
        _jsonExportService = jsonExportService;
        _maxConcurrency = 5;
        _semaphore = new SemaphoreSlim(_maxConcurrency);
    }

    /// <inheritdoc/>
    public Task SubmitAsync(QueryTask task)
    {
        // 用 Task.Run 确保每个任务在线程池独立执行，不受 UI 线程同步上下文限制
        _ = Task.Run(() => ExecuteTaskAsync(task));
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Cancel(Guid taskId)
    {
    }

    /// <inheritdoc/>
    public void CancelAll()
    {
    }

    /// <inheritdoc/>
    public void UpdateMaxConcurrency(int maxConcurrency)
    {
        if (maxConcurrency <= 0) return;
        _maxConcurrency = maxConcurrency;
        // 新建信号量供后续任务使用，已在执行的任务持有旧信号量的局部引用不受影响
        _semaphore = new SemaphoreSlim(maxConcurrency);
    }

    /// <summary>
    /// 在后台执行单个查询任务
    /// 捕获局部 semaphore 引用，确保 wait 和 release 操作同一个实例
    /// </summary>
    private async Task ExecuteTaskAsync(QueryTask task)
    {
        // 捕获当前信号量引用，确保 wait/release 操作同一个实例
        var semaphore = _semaphore;

        try
        {
            task.Status = QueryTaskStatus.Pending;
            task.ProgressMessage = "等待执行槽位...";

            await semaphore.WaitAsync(task.CancellationTokenSource.Token);

            try
            {
                task.Status = QueryTaskStatus.Running;
                task.ProgressMessage = "正在连接数据库...";

                var dbConfig = await _configService.GetDatabaseConfigAsync(task.DatabaseConfigId);
                if (dbConfig == null)
                {
                    task.Status = QueryTaskStatus.Failed;
                    task.ErrorMessage = "数据库配置不存在或已被删除";
                    return;
                }

                task.ProgressMessage = "正在执行查询...";

                var result = await _databaseService.ExecuteQueryAsync(
                    dbConfig,
                    task.Sql,
                    task.Parameters,
                    task.CancellationTokenSource.Token);

                task.Result = result;
                task.ElapsedMilliseconds = result.ElapsedMilliseconds;

                // 自动导出 JSON 文件到目标文件夹
                task.ProgressMessage = "正在导出 JSON...";
                var settings = await _configService.GetAppSettingsAsync();
                var exportOptions = new JsonExportOptions
                {
                    Indented = settings.JsonIndented,
                    UseBom = settings.JsonUseBom,
                    IndentSize = settings.JsonIndentSize,
                    CamelCasePropertyNames = settings.JsonCamelCase
                };
                await _jsonExportService.ExportAsync(result, task.FolderPath, task.FileName, exportOptions);

                task.Status = QueryTaskStatus.Completed;
                task.ProgressMessage = $"完成，共 {result.TotalRows} 行，耗时 {result.ElapsedMilliseconds}ms，已导出";
            }
            finally
            {
                // 释放的是同一个 semaphore 实例
                semaphore.Release();
            }
        }
        catch (OperationCanceledException)
        {
            task.Status = QueryTaskStatus.Cancelled;
            task.ProgressMessage = "已取消";
        }
        catch (Exception ex)
        {
            task.Status = QueryTaskStatus.Failed;
            task.ErrorMessage = ex.Message;
            task.ProgressMessage = "执行失败";
        }
    }
}
