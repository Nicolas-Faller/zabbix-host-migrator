using ZabbixHostMigrator.Domain.ValueObjects;

namespace ZabbixHostMigrator.Application.Abstractions.Services;

public interface IMigrationReportWriter
{
  Task<string> WriteAsync(MigrationReport report, CancellationToken cancellationToken = default);
}
