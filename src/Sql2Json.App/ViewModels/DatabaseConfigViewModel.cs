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
/// 数据库配置 ViewModel
/// 管理数据库连接的增删改查和连接测试
/// </summary>
public partial class DatabaseConfigViewModel : ObservableObject
{
    private readonly IConfigService _configService;
    private readonly IDatabaseService _databaseService;

    /// <summary>
    /// 所有数据库配置列表
    /// </summary>
    public ObservableCollection<DatabaseConfig> Configs { get; } = [];

    /// <summary>
    /// 当前选中/编辑的配置
    /// </summary>
    [ObservableProperty]
    private DatabaseConfig? _selectedConfig;

    /// <summary>
    /// 是否处于编辑模式（新增或修改）
    /// </summary>
    [ObservableProperty]
    private bool _isEditing;

    // ===== 编辑表单字段 =====

    /// <summary>
    /// 编辑中的配置名称
    /// </summary>
    [ObservableProperty]
    private string _editName = string.Empty;

    /// <summary>
    /// 编辑中的数据库类型
    /// </summary>
    [ObservableProperty]
    private DatabaseType _editDatabaseType;

    /// <summary>
    /// 编辑中的连接字符串
    /// </summary>
    [ObservableProperty]
    private string _editConnectionString = string.Empty;

    /// <summary>
    /// 连接测试结果信息
    /// </summary>
    [ObservableProperty]
    private string? _testResultMessage;

    /// <summary>
    /// 连接测试是否成功
    /// </summary>
    [ObservableProperty]
    private bool _testResultSuccess;

    /// <summary>
    /// 可选的数据库类型列表（用于下拉框绑定）
    /// </summary>
    public DatabaseType[] AvailableDatabaseTypes { get; } = Enum.GetValues<DatabaseType>();

    /// <summary>
    /// 当前是否正在编辑已有配置（false 表示新增）
    /// </summary>
    private Guid? _editingId;

    public DatabaseConfigViewModel(IConfigService configService, IDatabaseService databaseService)
    {
        _configService = configService;
        _databaseService = databaseService;
    }

    /// <summary>
    /// 加载所有数据库配置
    /// </summary>
    public async Task LoadAsync()
    {
        var configs = await _configService.GetAllDatabaseConfigsAsync();
        Configs.Clear();
        foreach (var config in configs)
            Configs.Add(config);
    }

    /// <summary>
    /// 开始新增配置
    /// </summary>
    [RelayCommand]
    private void StartAdd()
    {
        _editingId = null;
        EditName = string.Empty;
        EditDatabaseType = DatabaseType.SqlServer;
        EditConnectionString = string.Empty;
        TestResultMessage = null;
        IsEditing = true;
    }

    /// <summary>
    /// 开始编辑选中的配置
    /// </summary>
    [RelayCommand]
    private void StartEdit()
    {
        if (SelectedConfig == null) return;

        _editingId = SelectedConfig.Id;
        EditName = SelectedConfig.Name;
        EditDatabaseType = SelectedConfig.DatabaseType;
        EditConnectionString = SelectedConfig.ConnectionString;
        TestResultMessage = null;
        IsEditing = true;
    }

    /// <summary>
    /// 保存编辑中的配置（新增或更新）
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName) || string.IsNullOrWhiteSpace(EditConnectionString))
            return;

        Guid savedId;

        if (_editingId == null)
        {
            // 新增
            var config = new DatabaseConfig
            {
                Name = EditName.Trim(),
                DatabaseType = EditDatabaseType,
                ConnectionString = EditConnectionString.Trim()
            };
            await _configService.AddDatabaseConfigAsync(config);
            savedId = config.Id;
        }
        else
        {
            // 更新
            savedId = _editingId.Value;
            var config = new DatabaseConfig
            {
                Id = savedId,
                Name = EditName.Trim(),
                DatabaseType = EditDatabaseType,
                ConnectionString = EditConnectionString.Trim()
            };
            await _configService.UpdateDatabaseConfigAsync(config);
        }

        IsEditing = false;
        await LoadAsync();
        // 自动选中刚保存的项
        SelectedConfig = Configs.FirstOrDefault(c => c.Id == savedId);
        // 通知查询面板刷新数据库列表
        WeakReferenceMessenger.Default.Send(new DatabaseConfigChangedMessage());
    }

    /// <summary>
    /// 取消编辑
    /// </summary>
    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
    }

    /// <summary>
    /// 删除选中的配置
    /// </summary>
    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedConfig == null) return;

        await _configService.DeleteDatabaseConfigAsync(SelectedConfig.Id);
        await LoadAsync();
        WeakReferenceMessenger.Default.Send(new DatabaseConfigChangedMessage());
    }

    /// <summary>
    /// 测试当前编辑中的数据库连接
    /// </summary>
    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (string.IsNullOrWhiteSpace(EditConnectionString)) return;

        var testConfig = new DatabaseConfig
        {
            DatabaseType = EditDatabaseType,
            ConnectionString = EditConnectionString.Trim()
        };

        TestResultMessage = "正在测试连接...";
        var (success, message) = await _databaseService.TestConnectionAsync(testConfig);
        TestResultSuccess = success;
        TestResultMessage = message;
    }
}
