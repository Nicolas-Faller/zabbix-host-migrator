namespace ZabbixHostMigrator.Domain.Entities;

public sealed record ZabbixHostInterface(
    int Type,
    string Ip,
    string Dns,
    string Port,
    bool UseIp,
    bool Main);
