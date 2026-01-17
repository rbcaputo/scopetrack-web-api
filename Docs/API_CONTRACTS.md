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
[
  {
    "field": "Email",
    "message": "Client email must be a valid email address"
  },
  {
    "field": "Name",
    "message": "Client name is required"
  }
]
```

### Business Rule Violations (409 Conflict)
```json
{
  "error": "Client already exists"
}
```

### Not Found (404)
```json
{
  "error": "Client not found"
}
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
  "name": Acme Corporation,
  "email": "info@acme.corp"
}
```
**Response (200):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Acme Corporation",
  "contactEmail": "info@acme.corp",
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

---

## Contracts

### Add Contract to Client
**POST** `/api/client/{id}/contracts`

**Request:**
```json
{
  "clientID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Website Redesign",
  "description": "Marketing site overhaul",
  "type": "FixedPrice"
}
```
**Response (201):**
```json
{
  "id": "7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d",
  "clientID": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
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

**Headers:**
* `Location: /api/contract/{id}` – URL of the created resource

**Notes:**
* Contract type must be `"FixedPrice"` or `"TimeBased"`
* Initial status is always `"Draft"`
* Description is optional

---

### Update Contract Status
**PATCH** `/api/contract/{id}`

**Request:**
```json
{
  "newStatus": "Active"
}
```
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

## Deliverables

### Add Deliverable to Contract
**POST** `/api/contract/{id}/deliverables`

**Request:**
```json
{
  "contractID": "7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d",
  "title": "Homepage Design",
  "description": "Design new homepage layout",
  "dueDate": "2026-02-01T00:00:00Z"
}
```
**Response (201):**
```json
{
  "id": "9c1d2e3f-4a5b-6c7d-8e9f-0a1b2c3d4e5f",
  "contractID": "7b2a1c8d-3e4f-5a6b-7c8d-9e0f1a2b3c4d",
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

**Headers:**
* `Location: /api/deliverable/{id}` – URL of the created resource

**Notes:**
* Initial status is always `"Pending"`
* Due date is optional
* Cannot add deliverables to archived contracts

---

### Update Deliverable Status
**PATCH** `/api/deliverables/{id}`

**Request:**
```json
{
  "status": "InProgress"
}
```
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
* 400 if validation fails or invalid status transition
* 404 if deliverable or parent contract not found

**Valid status transitions:**
* `"Pending"` → `"InProgress"`
* `"InProgess"` → `"Completed"`
* `"InProgress"` → "`Pending"` (rollback)

**Invalid transitions:**
* `"Completed"` → `"Cancelled"`
* `"Cancelled"` → `"Completed"`
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
**GET** `/api/activitylog/{entityId}`

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
**Notes:**
* Retuns logs ordered by timestamp (newest first)
* Activity logs are created automatically via EF Core interceptor
* No way to create, update, or delete activity logs manually

---

### Get All Activity Logs
**GET**  `/api/activitylog`

**Response (200):**
```json
[
  {
    "entityType": "Client",
    "activityType": "Created",
    "description": "Client 'Acme Corp' created",
    "occurredAt": "2026-01-17T13:00:00Z"
  },
  {
    "entityType": "Contract",
    "activityType": "Created",
    "description": "Contract 'Website Redesign' created",
    "occurredAt": "2026-01-17T14:00:00Z"
  }
]
```
**Notes:**
* Returns all logs ordered by timestamp (newest first)
* Use this for audit trail or system-wide activity monitoring

---

## Status Enums

### ClientStatus
* `"Active"`
* `"Inactive"`

### ContractStatus
* `"Draft"`
* `"Active"`
* `"Completed"`
* `"Archived"`

### DeliverableStatus
* `"Pending"`
* `"InProgress"`
* `"Completed"`
* `"Cancelled"`

### ContractType
* `"FixedPrice"`
* `"TimeBased"`

### ActivityType
* `"Created"`
* `"Updated"`
* `"StatusChanged"`

### ActivityEntityType
* `"Client"`
* `"Contract"`
* `"Deliverable"`
