namespace ZabbixHostMigrator.Domain.Entities;

public sealed record ZabbixHost(
    string HostId,
    string Host,
    string VisibleName,
    int Status,
    IReadOnlyList<ZabbixHostGroup> Groups,
    IReadOnlyList<ZabbixHostInterface> Interfaces,
    IReadOnlyList<ZabbixHostTag> Tags);
