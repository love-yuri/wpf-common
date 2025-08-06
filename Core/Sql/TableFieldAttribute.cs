namespace LoveYuri.Core.Sql;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class TableFieldAttribute(string name) : Attribute {
    /// <summary>
    /// 数据库列名（默认使用属性名）
    /// </summary>
    public string Name { get; set; } = name;
}
