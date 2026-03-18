using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ZabbixHostMigrator.Application.Abstractions.Clients;
using ZabbixHostMigrator.Application.DTOs;
using ZabbixHostMigrator.Domain.Entities;

namespace ZabbixHostMigrator.Infrastructure.Clients;

public class ZabbixApiClient : IZabbixApiClient
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true
  };

  private readonly HttpClient _httpClient;
  private readonly ILogger<ZabbixApiClient> _logger;

  public ZabbixApiClient(
      HttpClient httpClient,
      ILogger<ZabbixApiClient> logger)
  {
    _httpClient = httpClient;
    _logger = logger;
  }

  public async Task<string> AuthenticateAsync(
      ZabbixInstanceOptions options,
      CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("Authenticating against {Url}", options.Url);

    var result = await SendRequestAsync<string>(
        url: options.Url,
        method: "user.login",
        @params: new
        {
          username = options.Username,
          password = options.Password
        },
        authToken: null,
        cancellationToken: cancellationToken);

    if (string.IsNullOrWhiteSpace(result))
      throw new InvalidOperationException("Zabbix authentication returned an empty token.");

    _logger.LogInformation("Authentication succeeded for {Url}", options.Url);

    return result;
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

  private async Task<T> SendRequestAsync<T>(
      string url,
      string method,
      object @params,
      string? authToken,
      CancellationToken cancellationToken)
  {
    var request = new JsonRpcRequest
    {
      JsonRpc = "2.0",
      Method = method,
      Params = @params,
      Id = 1,
      Auth = authToken
    };

    using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
    {
      Content = new StringContent(
            JsonSerializer.Serialize(request, JsonOptions),
            Encoding.UTF8,
            "application/json")
    };

    using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

    var content = await response.Content.ReadAsStringAsync(cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
      throw new HttpRequestException(
          $"Zabbix API request failed with status code {(int)response.StatusCode}: {content}");
    }

    var rpcResponse = JsonSerializer.Deserialize<JsonRpcResponse<T>>(content, JsonOptions);

    if (rpcResponse is null)
      throw new InvalidOperationException("Could not deserialize Zabbix API response.");

    if (rpcResponse.Error is not null)
    {
      throw new InvalidOperationException(
          $"Zabbix API error {rpcResponse.Error.Code}: {rpcResponse.Error.Message}. {rpcResponse.Error.Data}");
    }

    if (rpcResponse.Result is null)
      throw new InvalidOperationException("Zabbix API response did not contain a result.");

    return rpcResponse.Result;
  }

  private sealed class JsonRpcRequest
  {
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";

    [JsonPropertyName("method")]
    public string Method { get; init; } = string.Empty;

    [JsonPropertyName("params")]
    public object Params { get; init; } = default!;

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("auth")]
    public string? Auth { get; init; }
  }

  private sealed class JsonRpcResponse<T>
  {
    [JsonPropertyName("jsonrpc")]
    public string? JsonRpc { get; init; }

    [JsonPropertyName("result")]
    public T? Result { get; init; }

    [JsonPropertyName("error")]
    public JsonRpcError? Error { get; init; }

    [JsonPropertyName("id")]
    public int Id { get; init; }
  }

  private sealed class JsonRpcError
  {
    [JsonPropertyName("code")]
    public int Code { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("data")]
    public string? Data { get; init; }
  }
}
