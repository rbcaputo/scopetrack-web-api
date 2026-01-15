# ScopeTrack — Implementation Plan (Milestone-Driven)

## Purpose of This Document
This document defines **how ScopeTrack is implemented,** translating architectural intent into:
* concrete API endpoints
* domain structures expressed in code
* executable use cases
* a locked first milestone

It complements `ARCHITECTURE.md`. The goal is **predictable execution.**

---

## 1. Implementation Strategy

## Guiding Principles
* API-first development
* Architecture before endpoints
* Explicit domain rules
* No speculative abstractions
* No framework "magic" hiding behavior

Each milestone must:
* compile
* run
* be testable
* demonstrate business value

If a feature does not serve those goals, it does not enter the milestone.

---

## 2. Milestones

## 2.1 First Milestone — Locked Scope (API Only)

### Objective
Deliver a **working REST API** that supports:
> Tracking clients, their contracts, and the deliverables inside each contract, with explicit lifecycle rules.

This milestone is complete when:
* all defined endpoints exist
* database schema is applied via migrations
* core use cases are covered by automated tests
* API can be exercised via Swagger or Postman

No UI. No auth. No polish.

---

## 2.1.1 Milestone 1 — API Endpoints

### Clients
```text
POST /api/clients
GET  /api/clients
GET  /api/clients/{id}
PUT  /api/clients/{id}
```
**Client fields**
* Id (GUID)
* Name
* ContactEmail
* Status (Active | Inactive)
* CreatedAt
* UpdatedAt

---

### Contracts
```text
POST /api/clients/{clientId}/contracts
GET  /api/contracts{id}
POST  /api/contracts/{id}/activate
POST  /api/contracts/{id}/archive
```
**Contract fields**
* Id (GUID)
* ClientId
* Title
* Description
* Type (FixedPrice | TimeBased)
* Status (Draft | Active | Archived)
* CreatedAt
* UpdatedAt

---

### Deliverables
``` text
POST  /api/contracts/{contractId}/deliverables
GET   /api/contracts/{contractId}/deliverables
PATCH /api/deliverables/{id}/status
```
**Task fields**
* Id (GUID)
* ContractId
* Title
* Description
* Status (Planned | InProgress | Completed)
* DueDate (optional)
* CreatedAt
* UpdatedAt

---

### Explicit Exclusions (Milestone 1)
* Authentication / authorization
* Users
* Billing or payments
* Notifications
* File uploads
* Reporting
* UI

These are consciously deferred.

---

## 2.2 Second Milestone — Locked Scope
This is critical: **defined, contrained, and frozen.**

## 2.2.1 Milestone 2 — Read Models & Visibility

### Objective
Improve **read clarity** without changing domain behavior:
* no new writes
* no new mutations

### Scope
* Read-only projections
* Aggregated views
* No new entities

### Additions

#### Client Overview
```text
GET /api/clients/{id}/overview
```
Returns:
* client details
* active contracts count
* total deliverables
* completed deliverables

#### Contract Timeline
```text
GET /api/contracts/{id}/timeline
```
Returns:
* contract info
* deliverables
* ordered activity logs

---

### Explicit Exclusions (Milestone 2)
* Filtering DSLs
* Search engine
* UI state management
* Permissions

---

## 3. Initial Domain Model (Code-Level)

### Client (Aggregate Root)
```csharp
class Client
{
  Guid Id;
  string Name;
  string ContactEmail;
  ClientStatus Status;
  DateTime CreatedAt;
  DateTime UpdatedAt;

  List<Contract> Contracts;
}
```
Rules:
* A client owns contracts
* A client can be deactivated, but not deleted
* Contracts cannot exist without a client

---

### Contract (Aggregate Root) 
```csharp
class Contract
{
  Guid Id;
  Guid ClientId;
  string Title;
  string Description;
  ContractType Type;
  ContractStatus Status;
  DateTime CreatedAt;
  DateTime UpdatedAt;

  List<Deliverable> Deliverables;
}
```
Rules:
* A contract belongs to exactly one client
* A contract cannot be activated without at least one deliverable
* Archived contracts are immutable

---

### Deliverable (Entity)
```csharp
class Deliverable
{
  Guid Id;
  Guid ContractId;
  string Title;
  string Description;
  DeliverableStatus Status;
  DateTime? DueDate;
  DateTime CreatedAt;
  DateTime UpdatedAt;
}
```
Rules:
* A deliverable cannot exist outside a contract
* Status transitions must be explicit and valid
* A deliverable cannot be completed if its contract is inactive

