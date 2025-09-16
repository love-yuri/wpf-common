using System.Windows;
using System.Windows.Controls;

namespace LoveYuri.Core.Mvvm;

/// <summary>
/// 显示界面基类，自动添加使用viewModel
/// </summary>
/// <typeparam name="TVm">viewModel类型</typeparam>
public abstract class ViewModelWindow<TVm> : Window where TVm : class {
    /// <summary>
    /// 线程安全的获取当前view的viewModel
    /// 如果不存在则返回null
    /// </summary>
    protected TVm ViewModel {
        get {
            if (Dispatcher.CheckAccess()) {
                return DataContext as TVm ?? throw new InvalidOperationException($"DataContext 不是 {typeof(TVm).Name} 类型");
            }

            return Dispatcher.Invoke(() => DataContext as TVm ?? throw new InvalidOperationException($"DataContext 不是 {typeof(TVm).Name} 类型"));
        }
    }
}
