# ScopeTrack — Architecture Reference

## 1. Purpose of This Document
This document describes **how and why** the ScopeTrack system is designed and implemented.

It complements the README by:
* explaining architectural decisions
* defining system boundaries
* making abstractions explicit
* documenting reasoning behind tradeoffs

This is not a tutorial. This is a **design and architecture reference.**

---

## 2. System Overview
ScopeTrack is a small, focused system for tracking:
* clients
* contracts
* deliverables
* contract evolution over time

The system prioritizes:
* clarity of responsibility
* traceability of changes
* predictable behavior

The project is intentionally split into two repositories:
* ScopeTrack.API — backend system of record
* ScopeTrack.UI — minimal frontend client

---

## 3. Repository Structure

### 3.1 Backend Repository — `ScopeTrack.API`
```text
/ScopeTrack.API
/ScopeTack.Domain
/ScopeTrack.Application
/ScopeTrack.Infrastructure

/Tests
  /ScopeTrack.Domain.Tests
  /ScopeTrack.Application.Tests

/Docs
  ARCHITECTURE.md
  API_CONTRACTS.md
  IMPLEMENTATION.md

README.md
ScopeTrack.slnx
```

### 3.2 Frontend Repository — `ScopeTrack.UI`
```text
/src
  /app
    /core
    /features
    /shared

/docs
  ui-architecture.md

angular.json
```

---

## 4. Technology Stack

### Backend
* C# / .NET 10
* ASP.NET Core
* Entity Framework Core 10
* FluentValidation 12
* xUnit (tests)

### Frontend
* Angular
* TypeScript
* Angular Router
* HTTP Client
* Minimal CSS (no UI framework)

### Tooling
* Git
* GitHub Actions (CI)
* REST clients for manual testing (Swagger, Postman)

---

## 5. Architectural Foundations

### 5.1 Clean Architecture
The backend follows Clean Architecture principles:
* Domain is independent
* Application orchestrates use cases
* Infrastructure handles external concerns
* API is a thin delivery layer

Dependency direction always points inward.
```text
API → Application → Domain ← Infrastructure
```

---

### 5.2 Explicit Boundaries
Each layer has a single responsibility:
| Layer | Responsibility |
|-------|----------------|
| Domain | Business rules and invariants |
| Application | Use cases and orchestration |
| Infrastructure | Persistence and external services |
| API | HTTP boundary and input/output validation

No shortcuts across layers.

---

## 6. Domain Model

### 6.1 Core Aggregates

#### Client
* Identity
* Name
* Contact information (email)
* Status (Active/Inactive)
* Collection of Contracts

#### Contract
* Belongs to a Client
* Contract type (FixedPrice/TimeBased)
* Status lifecycle (Draft → Active → Completed/Archived)
* Collection of Deliverables

#### Deliverable
* Belongs to a Contract
* Title, description, optional due date
* Status lifecycle (Pending → InProgress → Completed)

---

### 6.2 Invariants
Examples:
* A contract cannot be activated without at least one deliverable
* A deliverable status cannot be changed if the contract is not active
* Status transitions must be valid and explicit
* Contracts cannot be added to inactive clients
* Activity logs are immutable and append-only

These rules live **only** in the Domain layer.

---

## 7. Application Layer

### 7.1 Use Case Orientation
Each operation is modeled as a use case:
* CreateClient
* UpdateClient
* ToggleClientStatus
* AddContractToClient
* UpdateContractStatus
* AddDeliverableToContract
* UpdateDeliverableStatus

Use cases:
* receive validated input (DTOs)
* load aggregates from persistence
* invoke domain behavior
* persist changes
* return results via `RequestResult<T>`

No business logic lives in the Application layer.

---

### 7.2 DTOs and Mapping
* DTOs are used at boundaries only
* Domain models are never exposed externally
* Mapping is explicit (no AutoMapper or reflection magic)
* Each entity has dedicated mapper (ClientMapper, ContractMapper, etc.)

### 7.3 Result Pattern
The application uses `RequestResult<T>` to represent success or failure:
* `RequestResult<T>.Success(value) – Operation succeeded
* `RequestResult<T>.Failure(error) – Operation failed with error message

This allows services to return failures without throwing exceptions for expected failures (like "Client not found").

---

## 8. Persistence Strategy

### 8.1 Entity Framework Core
* Code-first approach
* Explicit entity mappings via `IEntityTypeConfiguration`
* No lazy loading
* Controlled navigation usage
* Migrations tracked in Infrastructure layer

### 8.2 Concurrency Handling
* Database-level unique constraints (e.g., Client.Email)
* Early validation checks for better UX
* Exception handling for constraint violations
* No optimistic concurrency tokens (yet)

### 8.3 No Soft Deletion
* Entities are physically deleted via cascade
* Activity logs preserve historical record
* No IsDeleted flags or query filters

---

## 9. Activity Logging
Activity logging exists to:
* make **state changes traceable**
* provide **auditability** without event sourcing
* preserve intent behind mutations

It is **not** analytics, a messaging system, or a domain driver.\
It is a **historical record.**

