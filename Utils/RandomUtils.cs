using System;
using System.Collections.Generic;

namespace LoveYuri.Utils {
    
    /// <summary>
    /// 随机数工具类，每次new都会生成唯一random类用于后续素有随机数种子
    /// </summary>
    public class RandomUtils {
        private Random _random;
        
        /// <summary>
        /// 本轮使用的随机数种子
        /// </summary>
        public Random Random => _random ?? (_random = new Random());

        /// <summary>
        /// 数据字典
        /// </summary>
        private readonly Dictionary<string, HashSet<int>> _valueMap = new Dictionary<string, HashSet<int>>();
        
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
            
            if (!_valueMap.TryGetValue(name, out var history)) {
                _valueMap.Add(name, new HashSet<int> { data });
                return data;
            }

            // 容量已满
            if (history.Count == max - min) {
                history.Clear();
                history.Add(data);
                return data;
            }

            // 循环出下一个不存在的
            while (history.Contains(data)) {
                data = Random.Next(min, max);
            }
            return data;
        } 

    }
}