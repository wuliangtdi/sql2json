using System;
using Avalonia.Controls;
using Sql2Json.App.ViewModels;

namespace Sql2Json.App.Views;

/// <summary>
/// 主窗口代码后置，负责窗口生命周期事件
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // 窗口加载完成后触发数据加载
        Loaded += async (_, _) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                try
                {
                    await vm.LoadDataCommand.ExecuteAsync(null);
                }
                catch (Exception ex)
                {
                    // 首次启动数据库创建失败时不崩溃，显示错误信息
                    Console.Error.WriteLine($"数据加载失败: {ex.Message}");
                }
            }
        };
    }
}
