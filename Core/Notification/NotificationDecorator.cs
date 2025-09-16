using System.Runtime.CompilerServices;
using System.Windows;
using LoveYuri.Utils;

namespace LoveYuri.Core.Notification;

/// <summary>
/// 通知装饰器，提供在WPF窗口上显示各种类型通知的扩展方法
/// </summary>
public static class NotificationDecorator
{
    private static readonly ConditionalWeakTable<Window, NotificationService> Services = new();

    /// <summary>
    /// 获取通知service，同一实例只会实现一次
    /// </summary>
    /// <param name="window">目标窗口</param>
    /// <returns>窗口对应的NotificationService实例</returns>
    private static NotificationService GetNotificationService(this Window window)
    {
        return Services.GetValue(
            window,
            win => new NotificationService(win)
        );
    }

    /// <summary>
    /// 在目标window上通知成功消息
    /// </summary>
    /// <param name="window">目标窗口</param>
    /// <param name="message">消息</param>
    /// <param name="autoClose">是否自动关闭-默认为true</param>
    /// <param name="duration">持续时间-默认2s</param>
    public static void NotifySuccess(this Window window, string message, bool autoClose = true, int duration = 2000)
    {
        window.GetNotificationService().ShowNotification(message, NotificationType.Success, autoClose, duration);
    }

    /// <summary>
    /// 在目标window上通知信息消息
    /// </summary>
    /// <param name="window">目标窗口</param>
    /// <param name="message">消息</param>
    /// <param name="autoClose">是否自动关闭-默认为true</param>
    /// <param name="duration">持续时间-默认2s</param>
    public static void NotifyInfo(this Window window, string message, bool autoClose = true, int duration = 2000)
    {
        window.GetNotificationService().ShowNotification(message, NotificationType.Info, autoClose, duration);
    }

    /// <summary>
    /// 在目标window上通知警告消息
    /// </summary>
    /// <param name="window">目标窗口</param>
    /// <param name="message">消息</param>
    /// <param name="autoClose">是否自动关闭-默认为true</param>
    /// <param name="duration">持续时间-默认3s</param>
    public static void NotifyWarning(this Window window, string message, bool autoClose = true, int duration = 3000)
    {
        window.GetNotificationService().ShowNotification(message, NotificationType.Warning, autoClose, duration);
    }

    /// <summary>
    /// 在目标window上通知错误消息
    /// </summary>
    /// <param name="window">目标窗口</param>
    /// <param name="message">消息</param>
    /// <param name="autoClose">是否自动关闭-默认为true</param>
    /// <param name="duration">持续时间-默认3s</param>
    public static void NotifyError(this Window window, string message, bool autoClose = true, int duration = 3000)
    {
        window.GetNotificationService().ShowNotification(message, NotificationType.Error, autoClose, duration);
    }

    /// <summary>
    /// 在目标window上通知成功消息-搜索第一个可见窗口为媒体
    /// </summary>
    /// <param name="message">消息</param>
    /// <param name="autoClose">是否自动关闭-默认为true</param>
    /// <param name="duration">持续时间-默认2s</param>
    public static void NotifySuccess(string message, bool autoClose = true, int duration = 2000)
    {
        WindowUtils.SafeMainWindow?.GetNotificationService().ShowNotification(message, NotificationType.Success, autoClose, duration);
    }

    /// <summary>
    /// 在主窗口上通知信息消息-搜索第一个可见窗口为载体
    /// </summary>
    /// <param name="message">消息</param>
    /// <param name="autoClose">是否自动关闭-默认为true</param>
    /// <param name="duration">持续时间-默认2s</param>
    public static void NotifyInfo(string message, bool autoClose = true, int duration = 2000)
    {
        WindowUtils.SafeMainWindow?.GetNotificationService().ShowNotification(message, NotificationType.Info, autoClose, duration);
    }

    /// <summary>
    /// 在主窗口上通知警告消息-搜索第一个可见窗口为载体
    /// </summary>
    /// <param name="message">消息</param>
    /// <param name="autoClose">是否自动关闭-默认为true</param>
    /// <param name="duration">持续时间-默认3s</param>
    public static void NotifyWarning(string message, bool autoClose = true, int duration = 3000)
    {
        WindowUtils.SafeMainWindow?.GetNotificationService().ShowNotification(message, NotificationType.Warning, autoClose, duration);
    }

    /// <summary>
    /// 在主窗口上通知错误消息-搜索第一个可见窗口为载体
    /// </summary>
    /// <param name="message">消息</param>
    /// <param name="autoClose">是否自动关闭-默认为true</param>
    /// <param name="duration">持续时间-默认3s</param>
    public static void NotifyError(string message, bool autoClose = true, int duration = 3000)
    {
        WindowUtils.SafeMainWindow?.GetNotificationService().ShowNotification(message, NotificationType.Error, autoClose, duration);
    }
}
