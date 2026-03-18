<div align="center">

# Zabbix Host Migrator

A .NET 8 CLI application for migrating hosts between Zabbix instances through the Zabbix JSON-RPC API, with a complete mock mode for local execution and portfolio demonstration.

</div>

---

## Overview

**Zabbix Host Migrator** is a backend and automation-focused portfolio project designed to simulate a real operational migration workflow between two Zabbix environments.

The project currently supports:

- configuration-based source and destination instances
- authentication flow
- source host retrieval
- filtering by group and host name
- duplicate detection in destination
- mock host creation
- dry-run execution
- JSON report generation

This project was built to demonstrate practical backend and integration skills in a scenario that resembles infrastructure automation and operational tooling.

---

## Current Status

The project already runs end-to-end in **mock mode**, which means it can be executed locally without access to a real Zabbix environment.

### What is implemented

- CLI application with layered architecture
- source and destination configuration through `appsettings.json`
- mock authentication for both instances
- mock host retrieval from source
- filtering by source group name
- duplicate detection in destination
- mock host creation
- dry-run support
- migration report generation in JSON

### What is scaffolded

- Zabbix JSON-RPC client structure
- authentication against real API endpoints
- host retrieval through `host.get`
- destination lookup through `host.get`
- host creation structure for `host.create`

---

## Features

- Read source and destination settings from configuration
- Authenticate against source and destination instances
- Retrieve hosts from source instance
- Filter hosts by group and host name
- Check whether a host already exists in destination
- Skip duplicated hosts when configured
- Support dry-run execution
- Create hosts in mock mode
- Generate migration reports in JSON format

---

## Tech Stack

- **.NET 8**
- **C#**
- **Console / CLI application**
- **Microsoft.Extensions.Hosting**
- **Microsoft.Extensions.Options**
- **Microsoft.Extensions.DependencyInjection**
- **HttpClient**
- **System.Text.Json**
- **xUnit**
- **FluentAssertions**
- **FluentValidation**

---

## Project Structure

```text
zabbix-host-migrator/
├── src/
│   ├── ZabbixHostMigrator.Cli/
│   ├── ZabbixHostMigrator.Application/
│   ├── ZabbixHostMigrator.Domain/
│   └── ZabbixHostMigrator.Infrastructure/
├── tests/
│   ├── ZabbixHostMigrator.UnitTests/
│   └── ZabbixHostMigrator.IntegrationTests/
├── samples/
│   ├── requests/
│   └── outputs/
└── README.md
```

---

## Architecture

### Domain
Core entities and value objects such as hosts, groups, interfaces, tags, and migration report models.

### Application
Contracts, DTOs, and orchestration logic for the migration workflow.

### Infrastructure
HTTP client logic, Zabbix API integration, mock data flow, request mapping, and report writing.

### CLI
Application entry point, configuration loading, dependency injection setup, and execution bootstrap.

---

## Main Use Case

Migrating monitoring assets between Zabbix environments often requires repetitive validation and manual recreation of hosts.

This project simulates that workflow by:

- reading hosts from a source instance
- checking whether they already exist in the destination
- deciding whether to skip or create them
- generating a detailed execution report

This makes the tool useful as a portfolio example of:

- API integration
- automation workflow design
- dry-run execution strategy
- operational reporting

---

## Configuration

The application uses `appsettings.json`.

Example:

```json
{
  "Source": {
    "Url": "https://mock-source-zabbix/api_jsonrpc.php",
    "Username": "Admin",
    "Password": "change-me",
    "UseMock": true
  },
  "Destination": {
    "Url": "https://mock-destination-zabbix/api_jsonrpc.php",
    "Username": "Admin",
    "Password": "change-me",
    "UseMock": true
  },
  "Migration": {
    "SourceGroupName": "Linux Servers",
    "DestinationGroupName": "Migrated/Linux Servers",
    "HostNameContains": "",
    "SkipIfHostExists": true,
    "DryRun": false
  }
}
```

### Important fields

