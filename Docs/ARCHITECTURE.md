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

/ScopeTrack.Tests
  /API/Controllers
  /Application/Services
  /Domain/Entities

/Docs
  ARCHITECTURE.md
  API_CONTRACTS.md

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
* FluentAssertions (test assertions)

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

### 6.1 Core Entities

#### ClientModel
* Identity: Guid Id
* Name: string
* Email: string (unique)
* Status: ClientStatus enum (Active/Inactive)
* CreatedAt DateTime
* UpdatedAt: DateTime
* Relationships: Collection of ContractModel

**Business Methods:**
* `UpdateDetails(name, email)` — Updates client information
* `ToggleStatus()` — Switches between Active/Inactive
* `AddContract(contract)` — Adds contract (enforces client must be Active)

#### ContractModel
* Identity: Guid Id
* ClientId: Guid
* Title: string
* Description: string (optional)
* Type: ContractType enum (FixedPrice/TimeBased)
* Status: ContractStatus enum
* CreatedAt: DateTime
* UpdatedAt: DateTime
* Relationships: Collection of DeliverableModel

**Business Methods:**
* `Activate()` — Transitions from Draft to Active (enforces deliverable requirement)
* `Complete()` — Transitions from Active to Completed
* `Archive()` — Transitions to Archived state
* `AddDeliverable(deliverable)` — Adds deliverable (enforce contract must be Draft/Active)

#### DeliverableModel
* Identity: Guid Id
* Contract Id: Guid
* Title: string
* Description: string (optional)
* Status: DeliverableStatus enum (Pending/InProgress/Completed/Cancelled)
* DueDate: DateTime (optional)
* CreatedAt: DateTime
* UpdatedAt DateTime

**Business Methods:**
* `ChangeStatus(newStatus, contractStatus) — Updates status with validation

#### ActivityLogModel
* Identity: Guid Id
* EntityType: ActivityEntityType enum (Client/Contract/Deliverable)
* EntityId: Guid
* ActivityType: ActivityType enum (Created/Updated/StatusChanged)
* Description: string
* Timestamp: DateTime

**Characteristics:**
* Immutable (no setter methods)
* Created only via constructor
* Append-only record

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

### 7.1 Service-Based Architecture
Each aggregate has a dedicated service:
* `ClientService` — Client operations
* `ContractService` — Contract operations
* `DeliverableService` — Deliverable operations
* `ActityLogService` — Activity log queries

Services:
* receive validated input (DTOs)
* load entities from DbContext
* invoke domain behavior
* persist changes
* return results via `RequestResult<T>`

No business logic lives in the Application layer.

---

### 7.2 DTOs and Mapping
* DTOs are used at boundaries only
* Domain models are never exposed externally
* Mapping is explicit via static mapper classes
* Each entity has dedicated mapper (ClientMapper, ContractMapper, etc.)

**No AutoMapper** — Manula mapping for clarity and control.

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

### 8.2 Direct DbContext Usage
**No Repository Pattern** — Services use `ScopeTrackDbContext` directly because:
* EF Core DbSet already provides repository-like abstraction
* No need for an extra layer of indirection
* DbContext is mockable for testing if needed
* Simplifies codebase

If multiple ORMs were requires, repositories would make sense.

### 8.3 Concurrency Handling
* Database-level unique constraints (e.g., Client.Email)
* Early validation checks for better UX
* Exception handling for constraint violations
* No optimistic concurrency tokens (yet)

### 8.4 No Soft Deletion
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
* **Client:** Created, Updated(name/email), StatusChanged
* **Contract:** Created, StatusChanged
* **Deliverable:** Created, StatusChanged

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
* **201 Created:** Resource created (includes Location header)
* **400 Bad Request:** Validation failure with structured errors
* **404 Not Found:** Entity not found
* **409 Conflict:** Business rule violation (e.g., invalid status transition)

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

Located in `ScopeTrack.Tests/Domain/Entities/`

### Application Tests
* Use case behavior
* Service interactions
* In-memory database for isolation

Located in `ScopeTrack.Tests/Application/Services/`

### Integration Tests
* API endpoints
* Full request/response cycle
* Uses `TestApiFactory` with in-memory database

Located in `ScopeTrack.Tests/API/Controllers/`

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
* Direct DbContext usage over respository abstraction

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

### 16.3 Why No Repository Pattern?
Services directly use DbContext because:
* EF Core DbSet already provides repository-like abstraction
* No need for an extra layer of indirection
* DbContext is mockable for testing if needed
* Simplifies codebase

If multiple ORMs were required, repositories would make sense.

### 16.4 Why Static Mappers Instead of AutoMapper?
Manual mapping via static methods because:
* Explicit and easier to debug
* No reflection overhead
* Clear mapping logic visible in code
* No configuration conventions to remember
* Simpler for small to medium projects

### 16.5 Why RequestResult<T> Pattern?
Instead of throwing exceptions for expected failures:
* Clearer intent (success vs. failure)
* Better control flow
* Easier to test
* Exceptions reserved for unexpected errors

## 17. Data Flow

### Command Flow (Write Operations)
```text
1. HTTP Request → Controller
2. Controller validates with FluentValidation
3. Controller → Service Method
4. Service → DbContext (load entity)
5. Service → Domain entity (business logic)
6. Service → DbContext (save changes)
7. ActivityLogInterceptor creates logs
8. Response ← Controller (via RequestReault<T>)
```

### Query Flow (Read Operations)
```text
1. HTTP Request → Controller
2. Controller → Service Method
3. Service → DbContext (query with Include for navigation properties)
4. Service → Mapper (entity to DTO)
5. Response ← Controller
```

---

## 18. Database Design

### Entity Framework Core Configuration
* Fluent API for all entity mappings (no attributes)
* Enum conversion to int
* Relationship configurations
* Index definitions
* Unique constraints

### Key Relationships
* `Client` → `Contract` (one-to-many, cascade delete)
* `Contract` → `Deliverable` (one-to-many, cascade delete)
* `ActivityLog` (independent, references entities by ID and type)

### Migration Strategy
* Code-first migrations
* Version controlled schema changes
* Migrations stored in Infrasctructure project

---

## 19. Error Handling

### Exception Strategy
* Domain methods throw `InvalidOperationException` for business rule violations
* Services catch and convert to `RequestResult<T>.Failure()`
* Controllers return appropriate HTTP status code
* Global exceptions handler for unexpected error (implicit in ASP.NET Core)

### HTTP Status Code
* `200 OK` — Successful retrieval or update
* `201 Created` — Resource created (with Location header)
* `400 Bad Request` — Validation error
* `404 Not Found` — Resource not found
* `409 Conflict` — Business rule violation
* `500 Internal Server Error` — Unexpected error

---

## 20. Performance Considerations

### Query Optimization
* Eager loding for related entities via `Include()`
* AsNoTracking for read-only queries
* Projection to DTOs in queries
* Indexed foreign keys
* Composite index on ActivityLog (EntityType, EntityId)

### Async/Await Throughout
* All database operations are async
* All controller actions are async
* Improves scalability

---

## 21. Security Considerations (Future Implementation)

### Authentication
* JWT bearer tokens
* Identity integration
* Role-based access control

### Authorization
* Resource-based authorization
* Ownership verification
* Admin vs. user permissions

### Data Protection
* SQL injection prevention (parameterized queries via EF Core)
* Input sanitization via validation
* HTTPS enforcement
* CORS configuration

---

## 22. CORS Configuration
* Development: `AllowAll` policy for ease of testing
* Production: Should restrict to specific origins

Configured in `Program.cs`:
```csharp
builder.Services.AddCores(options =>
{
  options.AddPolicy("AllowAll", policy =>
  {
    policy.AllowAnyOrigin()
      .AllowAnyMethod()
      .AllowAnyHeaded();
  });
});
```

---

## 23. Dependency Injection

### Service Registration
All dependencies registered in `Program.cs`:
```csharp
// Database
builder.Services.AddDbContext<ScopeTrackDbContext>(options =>
{
  options.UseSqlServer(connectionString);
  options.AddInterceptors(new ActivityLogInterceptor());
});

