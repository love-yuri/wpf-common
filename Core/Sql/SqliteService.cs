using System.Collections;
using System.Reflection;
using System.Collections.Concurrent;
using Dapper;
using Microsoft.Data.Sqlite;

// ReSharper disable MemberCanBePrivate.Global

namespace LoveYuri.Core.Sql;

public static class SqliteService
{
    // 缓存表信息以避免重复反射
    private static readonly ConcurrentDictionary<Type, (string DataSource, string TableName)> TableInfoCache = new();

    // 缓存属性信息以避免重复反射
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    // 缓存SQL语句以避免重复构建
    private static readonly ConcurrentDictionary<Type, string> InsertSqlCache = new();

    /// <summary>
    /// 获取表信息（带缓存）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static (string DataSource, string TableName) GetTableInfo<T>()
    {
        var type = typeof(T);
        return TableInfoCache.GetOrAdd(type, t =>
        {
            var tableInfo = t.GetCustomAttribute<TableInfoAttribute>()
                ?? throw new InvalidOperationException($"类型 {t.Name} 未找到 TableInfoAttribute");
            return (tableInfo.DataSource, tableInfo.TableName);
        });
    }

    /// <summary>
    /// 获取类型的属性信息（带缓存）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static PropertyInfo[] GetProperties<T>()
    {
        return PropertyCache.GetOrAdd(typeof(T),
            type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                       .Where(p => p is { CanRead: true, CanWrite: true })
                       .ToArray());
    }

    /// <summary>
    /// 创建数据库连接
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    private static SqliteConnection CreateConnection(string dataSource)
    {
        var connectionString = $"DataSource={dataSource};Cache=Shared;";
        return new SqliteConnection(connectionString);
    }

    /// <summary>
    /// 查询所有数据
    /// </summary>
    public static List<T> Select<T>(this QueryWrapper<T> queryWrapper) where T : class, new()
    {
        (string dataSource, string tableName) = GetTableInfo<T>();
        var sql = $"SELECT * FROM {tableName} {queryWrapper.BuildSql()}";

        using var connection = CreateConnection(dataSource);
        return connection.Query<T>(sql, queryWrapper.Values).AsList();
    }

    /// <summary>
    /// 查询所有数据（异步）
    /// </summary>
    public static async Task<List<T>> SelectAsync<T>(this QueryWrapper<T> queryWrapper) where T : class, new()
    {
        (string dataSource, string tableName) = GetTableInfo<T>();
        var sql = $"SELECT * FROM {tableName} {queryWrapper.BuildSql()}";

        await using var connection = CreateConnection(dataSource);
        var result = await connection.QueryAsync<T>(sql, queryWrapper.Values);
        return result.AsList();
    }

    /// <summary>
    /// 查询单个数据
    /// </summary>
    public static T? SelectOne<T>(this QueryWrapper<T> queryWrapper) where T : class, new()
    {
        (string dataSource, string tableName) = GetTableInfo<T>();
        var sql = $"SELECT * FROM {tableName} {queryWrapper.BuildSql()} LIMIT 1";

        using var connection = CreateConnection(dataSource);
        return connection.QueryFirstOrDefault<T>(sql, queryWrapper.Values);
    }

    /// <summary>
    /// 查询单个数据（异步）
    /// </summary>
    public static async Task<T?> SelectOneAsync<T>(this QueryWrapper<T> queryWrapper) where T : class, new()
    {
        (string dataSource, string tableName) = GetTableInfo<T>();
        var sql = $"SELECT * FROM {tableName} {queryWrapper.BuildSql()} LIMIT 1";

        await using var connection = CreateConnection(dataSource);
        return await connection.QueryFirstOrDefaultAsync<T>(sql, queryWrapper.Values);
    }

    /// <summary>
    /// 查询数量
    /// </summary>
    public static int Count<T>(this QueryWrapper<T> queryWrapper) where T : class, new()
    {
        (string dataSource, string tableName) = GetTableInfo<T>();
        var sql = $"SELECT COUNT(*) FROM {tableName} {queryWrapper.BuildSql(BuildSqlType.Count)}";

        using var connection = CreateConnection(dataSource);
        return connection.QuerySingle<int>(sql, queryWrapper.Values);
    }

    /// <summary>
    /// 查询数量（异步）
    /// </summary>
    public static async Task<int> CountAsync<T>(this QueryWrapper<T> queryWrapper) where T : class, new()
    {
        (string dataSource, string tableName) = GetTableInfo<T>();
        var sql = $"SELECT COUNT(*) FROM {tableName} {queryWrapper.BuildSql(BuildSqlType.Count)}";

        await using var connection = CreateConnection(dataSource);
        return await connection.QuerySingleAsync<int>(sql, queryWrapper.Values);
    }

    /// <summary>
    /// 删除
    /// </summary>
    public static int Delete<T>(this QueryWrapper<T> queryWrapper) where T : class, new()
    {
        (string dataSource, string tableName) = GetTableInfo<T>();
        var sql = $"DELETE FROM {tableName} {queryWrapper.BuildSql(BuildSqlType.Delete)}";

        using var connection = CreateConnection(dataSource);
        return connection.Execute(sql, queryWrapper.Values);
    }

    /// <summary>
    /// 删除（异步）
    /// </summary>
    public static async Task<int> DeleteAsync<T>(this QueryWrapper<T> queryWrapper) where T : class, new()
    {
        (string dataSource, string tableName) = GetTableInfo<T>();
        var sql = $"DELETE FROM {tableName} {queryWrapper.BuildSql(BuildSqlType.Delete)}";

        await using var connection = CreateConnection(dataSource);
        return await connection.ExecuteAsync(sql, queryWrapper.Values);
    }

