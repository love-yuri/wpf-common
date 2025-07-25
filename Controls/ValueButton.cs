using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace LoveYuri.Controls;

/// <summary>
/// 阀门开关按钮
/// </summary>
public class ValueButton : FrameworkElement {
    public static readonly DependencyProperty CirclePartColorProperty = DependencyProperty.Register(
        nameof(CirclePartColor), typeof(Brush), typeof(ValueButton),
        new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0x00, 0x4A, 0x8C)),
            FrameworkPropertyMetadataOptions.AffectsRender)
    );

    public static readonly DependencyProperty CenterPartColorProperty = DependencyProperty.Register(
        nameof(CenterPartColor), typeof(Brush), typeof(ValueButton),
        new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xDF, 0xD5, 0xD5, 0xD5)),
            FrameworkPropertyMetadataOptions.AffectsRender)
    );

    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
        nameof(IsChecked), typeof(bool), typeof(ValueButton),
        new FrameworkPropertyMetadata(
            false,
            FrameworkPropertyMetadataOptions.AffectsRender
            | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
        )
    );

    /// <summary>
    /// 是否选中
    /// </summary>
    public bool IsChecked {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    /// <summary>
    /// 获取或设置上下部分颜色
    /// </summary>
    public Brush CirclePartColor {
        get => (Brush)GetValue(CirclePartColorProperty);
        set => SetValue(CirclePartColorProperty, value);
    }

    /// <summary>
    /// 获取或设置中心部分颜色
    /// </summary>
    public Brush CenterPartColor {
        get => (Brush)GetValue(CenterPartColorProperty);
        set => SetValue(CenterPartColorProperty, value);
    }

    protected override void OnMouseEnter(MouseEventArgs e) {
        base.OnMouseEnter(e);
        Cursor = Cursors.Hand;
    }

    protected override void OnMouseLeave(MouseEventArgs e) {
        base.OnMouseLeave(e);
        Cursor = Cursors.Arrow;
    }

    protected override void OnRender(DrawingContext drawingContext) {
        base.OnRender(drawingContext);

        // 设置抗锯齿选项
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);

        double radius = Math.Min(ActualWidth, ActualHeight) / 2;
        var center = new Point(ActualWidth / 2, ActualHeight / 2);

        // 创建圆形剪裁区域
        var clipGeometry = new EllipseGeometry(center, radius, radius);
        drawingContext.PushClip(clipGeometry);

        // 绘制圆形边框
        drawingContext.DrawEllipse(CirclePartColor, null, center, radius, radius);

        // 绘制宽度等于直径的矩形
        double rectWidth = radius * 2;
        double rectHeight = ActualHeight / 3;

        // 根据按钮是否被选中来绘制不同的矩形
        bool isChecked = IsChecked;
        double x = center.X - (isChecked ? rectHeight : rectWidth) / 2;
        double y = center.Y - (isChecked ? rectWidth : rectHeight) / 2;
        double width = isChecked ? rectHeight : rectWidth;
        double height = isChecked ? rectWidth : rectHeight;
        var rect = new Rect(x, y, width, height);
        drawingContext.DrawRectangle(CenterPartColor, null, rect);

        // 恢复剪裁
        drawingContext.Pop();
    }

    // 添加命中测试，确保只在圆形区域内响应鼠标事件
    private HitTestResult HitTest(Point point) {
        double radius = Math.Min(ActualWidth, ActualHeight) / 2;
        var center = new Point(ActualWidth / 2, ActualHeight / 2);

        // 计算点到中心的距离
        double distance = Math.Sqrt(Math.Pow(point.X - center.X, 2) + Math.Pow(point.Y - center.Y, 2));

        // 如果点在圆内，则返回命中结果
        return distance <= radius ? new PointHitTestResult(this, point) : null;
    }

    // 在鼠标点击事件中检测命中对象
    protected override void OnMouseDown(MouseButtonEventArgs e) {
        if (HitTest(e.GetPosition(this)) != null) {
            IsChecked = !IsChecked; 
        }
    }
}