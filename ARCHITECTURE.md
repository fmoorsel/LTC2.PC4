# LTC2.PC4 — Software Architecture Document

## Overview

LTC2.PC4 (*Long Term Challenge, Postcode 4*) is a geospatial fitness-tracking platform that monitors athlete activities from Strava and RideWithGPS, computes how many Dutch postcode areas each athlete has visited, and presents the results through a web UI embedded in a desktop application.

The system consists of five executable components and fifteen shared libraries, all targeting .NET 10.

---

## Components

### Executables

| Project | Type | Responsibility |
|---|---|---|
| `LTC2.Desktopclients.ProfileManager` | WinForms | Launcher and control panel; starts and monitors Calculator and WebApp processes |
| `LTC2.Desktopclients.WindowsClient` | WinForms | Main desktop client; hosts a WebView2 browser; drives activity updates |
| `LTC2.DesktopClients.ArchiveImporter` | WinForms | One-off tool; imports activity archives (FIT, GPX, TCX, JSON) |
| `LTC2.Webapps.MainApp` | ASP.NET Core | REST API; handles OAuth, profile queries, route checking, scoring triggers |
| `LTC2.Services.Calculator` | .NET Worker | Long-running background service; performs the actual score calculation |

### Shared Libraries

| Project | Responsibility |
|---|---|
| `LTC2.Shared.Models` | Domain types, DTOs, settings classes |
| `LTC2.Shared.Common` | `ConnectorFactory`, `IConnector` interface, keyed DI wiring |
| `LTC2.Shared.Http` | `ILTC2HttpProxy` / `LTC2HttpProxy` — HTTP client for internal API calls |
| `LTC2.Shared.StravaConnector` | Strava API integration (OAuth, activity browsing, track fetching) |
| `LTC2.Shared.RideWithGpsConnector` | RideWithGPS API integration (OAuth, route enumeration) |
| `LTC2.Shared.Repositories` | Repository interfaces + abstract Elasticsearch base |
| `LTC2.Shared.SpatiaLiteRepository` | SQLite/SpatiaLite implementations of repository interfaces |
| `LTC2.Shared.Database` | SQL Server connectivity |
| `LTC2.Shared.Stores` | Session persistence (`FileSessionStore`, `DatabaseSessionStore`) |
| `LTC2.Shared.Messaging` | File-based message broker (folder monitoring, producer/consumer) |
| `LTC2.Shared.Secrets` | Windows DPAPI vault for encrypted credentials |
| `LTC2.Shared.AcivityFormats` | Activity file parsers (FIT via Garmin SDK, GPX/TCX via XSD, JSON) |
| `LTC2.Shared.Utils` | Bootstrap helpers, logging setup, geometry producers |
| `LTC2.Shared.BaseMessages` / `LTC2.Shared.Messages` | Localization / translation services |

---

## Dependency Layers

```
┌──────────────────────────────────────────────────────────────────┐
│  Executables                                                      │
│  ProfileManager · WindowsClient · ArchiveImporter                │
│  MainApp (ASP.NET)         Calculator (Worker)                   │
└──────────────────────┬────────────────────┬─────────────────────┘
                       │                    │
┌──────────────────────▼────────────────────▼─────────────────────┐
│  Infrastructure                                                   │
│  Common · Http · Messaging · Secrets · Stores · Utils            │
│  StravaConnector · RideWithGpsConnector · ActivityFormats        │
└──────────────────────────────────┬──────────────────────────────┘
                                   │
┌──────────────────────────────────▼──────────────────────────────┐
│  Data Access                                                      │
│  Repositories (interfaces)  ·  SpatiaLiteRepository             │
│  Database (SQL Server)                                            │
└──────────────────────────────────┬──────────────────────────────┘
                                   │
┌──────────────────────────────────▼──────────────────────────────┐
│  Domain                                                           │
│  Models (Activity, Visit, Place, Profile, Session, ...)          │
└──────────────────────────────────────────────────────────────────┘
```

---

## Key Domain Concepts

| Concept | Description |
|---|---|
| **Place** | A Dutch postcode area, stored as a polygon geometry |
| **Activity** | A single recorded exercise (ride, run, walk) from Strava or RideWithGPS |
| **Visit** | An occurrence of an activity track intersecting a Place polygon |
| **Session** | An OAuth session (access token + refresh token) tied to one Athlete and one connector source (`s`=Strava, `r`=RideWithGPS) |
| **Profile** | Computed aggregate per Athlete: visited places for all-time, current year, and last ride |
| **CalculationJob** | Command message: which Athlete, which connector, which activity types to (re)calculate |
| **CalculationResult** | Transient state during computation: accumulated visits keyed by time period |
| **CalculationType** | Enum: `Bike`, `Multi`, `Foot`, `All` (Strava); free strings like `"cycling:road"` (RideWithGPS) |

---

## Data Flow: "Update Activities"

