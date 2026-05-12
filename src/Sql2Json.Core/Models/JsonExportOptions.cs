namespace Sql2Json.Core.Models;

/// <summary>
/// JSON 导出选项配置
/// </summary>
public class JsonExportOptions
{
    /// <summary>
    /// 是否格式化缩进（true=美化输出，false=紧凑输出）
    /// </summary>
    public bool Indented { get; set; } = true;

    /// <summary>
    /// 是否使用 UTF-8 BOM 编码（某些工具需要 BOM 才能正确识别编码）
    /// </summary>
    public bool UseBom { get; set; }

    /// <summary>
    /// 缩进字符数（仅在 Indented=true 时生效）
    /// </summary>
    public int IndentSize { get; set; } = 2;

    /// <summary>
    /// 是否将属性名转为驼峰命名
    /// </summary>
    public bool CamelCasePropertyNames { get; set; }
}
