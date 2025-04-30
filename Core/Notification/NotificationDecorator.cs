using System.Runtime.CompilerServices;
using System.Windows;

namespace LoveYuri.Core.Notification {
    public static class NotificationDecorator {
        private static readonly ConditionalWeakTable<Window, NotificationService> Services =
            new ConditionalWeakTable<Window, NotificationService>();

        /// <summary>
        /// 获取通知service，同一实例只会实现一次
        /// </summary>
        private static NotificationService GetNotificationService(this Window window) {
            return Services.GetValue(
                window,
                win => new NotificationService(win)
            );
        }

        public static void ShowSuccess(this Window window, string message, bool autoClose = true, int duration = 2000) {
            window.GetNotificationService().ShowNotification(message, NotificationType.Success, autoClose, duration);
        }

        public static void ShowInfo(this Window window, string message,bool autoClose = true, int duration = 2000) {
            window.GetNotificationService().ShowNotification(message, NotificationType.Info, autoClose, duration);
        }

        public static void ShowWarning(this Window window, string message,bool autoClose = true, int duration = 3000) {
            window.GetNotificationService().ShowNotification(message, NotificationType.Warning, autoClose, duration);
        }

        public static void ShowError(this Window window, string message,bool autoClose = true, int duration = 3000) {
            window.GetNotificationService().ShowNotification(message, NotificationType.Error, autoClose, duration);
        }
    }
}
