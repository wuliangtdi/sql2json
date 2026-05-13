using Sql2Json.Core.Enums;

namespace Sql2Json.Core.Helpers;

/// <summary>
/// 各数据库类型的连接字符串模板，选择类型后自动填充供用户修改
/// </summary>
public static class ConnectionStringTemplates
{
    /// <summary>
    /// 根据数据库类型获取对应的连接字符串模板
    /// </summary>
    /// <param name="type">数据库类型</param>
    /// <returns>带占位符的连接字符串模板</returns>
    public static string GetTemplate(DatabaseType type)
    {
        return type switch
        {
            DatabaseType.SqlServer => "Server=localhost;Database=数据库名;User Id=用户名;Password=密码;TrustServerCertificate=True;",
            DatabaseType.MySql => "Server=localhost;Port=3306;Database=数据库名;User=用户名;Password=密码;",
            DatabaseType.MariaDb => "Server=localhost;Port=3306;Database=数据库名;User=用户名;Password=密码;",
            DatabaseType.PostgreSql => "Host=localhost;Port=5432;Database=数据库名;Username=用户名;Password=密码;",
            DatabaseType.Oracle => "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=服务名)));User Id=用户名;Password=密码;",
            DatabaseType.Sqlite => "Data Source=数据库文件路径.db;",
            _ => string.Empty
        };
    }
}
