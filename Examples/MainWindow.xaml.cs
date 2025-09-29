using System.Windows;
using LoveYuri.Core.Notification;
using LoveYuri.Core.Sql;
using LoveYuri.Examples.Entity;
using LoveYuri.Modern;
using LoveYuri.Utils;

namespace LoveYuri.Examples;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow  {
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Test1(object sender, RoutedEventArgs e)
    {
        var query = QueryWrapper<SysComponents>.Query
            .Eq(k => k.ComID, 1)
            .In(k => k.ComID, [2, 3, 4, 5, 6], LogicalOperatorType.Or)
            .Group(w => {
                w.Eq(k => k.ComID, 11);
                w.Eq(k => k.ComID, 12, LogicalOperatorType.Or);
            }, LogicalOperatorType.Or)
            .OrderBy(k => k.ComID)
            .OrderByDesc(k => k.ComName);

        List<SysComponents> list = [
            new() { ComID = 30, ComName = "yuri22", Enabled = true },
            new() { ComID = 31, ComName = "yuri23", Enabled = true },
            new() { ComID = 32, ComName = "yuri24", Enabled = true },
        ];

        var ret = list.InsertBatch();
        Log.Info($"ret -> {ret}");

        ret = UpdateQueryWrapper<SysComponents>
            .Query
            .Set(k => k.ComTypeName, "yuri22")
            .Eq(k => k.ComID, 21)
            .Update();
        Log.Info($"ret -> {ret}");
    }
}
