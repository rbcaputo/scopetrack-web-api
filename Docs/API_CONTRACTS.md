# ScopeTrack — API Contracts

## General Conventions
* JSON over HTTP
* UTC timestamps
* GUID identifiers
* Enum values as strings
* All endpoints prefixed with `/api`

---

## Error Responses

### Validation Errors (400 Bad Request)
```json
{
  "errors": [
    "Client email must be a valid email address",
    "Client name is required"
  ]
}
```

### Business Rule Violations (409 Conflict)
Returns plain text error message:
```text
Cannot add contracts to an inactive client
```

### Not Found (404)
Returns plain text error message:
```text
Client not found
```

---

## Clients

### Create Client
**POST** `/api/client`

**Request:**
```json
{
  "name": "Acme Corp",
  "email": "contact@acme.corp"
}
```
**Validation Rules:**
* `name` — Required, 3-100 characters
* `email` — Required, valid email format, 5-100 characters

**Response (201):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Acme Corp",
  "email": "contact@acme.corp,
  "status": "Active",
  "createdAt": "2026-01-13T15:00:00Z",
  "updatedAt": "2026-01-13T15:00:00Z",
  "contracts": []
}
```
**Errors:**
* 400 if validation fails
* 409 if email already exists

**Headers:**
* `Location: /api/client/{id}` – URL of the created resource

---

### Update Client
**PUT** `/api/client/{id}`

**Request:**
```json
{
  "name": "Acme Corporation",
  "email": "info@acme.com"
}
```
**Validation Rules:**
* `name` — Required, 3-100 characters
* `email` — Required, valid email format, 5-100 characters

**Response (200):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Acme Corporation",
  "contactEmail": "info@acme.com",
  "status": "Active",
  "createdAt": "2026-01-13T15:00:00Z",
  "updatedAt": "2026-01-13T20:00:00Z",
  "contracts": []
}
```
**Errors:**
* 400 if validation fails
* 404 if client not found

---

### Toggle Client Status
**PATCH** `/api/client/{id}`

**Response (200):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Acme Corp",
  "email": "contact@acme.corp",
  "status": "Inactive",
  "createdAt": "2026-01-13T15:00:00Z",
  "updatedAt": "2026-01-13T20:00:00Z",
  "contracts": []
```
**Errors:**
* 404 if client not found

**Notes:**
* Toggles between `"Active"` and `"Inactive"`
* No request body needed

---

### Add Contract to Client
**POST** `/api/client/{id}/contracts`

**Request:**
```json
{
  "title": "Website Redesign",
  "description": "Marketing site overhaul",
  "type": "FixedPrice"
}
```
**Validation Rules:**
* `title` — Required, 10-20 characters
* `description` — Optional, max 1000 characters
* `type` — Required, must be `"FixedPrice"` or `"TimeBased"`

**Response (201):**
```json
{
  "id": "7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d",
  "clientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Website Redesign",
  "description": "Marketing site overhaul",
  "type": "FixedPrice",
  "status": "Draft",
  "createdAt": "2026-01-13T16:00:00Z",
  "updatedAt": "2026-01-13T16:00:00Z",
  "deliverables": []
}
```
**Errors:**
* 400 if validation fails
* 404 if client not found
* 409 if client is inactive

**Headers:**
* `Location: /api/contract/{id}` — URL of the created resource

**Notes:**
* Contract type must be `"FixedPrice"` or `"TimeBased"`
* Initial status is always `"Draft"`
* Description is optional
* Client must be Active to add contracts

---

### Get Client by ID
***GET** `/api/client/{id}`

**Response(200):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Acme Corp",
  "email": "contact@acme.corp",
  "status": "Active",
  "createdAt": "2026-01-13T15:00:00Z",
  "updatedAt": "2026-01-13T15:00:00Z",
  "contracts": [
    {
      "id": "7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d",
      "clientID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "title": "Website Redesign",
      "description": "Complete website overhaul",
      "type": "FixedPrice",
      "status": "Draft",
      "createdAt": "2026-01-13T16:00:00Z",
      "updatedAt": "2026-01-13T16:00:00Z",
      "deliverables": []
    }
  ]
}
```
**Errors:**
* 404 if client not found

---

### Get All Clients
**GET** `/api/client`

