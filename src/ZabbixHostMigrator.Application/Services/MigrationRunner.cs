using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZabbixHostMigrator.Application.Abstractions.Clients;
using ZabbixHostMigrator.Application.Abstractions.Services;
using ZabbixHostMigrator.Application.DTOs;
using ZabbixHostMigrator.Domain.ValueObjects;

namespace ZabbixHostMigrator.Application.Services;

public class MigrationRunner : IMigrationRunner
{
  private readonly ILogger<MigrationRunner> _logger;
  private readonly IZabbixApiClient _zabbixApiClient;
  private readonly IMigrationReportWriter _migrationReportWriter;
  private readonly IOptionsMonitor<ZabbixInstanceOptions> _zabbixOptionsMonitor;
  private readonly IOptions<MigrationOptions> _migrationOptions;

  public MigrationRunner(
      ILogger<MigrationRunner> logger,
      IZabbixApiClient zabbixApiClient,
      IMigrationReportWriter migrationReportWriter,
      IOptionsMonitor<ZabbixInstanceOptions> zabbixOptionsMonitor,
      IOptions<MigrationOptions> migrationOptions)
  {
    _logger = logger;
    _zabbixApiClient = zabbixApiClient;
    _migrationReportWriter = migrationReportWriter;
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

    var results = new List<MigrationHostResult>();
    var totalMigrated = 0;
    var totalSkipped = 0;
    var totalFailed = 0;

    foreach (var host in hosts)
    {
      try
      {
        var existsInDestination = false;

        if (migration.SkipIfHostExists)
        {
          existsInDestination = await _zabbixApiClient.HostExistsAsync(
              destination,
              destinationToken,
              host.Host,
              cancellationToken);
        }

        if (existsInDestination)
        {
          totalSkipped++;

          results.Add(new MigrationHostResult(
              Host: host.Host,
              VisibleName: host.VisibleName,
              Action: "Skipped",
              Success: true,
              Message: "Host already exists in destination.",
              CreatedHostId: null));

          continue;
        }

        if (migration.DryRun)
        {
          results.Add(new MigrationHostResult(
              Host: host.Host,
              VisibleName: host.VisibleName,
              Action: "WouldCreate",
              Success: true,
              Message: "Dry run enabled. Host was not created.",
              CreatedHostId: null));

          continue;
        }

        var createdHostId = await _zabbixApiClient.CreateHostAsync(
            destination,
            destinationToken,
            host,
            migration.DestinationGroupName,
            cancellationToken);

        totalMigrated++;

        results.Add(new MigrationHostResult(
            Host: host.Host,
            VisibleName: host.VisibleName,
            Action: "Migrated",
            Success: true,
            Message: "Host created in destination.",
            CreatedHostId: createdHostId));
      }
      catch (Exception ex)
      {
        totalFailed++;

        results.Add(new MigrationHostResult(
            Host: host.Host,
            VisibleName: host.VisibleName,
            Action: "Failed",
            Success: false,
            Message: ex.Message,
            CreatedHostId: null));

        _logger.LogError(ex, "Failed to process host {Host}", host.Host);
      }
    }

    var summary = new MigrationExecutionResult(
        TotalRead: hosts.Count,
        TotalEligible: hosts.Count - totalSkipped,
        TotalMigrated: totalMigrated,
        TotalSkipped: totalSkipped,
        TotalFailed: totalFailed);

    var report = new MigrationReport(
        SourceUrl: source.Url,
        DestinationUrl: destination.Url,
        DryRun: migration.DryRun,
        GeneratedAtUtc: DateTime.UtcNow,
        Summary: summary,
        Items: results);

    var reportPath = await _migrationReportWriter.WriteAsync(report, cancellationToken);

    _logger.LogInformation("Migration summary:");
    _logger.LogInformation("TotalRead: {TotalRead}", summary.TotalRead);
    _logger.LogInformation("TotalEligible: {TotalEligible}", summary.TotalEligible);
    _logger.LogInformation("TotalMigrated: {TotalMigrated}", summary.TotalMigrated);
    _logger.LogInformation("TotalSkipped: {TotalSkipped}", summary.TotalSkipped);
    _logger.LogInformation("TotalFailed: {TotalFailed}", summary.TotalFailed);
    _logger.LogInformation("Report saved at: {ReportPath}", reportPath);
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
