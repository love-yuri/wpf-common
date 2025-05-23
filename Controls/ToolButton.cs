using System.Windows;
using System.Windows.Controls;

namespace LoveYuri.Controls {
    /// <summary>
    /// 操作按钮，是自动应用样式的Button，支持加载状态
    /// </summary>
    public class ToolButton : Button {
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(ToolButton),
                new PropertyMetadata(false));

        static ToolButton() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolButton),
                new FrameworkPropertyMetadata(typeof(ToolButton)));
        }

        /// <summary>
        /// 是否处于加载状态
        /// </summary>
        public bool IsLoading {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }
    }
}
