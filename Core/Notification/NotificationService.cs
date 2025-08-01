using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;
using System.Windows.Threading;
using LoveYuri.Controls;

namespace LoveYuri.Core.Notification;

/// <summary>
/// 通知服务
/// </summary>
public class NotificationService {
    private const int DefaultDuration = 3000;
    private const int ReCloseStartDuration = 800;
    private Panel? _container;
    private readonly Window _window;
    private readonly List<NotificationMessage> _activeNotifications = [];
    private bool _isMouseOverAnyNotification;

    /// <summary>
    /// 创建通知服务实例
    /// </summary>
    /// <param name="window">关联的窗口</param>
    public NotificationService(Window window) {
        _window = window;
        window.Dispatcher.InvokeAsync(Init);
    }

    private void Init()
    {
        // 基础顶部偏移量
        var marginTop = 2.0;
        if (_window is ModernWindow) {
            marginTop = 2;
        } else {
            var windowChrome = WindowChrome.GetWindowChrome(_window);
            if (windowChrome != null) {
                marginTop += windowChrome.CaptionHeight;
            }
        }

        // 创建通知容器 - 居中显示
        _container = new StackPanel {
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, marginTop, 0, 0)
        };

        // 添加到窗口
        Panel.SetZIndex(_container, 9999);

        object originalContent = _window.Content;
        Grid grid;
        if (_window is ModernWindow w) {
            grid = w.NotificationGrid;
            Grid.SetRow(_container, 2);
        } else {
            grid = new Grid();
            _window.Content = grid;
            if (originalContent is UIElement element) {
                grid.Children.Add(element);
            }
        }

        grid.Children.Add(_container);
    }

    /// <summary>
    /// 暂停所有通知的自动关闭计时器
    /// </summary>
    private void PauseAllTimers()
    {
        foreach (var notification in _activeNotifications) {
            notification.CloseTimer?.Stop();
        }
    }

    /// <summary>
    /// 恢复所有通知的自动关闭计时器
    /// </summary>
    private void ResumeAllTimers()
    {
        int basicTimeout = ReCloseStartDuration;
        foreach (var notification in _activeNotifications) {
            if (notification.CloseTimer != null) {
                notification.CloseTimer.Interval = TimeSpan.FromMilliseconds(basicTimeout += 100);
            }
            notification.CloseTimer?.Start();
        }
    }

    /// <summary>
    /// 展示通知
    /// </summary>
    /// <param name="message">消息</param>
    /// <param name="type">消息类型</param>
    /// <param name="autoClose">是否自动关闭</param>
    /// <param name="duration">持续时间</param>
    public void ShowNotification(
        string message,
        NotificationType type = NotificationType.Success,
        bool autoClose = true,
        int duration = DefaultDuration
    ) {
        // 异步通知
        _window.Dispatcher.InvokeAsync(() => {
            var notification = NotificationMessage.GetNotificationMessage(message, type);

            // 添加鼠标进入事件处理
            notification.MouseEnter += (_, _) => {
                _isMouseOverAnyNotification = true;
                PauseAllTimers();
            };

            // 添加鼠标离开事件处理
            notification.MouseLeave += (_, _) => {
                _isMouseOverAnyNotification = false;
                ResumeAllTimers();
            };

            // 添加到容器和活动通知列表
            _container?.Children.Add(notification);
            _activeNotifications.Add(notification);

            // 关联关闭
            notification.Closed += (_, _) => {
                _container?.Children.Remove(notification);
                _activeNotifications.Remove(notification);
            };

            // 自动关闭
            if (!autoClose) {
                return;
            }

            var timer = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds(duration)
            };

            // 关联timer
            notification.CloseTimer = timer;
            timer.Tick += (_, _) => {
                timer.Stop();
                notification.BeginFadeOutAnimation();
            };

            // 如果当前没有鼠标悬浮在任何通知上，则启动计时器
            if (!_isMouseOverAnyNotification) {
                timer.Start();
            }
        });
    }
}
