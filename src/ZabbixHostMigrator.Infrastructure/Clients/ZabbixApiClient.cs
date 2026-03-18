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
    if (options.UseMock)
    {
      _logger.LogInformation("Mock authentication enabled for {Url}", options.Url);
      await Task.Delay(50, cancellationToken);
      return $"mock-token-{Guid.NewGuid():N}";
    }

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

  public async Task<IReadOnlyList<ZabbixHost>> GetHostsAsync(
      ZabbixInstanceOptions options,
      string authToken,
      string? sourceGroupName,
      string? hostNameContains,
      CancellationToken cancellationToken = default)
  {
    if (options.UseMock)
    {
      _logger.LogInformation("Mock host retrieval enabled for {Url}", options.Url);

      var mockHosts = CreateMockHosts();

      if (!string.IsNullOrWhiteSpace(sourceGroupName))
      {
        mockHosts = mockHosts
            .Where(x => x.Groups.Any(g =>
                string.Equals(g.Name, sourceGroupName, StringComparison.OrdinalIgnoreCase)))
            .ToList();
      }

      if (!string.IsNullOrWhiteSpace(hostNameContains))
      {
        var term = hostNameContains.Trim();

        mockHosts = mockHosts
            .Where(x =>
                x.Host.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.VisibleName.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();
      }

      await Task.Delay(100, cancellationToken);
      return mockHosts;
    }

    _logger.LogInformation("Retrieving hosts from {Url}", options.Url);

    var rawHosts = await SendRequestAsync<List<HostGetResult>>(
        url: options.Url,
        method: "host.get",
        @params: new
        {
          output = new[] { "hostid", "host", "name", "status" },
          selectHostGroups = new[] { "groupid", "name" },
          selectInterfaces = new[] { "type", "ip", "dns", "port", "useip", "main" },
          selectTags = "extend"
        },
        authToken: authToken,
        cancellationToken: cancellationToken);

    var mappedHosts = rawHosts
        .Select(MapHost)
        .ToList();

    if (!string.IsNullOrWhiteSpace(sourceGroupName))
    {
      mappedHosts = mappedHosts
          .Where(x => x.Groups.Any(g =>
              string.Equals(g.Name, sourceGroupName, StringComparison.OrdinalIgnoreCase)))
          .ToList();
    }

    if (!string.IsNullOrWhiteSpace(hostNameContains))
    {
      var term = hostNameContains.Trim();

      mappedHosts = mappedHosts
          .Where(x =>
              x.Host.Contains(term, StringComparison.OrdinalIgnoreCase) ||
              x.VisibleName.Contains(term, StringComparison.OrdinalIgnoreCase))
          .ToList();
    }

    _logger.LogInformation("Retrieved {Count} hosts after filtering.", mappedHosts.Count);

    return mappedHosts;
  }

  public async Task<bool> HostExistsAsync(
      ZabbixInstanceOptions options,
      string authToken,
      string hostName,
      CancellationToken cancellationToken = default)
  {
    if (options.UseMock)
    {
      await Task.Delay(30, cancellationToken);

      var existingHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "db-prod-01"
            };

      var exists = existingHosts.Contains(hostName);

      _logger.LogInformation(
          "Mock destination lookup for {HostName}: Exists = {Exists}",
          hostName,
          exists);

      return exists;
    }

    _logger.LogInformation("Checking if host exists in destination: {HostName}", hostName);

    var rawHosts = await SendRequestAsync<List<HostExistsResult>>(
        url: options.Url,
        method: "host.get",
        @params: new
        {
          output = new[] { "hostid", "host" },
          filter = new
          {
            host = new[] { hostName }
          }
        },
        authToken: authToken,
        cancellationToken: cancellationToken);

    return rawHosts.Count > 0;
  }

  public async Task<string?> CreateHostAsync(
      ZabbixInstanceOptions options,
      string authToken,
      ZabbixHost host,
      string? destinationGroupName,
      CancellationToken cancellationToken = default)
  {
    if (options.UseMock)
    {
      await Task.Delay(80, cancellationToken);

      var createdId = Random.Shared.Next(50000, 99999).ToString();

      _logger.LogInformation(
          "Mock host created in destination: {Host} -> {CreatedId}",
          host.Host,
          createdId);

      return createdId;
    }

    _logger.LogInformation("CreateHostAsync called for {Host}", host.Host);
    throw new NotImplementedException("Real host creation will be implemented in a later step.");
  }

  private static List<ZabbixHost> CreateMockHosts()
  {
    return new List<ZabbixHost>
        {
            new(
                HostId: "10101",
                Host: "web-prod-01",
                VisibleName: "Web Prod 01",
                Status: 0,
                Groups: new List<ZabbixHostGroup>
                {
                    new("10", "Linux Servers")
                },
                Interfaces: new List<ZabbixHostInterface>
                {
                    new(1, "10.0.0.11", "", "10050", true, true)
                },
                Tags: new List<ZabbixHostTag>
                {
                    new("env", "prod"),
                    new("role", "web")
                }),

            new(
                HostId: "10102",
                Host: "db-prod-01",
                VisibleName: "Database Prod 01",
                Status: 0,
                Groups: new List<ZabbixHostGroup>
                {
                    new("10", "Linux Servers")
                },
                Interfaces: new List<ZabbixHostInterface>
                {
                    new(1, "10.0.0.21", "", "10050", true, true)
                },
                Tags: new List<ZabbixHostTag>
                {
                    new("env", "prod"),
                    new("role", "database")
                }),

            new(
                HostId: "10103",
                Host: "web-dev-01",
                VisibleName: "Web Dev 01",
                Status: 0,
                Groups: new List<ZabbixHostGroup>
                {
                    new("10", "Linux Servers")
                },
                Interfaces: new List<ZabbixHostInterface>
                {
                    new(1, "10.0.1.31", "", "10050", true, true)
                },
                Tags: new List<ZabbixHostTag>
                {
                    new("env", "dev"),
                    new("role", "web")
                }),

            new(
                HostId: "10104",
                Host: "win-files-01",
                VisibleName: "Windows Files 01",
                Status: 0,
                Groups: new List<ZabbixHostGroup>
                {
                    new("20", "Windows Servers")
                },
                Interfaces: new List<ZabbixHostInterface>
                {
                    new(1, "10.0.2.41", "", "10050", true, true)
                },
                Tags: new List<ZabbixHostTag>
                {
                    new("env", "prod"),
                    new("role", "fileserver")
                })
        };
  }

  private static ZabbixHost MapHost(HostGetResult raw)
  {
    var groups = raw.HostGroups?
        .Select(x => new ZabbixHostGroup(
            x.GroupId ?? string.Empty,
            x.Name ?? string.Empty))
        .ToList()
        ?? new List<ZabbixHostGroup>();

    var interfaces = raw.Interfaces?
        .Select(x => new ZabbixHostInterface(
            Type: ParseInt(x.Type),
            Ip: x.Ip ?? string.Empty,
            Dns: x.Dns ?? string.Empty,
            Port: x.Port ?? string.Empty,
            UseIp: ParseBool(x.UseIp),
            Main: ParseBool(x.Main)))
        .ToList()
        ?? new List<ZabbixHostInterface>();

    var tags = raw.Tags?
        .Select(x => new ZabbixHostTag(
            x.Tag ?? string.Empty,
            x.Value ?? string.Empty))
        .ToList()
        ?? new List<ZabbixHostTag>();

    return new ZabbixHost(
        HostId: raw.HostId ?? string.Empty,
        Host: raw.Host ?? string.Empty,
        VisibleName: string.IsNullOrWhiteSpace(raw.Name) ? raw.Host ?? string.Empty : raw.Name,
        Status: ParseInt(raw.Status),
        Groups: groups,
        Interfaces: interfaces,
        Tags: tags);
  }

  private static int ParseInt(string? value)
  {
    return int.TryParse(value, out var parsed) ? parsed : 0;
  }

  private static bool ParseBool(string? value)
  {
    return value == "1";
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

  private sealed class HostGetResult
  {
    [JsonPropertyName("hostid")]
    public string? HostId { get; init; }

    [JsonPropertyName("host")]
    public string? Host { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("hostgroups")]
    public List<HostGroupResult>? HostGroups { get; init; }

    [JsonPropertyName("interfaces")]
    public List<HostInterfaceResult>? Interfaces { get; init; }

    [JsonPropertyName("tags")]
    public List<HostTagResult>? Tags { get; init; }
  }

  private sealed class HostExistsResult
  {
    [JsonPropertyName("hostid")]
    public string? HostId { get; init; }

    [JsonPropertyName("host")]
    public string? Host { get; init; }
  }

  private sealed class HostGroupResult
  {
    [JsonPropertyName("groupid")]
    public string? GroupId { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
  }

  private sealed class HostInterfaceResult
  {
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("ip")]
    public string? Ip { get; init; }

    [JsonPropertyName("dns")]
    public string? Dns { get; init; }

    [JsonPropertyName("port")]
    public string? Port { get; init; }

    [JsonPropertyName("useip")]
    public string? UseIp { get; init; }

    [JsonPropertyName("main")]
    public string? Main { get; init; }
  }

  private sealed class HostTagResult
  {
    [JsonPropertyName("tag")]
    public string? Tag { get; init; }

    [JsonPropertyName("value")]
    public string? Value { get; init; }
  }
}
