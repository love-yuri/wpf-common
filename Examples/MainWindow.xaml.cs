using System.Windows;
using LoveYuri.Core.Sql;
using LoveYuri.Examples.Entity;
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
        var query = QueryWrapper<SysComponents>.Builder()
            .Eq(k => k.ComID, 1)
            .In(k => k.ComID, [2, 3, 4, 5, 6], LogicalOperatorType.Or)
            .OrderBy(k => k.ComID)
            .OrderByDesc(k => k.ComName);


        Log.Info($"{query.BuildSql()}");
        var sp = query.Select();
        sp.ForEach(k => {
            Log.Info($"id: {k.ComID} name: {k.ComName}");
        });
    }
}
