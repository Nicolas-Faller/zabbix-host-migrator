using ZabbixHostMigrator.Domain.Entities;

namespace ZabbixHostMigrator.Infrastructure.Clients;

public sealed class ZabbixHostCreateRequestMapper
{
  public HostCreateRequest Map(ZabbixHost host, string destinationGroupId)
  {
    return new HostCreateRequest
    {
      Host = host.Host,
      Name = string.IsNullOrWhiteSpace(host.VisibleName) ? host.Host : host.VisibleName,
      Status = host.Status,
      Groups =
        [
            new HostGroupCreateRequest
                {
                    GroupId = destinationGroupId
                }
        ],
      Interfaces = host.Interfaces
            .Select(MapInterface)
            .ToList(),
      Tags = host.Tags
            .Select(MapTag)
            .ToList()
    };
  }

  private static HostInterfaceCreateRequest MapInterface(ZabbixHostInterface source)
  {
    return new HostInterfaceCreateRequest
    {
      Type = source.Type,
      Main = source.Main ? 1 : 0,
      UseIp = source.UseIp ? 1 : 0,
      Ip = source.Ip,
      Dns = source.Dns,
      Port = string.IsNullOrWhiteSpace(source.Port) ? "10050" : source.Port
    };
  }

  private static HostTagCreateRequest MapTag(ZabbixHostTag source)
  {
    return new HostTagCreateRequest
    {
      Tag = source.Tag,
      Value = source.Value
    };
  }

  public sealed class HostCreateRequest
  {
    public string Host { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Status { get; init; }
    public List<HostGroupCreateRequest> Groups { get; init; } = [];
    public List<HostInterfaceCreateRequest> Interfaces { get; init; } = [];
    public List<HostTagCreateRequest> Tags { get; init; } = [];
  }

  public sealed class HostGroupCreateRequest
  {
    public string GroupId { get; init; } = string.Empty;
  }

  public sealed class HostInterfaceCreateRequest
  {
    public int Type { get; init; }
    public int Main { get; init; }
    public int UseIp { get; init; }
    public string Ip { get; init; } = string.Empty;
    public string Dns { get; init; } = string.Empty;
    public string Port { get; init; } = string.Empty;
  }

  public sealed class HostTagCreateRequest
  {
    public string Tag { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
  }
}
