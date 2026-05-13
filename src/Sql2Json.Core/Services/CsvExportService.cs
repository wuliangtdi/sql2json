using System.Text;
using Sql2Json.Core.Models;

namespace Sql2Json.Core.Services;

/// <summary>
/// CSV 导出服务，将查询结果导出为 CSV 文件
/// </summary>
public class CsvExportService
{
    /// <summary>
    /// 将查询结果导出为 CSV 文件
    /// </summary>
    /// <param name="result">查询结果</param>
    /// <param name="folderPath">输出文件夹</param>
    /// <param name="fileName">文件名（不含扩展名）</param>
    /// <returns>导出的文件路径列表（多结果集会生成多个文件）</returns>
    public List<string> Export(QueryResult result, string folderPath, string fileName)
    {
        Directory.CreateDirectory(folderPath);
        var files = new List<string>();

        foreach (var resultSet in result.ResultSets)
        {
            // 多结果集时文件名加后缀
            var actualName = result.ResultSets.Count == 1
                ? fileName
                : $"{fileName}_result_{resultSet.Index}";

            var fullPath = Path.Combine(folderPath, $"{actualName}.csv");
            var sb = new StringBuilder();

            // 写入列头
            sb.AppendLine(string.Join(",", resultSet.Columns.Select(EscapeCsvField)));

            // 写入数据行
            foreach (var row in resultSet.Rows)
            {
                var values = resultSet.Columns.Select(col =>
                {
                    row.TryGetValue(col, out var value);
                    return EscapeCsvField(value?.ToString() ?? "");
                });
                sb.AppendLine(string.Join(",", values));
            }

            File.WriteAllText(fullPath, sb.ToString(), new UTF8Encoding(true));
            files.Add(fullPath);
        }

        return files;
    }

    /// <summary>
    /// CSV 字段转义：包含逗号、换行、双引号时用双引号包裹
    /// </summary>
    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
