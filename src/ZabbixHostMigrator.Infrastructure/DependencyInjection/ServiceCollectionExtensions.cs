using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using ZabbixHostMigrator.Application.Abstractions.Clients;
using ZabbixHostMigrator.Infrastructure.Clients;

namespace ZabbixHostMigrator.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services)
  {
    services.AddHttpClient<IZabbixApiClient, ZabbixApiClient>(client =>
    {
      client.Timeout = TimeSpan.FromSeconds(60);
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Accept.Add(
              new MediaTypeWithQualityHeaderValue("application/json"));
    });

    return services;
  }
}