**Response (200):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Acme Corp",
    "email": "contact@acme.corp",
    "status": "Active",
    "createdAt": "2026-01-13T15:00:00Z",
    "updatedAt": "2026-01-13T15:00:00Z",
    "contracts": []
  }
]
```
**Notes:**
* Returns clients ordered by status (Active first)
* Includes contracts collection

---

## Contracts

### Update Contract Status
**PATCH** `/api/contract/{id}`

**Request:**
```json
{
  "newStatus": "Active"
}
```
**Validation Rules:**
* `newStatus` — Required, must be `"Active"`, `"Completed"`, or `"Archived"`

**Response (200):**
```json
{
  "id": "7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d",
  "clientID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Website Redesign",
  "description": "Marketing site overhaul",
  "type": "FixedPrice",
  "status": "Active",
  "createdAt": "2026-01-13T16:00:00Z",
  "updatedAt": "2026-01-13T17:00:00Z",
  "deliverables": []
}
```
**Errors:**
* 400 if validation fails or invalid status transition
* 404 if contract not found
* 409 if invalid status transition

**Valid status transitions:**
* `"Draft"` → `"Active"` (requites at least one deliverable)
* `"Active"` → `"Completed"`
* `"Active"` → `"Archived"`
* `"Draft"` → `"Archived"`
* `"Completed"` → `"Archived"`

**Invalid transitions:**
* Cannot activate contract without deliverables
* Cannot change status from `"Archived"`
* Cannot activate `"Completed"` contracts
* Cannot complete `"Draft"` contracts

---

### Add Deliverable to Contract
**POST** `/api/contract/{id}/deliverables`

**Request:**
```json
{
  "title": "Homepage Design",
  "description": "Design new homepage layout",
  "dueDate": "2026-02-01T00:00:00Z"
}
```
**Validation Rules:**
* `title` — Requires, 10-200 characters
* `description` — Optional, max 500 characters
* `dueDate` — Optional, valid ISO 8601 datetime

**Response:**
```json
{
  "id": "9c1d2e3f-4a5b-6c7d-8e9f-0a1b2c3d4e5f",
  "contractId": "7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d",
  "title": "Homepage Design",
  "description": "Design new homepage layout",
  "status": "Pending",
  "dueDate": "2026-02-01T00:00:00Z",
  "createdAt": "2026-01-17T13:00:00Z",
  "updatedAt": "2026-01-17T13:00:00Z"
}
```
**Errors:**
* 400 if validation fails
* 404 if contract not found
* 409 if contract is completed or archived

**Headers:**
* `Location: /api/deliverable/{id}` — URL of the created resource

**Notes:**
* Initital status is always `"Pending"`
* Due date is optional
* Cannot add deliverables to completed or archived contracts

---

### Get Contract by ID
**GET** `/api/contract/{id}`

**Response(200):**
```json
{
  "id": "7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d",
  "clientID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Website Redesign",
  "description": "Marketing site overhaul",
  "type": "FixedPrice",
  "status": "Active",
  "createdAt": "2026-01-13T16:00:00Z",
  "updatedAt": "2026-01-13T17:00:00Z",
  "deliverables": [
    {
      "id": "9c1d2e3f-4a5b-6c7d-8e9f-0a1b2c3d4e5f",
      "contractID": "7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d",
      "title": "Homepage Design",
      "description": "Design new homepage layout",
      "status": "Pending",
      "dueDate": "2026-02-01T00:00:00Z",
      "createdAt": "2026-01-13T17:00:00Z",
      "updatedAt": "2026-01-13T17:00:00Z"
    }
  ]
}
```
**Errors:**
* 404 if contract not found

---

### Get All Contracts
**GET** `/api/contract`

**Response (200):**
```json
[
  {
    "id": "7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d",
    "clientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Website Redesign",
    "description": "Marketing site overhaul",
    "type": "FixedPrice",
    "status": "Active",
    "createdAt": "2026-01-13T16:00:00Z",
    "updatedAt": "2026-01-13T17:00:00Z",
    "deliverables": []
  }
]
```
**Notes:**
* Include deliverables collection

---

## Deliverables

### Update Deliverable Status
**PATCH** `/api/deliverables/{id}`

**Request:**
```json
{
  "status": "InProgress"
}
```
**Validation Rules:**
* `newStatus` — Required, must be `"Pending"`, `"InProgress"`, `"Completed"`, or `"Cancelled"`

**Response (200):**
```json
{
  "id": "9c1d2e3f-4a5b-6c7d-8e9f-0a1b2c3d4e5f",
  "contractID": "7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d",
  "title": "Homepage Design",
  "description": "Design new homepage layout",
  "status": "InProgress",
  "dueDate": "2026-02-01T00:00:00Z",
  "createdAt": "2026-01-17T13:00:00Z",
  "updatedAt": "2026-01-17T14:00:00Z"
}
```
**Errors:**
* 400 if validation fails or invalid status value
* 404 if deliverable or parent contract not found
* 409 if invalid status transition

**Valid status transitions:**
* `"Pending"` → `"InProgress"`
* `"Pending"` → `"Cancelled"`
* `"InProgess"` → `"Completed"`
* `"InProgress"` → "`Cancelled"`

