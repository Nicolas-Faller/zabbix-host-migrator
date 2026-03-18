using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZabbixHostMigrator.Application.Abstractions.Clients;
using ZabbixHostMigrator.Application.Abstractions.Services;
using ZabbixHostMigrator.Application.DTOs;

namespace ZabbixHostMigrator.Application.Services;

public class MigrationRunner : IMigrationRunner
{
  private readonly ILogger<MigrationRunner> _logger;
  private readonly IZabbixApiClient _zabbixApiClient;
  private readonly IOptionsMonitor<ZabbixInstanceOptions> _zabbixOptionsMonitor;
  private readonly IOptions<MigrationOptions> _migrationOptions;

  public MigrationRunner(
      ILogger<MigrationRunner> logger,
      IZabbixApiClient zabbixApiClient,
      IOptionsMonitor<ZabbixInstanceOptions> zabbixOptionsMonitor,
      IOptions<MigrationOptions> migrationOptions)
  {
    _logger = logger;
    _zabbixApiClient = zabbixApiClient;
    _zabbixOptionsMonitor = zabbixOptionsMonitor;
    _migrationOptions = migrationOptions;
  }

  public async Task RunAsync(CancellationToken cancellationToken = default)
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

    var sourceToken = await _zabbixApiClient.AuthenticateAsync(source, cancellationToken);
    var destinationToken = await _zabbixApiClient.AuthenticateAsync(destination, cancellationToken);

    _logger.LogInformation(
        "Source authentication succeeded. Token length: {Length}",
        sourceToken.Length);

    _logger.LogInformation(
        "Destination authentication succeeded. Token length: {Length}",
        destinationToken.Length);

    var hosts = await _zabbixApiClient.GetHostsAsync(
        source,
        sourceToken,
        migration.SourceGroupName,
        migration.HostNameContains,
        cancellationToken);

    _logger.LogInformation("Retrieved {Count} source hosts.", hosts.Count);

    foreach (var host in hosts.Take(5))
    {
      _logger.LogInformation(
          "Host: {Host} | VisibleName: {VisibleName} | Groups: {GroupCount} | Interfaces: {InterfaceCount} | Tags: {TagCount}",
          host.Host,
          host.VisibleName,
          host.Groups.Count,
          host.Interfaces.Count,
          host.Tags.Count);
    }

    _logger.LogInformation("Host retrieval step completed successfully.");
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
