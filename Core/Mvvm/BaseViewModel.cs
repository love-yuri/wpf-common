using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LoveYuri.Core.Mvvm {
    /// <summary>
    /// viewmodel 基类
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 更新属性（线程安全）
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            // 获取当前的事件处理器引用，避免在调用过程中被修改
            var handler = PropertyChanged;
            if (handler == null) return;

            // 使用线程安全的方式触发事件
            var args = new PropertyChangedEventArgs(propertyName);
            foreach (var @delegate in handler.GetInvocationList()) {
                var del = (PropertyChangedEventHandler)@delegate;
                var synchronizationContext = SynchronizationContext.Current;
                if (synchronizationContext != null) {
                    synchronizationContext.Post(
                        _ => del.Invoke(this, args), 
                        null
                    );
                } else {
                    del(this, args);
                }
            }
        }

        /// <summary>
        /// 设置字段，如果不同，则提示更新
        /// </summary>
        /// <param name="field">字段引用</param>
        /// <param name="value">待更新的数据</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>返回是否更新成功</returns>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 带前置检查的设置属性
        /// </summary>
        /// <param name="field">字段引用</param>
        /// <param name="value">待更新的数据</param>
        /// <param name="check">前置检查，如果返回值为false则不会处理更新操作</param>
        /// <param name="propertyName">属性名称</param>
        /// <typeparam name="T"></typeparam>
        protected bool SetField<T>(ref T field, T value, Func<T, bool> check, [CallerMemberName] string propertyName = null) {
            // 如果值没有变化直接返回
            if (EqualityComparer<T>.Default.Equals(field, value)) {
                return false;
            }

            if (propertyName == null || !check(value)) {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
