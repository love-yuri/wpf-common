using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Shell;
using LoveYuri.Core.Mvvm;

// ReSharper disable MemberCanBePrivate.Global

namespace LoveYuri.Components {
    public class ModernWindow : Window {
        public const string PartMaximizeIcon = "PART_MaximizeIcon";
        private const string MaximizeIconPath = "M0,2 L8,2 L8,10 L0,10 Z M2,0 L10,0 L10,8 L8,8 L8,2 L2,2 Z";
        private const string NormalIconPath = "M0,0 L10,0 L10,10 L0,10 Z";

        /// <summary>
        /// 状态栏的背景颜色
        /// </summary>
        public static readonly DependencyProperty TopbarBackgroundColorProperty = DependencyProperty.Register(
            nameof(TopbarBackgroundColor), typeof(Brush), typeof(ModernWindow), new PropertyMetadata(new LinearGradientBrush {
                StartPoint = new Point(0, 0),  // 从左开始
                EndPoint = new Point(1, 0),    // 到右结束
                GradientStops = new GradientStopCollection {
                    new GradientStop(Color.FromRgb(0x3B, 0x8D, 0x99), 0.0),  // 起始色 #3b8d99
                    new GradientStop(Color.FromRgb(0x6B, 0x6B, 0x83), 0.5),  // 中间色 #6b6b83
                    new GradientStop(Color.FromRgb(0xAA, 0x4B, 0x6B), 1.0)   // 结束色 #aa4b6b
                }
            })
        );

        public Brush TopbarBackgroundColor {
            get => (Brush)GetValue(TopbarBackgroundColorProperty);
            set => SetValue(TopbarBackgroundColorProperty, value);
        }

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
        public  ICommand MinimizeCommand { get; }
        public  ICommand MaximizeCommand { get; }
        public  ICommand PinDownCommand { get; }

        protected ModernWindow () {
            CloseCommand = new RelayCommand(Close);
            MaximizeCommand = new RelayCommand(Maximize);
            MinimizeCommand = new RelayCommand(Minimize);
            PinDownCommand = new RelayCommand(PinDown);

            var windowChrome = new WindowChrome {
                GlassFrameThickness = new Thickness(-1),
                ResizeBorderThickness = new Thickness(5),
                CaptionHeight = 40,
                UseAeroCaptionButtons = false
            };
            WindowChrome.SetWindowChrome(this, windowChrome);
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
            if (GetTemplateChild(PartMaximizeIcon) is Path maximizeIcon) {
                maximizeIcon.Data = Geometry.Parse(WindowState == WindowState.Maximized
                    ? MaximizeIconPath
                    : NormalIconPath
                );
            }
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
}
