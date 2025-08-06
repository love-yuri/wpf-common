namespace LoveYuri.Core.Sql;

using System;

[AttributeUsage(AttributeTargets.Class)]
public class TableInfoAttribute(string dataSource, string tableName) : Attribute {
    public string DataSource { get; } = dataSource;
    public string TableName { get; } = tableName;
}
