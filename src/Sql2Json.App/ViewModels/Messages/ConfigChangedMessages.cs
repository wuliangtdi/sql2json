using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Sql2Json.App.ViewModels.Messages;

/// <summary>
/// 数据库配置变更消息，通知其他 ViewModel 刷新数据库列表
/// </summary>
public class DatabaseConfigChangedMessage : ValueChangedMessage<bool>
{
    public DatabaseConfigChangedMessage() : base(true) { }
}

/// <summary>
/// 文件夹配置变更消息，通知其他 ViewModel 刷新文件夹列表
/// </summary>
public class FolderConfigChangedMessage : ValueChangedMessage<bool>
{
    public FolderConfigChangedMessage() : base(true) { }
}
