using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sql2Json.Core.Models;

namespace Sql2Json.Core.Services;

/// <summary>
/// JSON 导出服务实现
/// 负责将查询结果序列化为 JSON 并写入文件，支持格式化选项和批量导出
/// </summary>
public class JsonExportService : IJsonExportService
{
    /// <inheritdoc/>
    public async Task<string> ExportAsync(
        QueryResult result,
        string folderPath,
        string fileName,
        JsonExportOptions? options = null)
    {
        options ??= new JsonExportOptions();

        // 确保输出目录存在
        Directory.CreateDirectory(folderPath);

        // 安全校验：只允许纯文件名，拒绝路径分隔符和非法字符
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitizedFileName = fileName.Trim();
        if (sanitizedFileName.Any(c => invalidChars.Contains(c)) ||
            sanitizedFileName.Contains("..") ||
            Path.IsPathRooted(sanitizedFileName))
        {
            throw new ArgumentException($"文件名包含非法字符: {sanitizedFileName}");
        }

        // 拼接完整文件路径（自动补 .json 扩展名）
        var fullFileName = sanitizedFileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            ? sanitizedFileName
            : $"{sanitizedFileName}.json";
        var filePath = Path.Combine(folderPath, fullFileName);

        // 二次校验：确保最终路径在目标文件夹内
        var resolvedPath = Path.GetFullPath(filePath);
        var resolvedFolder = Path.GetFullPath(folderPath);
        if (!resolvedPath.StartsWith(resolvedFolder, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"文件路径逃逸了目标文件夹: {resolvedPath}");
        }

        // 序列化为 JSON 字符串
        var json = Serialize(result, options);

        // 根据编码选项写入文件
        var encoding = options.UseBom
            ? new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)
            : new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        await File.WriteAllTextAsync(filePath, json, encoding);

        return filePath;
    }

    /// <inheritdoc/>
    public async Task<List<string>> BatchExportAsync(
        List<QueryTask> tasks,
        JsonExportOptions? options = null)
    {
        var exportedFiles = new List<string>();

        foreach (var task in tasks)
        {
            // 只导出已完成且有结果的任务
            if (task.Result == null) continue;

            var filePath = await ExportAsync(
                task.Result,
                task.FolderPath,
                task.FileName,
                options);

            exportedFiles.Add(filePath);
        }

        return exportedFiles;
    }

    /// <inheritdoc/>
    public string Serialize(QueryResult result, JsonExportOptions? options = null)
    {
        options ??= new JsonExportOptions();

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = options.Indented,
            // 自定义缩进大小
            IndentSize = options.IndentSize,
            // 不转义中文字符
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            // 处理循环引用
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            // 属性命名策略
            PropertyNamingPolicy = options.CamelCasePropertyNames
                ? JsonNamingPolicy.CamelCase
                : null
        };

        // 单结果集：直接输出为数组
        // 多结果集：输出为对象，key 为 result_0, result_1, ...
        if (result.ResultSets.Count == 1)
        {
            return JsonSerializer.Serialize(result.ResultSets[0].Rows, jsonOptions);
        }

        // 多结果集，构建字典
        var multiResult = new Dictionary<string, List<Dictionary<string, object?>>>();
        foreach (var resultSet in result.ResultSets)
        {
            multiResult[$"result_{resultSet.Index}"] = resultSet.Rows;
        }

        return JsonSerializer.Serialize(multiResult, jsonOptions);
    }

    /// <inheritdoc/>
    public string SerializePreview(QueryResult result, int maxRows, JsonExportOptions? options = null)
    {
        // 构建限制行数的预览结果
        var previewResult = new QueryResult
        {
            ElapsedMilliseconds = result.ElapsedMilliseconds,
            ResultSets = result.ResultSets.Select(rs => new ResultSet
            {
                Index = rs.Index,
                Columns = rs.Columns,
                Rows = rs.Rows.Take(maxRows).ToList()
            }).ToList()
        };

        // 返回纯 JSON，不附加非 JSON 内容
        return Serialize(previewResult, options);
    }
}
