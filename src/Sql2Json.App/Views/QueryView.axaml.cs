using System.Xml;
using Avalonia.Controls;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using Sql2Json.App.ViewModels;

namespace Sql2Json.App.Views;

/// <summary>
/// 查询面板视图代码后置
/// 负责初始化 SQL 语法高亮编辑器，并手动同步编辑器文本与 ViewModel
/// </summary>
public partial class QueryView : UserControl
{
    public QueryView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LoadSqlHighlighting();
        BindEditorToViewModel();
    }

    /// <summary>
    /// 手动绑定编辑器文本与 ViewModel.SqlText
    /// AvaloniaEdit 的 TextEditor 不支持直接 XAML 绑定 Text 属性
    /// </summary>
    private void BindEditorToViewModel()
    {
        if (DataContext is not QueryViewModel vm) return;

        // 初始同步：ViewModel → Editor
        SqlEditor.Text = vm.SqlText;

        // Editor 文本变更 → 同步到 ViewModel
        SqlEditor.TextChanged += (_, _) =>
        {
            if (DataContext is QueryViewModel currentVm)
            {
                currentVm.SqlText = SqlEditor.Text;
            }
        };

        // ViewModel.SqlText 变更 → 同步回 Editor（如选择历史查询时）
        vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(QueryViewModel.SqlText) && DataContext is QueryViewModel v)
            {
                if (SqlEditor.Text != v.SqlText)
                {
                    SqlEditor.Text = v.SqlText;
                }
            }
        };
    }

    /// <summary>
    /// 从嵌入资源加载 SQL 语法高亮规则
    /// </summary>
    private void LoadSqlHighlighting()
    {
        try
        {
            var assembly = typeof(QueryView).Assembly;
            using var stream = assembly.GetManifestResourceStream("Sql2Json.App.Assets.SqlSyntax.xshd");
            if (stream == null) return;

            using var reader = new XmlTextReader(stream);
            var highlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            SqlEditor.SyntaxHighlighting = highlighting;
        }
        catch
        {
            // 语法高亮加载失败不影响核心功能
        }
    }
}
