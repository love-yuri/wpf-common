using System;
using System.Threading;

namespace LoveYuri.Utils {
    /// <summary>
    /// 基于定时器的高性能防抖函数
    /// </summary>
    public class Debouncer {
        private readonly TimeSpan delay;
        private readonly object @lock = new object();
        private Timer timer;
        private Action pendingAction;
        private volatile int count;

        public Debouncer(TimeSpan delay)
        {
            if (delay <= TimeSpan.Zero) {
                throw new ArgumentException("延迟时间必须大于零", nameof(delay));
            }

            this.delay = delay;
        }

        /// <summary>
        /// 防抖执行操作
        /// </summary>
        /// <param name="action">要执行的操作</param>
        public void Debounce(Action action)
        {
            lock (@lock) {
                pendingAction = action;
                Interlocked.Increment(ref count);

                if (timer == null) {
                    // 首次创建定时器
                    timer = new Timer(OnTimerElapsed, null, delay, Timeout.InfiniteTimeSpan);
                } else {
                    // 重置定时器时间
                    timer.Change(delay, Timeout.InfiniteTimeSpan);
                }
            }
        }

        private void OnTimerElapsed(object state)
        {
            Action actionToExecute;

            lock (@lock) {

                actionToExecute = pendingAction;
                pendingAction = null;

                // 停止定时器直到下次需要
                timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            }

            // 在锁外执行操作，避免死锁
            try {
                actionToExecute?.Invoke();
            } catch(Exception e) {
                Console.Error.WriteLine($"防抖函数执行异常: {e.Message}");
            }
        }
    }
}
