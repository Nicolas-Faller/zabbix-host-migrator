using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ZabbixHostMigrator.Application.Abstractions.Services;
using ZabbixHostMigrator.Application.DTOs;
using ZabbixHostMigrator.Application.Services;
using ZabbixHostMigrator.Infrastructure.DependencyInjection;

var settings = new HostApplicationBuilderSettings
{
  Args = args,
  ContentRootPath = AppContext.BaseDirectory
};

var builder = Host.CreateApplicationBuilder(settings);

builder.Services.Configure<ZabbixInstanceOptions>(
    "Source",
    builder.Configuration.GetSection("Source"));

builder.Services.Configure<ZabbixInstanceOptions>(
    "Destination",
    builder.Configuration.GetSection("Destination"));

builder.Services.Configure<MigrationOptions>(
    builder.Configuration.GetSection("Migration"));

builder.Services.AddInfrastructure();
builder.Services.AddScoped<IMigrationRunner, MigrationRunner>();

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
await runner.RunAsync();