### 9.1 Design Decisions
* Append-only
* Immutable
* Written **automatically** via EF Core interceptor
* Domain models remain ignorant of logging
* Logs are created in the same transaction as entity changes

This avoids:
* Polluting domain logic with persistence concerns
* Manual staging of activity logs in services
* Inconsistent states if SaveChanges fails

### 9.2 Implementation via Interceptor
The `ActivityLogInterceptor` in the Infrastructure layer:
1. Intercepts `SaveChanges` and `SaveChangesAsync` calls
2. Inspects the ChangeTracker for added/modified entities
3. Creates activity logs entries for Client, Contract, and Deliverable changes
4. Adds logs to the context before the transaction commits

This ensures atomic logging—logs are only written if entity changes succeed.

### 9.3 What Gets Logged
* **Client:** Created, Updated(name/email), Status Changed
* **Contract:** Created, Status Changed
* **Deliverable:** Created, Status Changed

Each log entry includes:
* Entity type and ID
* Activity type (Created, Updated, StatusChanged)
* Human-readable description
* UTC timestamp

---

## 10. API Design

### 10.1 REST Principles
* Resource-oriented endpoints
* Clear HTTP semantics
* Predictable status codes
* Nested routes for hierarchical relationships

Example:
```text
POST  /api/client                     (Create client)
POST  /api/client/{id}/contracts      (Add contract to client)
PATCH /api/contract/{id}              (Update contract status)
POST  /api/contract/{id}/deliverables (Add deliverable to contract)
PATCH /api/deliverable/{id}           (Update deliverable status)
```

---

### 10.2 Validation
* Input validation at API boundary via FluentValidation
* Business validation in Domain models
* Clear error responses (400 for validation, 409 for business rule violations)

No silent failures.

### 10.3 Response Patterns
* **200 OK:** Success with entity data
* **400 Bad Request:** Validation failure with structured errors
* **404 Not Found:** Entity not found
* **409 Conflict:** Business rule violation (e.g., duplicate email)

---

## 11. Frontend Architecture

### 11.1 Structure
* Feature-based organization
* Shared UI components
* Thin services wrapping API calls

### 11.2 Scope
The UI is intentionally minimal:
* CRUD operations
* Clear state transitions
* No complex state management libraries

The UI exists to **exercise and demonstrate** the backend.

---

## 12. Testing Strategy

### Domain Tests
* Validate invariants
* Test status transitions
* No mocks (pure unit tests)

### Application Tests
* Use case behavior
* Interaction between layers
* Infrastructure mocked where needed

### Integration Tests
* API endpoints
* Database interactions
* Full request/response cycle

No UI tests (initially).

---

## 13. Non-Goals (Explicitly Excluded)
* Authentication / authorization
* Billing or payments
* Notifications
* Integrations with external systems
* Multi-tenancy
* AI features
* Real-time updates (WebSockets, SignalR)
* File uploads

Excluding these is a design decision, not a limitation.

---

## 14. Design Reasoning
Key principles guiding decisions:
* Prefer clarity over cleverness
* Favor explicit rules over implicit behavior
* Model business constraints directly in domain
* Keep scope intentionally narrow
* Optimize for maintainability, not novelty
* Use framework featuress appropriately (interceptors for cross-cutting concerns)

This project values **correctness and structure** over feature count.

---

## 15. Extension Points
The system can be extended by:
* Adding authentication (ASP.NET Core Identity or JWT)
* Introducing billing modules
* Expanding reporting capabilities
* Adding integrations (webhooks, external APIs)
* Implementing soft deletion if history preservation is needed
* Adding optimistic concurrency if concurrent edits become an issue

The are consciously deferred.

---

## 16. Key Architectural Decisions

### 16.1 Why EF Core Interceptors for Activity Logging?
**Problem:** Manual staging of activity logs in services leads to:
* Code duplication across services
* Risk of forgetting to log changes
* Inconsistent state if SaveChanges fails after staging logs

**Solution:** `ActivityLogInterceptor` automatically creates logs before SaveChanges commits.

**Trade-offs:**
* ✓ Atomic logging (all-or-nothing)
* ✓ No manual staging needed
* ✓ Centralized logging logic
* ⚠ Implicit behavior (less visible than explicit calls)
* ⚠ Coupled to EF Core (but acceptable in Infrastructure layer)

### 16.2 Why Validation in Controllers?
Input validation happens at the API boundary because:
* It's an HTTP concern (malformed requests → 400)
* Business validation lives in domain (rule violations → 409)
* Keeps validation error separate from business failures
* Makes it easy to return proper HTTP status codes

### Why No Repository Pattern?
Services directly use DbContext because:
* EF Core DbSet already provides repository-like abstraction
* No need for an extra layer of indirection
* DbContext is mockable for testing if needed
* Simplifies codebase

If multiple ORMs were required, repositories would make sense.

---

## 17. Conclusion
ScopeTrack is designed as:
* a realistic backend system
* a clear demonstration of architectural discipline
* a foundation that could evolve without rework

The project emphasizes **thinking in systems,** not just shipping code.
