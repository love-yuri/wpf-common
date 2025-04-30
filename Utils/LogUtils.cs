using System;
// ReSharper disable MemberCanBePrivate.Global

namespace LoveYuri.Utils {
    internal enum LogLevel {
        Debug,
        Info,
        Error
    }

    /// <summary>
    /// 简单日志类
    /// </summary>
    public static class Log {
        /// <summary>
        /// 格式化消息日志
        /// </summary>
        /// <param name="level">消息等级 - 可以自定义</param>
        /// <param name="msg">消息</param>
        /// <returns></returns>
        public static string FormatMsg(object level, string msg) {
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string levelStr = level.ToString().PadLeft(5);
            return $"[{time} {levelStr}] {msg}";
        }

        /// <summary>
        /// 打印日志 info级别
        /// </summary>
        /// <param name="msg"></param>
        public static void Info(string msg) {
            string formatMsg = FormatMsg(LogLevel.Info, msg);
            Console.WriteLine(formatMsg);
        }

        /// <summary>
        /// Debug模式，只在debug下打印，级别等同于info
        /// </summary>
        /// <param name="msg"></param>
        public static void Debug(string msg) {
        #if DEBUG
            string formatMsg = FormatMsg(LogLevel.Debug, msg);
            Console.WriteLine(formatMsg); 
        #endif
        }

        /// <summary>
        /// 输出到标准错误流
        /// </summary>
        /// <param name="msg"></param>
        public static void Error(string msg) {
            string formatMsg = FormatMsg(LogLevel.Error, msg);
            Console.Error.WriteLine(formatMsg);
        }
    }
}
