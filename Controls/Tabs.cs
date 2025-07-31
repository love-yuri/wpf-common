using System.Windows;
using System.Windows.Controls;

namespace LoveYuri.Controls;

/// <summary>
/// 标签页控件，支持自动应用样式
/// </summary>
public class Tabs : TabControl {
    static Tabs() {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Tabs),
            new FrameworkPropertyMetadata(typeof(Tabs)));
    }
}
