using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Sql2Json.App.ViewModels.Messages;
using Sql2Json.Core.Models;
using Sql2Json.Core.Services;

namespace Sql2Json.App.ViewModels;

/// <summary>
/// 文件夹配置 ViewModel
/// 管理输出文件夹的增删改
/// </summary>
public partial class FolderConfigViewModel : ObservableObject
{
    private readonly IConfigService _configService;

    /// <summary>
    /// 所有文件夹配置列表
    /// </summary>
    public ObservableCollection<FolderConfig> Configs { get; } = [];

    /// <summary>
    /// 当前选中的文件夹配置
    /// </summary>
    [ObservableProperty]
    private FolderConfig? _selectedConfig;

    /// <summary>
    /// 是否处于编辑模式
    /// </summary>
    [ObservableProperty]
    private bool _isEditing;

    // ===== 编辑表单字段 =====

    /// <summary>
    /// 编辑中的文件夹名称
    /// </summary>
    [ObservableProperty]
    private string _editName = string.Empty;

    /// <summary>
    /// 编辑中的文件夹路径
    /// </summary>
    [ObservableProperty]
    private string _editPath = string.Empty;

    /// <summary>
    /// 当前是否正在编辑已有配置（null 表示新增）
    /// </summary>
    private Guid? _editingId;

    public FolderConfigViewModel(IConfigService configService)
    {
        _configService = configService;
    }

    /// <summary>
    /// 加载所有文件夹配置
    /// </summary>
    public async Task LoadAsync()
    {
        var configs = await _configService.GetAllFolderConfigsAsync();
        Configs.Clear();
        foreach (var config in configs)
            Configs.Add(config);
    }

    /// <summary>
    /// 开始新增文件夹配置
    /// </summary>
    [RelayCommand]
    private void StartAdd()
    {
        _editingId = null;
        EditName = string.Empty;
        EditPath = string.Empty;
        IsEditing = true;
    }

    /// <summary>
    /// 开始编辑选中的文件夹配置
    /// </summary>
    [RelayCommand]
    private void StartEdit()
    {
        if (SelectedConfig == null) return;

        _editingId = SelectedConfig.Id;
        EditName = SelectedConfig.Name;
        EditPath = SelectedConfig.Path;
        IsEditing = true;
    }

    /// <summary>
    /// 保存编辑中的文件夹配置
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName) || string.IsNullOrWhiteSpace(EditPath))
            return;

        Guid savedId;

        if (_editingId == null)
        {
            // 新增
            var config = new FolderConfig
            {
                Name = EditName.Trim(),
                Path = EditPath.Trim()
            };
            await _configService.AddFolderConfigAsync(config);
            savedId = config.Id;
        }
        else
        {
            // 更新
            savedId = _editingId.Value;
            var config = new FolderConfig
            {
                Id = savedId,
                Name = EditName.Trim(),
                Path = EditPath.Trim()
            };
            await _configService.UpdateFolderConfigAsync(config);
        }

        IsEditing = false;
        await LoadAsync();
        // 自动选中刚保存的项
        SelectedConfig = Configs.FirstOrDefault(c => c.Id == savedId);
        WeakReferenceMessenger.Default.Send(new FolderConfigChangedMessage());
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
    /// 删除选中的文件夹配置
    /// </summary>
    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedConfig == null) return;

        await _configService.DeleteFolderConfigAsync(SelectedConfig.Id);
        await LoadAsync();
        WeakReferenceMessenger.Default.Send(new FolderConfigChangedMessage());
    }
}
