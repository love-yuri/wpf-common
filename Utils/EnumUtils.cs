using System.ComponentModel;
using System.Reflection;

namespace LoveYuri.Utils;

/// <summary>
/// 枚举工具
/// </summary>
public static class EnumUtils {

    /// <summary>
    /// 获取枚举的Description属性，如果不存在则返回toString
    /// </summary>
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }



}
