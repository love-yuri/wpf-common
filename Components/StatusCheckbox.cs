using System.Windows;
using System.Windows.Controls;

namespace LoveYuri.Components {
    /// <summary>
    /// 显示状态的checkbox
    /// </summary>
    public class StatusCheckbox : CheckBox {
        static StatusCheckbox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusCheckbox),
                new FrameworkPropertyMetadata(typeof(StatusCheckbox)));
        }
    }
}
