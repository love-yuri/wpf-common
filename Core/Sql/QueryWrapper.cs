// ReSharper disable InvalidXmlDocComment

using System.Reflection;
using Dapper;
using Microsoft.Data.Sqlite;

namespace LoveYuri.Core.Sql;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

// 逻辑操作符 默认And
public enum LogicalOperatorType {
    And = 0,// 与
    Or = 1, // 或
}

/// <summary>
/// 泛型查询构造器，提供强类型的链式查询条件构建
/// </summary>
/// <typeparam name="T">实体类型，必须实现 IMessage 接口</typeparam>
public class QueryWrapper<T> where T : new()
{
    private readonly StringBuilder conditionBuilder = new();
    private readonly StringBuilder orderByBuilder = new();
    protected QueryWrapper() {}

    /// <summary>
    /// 参数值字典，用于参数化查询
    /// </summary>
    protected Dictionary<string, object> Values { get; init; } = null!;

    /// <summary>
    /// limit sql
    /// </summary>
    public string LimitSql { get; private set; } = string.Empty;

    /// <summary>
    /// 构建完整sql语句
    /// </summary>
    /// <returns></returns>
    public string BuildSql()
    {
        var stringBuilder = new StringBuilder();
        if (conditionBuilder.Length > 0) {
            stringBuilder.Append("where (");
            stringBuilder.Append(conditionBuilder);
            stringBuilder.Append(')');
        }

        if (orderByBuilder.Length > 0) {
            stringBuilder.Append(" ORDER BY ");
            stringBuilder.Append(orderByBuilder);
        }

        stringBuilder.Append(LimitSql);
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 创建 QueryWrapper 实例
    /// </summary>
    /// <returns>新的 QueryWrapper 实例</returns>
    public static QueryWrapper<T> Builder()
    {
        return new QueryWrapper<T> {
            Values = new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// 从 Lambda 表达式中获取属性名
    /// </summary>
    /// <param name="expression">属性选择表达式</param>
    /// <returns>属性名</returns>
    /// <exception cref="ArgumentException">当表达式不是有效的属性访问时抛出</exception>
    protected static string GetFieldName<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        return expression.Body switch
        {
            MemberExpression memberExpr => memberExpr.Member.Name,
            UnaryExpression { Operand: MemberExpression memberExpr } => memberExpr.Member.Name,
            _ => throw new ArgumentException("表达式必须是简单的属性访问，如：p => p.PropertyName", nameof(expression))
        };
    }

    /// <summary>
    /// 添加查询条件
    /// </summary>
    /// <param name="fieldName">字段名</param>
    /// <param name="op">操作符</param>
    /// <param name="value">值</param>
    /// <param name="logical">逻辑操作符</param>
    private void AddCondition(string fieldName, string op, object? value, LogicalOperatorType logical)
    {
        // 生成唯一的参数名
        string paramKey = GenerateUniqueParamKey(fieldName);

        // 添加逻辑操作符
        if (conditionBuilder.Length > 0) {
            string logicalOp = logical == LogicalOperatorType.And ? "AND" : "OR";
            conditionBuilder.Append($" {logicalOp} ");
        }

        // 添加条件
        conditionBuilder.Append($"({fieldName} {op} {paramKey})");

        Values[paramKey] = value!;
    }

    /// <summary>
    /// 生成唯一的参数键名
    /// </summary>
    /// <param name="fieldName">字段名</param>
    /// <returns>唯一的参数键名</returns>
    protected string GenerateUniqueParamKey(string fieldName)
    {
        var baseKey = $"@{fieldName}";
        if (!Values.ContainsKey(baseKey)) {
            return baseKey;
        }

        var index = 1;
        string key;
        do {
            key = $"@{fieldName}_{index++}";
        } while (Values.ContainsKey(key));

        return key;
    }

    #region 比较操作符

    /// <summary>
    /// 相等条件（字段等于指定值）
    /// </summary>
    /// <param name="expression">字段选择表达式</param>
    /// <param name="value">要匹配的值</param>
    /// <param name="logical">逻辑操作符，默认为 AND</param>
    /// <example>
    /// <code>
    /// wrapper.Eq(x => x.ComId, 1)
    /// </code>
    /// 生成的SQL：WHERE ComId = @ComId
    /// </example>
    public QueryWrapper<T> Eq<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value, LogicalOperatorType logical = LogicalOperatorType.And)
    {
        string fieldName = GetFieldName(expression);
        AddCondition(fieldName, "=", value, logical);
        return this;
    }

    /// <summary>
    /// 不等条件（字段不等于指定值）
    /// </summary>
    /// <param name="expression">字段选择表达式</param>
    /// <param name="value">要比较的值</param>
    /// <param name="logical">逻辑操作符，默认为 AND</param>
    /// <example>
    /// <code>
    /// wrapper.Neq(x => x.Status, "deleted")
    /// </code>
    /// 生成的SQL：WHERE Status != @Status
    /// </example>
    public QueryWrapper<T> Neq<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value, LogicalOperatorType logical = LogicalOperatorType.And)
    {
        string fieldName = GetFieldName(expression);
        AddCondition(fieldName, "!=", value, logical);
        return this;
    }

