using System;
using System.Collections.Generic;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global

namespace LoveYuri.Utils;

/// <summary>
/// 随机数工具类，每次new都会生成唯一random类用于后续素有随机数种子
/// </summary>
public class RandomUtils {
    private Random random;

    /// <summary>
    /// 本轮使用的随机数种子
    /// </summary>
    public Random Random => random ?? (random = new Random());

    /// <summary>
    /// 数据字典
    /// </summary>
    private readonly Dictionary<string, HashSet<int>> valueMap = new Dictionary<string, HashSet<int>>();

    /// <summary>
    /// 在min-max的时间段内随机执行callback
    /// 如果callback抛出异常则中断执行
    /// </summary>
    /// <param name="name"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="callback"></param>
    public void RandomInterval(string name, int min, int max, Action callback)
    {
        RandomNext(name, min, max).Timeout(RealCallback);
        return;

        void RealCallback()
        {
            try {
                // 已经结束
                if (valueMap[name].Count == max - min) {
                    valueMap.Remove(name);
                    return;
                }

                callback.Invoke();
                RandomNext(name, min, max).Timeout(RealCallback);
            } catch (Exception e) {
                Log.Info($"callback: {callback} 发生异常: {e.Message}");
            }
        }
    }

    /// <summary>
    /// 在指定范围内随机产生下一个整数，且该数在该轮次从未出现过
    /// 如果所有数都出现了，将会清空计数器重新开始
    /// </summary>
    /// <param name="name">随机key</param>
    /// <param name="min">最小值</param>
    /// <param name="max">最大值</param>
    public int RandomNext(string name, int min, int max)
    {
        int data = Random.Next(min, max);

        if (!valueMap.TryGetValue(name, out var history)) {
            valueMap.Add(name, new HashSet<int> { data });
            return data;
        }

        // 容量已满
        if (history.Count >= max - min) {
            history.Clear();
            history.Add(data);
            return data;
        }

        // 循环出下一个不存在的
        while (history.Contains(data)) {
            data = Random.Next(min, max);
        }

        history.Add(data);
        return data;
    }
}