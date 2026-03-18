namespace ZabbixHostMigrator.Application.DTOs;

public sealed class MigrationOptions
{
  public string? SourceGroupName { get; init; }
  public string? DestinationGroupName { get; init; }
  public string? HostNameContains { get; init; }
  public bool SkipIfHostExists { get; init; } = true;
  public bool DryRun { get; init; } = true;
}