// Services
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IContractService, ClientService>();
etc.

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<ClientPostDtoValidator>();
```

### Service Lifetime
* **Scoped:** DbContext, Services (per HTTP request)
* **Transient:** Validators (created each time)
* **Singleton:** Configuration, Logging

---

## 24. Development Workflow

## Adding New Features
1. Define domain entities and business methods
2. Create application DTOs
3. Implement service methods
4. Add API endpoints
5. Create validators
6. Write tests
7. Update documentation

### Modifying Existing Features
1. Identify affected layer
2. Update domain logic if needed
3. Modify service methods
4. Update API contracts if needed
5. Update validators
6. Migrate database if required
7. Update tests and documentation

---

## 25. CI/CD Pipeline

### GitHub Actions Workflow
Located in `.github/workflows/ci.yml`:
* Triggers on push to main and pull requests
* Steps:
  1. Checkout repository
  2. Setup .NET 10 SDK
  3. Restore dependencies
  4. Build solution
  5. Runstests with code coverage
  6. Upload coverage to Codecov

### Code Coverage
* Target: High coverage on Domain and Application layer
* Tool: Codecov
* Badge displayed in README

---

## 26. Conclusion
ScopeTrack is designed as:
* a realistic backend system
* a clear demonstration of architectural discipline
* a foundation that could evolve without rework

The project emphasizes **thinking in systems,** not just shipping code.

**Core Strengths:**
* Clean Architecture with explicit boundaries
* Rich domain models with business logic
* Automatic activity logging via interceptors
* Comprehensive testing at all layers
* Clear separation of concerns
* Direct, pragmatic implementations (no over-engineering)

**Intentional Simplicity:**
* No repository pattern (DbContext is sufficient)
* No AutoMapper (static mappers are clearer)
* No CQRS/MediatR (services are sufficient)
* No event sourcing (activity logs are sufficient)

This architecture balances **enterprise patterns** with **pragmatic simplicity** for a system of this scope.
