using System.Windows;
using System.Windows.Controls;

namespace LoveYuri.Components {
    public class StatusIndicator : Control {
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            nameof(IsActive), typeof(bool), typeof(StatusIndicator),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(StatusIndicator),
                new PropertyMetadata(string.Empty));

        static StatusIndicator() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusIndicator),
                new FrameworkPropertyMetadata(typeof(StatusIndicator)));
        }

        public string Label {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public bool IsActive {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }
    }
}