    /// <summary>
    /// 大于条件（字段大于指定值）
    /// </summary>
    /// <param name="expression">字段选择表达式</param>
    /// <param name="value">要比较的值</param>
    /// <param name="logical">逻辑操作符，默认为 AND</param>
    /// <example>
    /// <code>
    /// wrapper.Gt(x => x.CreateTime, DateTime.Now.AddDays(-7))
    /// </code>
    /// 生成的SQL：WHERE CreateTime > @CreateTime
    /// </example>
    public QueryWrapper<T> Gt<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value, LogicalOperatorType logical = LogicalOperatorType.And)
    {
        string fieldName = GetFieldName(expression);
        AddCondition(fieldName, ">", value, logical);
        return this;
    }

    /// <summary>
    /// 大于等于条件（字段大于等于指定值）
    /// </summary>
    /// <param name="expression">字段选择表达式</param>
    /// <param name="value">要比较的值</param>
    /// <param name="logical">逻辑操作符，默认为 AND</param>
    /// <example>
    /// <code>
    /// wrapper.Gte(x => x.Score, 60)
    /// </code>
    /// 生成的SQL：WHERE Score >= @Score
    /// </example>
    public QueryWrapper<T> Gte<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value, LogicalOperatorType logical = LogicalOperatorType.And)
    {
        string fieldName = GetFieldName(expression);
        AddCondition(fieldName, ">=", value, logical);
        return this;
    }

    /// <summary>
    /// 小于条件（字段小于指定值）
    /// </summary>
    /// <param name="expression">字段选择表达式</param>
    /// <param name="value">要比较的值</param>
    /// <param name="logical">逻辑操作符，默认为 AND</param>
    /// <example>
    /// <code>
    /// wrapper.Lt(x => x.Age, 18)
    /// </code>
    /// 生成的SQL：WHERE Age < @Age
    /// </example>
    public QueryWrapper<T> Lt<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value, LogicalOperatorType logical = LogicalOperatorType.And)
    {
        string fieldName = GetFieldName(expression);
        AddCondition(fieldName, "<", value, logical);
        return this;
    }

    /// <summary>
    /// 小于等于条件（字段小于等于指定值）
    /// </summary>
    /// <param name="expression">字段选择表达式</param>
    /// <param name="value">要比较的值</param>
    /// <param name="logical">逻辑操作符，默认为 AND</param>
    /// <example>
    /// <code>
    /// wrapper.Lte(x => x.Priority, 5)
    /// </code>
    /// 生成的SQL：WHERE Priority <= @Priority
    /// </example>
    public QueryWrapper<T> Lte<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value, LogicalOperatorType logical = LogicalOperatorType.And)
    {
        string fieldName = GetFieldName(expression);
        AddCondition(fieldName, "<=", value, logical);
        return this;
    }

    #endregion

    #region 模糊匹配和范围查询

    /// <summary>
    /// 模糊匹配条件（字段包含指定值）
    /// </summary>
    /// <param name="expression">字段选择表达式</param>
    /// <param name="value">要模糊匹配的值</param>
    /// <param name="logical">逻辑操作符，默认为 AND</param>
    /// <example>
    /// <code>
    /// wrapper.Like(x => x.Name, "test")
    /// </code>
    /// 生成的SQL：WHERE Name LIKE @Name
    /// </example>
    public QueryWrapper<T> Like(Expression<Func<T, string>> expression, string value, LogicalOperatorType logical = LogicalOperatorType.And)
    {
        string fieldName = GetFieldName(expression);
        AddCondition(fieldName, "LIKE", value, logical);
        return this;
    }

    /// <summary>
    /// IN 条件（字段值在指定集合中）
    /// </summary>
    /// <param name="expression">字段选择表达式</param>
    /// <param name="values">要匹配的值集合</param>
    /// <param name="logical">逻辑操作符，默认为 AND</param>
    /// <example>
    /// <code>
    /// wrapper.In(x => x.Status, new[] { "active", "pending", "approved" })
    /// </code>
    /// 生成的SQL：WHERE Status IN (@Status)
    /// </example>
    public QueryWrapper<T> In<TProperty>(Expression<Func<T, TProperty>> expression, IEnumerable<TProperty> values, LogicalOperatorType logical = LogicalOperatorType.And)
    {
        string fieldName = GetFieldName(expression);
        var valueList = values.ToList();
        if (valueList.Count == 0) {
            return this;
        }

        string paramKey = GenerateUniqueParamKey(fieldName);
        if (conditionBuilder.Length > 0) {
            string logicalOp = logical == LogicalOperatorType.And ? "AND" : "OR";
            conditionBuilder.Append($" {logicalOp} ");
        }

        conditionBuilder.Append($"({fieldName} IN {paramKey})");
        Values[paramKey] = valueList;
        return this;
    }

    /// <summary>
    /// NOT IN 条件（字段值不在指定集合中）
    /// </summary>
    /// <param name="expression">字段选择表达式</param>
    /// <param name="values">要排除的值集合</param>
    /// <param name="logical">逻辑操作符，默认为 AND</param>
    /// <example>
    /// <code>
    /// wrapper.NotIn(x => x.Status, new[] { "deleted", "archived" })
    /// </code>
    /// 生成的SQL：WHERE Status NOT IN (@Status)
    /// </example>
    public QueryWrapper<T> NotIn<TProperty>(Expression<Func<T, TProperty>> expression, IEnumerable<TProperty> values, LogicalOperatorType logical = LogicalOperatorType.And)
    {
        string fieldName = GetFieldName(expression);
        var valueList = values.ToList();
        if (valueList.Count == 0) {
            return this;
        }

        string paramKey = GenerateUniqueParamKey(fieldName);
        if (conditionBuilder.Length > 0) {
            string logicalOp = logical == LogicalOperatorType.And ? "AND" : "OR";
            conditionBuilder.Append($" {logicalOp} ");
        }

        conditionBuilder.Append($"({fieldName} NOT IN {paramKey})");
        Values[paramKey] = valueList;
        return this;
    }

    /// <summary>
    /// BETWEEN 条件（字段值在指定区间内）
    /// </summary>
    /// <param name="expression">字段选择表达式</param>
    /// <param name="start">区间起始值</param>
    /// <param name="end">区间结束值</param>
    /// <param name="logical">逻辑操作符，默认为 AND</param>
    /// <example>
    /// <code>
    /// wrapper.Between(x => x.Age, 18, 65)
    /// </code>
    /// 生成的SQL：WHERE Age BETWEEN @Age_start AND @Age_end
    /// </example>
    public QueryWrapper<T> Between<TProperty>(Expression<Func<T, TProperty>> expression, TProperty start, TProperty end, LogicalOperatorType logical = LogicalOperatorType.And)
    {
        string fieldName = GetFieldName(expression);
        string startKey = GenerateUniqueParamKey($"{fieldName}_start");
        string endKey = GenerateUniqueParamKey($"{fieldName}_end");

        if (conditionBuilder.Length > 0) {
            string logicalOp = logical == LogicalOperatorType.And ? "AND" : "OR";
            conditionBuilder.Append($" {logicalOp} ");
        }

        conditionBuilder.Append($"({fieldName} BETWEEN {startKey} AND {endKey})");
        Values[startKey] = start!;
        Values[endKey] = end!;
        return this;
    }

    /// <summary>
    /// NOT BETWEEN 条件（字段值不在指定区间内）
    /// </summary>
    /// <param name="expression">字段选择表达式</param>
    /// <param name="start">区间起始值</param>
    /// <param name="end">区间结束值</param>
    /// <param name="logical">逻辑操作符，默认为 AND</param>
    /// <example>
    /// <code>
    /// wrapper.NotBetween(x => x.Score, 0, 59)
    /// </code>
    /// 生成的SQL：WHERE Score NOT BETWEEN @Score_start AND @Score_end
    /// </example>
    public QueryWrapper<T> NotBetween<TProperty>(Expression<Func<T, TProperty>> expression, TProperty start, TProperty end, LogicalOperatorType logical = LogicalOperatorType.And)
    {
        string fieldName = GetFieldName(expression);
        string startKey = GenerateUniqueParamKey($"{fieldName}_start");
        string endKey = GenerateUniqueParamKey($"{fieldName}_end");

        if (conditionBuilder.Length > 0) {
            string logicalOp = logical == LogicalOperatorType.And ? "AND" : "OR";
            conditionBuilder.Append($" {logicalOp} ");
        }

        conditionBuilder.Append($"({fieldName} NOT BETWEEN {startKey} AND {endKey})");
        Values[startKey] = start!;
        Values[endKey] = end!;
        return this;
    }

    #endregion

    #region NULL 判断

    /// <summary>
    /// IS NULL 条件（字段为空）
    /// </summary>
    /// <param name="expression">字段选择表达式</param>
    /// <param name="logical">逻辑操作符，默认为 AND</param>
    /// <example>
    /// <code>
    /// wrapper.IsNull(x => x.DeletedAt)
    /// </code>
    /// 生成的SQL：WHERE DeletedAt IS NULL
    /// </example>
    public QueryWrapper<T> IsNull<TProperty>(Expression<Func<T, TProperty>> expression, LogicalOperatorType logical = LogicalOperatorType.And)
    {
        string fieldName = GetFieldName(expression);

        if (conditionBuilder.Length > 0) {
            string logicalOp = logical == LogicalOperatorType.And ? "AND" : "OR";
            conditionBuilder.Append($" {logicalOp} ");
        }

        conditionBuilder.Append($"({fieldName} IS NULL)");
        return this;
    }

    /// <summary>
    /// IS NOT NULL 条件（字段不为空）
    /// </summary>
    /// <param name="expression">字段选择表达式</param>
    /// <param name="logical">逻辑操作符，默认为 AND</param>
    /// <example>
    /// <code>
    /// wrapper.IsNotNull(x => x.UpdatedAt)
    /// </code>
    /// 生成的SQL：WHERE UpdatedAt IS NOT NULL
    /// </example>
    public QueryWrapper<T> IsNotNull<TProperty>(Expression<Func<T, TProperty>> expression, LogicalOperatorType logical = LogicalOperatorType.And)
    {
        string fieldName = GetFieldName(expression);

        if (conditionBuilder.Length > 0) {
            string logicalOp = logical == LogicalOperatorType.And ? "AND" : "OR";
            conditionBuilder.Append($" {logicalOp} ");
        }

        conditionBuilder.Append($"({fieldName} IS NOT NULL)");
        return this;
    }

    #endregion

    #region 分组条件

    /// <summary>
    /// 条件分组（用括号包裹一组条件）
    /// </summary>
    /// <param name="action">分组内的条件构建动作</param>
    /// <param name="logical">与外层条件的逻辑关系，默认为 AND</param>
    /// <example>
    /// <code>
    /// wrapper.Eq(x => x.Status, "active")
    ///        .Group(sub => sub.Eq(x => x.Type, "A").Or().Eq(x => x.Type, "B"));
    /// </code>
    /// 生成的SQL：WHERE Status = @Status AND (Type = @Type OR Type = @Type_1)
    /// </example>
    public QueryWrapper<T> Group(Action<QueryWrapper<T>> action, LogicalOperatorType logical = LogicalOperatorType.And)
    {
        var subWrapper = new QueryWrapper<T> {
            Values = Values
        };
        action.Invoke(subWrapper);
        if (subWrapper.conditionBuilder.Length > 0) {
            if (conditionBuilder.Length > 0) {
                string logicalOp = logical == LogicalOperatorType.And ? "AND" : "OR";
                conditionBuilder.Append($" {logicalOp} ");
            }

            conditionBuilder.Append($"({subWrapper.conditionBuilder})");
        }

        return this;
    }

    /// <summary>
    /// OR 逻辑操作符的便捷方法
    /// </summary>
    /// <returns>返回当前 QueryWrapper 实例</returns>
    /// <example>
    /// <code>
    /// wrapper.Eq(x => x.Status, "active").Or().Eq(x => x.Status, "pending")
    /// </code>
    /// </example>
    public QueryWrapper<T> Or()
    {
        // 这是一个标记方法，实际的 OR 逻辑在下一个条件方法中处理
        return this;
    }

    #endregion

    #region 排序

    /// <summary>
    /// 升序排序（按指定字段升序排列结果）
    /// </summary>
    /// <param name="expression">要排序的字段表达式</param>
    /// <returns>返回当前 QueryWrapper 实例，支持链式调用</returns>
    /// <example>
    /// <code>
    /// wrapper.OrderBy(x => x.CreateTime)
    ///        .OrderBy(x => x.Id)  // 多字段排序
    /// </code>
    /// 生成的SQL：ORDER BY CreateTime ASC, Id ASC
    /// </example>
    public QueryWrapper<T> OrderBy<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        string fieldName = GetFieldName(expression);
        if (orderByBuilder.Length > 0) {
            orderByBuilder.Append(", ");
        }
        orderByBuilder.Append($"{fieldName} ASC");
        return this;
    }

    /// <summary>
    /// 降序排序（按指定字段降序排列结果）
    /// </summary>
    /// <param name="expression">要排序的字段表达式</param>
    /// <returns>返回当前 QueryWrapper 实例，支持链式调用</returns>
    /// <example>
    /// <code>
    /// wrapper.OrderByDesc(x => x.UpdateTime)
    ///        .OrderBy(x => x.Priority)  // 混合排序
    /// </code>
    /// 生成的SQL：ORDER BY UpdateTime DESC, Priority ASC
    /// </example>
    public QueryWrapper<T> OrderByDesc<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        string fieldName = GetFieldName(expression);
        if (orderByBuilder.Length > 0) {
            orderByBuilder.Append(", ");
        }
        orderByBuilder.Append($"{fieldName} DESC");
        return this;
    }

    #endregion

    #region 分页

    /// <summary>
    /// 限制查询结果数量（分页查询）
    /// </summary>
    /// <param name="limit">限制返回的记录数量，必须大于0</param>
    /// <param name="offset">跳过的记录数量，默认为0（从第一条记录开始）</param>
    /// <returns>返回当前 QueryWrapper 实例，支持链式调用</returns>
    /// <example>
    /// <code>
    /// // 获取前10条记录
    /// wrapper.Limit(10)
    ///
    /// // 分页查询 - 跳过前20条，获取接下来的10条记录（第3页，每页10条）
    /// wrapper.Limit(10, 20)
    ///
    /// // 结合排序和分页
    /// wrapper.OrderByDesc(x => x.CreateTime)
    ///        .Limit(20, 0)  // 获取最新的20条记录
    /// </code>
    /// 生成的SQL：LIMIT 10 OFFSET 20
    /// </example>
    public QueryWrapper<T> Limit(int limit, int offset = 0)
    {
        if (limit <= 0) {
            throw new ArgumentException("Limit must be greater than 0", nameof(limit));
        }

        if (offset < 0) {
            throw new ArgumentException("Offset must be greater than or equal to 0", nameof(offset));
        }

        LimitSql = offset > 0 ? $" limit {limit} offset {offset} " : $" limit {limit} ";
        return this;
    }

    #endregion


    /// <summary>
    /// 查询
    /// </summary>
    /// <returns></returns>
    public List<T> Select()
    {
        var type = typeof(T);
        var tableInfo = type.GetCustomAttribute<TableInfoAttribute>() ?? throw new Exception("未找到TableInfo!!");
        var sql = $"select * from {tableInfo.TableName} {BuildSql()}";
        using var connect = new SqliteConnection($"DataSource={tableInfo.DataSource}");
        return connect.Query<T>(sql, Values).ToList();
    }
}

