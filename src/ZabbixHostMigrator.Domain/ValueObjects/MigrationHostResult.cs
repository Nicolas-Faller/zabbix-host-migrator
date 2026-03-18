namespace ZabbixHostMigrator.Domain.ValueObjects;

public sealed record MigrationHostResult(
    string Host,
    string VisibleName,
    string Action,
    bool Success,
    string? Message,
    string? CreatedHostId);
