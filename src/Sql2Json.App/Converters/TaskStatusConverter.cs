using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Sql2Json.Core.Enums;

namespace Sql2Json.App.Converters;

/// <summary>
/// 将 QueryTaskStatus 枚举转换为对应的状态图标字符
/// </summary>
public class TaskStatusConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is QueryTaskStatus status)
        {
            return status switch
            {
                QueryTaskStatus.Pending => "○",    // 等待中
                QueryTaskStatus.Running => "⟳",    // 执行中
                QueryTaskStatus.Completed => "✓",  // 已完成
                QueryTaskStatus.Failed => "✕",     // 失败
                QueryTaskStatus.Cancelled => "⊘",  // 已取消
                _ => "?"
            };
        }
        return "?";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// 将 QueryTaskStatus 枚举转换为对应的颜色（用于状态文本着色）
/// </summary>
public class TaskStatusColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is QueryTaskStatus status)
        {
            return status switch
            {
                QueryTaskStatus.Pending => Avalonia.Media.Brushes.Gray,
                QueryTaskStatus.Running => Avalonia.Media.Brushes.DodgerBlue,
                QueryTaskStatus.Completed => Avalonia.Media.Brushes.Green,
                QueryTaskStatus.Failed => Avalonia.Media.Brushes.Red,
                QueryTaskStatus.Cancelled => Avalonia.Media.Brushes.Orange,
                _ => Avalonia.Media.Brushes.Gray
            };
        }
        return Avalonia.Media.Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
