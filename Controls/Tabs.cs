using System.Windows;
using System.Windows.Controls;

namespace LoveYuri.Controls;

/// <summary>
/// 操作按钮，是自动应用样式的Button，支持加载状态
/// </summary>
public class Tabs : TabControl {
    static Tabs() {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Tabs),
            new FrameworkPropertyMetadata(typeof(Tabs)));
    }
}