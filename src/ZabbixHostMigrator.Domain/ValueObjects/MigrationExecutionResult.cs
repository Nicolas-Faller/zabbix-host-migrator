namespace ZabbixHostMigrator.Domain.ValueObjects;

public sealed record MigrationExecutionResult(
    int TotalRead,
    int TotalEligible,
    int TotalMigrated,
    int TotalSkipped,
    int TotalFailed);
