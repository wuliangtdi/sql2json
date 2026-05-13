using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Sql2Json.App.ViewModels.Messages;
using Sql2Json.Core.Enums;
using Sql2Json.Core.Models;
using Sql2Json.Core.Services;

namespace Sql2Json.App.ViewModels;

/// <summary>
/// 查询面板 ViewModel
/// 管理 SQL 输入、参数配置、数据库/文件夹选择、查询历史等
/// 订阅配置变更消息，自动刷新下拉列表
/// </summary>
public partial class QueryViewModel : ObservableObject,
    IRecipient<DatabaseConfigChangedMessage>,
    IRecipient<FolderConfigChangedMessage>
{
    private readonly IConfigService _configService;
    private readonly ITaskExecutorService _taskExecutorService;
    private readonly IQueryHistoryService _queryHistoryService;
    private readonly TaskListViewModel _taskListViewModel;

    // ===== 数据库和文件夹选择 =====

    /// <summary>
    /// 可用的数据库配置列表
    /// </summary>
    public ObservableCollection<DatabaseConfig> DatabaseConfigs { get; } = [];

    /// <summary>
    /// 当前选中的数据库配置
    /// </summary>
    [ObservableProperty]
    private DatabaseConfig? _selectedDatabase;

    /// <summary>
    /// 可用的输出文件夹列表
    /// </summary>
    public ObservableCollection<FolderConfig> FolderConfigs { get; } = [];

    /// <summary>
    /// 当前选中的输出文件夹
    /// </summary>
    [ObservableProperty]
    private FolderConfig? _selectedFolder;

    // ===== SQL 输入 =====

    /// <summary>
    /// 用户输入的 SQL 语句
    /// </summary>
    [ObservableProperty]
    private string _sqlText = string.Empty;

    /// <summary>
    /// 输出文件名（不含扩展名）
    /// </summary>
    [ObservableProperty]
    private string _fileName = string.Empty;

    // ===== 导出格式 =====

    /// <summary>
    /// 当前选择的导出格式
    /// </summary>
    [ObservableProperty]
    private ExportFormat _selectedExportFormat = ExportFormat.Json;

    /// <summary>
    /// 可选的导出格式列表
    /// </summary>
    public ExportFormat[] AvailableExportFormats { get; } = Enum.GetValues<ExportFormat>();

    // ===== 参数化查询 =====

    /// <summary>
    /// 查询参数列表
    /// </summary>
    public ObservableCollection<QueryParameter> Parameters { get; } = [];

    // ===== 查询历史 =====

    /// <summary>
    /// 查询历史列表
    /// </summary>
    public ObservableCollection<QueryHistory> QueryHistories { get; } = [];

    /// <summary>
    /// 当前选中的历史记录
    /// </summary>
    [ObservableProperty]
    private QueryHistory? _selectedHistory;

    // ===== 验证状态 =====

    /// <summary>
    /// 验证错误信息（为空表示验证通过）
    /// </summary>
    [ObservableProperty]
    private string? _validationError;

    public QueryViewModel(
        IConfigService configService,
        ITaskExecutorService taskExecutorService,
        IQueryHistoryService queryHistoryService,
        TaskListViewModel taskListViewModel)
    {
        _configService = configService;
        _taskExecutorService = taskExecutorService;
        _queryHistoryService = queryHistoryService;
        _taskListViewModel = taskListViewModel;

        // 订阅配置变更消息，自动刷新下拉列表
        WeakReferenceMessenger.Default.Register<DatabaseConfigChangedMessage>(this);
        WeakReferenceMessenger.Default.Register<FolderConfigChangedMessage>(this);
    }

    /// <summary>
    /// 收到数据库配置变更消息时，刷新数据库下拉列表
    /// </summary>
    public async void Receive(DatabaseConfigChangedMessage message)
    {
        await RefreshDatabasesAsync();
    }

    /// <summary>
    /// 收到文件夹配置变更消息时，刷新文件夹下拉列表
    /// </summary>
    public async void Receive(FolderConfigChangedMessage message)
    {
        await RefreshFoldersAsync();
    }

    /// <summary>
    /// 加载初始数据（数据库列表、文件夹列表、查询历史）
    /// </summary>
    public async Task LoadInitialDataAsync()
    {
        var databases = await _configService.GetAllDatabaseConfigsAsync();
        DatabaseConfigs.Clear();
        foreach (var db in databases)
            DatabaseConfigs.Add(db);

        var folders = await _configService.GetAllFolderConfigsAsync();
        FolderConfigs.Clear();
        foreach (var folder in folders)
            FolderConfigs.Add(folder);

        var histories = await _queryHistoryService.GetAllAsync();
        QueryHistories.Clear();
        foreach (var h in histories)
            QueryHistories.Add(h);
    }

    /// <summary>
    /// 刷新数据库配置列表（配置变更后调用），保留当前选中项
    /// </summary>
    [RelayCommand]
    private async Task RefreshDatabasesAsync()
    {
        var previousId = SelectedDatabase?.Id;
        var databases = await _configService.GetAllDatabaseConfigsAsync();
        DatabaseConfigs.Clear();
        foreach (var db in databases)
            DatabaseConfigs.Add(db);
        // 恢复之前的选中项
        if (previousId != null)
            SelectedDatabase = DatabaseConfigs.FirstOrDefault(d => d.Id == previousId);
    }

    /// <summary>
    /// 刷新文件夹配置列表（配置变更后调用），保留当前选中项
    /// </summary>
    [RelayCommand]
    private async Task RefreshFoldersAsync()
    {
        var previousId = SelectedFolder?.Id;
        var folders = await _configService.GetAllFolderConfigsAsync();
        FolderConfigs.Clear();
        foreach (var folder in folders)
            FolderConfigs.Add(folder);
        // 恢复之前的选中项
        if (previousId != null)
            SelectedFolder = FolderConfigs.FirstOrDefault(f => f.Id == previousId);
    }

    /// <summary>
    /// 添加查询参数
    /// </summary>
    [RelayCommand]
    private void AddParameter()
    {
        Parameters.Add(new QueryParameter());
    }

    /// <summary>
    /// 移除指定的查询参数
    /// </summary>
    [RelayCommand]
    private void RemoveParameter(QueryParameter parameter)
    {
        Parameters.Remove(parameter);
    }

    // ===== 属性变更时清除验证错误 =====

    partial void OnSelectedDatabaseChanged(DatabaseConfig? value) => ValidationError = null;
    partial void OnSelectedFolderChanged(FolderConfig? value) => ValidationError = null;
    partial void OnSqlTextChanged(string value) => ValidationError = null;
    partial void OnFileNameChanged(string value) => ValidationError = null;

    /// <summary>
    /// 选择历史记录时，自动填充 SQL 和参数
    /// </summary>
    partial void OnSelectedHistoryChanged(QueryHistory? value)
    {
        if (value == null) return;

        SqlText = value.Sql;

        // 反序列化历史参数
        Parameters.Clear();
        if (!string.IsNullOrEmpty(value.ParametersJson) && value.ParametersJson != "[]")
        {
            var parameters = System.Text.Json.JsonSerializer.Deserialize<List<QueryParameter>>(value.ParametersJson);
            if (parameters != null)
            {
                foreach (var p in parameters)
                    Parameters.Add(p);
            }
        }
    }

    /// <summary>
    /// 执行查询并加入任务队列
    /// 验证必填项后创建 QueryTask 提交给执行器
    /// </summary>
    [RelayCommand]
    private async Task ExecuteQueryAsync()
    {
        // 输入验证
        if (!Validate()) return;

        // 创建查询任务
        var task = new QueryTask
        {
            FileName = FileName.Trim(),
            Sql = SqlText.Trim(),
            Parameters = Parameters.ToList(),
            DatabaseConfigId = SelectedDatabase!.Id,
            DatabaseName = SelectedDatabase.Name,
            FolderConfigId = SelectedFolder!.Id,
            FolderPath = SelectedFolder.Path,
            ExportFormat = SelectedExportFormat
        };

        // 加入任务列表并提交执行
        _taskListViewModel.AddTask(task);
        await _taskExecutorService.SubmitAsync(task);

        // 清空文件名，避免忘记修改导致覆盖
        FileName = string.Empty;
    }

    /// <summary>
    /// 保存当前查询到历史记录
    /// </summary>
    [RelayCommand]
    private async Task SaveToHistoryAsync()
    {
        if (string.IsNullOrWhiteSpace(SqlText))
        {
            ValidationError = "SQL 语句不能为空";
            return;
        }

        var history = new QueryHistory
        {
            Name = string.IsNullOrWhiteSpace(FileName) ? $"查询_{DateTime.Now:yyyyMMdd_HHmmss}" : FileName,
            Sql = SqlText.Trim(),
            DatabaseConfigId = SelectedDatabase?.Id,
            ParametersJson = System.Text.Json.JsonSerializer.Serialize(Parameters.ToList())
        };

        await _queryHistoryService.SaveAsync(history);
        QueryHistories.Insert(0, history);
    }

    /// <summary>
    /// 验证所有必填项是否已填写
    /// </summary>
    /// <returns>验证通过返回 true</returns>
    private bool Validate()
    {
        if (SelectedDatabase == null)
        {
            ValidationError = "请选择数据库";
            return false;
        }

        if (SelectedFolder == null)
        {
            ValidationError = "请选择输出文件夹";
            return false;
        }

        if (string.IsNullOrWhiteSpace(FileName))
        {
            ValidationError = "请输入文件名";
            return false;
        }

        if (string.IsNullOrWhiteSpace(SqlText))
        {
            ValidationError = "请输入 SQL 语句";
            return false;
        }

        ValidationError = null;
        return true;
    }
}
