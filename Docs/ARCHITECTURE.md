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
/src
  /ScopeTack.Domain
  /ScopeTrack.Application
  /ScopeTrack.Infrastructure
  /ScopeTrack.Api

/tests
  /ScopeTrack.Domain.Tests
  /ScopeTrack.Application.Tests

/docs
  architecture.md
  api-contracts.md

ScopeTrack.sln
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
* C# / .NET
* ASP.NET Core
* Entity Framework Core
* xUnit (tests)
* FluentValidation

### Frontend
* Angular
* TypeScript
* Angular Router
* HTTP Client
* Minimal CSS (no UI framework)

### Tooling
* Git
* GitHub Actions (CI)
* REST clients for manual testing

---

## 5. Architectural Foundations

### 5.1 Clean Architecture
The backend follows Clean Architecture principles:
* Domain is independent
* Application orchestrates use cases
* Infrastructure handles external concerns
* API is a thin delivery layer

Dependency direction always points inward.

---

### 5.2 Explicit Boundaries
Each layer has a single responsibility:
| Layer | Responsibility |
|-------|----------------|
| Domain | Business rules and invariants |
| Application | Use cases and orchestration |
| Infrastructure | Persistence and external services |
| API | HTTP boundary and input/output

No shortcuts across layers.

---

## 6. Domain Model

### 6.1 Core Aggregates

#### Client
* Identity
* Name
* Contact information
* Status

#### Contract
* Belongs to a Client
* Contract type (fixed/time-based)
* Collection of Deliverables

#### Deliverable
* Belongs to a Contract
* Title, description, due date
* Status lifecycle

---

### 6.2 Invariants
Examples:
* A contract cannot be activated without at least one deliverable
* A deliverable cannot be completed if the contract is inactive
* Status transitions must be valid and explicit
* Activity logs are immutable

These rules live **only** in the Domain layer.

---

## 7. Application Layer

### 7.1 Use Case Orientation
Each operation is modeled as a use case:
* CreateClient
* CreateContract
* ActivateContract
* AddDeliverable
* ChangeDeliverableStatus

Use cases:
* receive validated input
* load aggregates
* invoke domain behavior
* pesist changes
* return results

No business logic lives in controllers.

---

### 7.2 DTOs and Mapping
* DTOs are used at boundaries only
* Domain models are never exposed externally
* Mapping is explicit (no magic reflection)

---

## 8. Persistence Strategy

### 8.1 Entity Framework Core
* Code-first approach
* Explicit entity mappings
* No lazy loading
* Controlled navigation usage

### 8.2 Soft Deletion
* Entities are never physically deleted
* Soft delete flags preserve history
* Queries filter deleted records by default

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
* Written **only** by the Application layer
* Domain raises *facts*, Application records *events*

This avoids polluting domain logic with persistence concerns.

### 9.2 Usage Rule (Strict)
> If a use case mutates state, it must write **exactly one** activity log entry.

This is enforceable in tests.

---

## 10. API Design

### 10.1 REST Principles
* Resource-oriented endpoints
* Clear HTTP semantics
* Predictable status codes

Example:
```text
POST  /clients
POST  /contracts/{id}/activate
PATCH /deliverables/{id}/status
```

---

### 10.2 Validation
* Input validation at API boundary
* Business validation in Domain
* Clear error responses

No silent failures.

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
* No mocks

### Application Tests
* Use case behavior
* Interaction between layers
* Infrastructure mocked where needed

No UI tests.

---

## 13. Non-Goals (Explicitly Excluded)
* Authentication / authorization
* Billing or payments
* Notifications
* Integrations
* Multi-tenancy
* AI features

Excluding these is a design decision, not a limitation.

---

## 14. Design Reasoning
Key principles guiding decisions:
* Prefer clarity over cleverness
* Favor explicit rules over implicit behavior
* Model business constraints directly
* Keep scope intentionally narrow
* Optimize for maintainability, not novelty

This project values **correctness and structure** over feature count.

---

## 15. Extension Points
The system can be extended by:
* adding authentication
* introducing billing modules
* expanding reporting
* adding integrations

The are consciously deferred.

---

## 16. Conclusion
ScopeTrack is designed as:
* a realistic backend system
* a clear demonstration of architectural discipline
* a foundation that could evolve without rework

The project emphasizes **thinking in systems,** not just shipping code.
