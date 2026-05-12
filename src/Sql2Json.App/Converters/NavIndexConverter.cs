using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Sql2Json.App.Converters;

/// <summary>
/// 将导航索引与目标索引比较，相等返回 true（用于控制页面可见性）
/// ConverterParameter 传入目标索引值
/// </summary>
public class NavIndexConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int currentIndex && parameter is string targetStr && int.TryParse(targetStr, out var targetIndex))
        {
            return currentIndex == targetIndex;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