**Invalid transitions (will return 409):**
* `"Completed"` → any status (terminal state)
* `"Cancelled"` → any status (terminal state)
* `"InProgress"` → `"Pending"` (cannot rollback)
* `"Pending"` → `"Completed"` (must go through InProgress)
* Any transtition when parent contract is not `"Active"`

---

### Get Deliverable by ID
**GET** `api/deliverable/{id}`

**Response (200):**
```json
  "id": "9c1d2e3f-4a5b-6c7d-8e9f-0a1b2c3d4e5f",
  "contractID": "7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d",
  "title": "Homepage Design",
  "description": "Design new homepage layout",
  "status": "InProgress",
  "dueDate": "2026-02-01T00:00:00Z",
  "createdAt": "2026-01-17T13:00:00Z",
  "updatedAt": "2026-01-17T14:00:00Z"
```
**Errors:**
* 404 if deliverable not found

---

## Activity Logs (Read-Only)

### Get Activity Logs for Entity
**GET** `/api/activitylog/{entityType}/{entityId}`

**Path Parameters:**
* `entityType` — Type of entity ("Client", "Contract", or "Deliverable")
* `entityId` — GUID of the entity

**Response (200):**
```json
[
  {
    "entityType": "Contract",
    "activityType": "Created",
    "description": "Contract 'Website Redesign' created",
    "occurredAt": "2026-01-17T13:00:00Z"
  },
  {
    "entityType": "Contract",
    "activityType": "StatusChanged",
    "description": "Contract 'Website Redesign' status changed to Active",
    "occurredAt": "2026-01-17T14:00:00Z"
  }
]
```
**Errors:**
* 400 if invalid entity type
* 404 if entity not found

**Notes:**
* Retuns logs ordered by timestamp (newest first)
* Activity logs are created automatically via EF Core interceptor
* No way to create, update, or delete activity logs manually
* Valid entity types: `"Client"`, `"Contract"`, `"Deliverable"`

---

## Status Enums

### ClientStatus
* `Active`
* `Inactive`

### ContractStatus
* `Draft`
* `Active`
* `Completed`
* `Archived`

### DeliverableStatus
* `Pending`
* `InProgress`
* `Completed`
* `Cancelled`

### ContractType
* `FixedPrice`
* `TimeBased`

### ActivityType
* `Created`
* `Updated`
* `StatusChanged`

### ActivityEntityType
* `"Client`
* `Contract`
* `Deliverable`

---

## Business Rules Summary

### Client Rules
* Email must be unique across all clients
* Client start as "Active"
* Inactive clients cannot have contracts added
* Toggling status switches between Active/Inactive

### Contract Rules
* Contracts start in "Draft" status
* Must have at least one deliverable to activate
* Cannot activate completed or archived contracts
* Can only add deliverables to "Draft" or "Active" contracts
* Archived status is final (no transitions out)

### Deliverable Rules
* Deliverables start in "Pending" status
* Can only change status when parent contract is "Active"
* "Completed" and "Canceled" are terminal states
* Must progress through "InProgress" to reach "Completed"

### Activity Log Rules
* Automatically created by system
* Immutable records
* Track all entity creation and status changes
* Cannot be manually created, updated, or deleted

---

## HTTP Status Codes Reference

| Code | Meaning | Used When |
|------|---------|-----------|
| 200 | OK | Successful GET, PUT, or PATCH |
| 201 | Created | Successful POST |
| 400 | Bad Request | Validation error or invalid input |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Business rule violation |
| 500 | Internal Server Error | Unexpected server error |

## Common Error Mesaages

### Validation Error (400)
```json
{
  "errors": [
    "Client name is required",
    "Client name must have at least 3 characters",
    "Client email must be a valid email address",
    "Contract title must have at least 10 characters",
    "Contract type must be either 'FixedPrice' or 'TimeBased'",
    "Deliverable title is required"
  ]
}
```

