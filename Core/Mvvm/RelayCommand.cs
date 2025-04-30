using System;
using System.Diagnostics;
using System.Windows.Input;

namespace LoveYuri.Core.Mvvm {
    /// <summary>
    /// 无参数版本的 RelayCommand 实现
    /// </summary>
    public class RelayCommand : ICommand {
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;

        /// <summary>
        /// 创建有条件执行的新命令
        /// </summary>
        public RelayCommand(Action execute, Func<bool> canExecute = null) {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter) {
            _execute();
        }

        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public static RelayCommand CreateCommand(Action execute) {
            return new RelayCommand(execute);
        }

        public void RaiseCanExecuteChanged() {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// 泛型 RelayCommand 实现，支持强类型参数和条件执行检查
    /// </summary>
    /// <typeparam name="T">命令参数类型</typeparam>
    public class RelayCommand<T> : ICommand {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;
        private readonly Func<object, T> _parameterConverter;

        /// <summary>
        /// 创建有条件执行的新命令
        /// </summary>
        /// <param name="execute">执行逻辑</param>
        /// <param name="canExecute">执行条件判断逻辑</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute) : this(execute, canExecute, null) { }

        /// <summary>
        /// 创建支持参数转换的新命令
        /// </summary>
        /// <param name="execute">执行逻辑</param>
        /// <param name="canExecute">执行条件判断逻辑</param>
        /// <param name="parameterConverter">参数转换器</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null,
            Func<object, T> parameterConverter = null) {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
            _parameterConverter = parameterConverter;
        }

        /// <summary>
        /// 判断命令是否可以执行
        /// </summary>
        [DebuggerStepThrough]
        public bool CanExecute(object parameter) {
            try {
                var typedParameter = ConvertParameter(parameter);
                return _canExecute == null || _canExecute(typedParameter);
            } catch {
                return false;
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        public void Execute(object parameter) {
            var typedParameter = ConvertParameter(parameter);
            _execute(typedParameter);
        }

        /// <summary>
        /// CanExecuteChanged 事件
        /// </summary>
        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// 转换参数到指定类型
        /// </summary>
        private T ConvertParameter(object parameter) {
            if (_parameterConverter != null) return _parameterConverter(parameter);

            switch (parameter) {
                case null when default(T) == null:
                    return default;
                case T typedParameter:
                    return typedParameter;
                default:
                    return (T)Convert.ChangeType(parameter, typeof(T));
            }
        }

        /// <summary>
        /// 触发 CanExecute 变化通知
        /// </summary>
        public void RaiseCanExecuteChanged() {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
