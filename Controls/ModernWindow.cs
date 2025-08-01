using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Shell;
using LoveYuri.Core.Mvvm;
using LoveYuri.Core.Service;

// ReSharper disable MemberCanBePrivate.Global

namespace LoveYuri.Controls;

/// <summary>
/// 泛型版本
/// </summary>
/// <typeparam name="TVm"></typeparam>
public class ModernWindow<TVm> : ModernWindow where TVm: class {
    protected ModernWindow() {
        DataContext = DiService.GetRequiredService<TVm>();
    }

    /// <summary>
    /// 线程安全的获取当前view的viewModel
    /// 如果不存在则返回null
    /// </summary>
    protected TVm ViewModel {
        get {
            if (Dispatcher.CheckAccess()) {
                return DataContext as TVm ?? throw new InvalidOperationException($"DataContext 不是 {typeof(TVm).Name} 类型");
            }

            return Dispatcher.Invoke(() => DataContext as TVm ?? throw new InvalidOperationException($"DataContext 不是 {typeof(TVm).Name} 类型"));
        }
    }
}

public class ModernWindow : Window {
    public const string PartMaximizeIcon = "PART_MaximizeIcon";
    public const string PartToolbarMenuButton = "PART_ToolbarMenuButton";
    public const string PartNotificationGrid = "PART_NotificationGrid";
    private const string MaximizeIconPath = "M0,2 L8,2 L8,10 L0,10 Z M2,0 L10,0 L10,8 L8,8 L8,2 L2,2 Z";
    private const string NormalIconPath = "M0,0 L10,0 L10,10 L0,10 Z";
    private Grid? _notificationGrid;
    private Button? _toolbarMenuButton;
    private Path? _resizeIconPath;

    #region 依赖属性

    /// <summary>
    /// 状态栏的背景颜色
    /// </summary>
    public static readonly DependencyProperty TopbarBackgroundColorProperty = DependencyProperty.Register(
        nameof(TopbarBackgroundColor), typeof(Brush), typeof(ModernWindow), new PropertyMetadata(new LinearGradientBrush {
            StartPoint = new Point(0, 0),  // 从左开始
            EndPoint = new Point(1, 0),    // 到右结束
            GradientStops = [
                new GradientStop(Color.FromRgb(0x3B, 0x8D, 0x99), 0.0), // 起始色 #3b8d99
                new GradientStop(Color.FromRgb(0x6B, 0x6B, 0x83), 0.5), // 中间色 #6b6b83
                new GradientStop(Color.FromRgb(0xAA, 0x4B, 0x6B), 1.0)
            ]
        })
    );

    /// <summary>
    /// 状态栏的菜单
    /// </summary>
    public static readonly DependencyProperty ToolbarMenuProperty = DependencyProperty.Register(
        nameof(ToolbarMenu), typeof(ContextMenu), typeof(ModernWindow),
        new PropertyMetadata(null)
    );

    public Brush TopbarBackgroundColor {
        get => (Brush)GetValue(TopbarBackgroundColorProperty);
        set => SetValue(TopbarBackgroundColorProperty, value);
    }

    public ContextMenu ToolbarMenu {
        get => (ContextMenu)GetValue(ToolbarMenuProperty);
        set => SetValue(ToolbarMenuProperty, value);
    }

    #endregion

    #region 控件

    /// <summary>
    /// 通知grid
    /// </summary>
    /// <returns></returns>
    internal Grid NotificationGrid => _notificationGrid ??= (Grid)GetTemplateChild(PartNotificationGrid)!;

    /// <summary>
    /// 状态栏菜单按钮
    /// </summary>
    internal Button ToolbarMenuButton => _toolbarMenuButton ??= (Button)GetTemplateChild(PartToolbarMenuButton)!;

    /// <summary>
    /// resize的path
    /// </summary>
    private Path ResizeIconPath => _resizeIconPath ??= (Path)GetTemplateChild(PartMaximizeIcon)!;

    #endregion

    static ModernWindow () {
        // 设置默认样式资源
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ModernWindow),
            new FrameworkPropertyMetadata(typeof(ModernWindow ))
        );
    }

    /// <summary>
    /// 关闭指令
    /// </summary>
    public ICommand CloseCommand { get; }
    public ICommand MinimizeCommand { get; }
    public ICommand MaximizeCommand { get; }
    public ICommand PinDownCommand { get; }
    public ICommand ToolbarMenuClickCommand { get; }
    protected ModernWindow () {
        CloseCommand = new RelayCommand(Close);
        MaximizeCommand = new RelayCommand(Maximize);
        MinimizeCommand = new RelayCommand(Minimize);
        PinDownCommand = new RelayCommand(PinDown);
        ToolbarMenuClickCommand = new RelayCommand(ToolbarMenuClick);

        var windowChrome = new WindowChrome {
            GlassFrameThickness = new Thickness(-1),
            ResizeBorderThickness = new Thickness(5),
            CaptionHeight = 40,
            UseAeroCaptionButtons = false
        };
        WindowChrome.SetWindowChrome(this, windowChrome);
    }

    private void ToolbarMenuClick()
    {
        if (ToolbarMenuButton is { ContextMenu: not null }) {
            ToolbarMenuButton.ContextMenu.PlacementTarget = ToolbarMenuButton;
            ToolbarMenuButton.ContextMenu.IsOpen = true;
        }
    }

    /// <summary>
    /// 最大化
    /// </summary>
    private void Maximize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
        base.OnRenderSizeChanged(sizeInfo);
        Margin = new Thickness(WindowState == WindowState.Maximized ? 5 : 0);
        ResizeIconPath.Data = Geometry.Parse(WindowState == WindowState.Maximized
            ? MaximizeIconPath
            : NormalIconPath
        );
    }

    /// <summary>
    /// 最小化
    /// </summary>
    private void Minimize()
    {
        WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// 钉住窗口，切换窗口的置顶状态
    /// </summary>
    private void PinDown() {
        Topmost = !Topmost;
    }
}
