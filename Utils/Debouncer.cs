
namespace LoveYuri.Utils;

/// <summary>
/// 基于定时器的高性能防抖函数 (.NET 8 版本，支持 nullable)
/// </summary>
public sealed class Debouncer : IDisposable
{
    private readonly TimeSpan delay;
    private readonly object @lock = new();
    private Timer? timer;
    private Action? pendingAction;
    private volatile int count;
    private volatile bool disposed;

    /// <summary>
    /// 获取当前等待执行的操作数量
    /// </summary>
    public int PendingCount => count;

    /// <summary>
    /// 获取是否已释放资源
    /// </summary>
    public bool IsDisposed => disposed;

    public Debouncer(TimeSpan delay)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(delay, TimeSpan.Zero, nameof(delay));
        this.delay = delay;
    }

    /// <summary>
    /// 防抖执行操作
    /// </summary>
    /// <param name="action">要执行的操作</param>
    /// <exception cref="ArgumentNullException">当 action 为 null 时抛出</exception>
    /// <exception cref="ObjectDisposedException">当对象已释放时抛出</exception>
    public void Debounce(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ObjectDisposedException.ThrowIf(disposed, this);

        lock (@lock) {
            if (disposed) return;

            pendingAction = action;
            Interlocked.Increment(ref count);

            timer ??= new Timer(OnTimerElapsed, null, delay, Timeout.InfiniteTimeSpan);
            timer.Change(delay, Timeout.InfiniteTimeSpan);
        }
    }

    /// <summary>
    /// 立即执行待处理的操作（如果有的话）
    /// </summary>
    public void Flush()
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        Action? actionToExecute;
        lock (@lock) {
            if (disposed) return;

            actionToExecute = pendingAction;
            pendingAction = null;
            timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        ExecuteAction(actionToExecute);
    }

    /// <summary>
    /// 取消所有待处理的操作
    /// </summary>
    public void Cancel()
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        lock (@lock) {
            if (disposed) return;

            pendingAction = null;
            timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            Interlocked.Exchange(ref count, 0);
        }
    }

    private void OnTimerElapsed(object? state)
    {
        if (disposed) return;

        Action? actionToExecute;
        lock (@lock) {
            if (disposed) return;

            actionToExecute = pendingAction;
            pendingAction = null;
            timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        ExecuteAction(actionToExecute);
    }

    private static void ExecuteAction(Action? action)
    {
        if (action is null) return;

        try {
            action.Invoke();
        } catch (Exception ex) {
            Console.Error.WriteLine($"防抖函数执行异常: {ex}");
        }
    }

    public void Dispose()
    {
        if (disposed) return;

        lock (@lock) {
            if (disposed) return;

            disposed = true;
            timer?.Dispose();
            timer = null;
            pendingAction = null;
        }
    }
}
