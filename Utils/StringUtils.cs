using System.Windows.Media;
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace LoveYuri.Utils;

/// <summary>
/// 关于字符串的常用方法
/// </summary>
public static class StringUtils {
    /// <summary>
    /// 将字符串转换成double类型，如果转换失败则返回默认值
    /// </summary>
    /// <param name="str">待转换的字符串</param>
    /// <param name="defaultValue">失败后的值，默认0</param>
    public static double TryToDouble(this string str, double defaultValue = 0.0) {
        return double.TryParse(str, out double value) ? value : defaultValue;
    }

    /// <summary>
    /// 将字符串转换成double类型，如果转换失败则抛出异常
    /// </summary>
    /// <param name="str">待转换的字符串</param>
    public static double ToDouble(this string str) {
        return double.Parse(str);
    }

    /// <summary>
    /// 将字符串转换成int类型，如果转换失败则抛出异常
    /// </summary>
    /// <param name="str">待转换的字符串</param>
    public static int ToInt(this string str) {
        return int.Parse(str);
    }

    /// <summary>
    /// 将字符串转换成int类型，如果转换失败则返回默认值
    /// </summary>
    /// <param name="str">待转换的字符串</param>
    /// <param name="defaultValue">失败后的值，默认0</param>
    public static int TryToInt(this string str, int defaultValue = 0) {
        return int.TryParse(str, out int value) ? value : defaultValue;
    }

    /// <summary>
    /// 转换字符串为color,如果转换失败则抛出异常
    /// </summary>
    public static Color ToColor(this string str) {
        object? res = ColorConverter.ConvertFromString(str);
        if (res == null) throw new Exception("转换失败");
        return (Color)res;
    }

    /// <summary>
    /// 尝试转换字符串为color,如果转换失败则返回默认值
    /// </summary>
    public static Color TryToColor(this string str, Color defaultValue = default) {
        try {
            object? res = ColorConverter.ConvertFromString(str);
            if (res == null) {
                return defaultValue;
            }
            return (Color)res;
        } catch (Exception) {
            return defaultValue;
        }
    }

    /// <summary>
    /// 将字符串格式化为枚举类型，如果失败则抛出异常
    /// </summary>
    public static T ToEnum<T>(this string v)
    {
        return (T)Enum.Parse(typeof(T), v);
    }

    /// <summary>
    /// 将字符串格式化为枚举类型，如果失败则抛出异常
    /// </summary>
    public static T TryToEnum<T>(this string v, T defaultValue = default) where T: struct, Enum
    {
        if (Enum.TryParse<T>(v, out var res)) {
            return res;
        }
        return defaultValue;
    }
}