/// <summary>
/// 更新条件查询
/// </summary>
public class UpdateQueryWrapper <T>: QueryWrapper<T> where T : new() {

    /// <summary>
    /// 构建set sql
    /// </summary>
    private readonly StringBuilder setClauseBuilder = new();

    /// <summary>
    /// 更新sql
    /// </summary>
    private string SetSql => setClauseBuilder.ToString();

    /// <summary>
    /// 更新 某个字段
    /// </summary>
    /// <param name="field">字段名</param>
    /// <example>
    /// <code>
    /// xxx.Set(p.ComId, 1)
    /// </code>
    /// 生成的SQL类似：set ComId = 1
    /// </example>
    public UpdateQueryWrapper<T> Set<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value)
    {
        string fieldName = GetFieldName(expression);
        string key = GenerateUniqueParamKey(fieldName);

        if (setClauseBuilder.Length != 0) {
            setClauseBuilder.Append(',');
        }

        setClauseBuilder.Append($"{fieldName} = {key}");
        Values[key] = value!;

        return this;
    }

    // public override QuerySqlRequest CreateQueryRequest()
    // {
    //     var request = base.CreateQueryRequest();
    //     request.SetClause = SetSql;
    //     return request;
    // }

    public static UpdateQueryWrapper<T> BuilderUpdate()
    {
        return new UpdateQueryWrapper<T> {
            Values = new Dictionary<string, object>()
        };
    }
}