- `UseMock`: enables mock execution without real Zabbix endpoints
- `SourceGroupName`: filters source hosts by group
- `DestinationGroupName`: target group name for host creation
- `SkipIfHostExists`: skips hosts already found in destination
- `DryRun`: simulates creation without creating hosts

---

## Migration Flow

1. Load configuration
2. Authenticate source instance
3. Authenticate destination instance
4. Retrieve source hosts
5. Filter hosts by group and optional host name
6. Check if each host already exists in destination
7. Skip or create host depending on configuration
8. Generate a JSON report with the final summary

---

## Mock Mode

Mock mode exists to make the project runnable without real Zabbix access.

When `UseMock = true`, the application:

- returns fake authentication tokens
- loads mock source hosts
- simulates destination duplicate detection
- simulates host creation
- generates a final migration report

This allows the full flow to be demonstrated locally.

---

## Current Mock Scenario

With the default mock setup:

- source group filter: `Linux Servers`
- retrieved hosts: `3`
- existing host in destination: `db-prod-01`
- created hosts in mock mode: `web-prod-01`, `web-dev-01`

Expected summary:

- `TotalRead = 3`
- `TotalEligible = 2`
- `TotalMigrated = 2`
- `TotalSkipped = 1`
- `TotalFailed = 0`

---

## Running the Project

### 1. Restore dependencies

```bash
dotnet restore
```

### 2. Build the solution

```bash
dotnet build
```

### 3. Run the CLI

```bash
dotnet run --project src/ZabbixHostMigrator.Cli
```

---

## Sample Output

The application writes JSON reports to:

```text
samples/outputs/
```

Example file name:

```text
migration-report-YYYYMMDD-HHMMSS.json
```

Example report:

```json
{
  "SourceUrl": "https://mock-source-zabbix/api_jsonrpc.php",
  "DestinationUrl": "https://mock-destination-zabbix/api_jsonrpc.php",
  "DryRun": false,
  "GeneratedAtUtc": "2026-03-18T15:16:06Z",
  "Summary": {
    "TotalRead": 3,
    "TotalEligible": 2,
    "TotalMigrated": 2,
    "TotalSkipped": 1,
    "TotalFailed": 0
  },
  "Items": [
    {
      "Host": "web-prod-01",
      "VisibleName": "Web Prod 01",
      "Action": "Migrated",
      "Success": true,
      "Message": "Host created in destination.",
      "CreatedHostId": "89422"
    },
    {
      "Host": "db-prod-01",
      "VisibleName": "Database Prod 01",
      "Action": "Skipped",
      "Success": true,
      "Message": "Host already exists in destination.",
      "CreatedHostId": null
    },
    {
      "Host": "web-dev-01",
      "VisibleName": "Web Dev 01",
      "Action": "Migrated",
      "Success": true,
      "Message": "Host created in destination.",
      "CreatedHostId": "98207"
    }
  ]
}
```

---

## Example Console Output

```text
Zabbix Host Migrator started.
Source authentication succeeded.
Destination authentication succeeded.
Retrieved 3 source hosts.
TotalRead: 3
TotalEligible: 2
TotalMigrated: 2
TotalSkipped: 1
TotalFailed: 0
Report saved at: samples/outputs/migration-report-YYYYMMDD-HHMMSS.json
```

---

## Why This Project

This project was built to demonstrate practical software development skills in a realistic integration scenario, including:

- API client design
- configuration-driven execution
- CLI application structure
- layered architecture
- dry-run safety patterns
- mockable integration workflows
- JSON report generation
- operational automation thinking

---

## Future Improvements

Possible next steps for this project:

- real `host.create` execution against live Zabbix environments
- support for more host fields and advanced mappings
- group creation when destination group does not exist
- rollback strategy
- richer validation rules
- CSV report generation
- test coverage expansion
- Docker support
- command-line arguments for runtime overrides

---

## Repository Goals

This repository is part of a backend and automation portfolio focused on:

- C# / .NET development
- API integrations
- operational tooling
- infrastructure-oriented software solutions

---

## Author

Built by **Nicolas** as part of a backend portfolio in **C# / .NET**.
