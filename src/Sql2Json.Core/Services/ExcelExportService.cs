using ClosedXML.Excel;
using Sql2Json.Core.Models;

namespace Sql2Json.Core.Services;

/// <summary>
/// Excel 导出服务，将查询结果导出为 .xlsx 文件
/// 每个结果集对应一个 Sheet
/// </summary>
public class ExcelExportService
{
    /// <summary>
    /// 将查询结果导出为 Excel 文件
    /// </summary>
    /// <param name="result">查询结果</param>
    /// <param name="folderPath">输出文件夹</param>
    /// <param name="fileName">文件名（不含扩展名）</param>
    /// <returns>导出的文件路径</returns>
    public string Export(QueryResult result, string folderPath, string fileName)
    {
        Directory.CreateDirectory(folderPath);
        var fullPath = Path.Combine(folderPath, $"{fileName}.xlsx");

        using var workbook = new XLWorkbook();

        foreach (var resultSet in result.ResultSets)
        {
            // 每个结果集一个 Sheet
            var sheetName = result.ResultSets.Count == 1
                ? "Sheet1"
                : $"Result_{resultSet.Index}";

            var worksheet = workbook.Worksheets.Add(sheetName);

            // 写入列头（第 1 行）
            for (var col = 0; col < resultSet.Columns.Count; col++)
            {
                worksheet.Cell(1, col + 1).Value = resultSet.Columns[col];
                worksheet.Cell(1, col + 1).Style.Font.Bold = true;
            }

            // 写入数据行（从第 2 行开始）
            for (var row = 0; row < resultSet.Rows.Count; row++)
            {
                for (var col = 0; col < resultSet.Columns.Count; col++)
                {
                    var columnName = resultSet.Columns[col];
                    resultSet.Rows[row].TryGetValue(columnName, out var value);
                    worksheet.Cell(row + 2, col + 1).Value = ConvertToXLValue(value);
                }
            }

            // 自动调整列宽
            worksheet.Columns().AdjustToContents(1, 50);
        }

        workbook.SaveAs(fullPath);
        return fullPath;
    }

    /// <summary>
    /// 将 .NET 对象转换为 ClosedXML 支持的单元格值
    /// </summary>
    private static XLCellValue ConvertToXLValue(object? value)
    {
        return value switch
        {
            null => Blank.Value,
            int i => i,
            long l => l,
            decimal d => d,
            double dbl => dbl,
            float f => f,
            DateTime dt => dt,
            bool b => b,
            _ => value.ToString() ?? ""
        };
    }
}
