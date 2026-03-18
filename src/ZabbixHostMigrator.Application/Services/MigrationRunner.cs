using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZabbixHostMigrator.Application.Abstractions.Services;
using ZabbixHostMigrator.Application.DTOs;

namespace ZabbixHostMigrator.Application.Services;

public class MigrationRunner : IMigrationRunner
{
  private readonly ILogger<MigrationRunner> _logger;
  private readonly IOptionsMonitor<ZabbixInstanceOptions> _zabbixOptionsMonitor;
  private readonly IOptions<MigrationOptions> _migrationOptions;

  public MigrationRunner(
      ILogger<MigrationRunner> logger,
      IOptionsMonitor<ZabbixInstanceOptions> zabbixOptionsMonitor,
      IOptions<MigrationOptions> migrationOptions)
  {
    _logger = logger;
    _zabbixOptionsMonitor = zabbixOptionsMonitor;
    _migrationOptions = migrationOptions;
  }

  public Task RunAsync(CancellationToken cancellationToken = default)
  {
    var source = _zabbixOptionsMonitor.Get("Source");
    var destination = _zabbixOptionsMonitor.Get("Destination");
    var migration = _migrationOptions.Value;

    ValidateInstance(source, "Source");
    ValidateInstance(destination, "Destination");

    _logger.LogInformation("Zabbix Host Migrator started.");
    _logger.LogInformation("Source URL: {Url}", source.Url);
    _logger.LogInformation("Destination URL: {Url}", destination.Url);
    _logger.LogInformation("SourceGroupName: {SourceGroupName}", migration.SourceGroupName);
    _logger.LogInformation("DestinationGroupName: {DestinationGroupName}", migration.DestinationGroupName);
    _logger.LogInformation("HostNameContains: {HostNameContains}", migration.HostNameContains);
    _logger.LogInformation("SkipIfHostExists: {SkipIfHostExists}", migration.SkipIfHostExists);
    _logger.LogInformation("DryRun: {DryRun}", migration.DryRun);
    _logger.LogInformation("Base project setup completed successfully.");

    return Task.CompletedTask;
  }

  private static void ValidateInstance(ZabbixInstanceOptions options, string sectionName)
  {
    if (string.IsNullOrWhiteSpace(options.Url))
      throw new InvalidOperationException($"{sectionName}:Url is required.");

    if (string.IsNullOrWhiteSpace(options.Username))
      throw new InvalidOperationException($"{sectionName}:Username is required.");

    if (string.IsNullOrWhiteSpace(options.Password))
      throw new InvalidOperationException($"{sectionName}:Password is required.");
  }
}
