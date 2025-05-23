using System.Windows;
using System.Windows.Controls;

namespace LoveYuri.Controls {
    /// <summary>
    /// 开关控件，是自用应用样式的checkbox
    /// </summary>
    public class ToggleSwitch : CheckBox {
        static ToggleSwitch() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
        }
    }
}
