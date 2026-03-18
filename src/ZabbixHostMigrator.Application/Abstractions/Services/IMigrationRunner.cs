namespace ZabbixHostMigrator.Application.Abstractions.Services;

public interface IMigrationRunner
{
  Task RunAsync(CancellationToken cancellationToken = default);
}
