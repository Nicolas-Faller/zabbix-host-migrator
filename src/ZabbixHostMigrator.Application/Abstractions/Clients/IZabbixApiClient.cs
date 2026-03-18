using ZabbixHostMigrator.Application.DTOs;
using ZabbixHostMigrator.Domain.Entities;

namespace ZabbixHostMigrator.Application.Abstractions.Clients;

public interface IZabbixApiClient
{
  Task<string> AuthenticateAsync(
      ZabbixInstanceOptions options,
      CancellationToken cancellationToken = default);

  Task<IReadOnlyList<ZabbixHost>> GetHostsAsync(
      ZabbixInstanceOptions options,
      string authToken,
      string? sourceGroupName,
      string? hostNameContains,
      CancellationToken cancellationToken = default);

  Task<bool> HostExistsAsync(
      ZabbixInstanceOptions options,
      string authToken,
      string hostName,
      CancellationToken cancellationToken = default);

  Task<string?> CreateHostAsync(
      ZabbixInstanceOptions options,
      string authToken,
      ZabbixHost host,
      string? destinationGroupName,
      CancellationToken cancellationToken = default);
}
