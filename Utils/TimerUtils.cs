using System;
using System.Timers;
using System.Windows.Threading;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace LoveYuri.Utils;

/// <summary>
/// 定时器相关的工具
/// </summary>
public static class TimerUtils {
    /// <summary>
    /// n 毫秒后执行任务，只执行一次，执行结束自己会释放
    /// </summary>
    /// <param name="milliseconds">延迟时间(毫秒)</param>
    /// <param name="func">回调函数</param>
    /// <param name="dispatcher">指定的UI调度器，为null则在后台线程执行</param>
    /// <returns>可用于取消定时器的对象</returns>
    public static Timer Timeout(this int milliseconds, Action func, Dispatcher dispatcher = null) {
        var timer = new Timer(milliseconds);
        timer.AutoReset = false;
        timer.Elapsed += (_, __) => {
            try {
                if (dispatcher != null) {
                    dispatcher.Invoke(func);
                } else {
                    func();
                }
            } finally {
                timer.Stop();
                timer.Dispose();
            }
        };

        timer.Start();

        // 返回一个可以用于取消定时器的对象
        return timer;
    }

    /// <summary>
    /// 一直执行，直到n次
    /// </summary>
    /// <param name="milliseconds">每多少毫秒执行一次</param>
    /// <param name="func">待执行的函数</param>
    /// <param name="n">执行次数</param>
    /// <param name="dispatcher">指定的UI调度器，为null则在后台线程执行</param>
    public static Timer Interval(this int milliseconds, Action func, int? n = null, Dispatcher dispatcher = null) {
        var timer = new Timer(milliseconds);

        timer.AutoReset = true;
        var count = 0;
        timer.Elapsed += (_, __) => {
            try {
                if (dispatcher != null) {
                    dispatcher.Invoke(func);
                } else {
                    func();
                }
            } finally {
                count++;
                if (count >= n) {
                    timer.Stop();
                    timer.Dispose();
                }
            }
        };

        timer.Start();
        return timer;
    }
}