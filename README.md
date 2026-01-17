# ScopeTrack
A REST API for tracking clients, contracts, and deliverables with automatic activity logging. Built with Clean Architecture principles using .NET 10 and Entity Framework Core.

---

## Table of Contents
* [Overview](#overview)
* [Architecture](#architecture)
* [Tech Stack](#tech-stack)
* [Project Structure](#project-structure)
* [Getting Started](#getting-started)
* [API Endpoints](#api-endpoints)
* [Database Schema](#database-schema)
* [Validation](#validation)
* [Activity Logging](#activity-logging)
* [Development](#development)

---

## Overview
ScopeTrack provides a complete solution for managing:
* **Clients** – Organizations or individuals you work with
* **Contracts** – Agreements with specific clients
* **Deliverables** – Individual work items within contracts
* **Activity Logs** – Automatic tracking of all entity changes via EF Core interceptors

Each entity has status management, timestamps, and hierarchical relationships that enforce business rules at the domain level.

---

## Architecture
This solution follows **Clean Architecture** principles with clear separation of concerns:
```text
[ScopeTrack.API] → [ScopeTrack.Application] → [ScopeTrack.Domain] → [ScopeTrack.Infrastructure]
```
| Layer | Responsibility |
|-------|----------------|
| API | Controllers, Validation |
| Application | Services, DTOs, Mappers |
| Domain | Entities, Business Rules |
| Infrastructure | DbContext, EF Config, Interceptors |

**Key Principles:**
* Domain models contain business logic and enforce invariants
* Application services orchestrate use cases
* Controllers handle HTTP concerns and input validation
* Infrastructure handles persistence and cross-cutting concerns
* Activity logging is automatic via EF Core interceptors

---

## Tech Stack
* **.NET 10** – Framework
* **ASP.NET Core** – Web API
* **Entity Framework Core 10** – ORM
* **SQL Server** – Database (LocalDB for development)
* **FluentValidation** – Input validation
* **Swagger/OpenAPI** – API documentation

---

## Project Structure
```text
ScopeTrack/
├─ ScopeTrack.API/
│  ├─ Controllers/
│  │  ├─ ClientController.cs
│  │  ├─ ContractController.cs
│  │  ├─ DeliverableController.cs
│  │  └─ ActivityLogController.cs
│  ├─ Program.cs
│  └─ appsettings.json
│
├─ ScopeTrack.Application/
│  ├─ DTOs/
│  │  ├─ ClientPostDTO.cs, ClientPutDTO.cs, ClientGetDTO.cs
│  │  ├─ ContractPostDTO.cs, ContractPatchDTO.cs, ContractGetDTO.cs
│  │  ├─ DeliverablePostDTO.cs, DeliverablePatchDTO.cs, DeliverableGetDTO.cs
│  │  └─ ActivityLogGetDTO.cs
│  ├─ Interfaces/
│  │  └─ IClientService.cs, IContractService.cs, IDeliverableService.cs, IActivityLogService.cs
│  ├─ Mappers/
│  │  └─ ClientMapper.cs, ContractMapper.cs, DeliverableMapper.cs, ActivityLogMapper.cs
│  ├─ Services/
│  │  └─ ClientService.cs, ContractService.cs, DeliverableService.cs, ActivityLogService.cs
│  ├─ Validators/
│  │  └─ ClientDTOValidator.cs, ContractDTOValidator.cs, DeliverableDTOValidator.cs
│  └─ RequestResult.cs
│
├─ ScopeTrack.Domain/
│  ├─ Entities/
│  │  ├─ ClientModel.cs
│  │  ├─ ContractModel.cs
│  │  ├─ DeliverableModel.cs
│  │  └─ ActivityLogModel.cs
│  └─ Enums/
│     ├─ ClientStatus.cs
│     ├─ ContractStatus.cs
│     ├─ DeliverableStatus.cs
│     ├─ ActivityEntityType.cs
│     └─ ActivityType.cs
│
└─ ScopeTrack.Infrastructure/
   ├─ Data/
   │  ├─ ScopeTrackDbContext.cs
   │  └─ Configurations/
   │     ├─ ClientEntityConfig.cs
   │     ├─ ContractEntityConfig.cs
   │     ├─ DeliverableEntityConfig.cs
   │     └─ ActivityLogEntityConfig.cs
   ├─ Interceptors/
   |  └─ ActivityLogInterceptor.cs
   └─ Migrations/
```

---

## Getting Started

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download)
* [SQL Server](https://www.microsoft.com/pt-br/sql-server) or LocalDB
* [Visual Studio 2022+](https://visualstudio.microsoft.com) or [VS Code](https://code.visualstudio.com)

### Installation

1. **Clone the repository**
```git
git clone https://github.com/rbcaputo/scopetrack-web-api
cd ScopeTrack
```

2. **Restore dependencies**
```bash
dotnet restore
```

3. **Configure connection string

Edit `ScopeTrack.API/appsettings.json` and add:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ScopeTrackDB;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```
For non-LocalDB installations, update the connection string accordingly.

4. **Create database and apply migrations**
```bash
dotnet ef migrations add InitialCreate --project ScopeTrack.Infrastructure --startup-project ScopeTrack.API
dotnet ef database update --project ScopeTrack.Infrastructure --startup-project ScopeTrack.API
```

5. **Run the API**
```bash
dotnet run --project ScopeTrack.API
```

6. **Access Swagger UI**

Open browser to: `https://localhost:7205` (or the port shown in console output)

---

## API Endpoints

### Clients
| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/client` | Create a new client |
| `PUT` | `/api/client/{id}` | Update client details |
| `PATCH` | `api/client/{id}` | Toggle client status |
| `POST` | `/api/client/{id}/contracts` | Add contract to client |
| `GET` | `/api/client/{id}` | Get client by ID |
| `GET`| `/api/client` | Get all clients |

### Contracts
| Method | Endpoint | Description |
|--------|----------|-------------|
| `PATCH` | `/api/contract/{id}/status` | Update contract status |
| `POST` | `/api/contract/{id}/deliverables` | Add deliverable to contract |
| `GET` | `/api/contract/{id}` | Get contract by ID |

### Deliverables
| Method | Endpoint | Description |
|--------|----------|-------------|
| `PATCH` | `/api/deliverable/{id}/status` | Update deliverable status |
| `GET` | `/api/deliverable/{id}` | Get deliverable by ID |

### Activity Logs
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/activitylog` | Get all activity logs |
| `GET` | `/api/activitylog/{entityId}` | Get logs for specific entity |

## Database Schema

### Clients Table
* `ID`: PK, GUID
* `Name`: nvarchar(100), required
* `Email`: nvarchar(100), required, unique
* `Status`: int, required
* `CreatedAt`: datetime2, required
* `UpdatedAt`: datetime2, required

### Contracts Table
* `ID`: PK, GUID
* `ClientID`: FK, GUID, required
* `Title`: nvarchar(200), required
* `Description`: nvarchar(1000)
* `Status`: int, required
* `CreatedAt`: datetime2, required
* `UpdatedAt`: datetime2, required

### Deliverables Table
* `ID`: PK, GUID
* `ContractID`: FK, GUID, required
* `Title`: nvarchar(200), required
* `Description`: nvarchar(1000)
* `Status`: int, required
* `DueDate`: datetime2, nullable
* `CreatedAt`: datetime2, required
* `UpdatedAt`: datetime2, required

### ActivityLogs Table
* `ID`: PK, GUID
* `EntityType`: int, required
* `EntityID`: GUID, required
* `ActivityType`: int, required
* `ActivityDescription`: nvarchar(500), required
* `Timestamp`: datetime2, required

**Indexes:**
* `(EntityType, EntityID)` – Composite index for efficient entity queries
* `Timestamp` – for chronological queries

**Relationships:**
* Client → Contracts (1:N, cascade delete)
* Contract → Deliverables (1:N, cascade delete)

---

## Validation
Input validation is handled at the API layer using **FluentValidation.**

### Client Validation Rules
* **Name:** Required, 3-100 characters
* **Email:** Required, valid email format, 5-100 characters

### Contract Validation Rules
* **Title:** Required, 10-200 characters
* **Description:** Optional, max 1000 characters
* **Type:** Required, must be `"FixedPrice"` or `"TimeBased"`
* **Status** (for updates): Must be `"Active"`, `"Completed"`, or `"Archived"`

### Deliverable Validation Rules
* **Title:** Required, 10-200 characters
* **Description:** Optional, max 500 characters
* **Status** (for updates): Must be `"Pending"`, `"InProgress"`, `"Completed"`, or `"Cancelled"`

Validation errors return **400 Bad Request** with structured error details:
```json
[
  {
    "field": "Email",
    "message": "Client email must be a valid email address"
  }
]
```

---

## Activity Logging
All entity changes are **automatically logged** to the `ActivityLogs` table via an EF Core interceptor.

### How it works
Activity logging is handled by `ActivityLogInterceptor`, which:
1. Intercepts `SaveChanges` and `SaveChangesAsync` calls
2. Inspects the ChangeTracker for added/modified entities
3. Creates activity log entries for Client, Contract, and Deliverable changes
4. Adds logs to the same transaction as the entity changes

This ensures:
* **Atomic logging:** Logs are only written if entity changes succeed
* **No manual staging:** Services don't need to explicitly create logs
* **Centralized logic:** All logging logic in one place

### Logged Activities
**Clients:**
* Created: `"Client '{name}' created"`
* Updated: `"Client '{name}' updated"`
* Status Changed: `Client '{name}' status changed to {status}"`

**Contracts:**
* Created: `"Contract '{title}' created"`
* Status Changed: `"Contract '{title}' status changed to {status}"`

**Deliverables:**
* Created: `"Deliverable '{title}' created"`
* Status Changed: `"Deliverable {title} status changed to {status}"`

**Activity Log Structure:**
```json
{
  "entityType": "Client | Contract | Deliverable",
  "activityType": "Created | Updated | StatusChanged",
  "description": "Human-readable description",
  "timestamp": "2026-01-17T14:30:00Z"
}
```

---

## Development

### Adding a New Migration
```bash
dotnet ef migrations add MigrationName --project ScopeTrack.Infrastructure --startup-project ScopeTrack.API
dotnet ef database update --project ScopeTrack.Infrasctructure --startup-project ScopeTrack.API
```

### Running Tests
```bash
dotnet test
```
*(Note: Tests are not yet implemented)*

### Code Organization Guidelines
1. **Domain Models** – Contain business logic, enforce invariants, never reference DTOs or infrastructure
2. **Application Services** – Orchestrate use cases, map between DTOs and models, no business logic
3. **Controllers** – Validate input, call services, return appropriate HTTP responses
4. **DTOs** – Data contracts between API and clients, validated by FluentValidation
5. **Mappers** – Convert between DTOs and domain models
6. **Interceptors** – Handle cross-cutting concerns like activity logging

### Business Rules
* Clients must have unique emails (enforced by database unique constraint)
* Contracts can only be added to active clients
* Deliverables can only be added to non-archived contracts
* Contracts cannot be activated without at least one deliverable
* Deliverable status can only be changed when contract is active
* Status transitions follow domain-defined state machines
* All timestamps are managed automatically by domain models

### Concurrency Handling
* Email uniqueness is enforced at the database level with a unique constraint
* The application performs early validation checks for better UX
* Race conditions are handled via try-catch on `DbUpdateException` with SQL error code 2601

---

## CORS Configuration
CORS is enabled for development with an `AllowAll` policy. For production, restrict origins in `Program.cs`:
```csharp
options.AddPolicy("Production", policy =>
{
  policy.WithOrigins("https://yourdomain.com")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials();
});
```

---

## License

---

## Contact
