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
        
        private Path MaximizeIcon => GetTemplateChild(PartMaximizeIcon) as Path;

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
            MaximizeIcon.Data = Geometry.Parse(WindowState == WindowState.Maximized
                ? "M0,2 L8,2 L8,10 L0,10 Z M2,0 L10,0 L10,8 L8,8 L8,2 L2,2 Z"
                : "M0,0 L10,0 L10,10 L0,10 Z"
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
}