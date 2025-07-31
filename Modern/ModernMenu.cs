using System.Windows;
using System.Windows.Controls;

namespace LoveYuri.Modern;

/// <summary>
/// Modern Context Menu
/// </summary>
public class ModernContextMenu: ContextMenu {
    static ModernContextMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ModernContextMenu),
            new FrameworkPropertyMetadata(typeof(ModernContextMenu))
        );
    }
}


/// <summary>
/// Modern Context Menu
/// </summary>
public class ModernMenuItem: MenuItem {
    static ModernMenuItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ModernMenuItem),
            new FrameworkPropertyMetadata(typeof(ModernMenuItem))
        );
    }
}
