using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;
using System.Windows.Threading;
using LoveYuri.Components;

namespace LoveYuri.Core.Notification {
    
    /// <summary>
    /// 通知服务
    /// </summary>
    public class NotificationService {
        private const int DefaultDuration = 3000;
        private Panel container;
        private readonly Window window;

        public NotificationService(Window window) {
            this.window = window;
            if (!window.Dispatcher.CheckAccess()) {
                window.Dispatcher.Invoke(Init);
            } else {
                Init();
            }
        }

        private void Init()
        {
            // 基础顶部偏移量
            var marginTop = 2.0;
            if (window is ToolbarWindow) {
                marginTop = -6.0;
            } else {
                var windowChrome = WindowChrome.GetWindowChrome(window);
                if (windowChrome != null) {
                    marginTop += windowChrome.CaptionHeight;
                }
            }

            // 创建通知容器 - 居中显示
            container = new StackPanel {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, marginTop, 0, 0)
            };

            // 添加到窗口
            Panel.SetZIndex(container, 9999);

            object originalContent = window.Content;
            var grid = new Grid();
            window.Content = grid;

            if (originalContent is UIElement element) {
                grid.Children.Add(element);
            }

            grid.Children.Add(container);
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
            window.Dispatcher.BeginInvoke(new Action(() => {
                var notification = NotificationMessage.GetNotificationMessage(message, type);

                // 添加到容器
                container.Children.Add(notification);
                
                // 关联关闭
                notification.Closed += (sender, args) => {
                    container.Children.Remove(notification);
                };
                
                // 自动关闭 
                if (autoClose) {
                    var timer = new DispatcherTimer {
                        Interval = TimeSpan.FromMilliseconds(duration)
                    };

                    // 关联timer
                    notification.CloseTimer = timer;
                    timer.Tick += (sender, args) => {
                        timer.Stop();
                        notification.BeginFadeOutAnimation();
                    };
                
                    timer.Start();
                }
            }));
        }
    }
}
