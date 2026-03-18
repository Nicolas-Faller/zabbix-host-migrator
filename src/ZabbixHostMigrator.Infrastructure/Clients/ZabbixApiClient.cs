using Microsoft.Extensions.Logging;
using ZabbixHostMigrator.Application.Abstractions.Clients;
using ZabbixHostMigrator.Application.DTOs;
using ZabbixHostMigrator.Domain.Entities;

namespace ZabbixHostMigrator.Infrastructure.Clients;

public class ZabbixApiClient : IZabbixApiClient
{
  private readonly HttpClient _httpClient;
  private readonly ILogger<ZabbixApiClient> _logger;

  public ZabbixApiClient(
      HttpClient httpClient,
      ILogger<ZabbixApiClient> logger)
  {
    _httpClient = httpClient;
    _logger = logger;
  }

  public Task<string> AuthenticateAsync(
      ZabbixInstanceOptions options,
      CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("AuthenticateAsync called for {Url}", options.Url);
    throw new NotImplementedException("Authentication will be implemented in the next step.");
  }

  public Task<IReadOnlyList<ZabbixHost>> GetHostsAsync(
      ZabbixInstanceOptions options,
      string authToken,
      string? sourceGroupName,
      string? hostNameContains,
      CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("GetHostsAsync called for {Url}", options.Url);
    throw new NotImplementedException("Host retrieval will be implemented in the next step.");
  }

  public Task<bool> HostExistsAsync(
      ZabbixInstanceOptions options,
      string authToken,
      string hostName,
      CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("HostExistsAsync called for {HostName}", hostName);
    throw new NotImplementedException("Destination lookup will be implemented in the next step.");
  }

  public Task<string?> CreateHostAsync(
      ZabbixInstanceOptions options,
      string authToken,
      ZabbixHost host,
      string? destinationGroupName,
      CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("CreateHostAsync called for {Host}", host.Host);
    throw new NotImplementedException("Host creation will be implemented in the next step.");
  }
}
