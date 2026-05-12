namespace Sql2Json.Core.Models;

/// <summary>
/// 查询结果模型，保存单次 SQL 执行返回的所有结果集数据
/// </summary>
public class QueryResult
{
    /// <summary>
    /// 结果集列表，一条 SQL 可能返回多个结果集（多个 SELECT）
    /// </summary>
    public List<ResultSet> ResultSets { get; set; } = [];

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public long ElapsedMilliseconds { get; set; }

    /// <summary>
    /// 总行数（所有结果集的行数之和）
    /// </summary>
    public int TotalRows => ResultSets.Sum(r => r.Rows.Count);
}

/// <summary>
/// 单个结果集，对应一个 SELECT 语句的返回数据
/// </summary>
public class ResultSet
{
    /// <summary>
    /// 结果集索引（从 0 开始）
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 列名列表
    /// </summary>
    public List<string> Columns { get; set; } = [];

    /// <summary>
    /// 行数据，每行是一个字典（列名 -> 值）
    /// </summary>
    public List<Dictionary<string, object?>> Rows { get; set; } = [];
}