### Business Rule Violations (409)
Plain text responses:
```text
Cannot add contracts to an inactive client
Cannot activate contract without at least one deliverable
Cannot activate a completed contract
Cannot activate an archived contract
Cannot complete a drafted contract
Cannot complete an archived contract
Cannot add deliverables to a completed contract
Cannot add deliverables to an archived contract
Cannot change deliverable status when contract is not active
Cannot change status of a completed deliverable
Cannot change status of a cancelled deliverable
Deliverable status is already {status}
Invalid status transition from {current} to {new}
```

### Not Found (404)
Plain text responses:
```text
Client not found
Contract not found
Deliverable not found
Entity not found
```

---

## Request/Response Examples

### Example 1: Create Client and Add Contract

#### Step 1: Create Client
```http
POST /api/client
Content-Type: application/json

{
  "name": "Acme Corp",
  "email": "contact@acme.com"
}
```
**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Acme Corp",
  "email": "contact@acme.com",
  "status": "Active",
  "createdAt": "2026-01-17T10:00:00Z",
  "updatedAt": "2026-01-17T10:00:00Z",
  "contracts": []
}
```

#### Step 2: Add Contract
```http
POST api/client/3fa85f64-5717-4562-b3fc-2c963f66afa6/contracts
Content-Type: application/json

{
  "title": "Website Development",
  "description": "New corporate website",
  "type": "FixedPrice"
}
```
**Response:
```json
{
  "id": "7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d",
  "clientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Website Development",
  "description": "New corporate website",
  "type": "FixedPrice",
  "status": "Draft",
  "createdAt": "2026-01-17T10:05:00Z",
  "updatedAt": "2026-01-17T10:05:00Z",
  "deliverables": []
}
```

---

### Example 2: Activate Contract

#### Step 1: Add Deliverable
```http
POST /api/contract/7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d/deliverables
Content-Type: application/json

{
  "title": "Homepage Design",
  "description": "Design homepage mockups",
  "dueDate": "2026-02-15T00:00:00Z"
}
```
**Response:**
```json
{
  "id": "9c1d2e3f-4a5b-6c7d-8e9f-0a1b2c3d4e5f",
  "contractId": "7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d",
  "title": "Homepage Design",
  "description": "Design homepage mockups",
  "status": "Pending",
  "dueDate": "2026-02-15T00:00:00Z",
  "createdAt": "2026-01-17T10:08:00Z",
  "updatedAt": "2026-01-17T10:08:00Z"
}
```

### Step 2: Activate Contract
```http
PATCH /api/contract/7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d
Content-Type: application/json

{
  "newStatus": "Active"
}
```
**Response:**
```json
{
  "id": "7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d",
  "clientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Website Development",
  "description": "New corporate website",
  "type": "FixedPrice",
  "status": "Active",
  "createdAt": "2026-01-17T10:05:00Z",
  "updatedAt": "2026-01-17T10:10:00Z",
  "deliverables": [
    {
      "id": "9c1d2e3f-4a5b-6c7d-8e9f-0a1b2c3d4e5f",
      "contractId": "7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d",
      "title": "Homepage Design",
      "description": "Design homepage mockups",
      "status": "Pending",
      "dueDate": "2026-02-15T00:00:00Z",
      "createdAt": "2026-01-17T10:08:00Z",
      "updatedAt": "2026-01-17T10:08:00Z"
    }
  ]
}
```

---

### Example 3: Update Deliverable Status
```http
PATCH /api/deliverable/9c1d2e3f-4a5b-6c7d-8e9f-0a1b2c3d4e5f
Content-Type: application/json

{
  "newStatus": "InProgress"
}
```
Response:
```json
{
  "id": "9c1d2e3f-4a5b-6c7d-8e9f-0a1b2c3d4e5f",
  "contractId": "7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d",
  "title": "Homepage Design",
  "description": "Design homepage mockups",
  "status": "InProgress",
  "dueDate": "2026-02-15T00:00:00Z",
  "createdAt": "2026-01-17T10:08:00Z",
  "updatedAt": "2026-01-17T11:00:00Z"
}
```

---

### Example 4: Get Activity Logs
```http
GET /api/activitylog/contract/7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d
```
**Response:**
```json
[
  {
    "entityType": "Contract",
    "activityType": "Created",
    "description": "Contract 'Website Development' created",
    "occurredAt": "2026-01-17T10:05:00Z"
  },
  {
    "entityType": "Contract",
    "activityType": "StatusChanged",
    "description": "Contract 'Website Development' status changed to Active",
    "occurredAt": "2026-01-17T10:10:00Z"
  }
]
```

