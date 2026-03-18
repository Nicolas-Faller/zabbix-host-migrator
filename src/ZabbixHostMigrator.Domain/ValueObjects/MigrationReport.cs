namespace ZabbixHostMigrator.Domain.ValueObjects;

public sealed record MigrationReport(
    string SourceUrl,
    string DestinationUrl,
    bool DryRun,
    DateTime GeneratedAtUtc,
    MigrationExecutionResult Summary,
    IReadOnlyList<MigrationHostResult> Items);
