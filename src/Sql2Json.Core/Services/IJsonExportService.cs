using Sql2Json.Core.Models;

namespace Sql2Json.Core.Services;

/// <summary>
/// JSON 导出服务接口，负责将查询结果序列化为 JSON 文件
/// </summary>
public interface IJsonExportService
{
    /// <summary>
    /// 将查询结果导出为 JSON 文件
    /// </summary>
    /// <param name="result">查询结果</param>
    /// <param name="folderPath">输出文件夹路径</param>
    /// <param name="fileName">文件名（不含扩展名）</param>
    /// <param name="options">JSON 格式化选项</param>
    /// <returns>导出的文件完整路径</returns>
    Task<string> ExportAsync(
        QueryResult result,
        string folderPath,
        string fileName,
        JsonExportOptions? options = null);

    /// <summary>
    /// 批量导出多个查询任务的结果
    /// </summary>
    /// <param name="tasks">已完成的查询任务列表</param>
    /// <param name="options">JSON 格式化选项</param>
    /// <returns>成功导出的文件路径列表</returns>
    Task<List<string>> BatchExportAsync(
        List<QueryTask> tasks,
        JsonExportOptions? options = null);

    /// <summary>
    /// 将查询结果序列化为 JSON 字符串（用于预览）
    /// </summary>
    /// <param name="result">查询结果</param>
    /// <param name="options">JSON 格式化选项</param>
    /// <returns>JSON 字符串</returns>
    string Serialize(QueryResult result, JsonExportOptions? options = null);

    /// <summary>
    /// 将查询结果序列化为 JSON 字符串（限制行数，用于预览）
    /// </summary>
    /// <param name="result">查询结果</param>
    /// <param name="maxRows">最大预览行数</param>
    /// <param name="options">JSON 格式化选项</param>
    /// <returns>JSON 字符串</returns>
    string SerializePreview(QueryResult result, int maxRows, JsonExportOptions? options = null);
}
