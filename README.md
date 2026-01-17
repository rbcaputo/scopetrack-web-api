# ScopeTrack
A REST API for tracking clients, contracts, and deliverables with activity logging. Built with Clean Architecture principles using .NET 10 and Entity Framework Core.

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
* **Activity Logs** – Automatic tracking of all entity changes

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
| Infrastructure | DbContext, EF Config |

**Key Principles:**
* Domain models contain business logic and enforce invariants
* Application services orchestrate use cases
* Controllers handle HTTP concerns and input validation
* Infrastructure is isolated and replaceable

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
│  │  └── ActivityLogController.cs
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
│  │  └─ IClientService.cs, IContractService.cs, etc.
│  ├─ Mappers/
│  │  └─ ClientMapper.cs, ContractMapper.cs, etc.
│  ├─ Services/
│  │  └─ ClientService.cs, ContractService.cs, etc.
│  ├─ Validators/
│  │  └─ ClientPostDTOValidator.cs, etc.
│  └─ Result.cs
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
   └─ Migrations/
```

---

## Getting Started

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download)
* [SQL Server](https://www.microsoft.com/pt-br/sql-server) or LocalDB
* [Visual Studio 2026](https://visualstudio.microsoft.com) or [VS Code](https://code.visualstudio.com)

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

3. **Update connection string** (if not using LocalDB) Edit `ScopeTrack.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ScopeTrackDB;Trusted_Connection=true;"
  }
}
```

4. **Create database and apply migrations**
```bash
dotnet ef migrations add InitialCreate --project ScopeTrack.Infrastructure --startup-project ScopeTrack.API
dotnet ef database update --project ScopeTrack.Infrastructure --startup-project ScopeTrack.API
```

5. **Run the API**
```bash
dotnet run --project ScopeTrack.API
```

6. **Access Swagger UI** Open browser to: `https://localhost:7205` (or the port shown in console output)

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
* `Status`: int, required
* `CreatedAt`: datetime2, required
* `UpdatedAt`: datetime2, required

### Deliverables Table
* `ID`: PK, GUID
* `ContractID`: FK, GUID, required
* `Description`: nvarchar(500), required
* `Status`: int, required
* `CreatedAt`: datetime2, required
* `UpdatedAt`: datetime2, required

### ActivityLogs Table
* `ID`: PK, GUID
* `EntityType`: int, required
* `EntityID`: GUID, required
* `ActivityType`: int, required
* `ActivityDescription`: nvarchar(500), required
* `Timestamp`: datetime2, required

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
* **Title:** Required, 5-100 characters
* **Status:** Must be valid enum value (Draft, Active, Completed, Archived)

### Deliverable Validation Rules
* **Description:** Required, 10-500 characters
* **Status:** Must be valid enum value (Pending, InProgress, Completed, Cancelled)

Validation errors return **400 Bad Request** with structured error details:
```json
[
  {
    "field": "Email",
    "message": "Client contact email must be a valid email address"
  }
]
```

---

## Activity Logging
All entity changes are automatically logged to the `ActivityLogs` table.

**Logged Activities:**
* Client created/updated/status changed
* Contract created/status changed
* Deliverable created/status changed

**Activity Log Structure:**
```json
{
  "id": "guid",
  "entityType": "Client | Contract | Deliverable",
  "entityID": "guid",
  "activityType": "Created | Updated | StatusChanged",
  "activityDescription": "Human-readable description",
  "timestamp": "2026-01-17T14:30:00Z"
```

Activity logs are staged during service operations and commited atomically with the main entity changes.

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

### Code Organization Guidelines
1. **Domain Models** – Contain business logic, enforce invariants, never reference DTOs
2. **Application Services** – Orchestrate use cases, use repositories, map between DTOs and models
3. **Controllers** – Validate input, call services, return HTTP responses
4. **DTOs** – Data contracts between API and clients, validated by FluentValidation
5. **Mappers** – Convert between DTOs and domain models

### Business Rules
* Clients must have unique emails (database constraint enforced)
* Contracts can only be added to active clients
* Deliverables can only be added to active contracts
* Status transitions follow domain logic (e.g., can't complete an archived contract)
* All timestamps are managed automatically

---

## CORS Configuration
CORS is enabled for development with an `AllowAll` policy. For production, restrict origins in `Program.cs`:
```csharp
options.AddPolicy("Production", policy =>
{
  policy.WithOrigins("https://yourdomain.com")
    .AllowAnyMethods()
    .AllowAnyHeaders()
    .AllowCredentials();
});
```

---

## License

---

## Contact
