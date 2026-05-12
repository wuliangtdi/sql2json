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
    private SemaphoreSlim _semaphore;
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
        // 取消操作通过 QueryTask 自身的 CancellationTokenSource 实现
        // 由 ViewModel 层持有任务引用并调用
    }

    /// <inheritdoc/>
    public void CancelAll()
    {
        // 由 ViewModel 层遍历所有任务并逐个取消
    }

    /// <inheritdoc/>
    public void UpdateMaxConcurrency(int maxConcurrency)
    {
        if (maxConcurrency <= 0) return;
        _maxConcurrency = maxConcurrency;
        // 重建信号量（已在执行的任务不受影响，新任务使用新的并发限制）
        _semaphore = new SemaphoreSlim(maxConcurrency);
    }

    /// <summary>
    /// 在后台执行单个查询任务
    /// 通过 SemaphoreSlim 控制并发数，确保不会同时打开过多数据库连接
    /// </summary>
    /// <param name="task">要执行的查询任务</param>
    private async Task ExecuteTaskAsync(QueryTask task)
    {
        try
        {
            // 等待信号量（如果已达到最大并发数，则排队等待）
            task.Status = QueryTaskStatus.Pending;
            task.ProgressMessage = "等待执行槽位...";

            await _semaphore.WaitAsync(task.CancellationTokenSource.Token);

            try
            {
                // 获取信号量成功，开始执行
                task.Status = QueryTaskStatus.Running;
                task.ProgressMessage = "正在连接数据库...";

                // 获取数据库配置
                var dbConfig = await _configService.GetDatabaseConfigAsync(task.DatabaseConfigId);
                if (dbConfig == null)
                {
                    task.Status = QueryTaskStatus.Failed;
                    task.ErrorMessage = "数据库配置不存在或已被删除";
                    return;
                }

                task.ProgressMessage = "正在执行查询...";

                // 执行 SQL 查询
                var result = await _databaseService.ExecuteQueryAsync(
                    dbConfig,
                    task.Sql,
                    task.Parameters,
                    task.CancellationTokenSource.Token);

                // 执行成功，更新任务状态
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
                // 无论成功失败，都要释放信号量
                _semaphore.Release();
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
