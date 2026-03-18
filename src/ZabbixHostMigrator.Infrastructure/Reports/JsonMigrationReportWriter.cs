using System.Text.Json;
using ZabbixHostMigrator.Application.Abstractions.Services;
using ZabbixHostMigrator.Domain.ValueObjects;

namespace ZabbixHostMigrator.Infrastructure.Reports;

public class JsonMigrationReportWriter : IMigrationReportWriter
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    WriteIndented = true
  };

  public async Task<string> WriteAsync(MigrationReport report, CancellationToken cancellationToken = default)
  {
    var baseDirectory = Directory.GetCurrentDirectory();
    var outputDirectory = Path.Combine(baseDirectory, "samples", "outputs");

    Directory.CreateDirectory(outputDirectory);

    var fileName = $"migration-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
    var filePath = Path.Combine(outputDirectory, fileName);

    var json = JsonSerializer.Serialize(report, JsonOptions);
    await File.WriteAllTextAsync(filePath, json, cancellationToken);

    return filePath;
  }
}
