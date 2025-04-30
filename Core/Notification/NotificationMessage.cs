using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using LoveYuri.Core.Mvvm;

namespace LoveYuri.Core.Notification {
    public enum NotificationType {
        Success,
        Info,
        Warning,
        Error
    }

    public class NotificationMessage : UserControl {
        /// <summary>
        /// 关闭指令
        /// </summary>
        public ICommand CloseCommand { get; }

        /// <summary>
        /// 具体消息
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 关闭定时器
        /// </summary>
        public DispatcherTimer CloseTimer { get; set; }

        /// <summary>
        /// 关闭事件
        /// </summary>
        public event EventHandler Closed;

        protected NotificationMessage(string message) {
            Message = message;
            CloseCommand = new RelayCommand(BeginFadeOutAnimation);

            // 创建淡入动画
            BeginFadeInAnimation();
        }

        protected override void OnMouseEnter(MouseEventArgs e) {
            // 鼠标悬浮后不再关闭
            // CloseTimer?.Stop();
        }

        private void BeginFadeInAnimation() {
            // 初始状态设置
            Opacity = 0;
            RenderTransform = new TransformGroup {
                Children = {
                    new ScaleTransform(0.8, 0.8),
                    new TranslateTransform(0, -20)
                }
            };

            // 创建动画组
            var storyboard = new Storyboard();

            // 透明度动画
            var fadeInAnimation = new DoubleAnimation {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(fadeInAnimation, this);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeInAnimation);

            // 缩放动画
            var scaleXAnimation = new DoubleAnimation {
                From = 0.8,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(350),
                EasingFunction = new ElasticEase { 
                    EasingMode = EasingMode.EaseOut,
                    Oscillations = 1,
                    Springiness = 3
                }
            };
            Storyboard.SetTarget(scaleXAnimation, this);
            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("RenderTransform.Children[0].ScaleX"));
            storyboard.Children.Add(scaleXAnimation);

            var scaleYAnimation = new DoubleAnimation {
                From = 0.8,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(350),
                EasingFunction = new ElasticEase { 
                    EasingMode = EasingMode.EaseOut,
                    Oscillations = 1,
                    Springiness = 3
                }
            };
            Storyboard.SetTarget(scaleYAnimation, this);
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("RenderTransform.Children[0].ScaleY"));
            storyboard.Children.Add(scaleYAnimation);

            // 位移动画
            var translateAnimation = new DoubleAnimation {
                From = -20,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new BackEase { 
                    EasingMode = EasingMode.EaseOut,
                    Amplitude = 0.5
                }
            };
            Storyboard.SetTarget(translateAnimation, this);
            Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("RenderTransform.Children[1].Y"));
            storyboard.Children.Add(translateAnimation);

            // 开始动画
            storyboard.Begin();
        }

        public void BeginFadeOutAnimation() {
            // 创建动画组
            var storyboard = new Storyboard();

            // 透明度动画
            var fadeOut = new DoubleAnimation {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(fadeOut, this);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeOut);

            // 缩放动画
            var scaleXAnimation = new DoubleAnimation {
                From = 1,
                To = 0.8,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(scaleXAnimation, this);
            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("RenderTransform.Children[0].ScaleX"));
            storyboard.Children.Add(scaleXAnimation);

            var scaleYAnimation = new DoubleAnimation {
                From = 1,
                To = 0.8,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(scaleYAnimation, this);
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("RenderTransform.Children[0].ScaleY"));
            storyboard.Children.Add(scaleYAnimation);

            // 位移动画
            var translateAnimation = new DoubleAnimation {
                From = 0,
                To = 20,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(translateAnimation, this);
            Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("RenderTransform.Children[1].Y"));
            storyboard.Children.Add(translateAnimation);

            // 高度收缩动画
            var heightAnimation = new DoubleAnimation {
                From = ActualHeight,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                BeginTime = TimeSpan.FromMilliseconds(100), // 稍微延迟开始
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(heightAnimation, this);
            Storyboard.SetTargetProperty(heightAnimation, new PropertyPath(HeightProperty));
            storyboard.Children.Add(heightAnimation);

            // 当动画完成时触发关闭事件
            storyboard.Completed += (s, e) => Closed?.Invoke(this, EventArgs.Empty);

            // 开始动画
            storyboard.Begin();
        }

        /// <summary>
        /// 根据type 返回通知
        /// </summary>
        public static NotificationMessage GetNotificationMessage(string message, NotificationType type) {
            switch (type) {
                case NotificationType.Success:
                    return new SuccessNotificationMessage(message);
                case NotificationType.Info:
                    return new InfoNotificationMessage(message);
                case NotificationType.Warning:
                    return new WarningNotificationMessage(message);
                case NotificationType.Error:
                    return new ErrorNotificationMessage(message);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }

    /// <summary>
    /// 成功消息
    /// </summary>
    public class SuccessNotificationMessage : NotificationMessage {
        static SuccessNotificationMessage() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SuccessNotificationMessage),
                new FrameworkPropertyMetadata(typeof(SuccessNotificationMessage)));
        }

        public SuccessNotificationMessage(string message) : base(message) { }
    }

    /// <summary>
    /// 错误消息
    /// </summary>
    public class ErrorNotificationMessage : NotificationMessage {
        static ErrorNotificationMessage() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ErrorNotificationMessage),
                new FrameworkPropertyMetadata(typeof(ErrorNotificationMessage)));
        }

        public ErrorNotificationMessage(string message) : base(message) { }
    }

    /// <summary>
    /// 信息消息
    /// </summary>
    public class InfoNotificationMessage : NotificationMessage {
        static InfoNotificationMessage() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InfoNotificationMessage),
                new FrameworkPropertyMetadata(typeof(InfoNotificationMessage)));
        }

        public InfoNotificationMessage(string message) : base(message) { }
    }

    /// <summary>
    /// 警告消息
    /// </summary>
    public class WarningNotificationMessage : NotificationMessage {
        static WarningNotificationMessage() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WarningNotificationMessage),
                new FrameworkPropertyMetadata(typeof(WarningNotificationMessage)));
        }

        public WarningNotificationMessage(string message) : base(message) { }
    }
}