    /// <summary>
    /// 更新
    /// </summary>
    public static int Update<T>(this QueryWrapper<T> queryWrapper) where T : class, new()
    {
        if (queryWrapper is not UpdateQueryWrapper<T>)
        {
            throw new ArgumentException("更新操作请使用 UpdateQueryWrapper", nameof(queryWrapper));
        }

        (string dataSource, string tableName) = GetTableInfo<T>();
        var sql = $"UPDATE {tableName} SET {queryWrapper.BuildSql(BuildSqlType.Update)}";

        using var connection = CreateConnection(dataSource);
        return connection.Execute(sql, queryWrapper.Values);
    }

    /// <summary>
    /// 更新（异步）
    /// </summary>
    public static async Task<int> UpdateAsync<T>(this QueryWrapper<T> queryWrapper) where T : class, new()
    {
        if (queryWrapper is not UpdateQueryWrapper<T>)
        {
            throw new ArgumentException("更新操作请使用 UpdateQueryWrapper", nameof(queryWrapper));
        }

        (string dataSource, string tableName) = GetTableInfo<T>();
        var sql = $"UPDATE {tableName} SET {queryWrapper.BuildSql(BuildSqlType.Update)}";

        await using var connection = CreateConnection(dataSource);
        return await connection.ExecuteAsync(sql, queryWrapper.Values);
    }

    /// <summary>
    /// 插入数据（优化版本）
    /// </summary>
    /// <param name="list">待插入的数据</param>
    public static int Insert<T>(this IEnumerable<T> list) where T : class
    {
        var dataList = list as T[] ?? list.ToArray();
        if (dataList.Length == 0) return 0;

        (string dataSource, string tableName) = GetTableInfo<T>();
        string sql = GetInsertSql<T>(tableName);

        using var connection = CreateConnection(dataSource);
        return connection.Execute(sql, dataList);
    }

    /// <summary>
    /// 插入数据（异步优化版本）
    /// </summary>
    /// <param name="list">待插入的数据</param>
    public static async Task<int> InsertAsync<T>(this IEnumerable<T> list) where T : class
    {
        var dataList = list as T[] ?? list.ToArray();
        if (dataList.Length == 0) return 0;

        (string dataSource, string tableName) = GetTableInfo<T>();
        string sql = GetInsertSql<T>(tableName);

        await using var connection = CreateConnection(dataSource);
        return await connection.ExecuteAsync(sql, dataList);
    }

    /// <summary>
    /// 插入单个对象
    /// </summary>
    /// <param name="entity">待插入的对象</param>
    public static int Insert<T>(this T entity) where T : class
    {
        (string dataSource, string tableName) = GetTableInfo<T>();
        string sql = GetInsertSql<T>(tableName);

        using var connection = CreateConnection(dataSource);
        return connection.Execute(sql, entity);
    }

    /// <summary>
    /// 插入单个对象（异步）
    /// </summary>
    /// <param name="entity">待插入的对象</param>
    public static async Task<int> InsertAsync<T>(this T entity) where T : class
    {
        (string dataSource, string tableName) = GetTableInfo<T>();
        string sql = GetInsertSql<T>(tableName);

        await using var connection = CreateConnection(dataSource);
        return await connection.ExecuteAsync(sql, entity);
    }

    /// <summary>
    /// 批量插入（事务处理）
    /// </summary>
    /// <param name="list">待插入的数据</param>
    /// <param name="batchSize">批次大小</param>
    public static int InsertBatch<T>(this IEnumerable<T> list, int batchSize = 1000) where T : class
    {
        var dataList = list as T[] ?? list.ToArray();
        if (dataList.Length == 0) return 0;

        (string dataSource, string tableName) = GetTableInfo<T>();
        string sql = GetInsertSql<T>(tableName);
        var totalAffected = 0;

        using var connection = CreateConnection(dataSource);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try {
            for (var i = 0; i < dataList.Length; i += batchSize) {
                var batch = dataList.Skip(i).Take(batchSize);
                totalAffected += connection.Execute(sql, batch, transaction);
            }

            transaction.Commit();
            return totalAffected;
        } catch {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// 批量插入异步（事务处理）
    /// </summary>
    /// <param name="list">待插入的数据</param>
    /// <param name="batchSize">批次大小</param>
    public static async Task<int> InsertBatchAsync<T>(this IEnumerable<T> list, int batchSize = 1000) where T : class
    {
        var dataList = list as T[] ?? list.ToArray();
        if (dataList.Length == 0) return 0;

        (string dataSource, string tableName) = GetTableInfo<T>();
        string sql = GetInsertSql<T>(tableName);
        var totalAffected = 0;

        await using var connection = CreateConnection(dataSource);
        connection.Open();
        await using var transaction = connection.BeginTransaction();

        try {
            for (var i = 0; i < dataList.Length; i += batchSize) {
                var batch = dataList.Skip(i).Take(batchSize);
                totalAffected += await connection.ExecuteAsync(sql, batch, transaction);
            }

            transaction.Commit();
            return totalAffected;
        } catch {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// 获取插入SQL（带缓存）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <returns></returns>
    private static string GetInsertSql<T>(string tableName)
    {
        return InsertSqlCache.GetOrAdd(typeof(T), _ => {
            var properties = GetProperties<T>();
            string[] fields = properties.Select(p => p.Name).ToArray();

            string fieldsStr = string.Join(", ", fields);
            string valuesStr = string.Join(", ", fields.Select(f => $"@{f}"));

            return $"INSERT INTO {tableName} ({fieldsStr}) VALUES ({valuesStr})";
        });
    }
}
