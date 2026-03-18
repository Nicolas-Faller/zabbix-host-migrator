namespace ZabbixHostMigrator.Application.DTOs;

public sealed class ZabbixInstanceOptions
{
  public string Url { get; init; } = string.Empty;
  public string Username { get; init; } = string.Empty;
  public string Password { get; init; } = string.Empty;
  public bool UseMock { get; init; }
}