---

### Activity Logging (Entity)
```csharp
class ActivityLog
{
  Guid Id;
  ActivityEntityType EntityType;
  Guid EntityId;
  ActivityType ActivityType;
  string Description;
  DateTime OccurredAt;
}
```
Rules:
* No dynamic schemas
* No JSON blobs
* Human-readable descriptions are intentional

#### Example Entries
* `Client Created`
* `Contract Activated`
* `Deliverable StatusChanged (Planned → InProgress)`

The **description is informational,** not authoritative.\
The authoritative state is always the aggregate.

---

### Enums
```csharp
enum ClientStatus { Active, Inactive }

enum ContractType { FixedPrice, TimeBased }
enum ContractStatus { Draft, Active, Archived }

enum DeliverableStatus { Planned, InProgress, Completed }

enum ActivityEntityType { Client, Contract, Deliverable }
enum ActivityType { Created, StatusChanged, Activated, Archived }
```
No inheritance. No polymorphic hierarchies. Explicit state machines over abstractions.

---

## 4. Database Schema (Draft)

### Clients
```text
Clients
-------
Id           UNIQUEIDENTIFIER (PK)
Name         NVARCHAR(200)
ContactEmail NVARCHAR(200)
Status       INT
CreatedAt    DATETIME2
UpdatedAt    DATETIME2
```

---

### Contracts
```text
Contracts
---------
Id          UNIQUEIDENTIFIER (PK)
ClientId    UNIQUEIDENTIFIER(FK)
Title       NVARCHAR(200)
Description NVARCHAR(1000)
Type        INT
Status      INT
CreatedAt   DATETIME2
UpdatedAt   DATETIME2
```
Foreign Key:
```text
Contracts.ClientId → Clients.Id
```

---

### Deliverables
```
Deliverables
------------
Id          UNIQUEIDENTIFIER (PK)
ContractId  UNIQUEIDENTIFIER (FK)
Title       NVARCHAR(200)
Description NVARCHAR(1000)
Status      INT
DueDate     DATETIME2 NULL
CreatedAt   DATETIME2
UpdatedAt   DATETIME2
```
Foreign key:
```text
Deliverables.ContractId → Contracts.Id
```

Cascade deletes enabled only from Clients → Contracts → Deliverables.

---

### Activity Logs
```text
ActivityLogs
------------
Id           UNIQUEIDENTIFIER (PK)
EntityType   INT
EntityId     UNIQUEIDENTIFIER
ActivityType INT
Description  NVARCHAR(500)
OccurredAt   DATETIME2
```

Indexes:
* `(EntityType, EntityId)`
* `OccurredAt`

No deletes ever.

---

## 5. First Use Case Specification

### Use Case: Create Client

#### Input
* Name
* ContactEmail

#### Steps
1. Validate name is not empty
2. Create client with `Active` status
3. Persist
4. Return client DTO

#### Output
* Client Id
* Name
* Status
* CreatedAt

---

### Use Case: Activate Contract

#### Input
* ContractId

#### Steps
1. Load contract
2. Ensure contract is in `Draft`
3. Ensure at least one deliverable exists
4. Change status to `Active`
5. Persist
6. Append activity log entry

---

### Use Case: Change Deliverable Status

#### Input
* DeliverableId
* NewStatus

#### Steps
1. Load deliverable and parent contract
2. Ensure contract is `Active`
3. Validate status transition
4. Apply change
5. Persist
6. Append activity log entry

---

## 6. Testing Strategy (Milestone 1)

### Domain tests
* Status transition rules
* Invariant enforcement
* Invalid state prevention

No mocks.

### Application tests
* Use case orchestration
* Repository interaction
* Failure paths

Infrastructure mocked where required.

### Out of scope:
* UI tests
* Performance testing
* Load testing

---

## 7. Deferred Milestones (Acknowledged, Not Planned)
These are **not commitments:**
* Authentication & users
* Role-based access
* Billing
* Reporting
* UI (Angular)
* Audit log querying

They exist only as future extension points.

---

## 8. Completion Criteria
Milestone 1 is complete when:
* API runs locally
* Database is applied via migration
* Swagger documents all endpoints
* Tests pass in CI
* README explains how to run the system

No extra features allowed past this line.

---

## Final Note
This implementation plan prioritizes:
* recognizability
* architectural consistency
* discipline

ScopeTrack is not trying to impress with breadth. It demonstrates **control, clarity, and execution.**
