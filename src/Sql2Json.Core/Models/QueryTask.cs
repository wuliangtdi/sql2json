using CommunityToolkit.Mvvm.ComponentModel;
using Sql2Json.Core.Enums;

namespace Sql2Json.Core.Models;

/// <summary>
/// 查询任务运行时模型（不持久化），表示一次 SQL 执行任务
/// 继承 ObservableObject 以支持 UI 绑定状态变更
/// </summary>
public partial class QueryTask : ObservableObject
{
    /// <summary>
    /// 任务唯一标识
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 输出文件名（不含扩展名）
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 要执行的 SQL 语句
    /// </summary>
    public string Sql { get; set; } = string.Empty;

    /// <summary>
    /// 查询参数列表
    /// </summary>
    public List<QueryParameter> Parameters { get; set; } = [];

    /// <summary>
    /// 目标数据库配置 ID
    /// </summary>
    public Guid DatabaseConfigId { get; set; }

    /// <summary>
    /// 目标数据库配置名称（用于 UI 显示）
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// 输出文件夹配置 ID
    /// </summary>
    public Guid FolderConfigId { get; set; }

    /// <summary>
    /// 输出文件夹路径（用于 UI 显示）
    /// </summary>
    public string FolderPath { get; set; } = string.Empty;

    /// <summary>
    /// 导出格式（Json / Csv / Excel）
    /// </summary>
    public ExportFormat ExportFormat { get; set; } = ExportFormat.Json;

    /// <summary>
    /// 任务创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 任务执行状态（可观察属性，变更时自动通知 UI）
    /// </summary>
    [ObservableProperty]
    private QueryTaskStatus _status = QueryTaskStatus.Pending;

    /// <summary>
    /// 执行进度信息（如"正在连接..."、"已获取 1000 行"）
    /// </summary>
    [ObservableProperty]
    private string _progressMessage = "等待执行";

    /// <summary>
    /// 错误信息（仅在 Failed 状态时有值）
    /// </summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// 查询结果（执行完成后填充）
    /// </summary>
    [ObservableProperty]
    private QueryResult? _result;

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    [ObservableProperty]
    private long _elapsedMilliseconds;

    /// <summary>
    /// 取消令牌源，用于支持取消正在执行的任务
    /// </summary>
    public CancellationTokenSource CancellationTokenSource { get; set; } = new();
}
