// ReSharper disable InconsistentNaming

using LoveYuri.Core.Sqlite;

namespace LoveYuri.Examples.Entity;

[TableInfo("ConfigDB.s3db", nameof(SysComponents))]
public class SysComponents {
    public int ComID { get; set; }
    public bool Enabled { get; set; }
    public string ComName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ComTypeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
