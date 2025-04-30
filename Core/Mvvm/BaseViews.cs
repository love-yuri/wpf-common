using System;
using System.Windows.Controls;

namespace LoveYuri.Core.Mvvm {
    /// <summary>
    /// 显示界面基类，自动添加使用viewModel
    /// </summary>
    /// <typeparam name="TVm">viewModel类型</typeparam>
    public abstract class BaseUserControl<TVm> : UserControl where TVm : BaseViewModel {
        protected BaseUserControl(TVm viewModel) {
            // 自动注入viewmodel
            DataContext = viewModel;
        }

        /// <summary>
        /// 当前view的viewModel
        /// </summary>
        /// <exception cref="Exception"></exception>
        protected TVm ViewModel {
            get {
                if (Dispatcher.CheckAccess()) {
                    return DataContext as TVm ?? throw new InvalidOperationException($"DataContext 不是 {typeof(TVm).Name} 类型");
                }
                    
                return Dispatcher.Invoke(() => DataContext as TVm ?? throw new InvalidOperationException($"DataContext 不是 {typeof(TVm).Name} 类型"));
            }
        }
    }
}
