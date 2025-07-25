using System;
using System.Windows;

namespace LoveYuri.Utils;

/// <summary>
/// window常用工具
/// </summary>
public static class WindowUtils {
    /// <summary>
    /// 将当前window围绕父级窗口显示，优先选择剩余空间最大的方向
    /// </summary>
    /// <param name="window">待处理的window</param>
    public static void AroundOwner(this Window window) {
        var owner = window.Owner;
        if (owner == null) return;

        // 获取当前屏幕工作区
        var currentScreen = SystemParameters.WorkArea;

        // 计算各个方向的可用空间
        double rightSpace = currentScreen.Right - (owner.Left + owner.Width);
        double leftSpace = owner.Left - currentScreen.Left;
        double bottomSpace = currentScreen.Bottom - (owner.Top + owner.Height);
        double topSpace = owner.Top - currentScreen.Top;

        // 找出最大的可用空间方向
        double maxSpace = Math.Max(Math.Max(rightSpace, leftSpace), Math.Max(topSpace, bottomSpace));

        // 根据最大可用空间放置窗口
        if (Math.Abs(maxSpace - rightSpace) < 1 && rightSpace >= window.Width) {
            // 放在右侧
            window.Left = owner.Left + owner.Width;
            window.Top = owner.Top;
        } else if (Math.Abs(maxSpace - leftSpace) < 1 && leftSpace >= window.Width) {
            // 放在左侧
            window.Left = owner.Left - window.Width;
            window.Top = owner.Top;
        } else if (Math.Abs(maxSpace - bottomSpace) < 1 && bottomSpace >= window.Height) {
            // 放在下方
            window.Left = owner.Left;
            window.Top = owner.Top + owner.Height;
        } else if (Math.Abs(maxSpace - topSpace) < 1 && topSpace >= window.Height) {
            // 放在上方
            window.Left = owner.Left;
            window.Top = owner.Top - window.Height;
        } else {
            // 如果四周都没有足够空间，则尝试放在空间最大的方向，并确保在屏幕内
            if (Math.Abs(maxSpace - rightSpace) < 1) {
                window.Left = Math.Min(owner.Left + owner.Width, currentScreen.Right - window.Width);
                window.Top = owner.Top;
            } else if (Math.Abs(maxSpace - leftSpace) < 1) {
                window.Left = Math.Max(currentScreen.Left, owner.Left - window.Width);
                window.Top = owner.Top;
            } else if (Math.Abs(maxSpace - bottomSpace) < 1) {
                window.Left = owner.Left;
                window.Top = Math.Min(owner.Top + owner.Height, currentScreen.Bottom - window.Height);
            } else {
                window.Left = owner.Left;
                window.Top = Math.Max(currentScreen.Top, owner.Top - window.Height);
            }
        }
    }
}