```
┌─────────────────────────────┐
│  WindowsClient (WinForms)   │
│  User clicks Update         │
└────────────────┬────────────┘
                 │ HTTP POST /update
                 ▼
┌─────────────────────────────┐
│  MainApp (ASP.NET)          │
│  UpdateController           │
│  Builds CalculationJob      │
│  → publishes to file queue  │
└────────────────┬────────────┘
                 │ JSON file written to
                 │ %LOCALAPPDATA%\LTC2\PC4\Queues\
                 ▼
┌─────────────────────────────┐
│  Calculator (Worker)        │
│  FileBasedBroker monitors   │
│  folder via FileSystemWatcher│
└────────────────┬────────────┘
                 │
          ┌──────▼──────────────────────────────────────────┐
          │  ScoreCalculator.Calculate(job)                  │
          │                                                  │
          │  1. Load OAuth session from FileSessionStore     │
          │     Auto-refresh token if expired                │
          │                                                  │
          │  2. BrowseActivities via IConnector              │
          │     (Strava paged API / RideWithGPS routes)      │
          │     Pre-filter by activity type                  │
          │     Skip activities before incremental cutoff    │
          │                                                  │
          │  3. Per activity:                                │
          │     GetTrackForActivity → [lat,lon] pairs        │
          │     SpatiaLiteMapRepository.GetPlacesForTrack()  │
          │     → SQL point-in-polygon vs Place geometries   │
          │     Record Visit for each matched Place          │
          │                                                  │
          │  4. Accumulate into CalculationResult            │
          │     (AllTime / CurrentYear / LastRide dicts)     │
          │                                                  │
          │  5. Persist to SQLite via ScoresRepository       │
          │     Optionally index to Elasticsearch            │
          │                                                  │
          │  6. Publish StatusMessage back to file queue     │
          └──────────────────────────────────────────────────┘
                 │ StatusMessage
                 ▼
┌─────────────────────────────┐
│  WindowsClient              │
│  StatusNotifier listener    │
│  Updates progress UI        │
└─────────────────────────────┘
```

---

## Data Stores

| Store | Technology | Contents |
|---|---|---|
| Primary database | SQLite + SpatiaLite extension | Place geometries, visits, scores, sessions |
| Search index | Elasticsearch (optional) | Activities for full-text and geospatial queries |
| SQL Server | Microsoft.Data.SqlClient | Alternative/supplementary store |
| Message queue | File system (`%LOCALAPPDATA%\LTC2\PC4\Queues\`) | CalculationJob commands, StatusMessage events |
| Sessions | File system (`%LOCALAPPDATA%\LTC2\PC4\Sessions\`) | Serialized OAuth sessions, encrypted via DPAPI |
| Secrets | File system (`%LOCALAPPDATA%\LTC2\PC4\Secrets\`) | API keys and credentials, encrypted via DPAPI |
| Intermediate results | File system (configurable cache folder) | Partial calculation state for restartability |

---

## Connector Abstraction

Both Strava and RideWithGPS are accessed through a common `IConnector` interface, resolved via keyed DI:

```
IConnectorFactory (ConnectorFactory)
  └─ GetConnector(ConnectorSource.Strava)   → StravaConnector
  └─ GetConnector(ConnectorSource.RideWithGps) → RideWithGpsConnector
```

`IConnector` exposes callback-based methods (`OnPreCheckActivity`, `OnCheckActivity`, `OnWaitingForSlot`) so the Calculator can receive streaming progress without polling.

---

## Configuration

Settings are loaded from `appsettings.json` in each executable project, bound to typed classes, and registered as DI singletons. The main setting groups are:

| Class | Contains |
|---|---|
| `AppSettings` | App name, UI paths, monitoring targets |
| `GenericSettings` | Data folder paths, SQLite connection string, ID |
| `StravaHttpProxySettings` | Strava client ID/secret, URL, rate limits |
| `RideWithGpsHttpProxySettings` | RideWithGPS credentials and URL |
| `AuthorizationSettings` | JWT signing key, issuer, audience |
| `DatabaseSettings` | SQL Server connection string |

Sensitive values (client secrets, API keys) that are absent from `appsettings.json` are read from the Windows DPAPI secrets vault at runtime.

---

## Key Design Patterns

### File-Based Message Queue
The Calculator and WebApp communicate via JSON files in a watched folder — no external broker dependency. `FileBasedBroker` uses `FileSystemWatcher` internally. This simplifies deployment (no RabbitMQ, no MSMQ) at the cost of single-machine scope.

### Repository Pattern
Data access is fully abstracted behind interfaces (`IScoresRepository`, `IMapRepository`, `IPlacesRepository`). The primary implementation is `SpatiaLiteRepository`; an Elasticsearch implementation exists for search features. Switching backends requires only a DI registration change.

### Delegate Callbacks on Long Operations
`IConnector.BrowseActivities` and `GetTrackForActivity` accept `Action<>` / `Func<>` callbacks rather than returning enumerables, allowing the Calculator to report intermediate progress and update the UI in real time.

### Keyed DI for Connector Selection
`ConnectorFactory` uses `IServiceProvider.GetRequiredKeyedService<IConnector>(source)` to select the right connector at runtime based on the athlete's `ConnectorSource`.

### WebView2 Bridge
The desktop client hosts a Chromium-based WebView2 control. `WebviewConnector` marshals messages between C# services and the embedded JavaScript SPA, avoiding the need for a separate browser process.

### Windows DPAPI Secrets Vault
`WindowsSecretsVault` (implements `ISecretsVault`) encrypts credentials with the current Windows user's identity key — secrets are unreadable outside the machine and user account without any key-management infrastructure.

---

## Security

- REST API endpoints are protected with JWT Bearer authentication.
- OAuth access tokens for Strava and RideWithGPS are stored encrypted (DPAPI) and auto-refreshed before expiry.
- Swagger/OpenAPI UI is available for local development; it should be disabled in production.

---

## External NuGet Dependencies (Architecturally Significant)

| Package | Purpose |
|---|---|
| `NEST` 7.x | Elasticsearch .NET client |
| `NetTopologySuite` | GIS geometry (WKT parsing, point-in-polygon) |
| `ProjNet` | Coordinate system transformations |
| `mod_spatialite` | SQLite spatial extension (native binary) |
| `Microsoft.Web.WebView2` | Embedded Chromium browser in WinForms |
| `Garmin.FIT.Sdk` | FIT file format parser |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT validation in the web API |
| `Serilog` | Structured logging |
| `Newtonsoft.Json` | JSON serialization across all projects |
| `System.Security.Cryptography.ProtectedData` | Windows DPAPI wrapper |
