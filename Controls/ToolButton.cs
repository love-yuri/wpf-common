using System.Windows;
using System.Windows.Controls;

namespace LoveYuri.Controls;

/// <summary>
///     操作按钮，是自动应用样式的Button，支持加载状态
/// </summary>
/// <summary>
///     扩展的ToolButton控件，支持自定义背景和加载状态
/// </summary>
public class ToolButton : Button {
    // 加载状态依赖属性
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(ToolButton),
            new PropertyMetadata(false)
        );

    static ToolButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ToolButton),
            new FrameworkPropertyMetadata(typeof(ToolButton))
        );
    }


    public bool IsLoading {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
}
