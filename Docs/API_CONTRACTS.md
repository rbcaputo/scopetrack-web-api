# ScopeTrack — API Contracts

## General Conventions
* JSON over HTTP
* UTC timestamps
* GUID identifiers
* Enum values as strings

Errors return:
```json
{
  "error": "ValidationError",
  "message": "Name must not be empty"
}
```

---

## Clients

### Create Client
**POST** `/api/clients`

Request:
```json
{
  "name": "Acme Corp",
  "contactEmail": "contact@acme.corp"
}
```
Response (201):
```json
{
  "id": "guid",
  "name": "Acme Corp",
  "status": "Active",
  "createdAt": "2026-01-13T15:00:00Z"
}
```

---

### Get Client
**GET** `/api/clients/{id}`

Response (200):
```json
{
  "id": "guid",
  "name": "Acme Corp",
  "contactEmail": "contact@acme.corp",
  "status": "Active",
  "createdAt": "2026-01-13T15:00:00Z",
  "updatedAt": "2026-01-13T15:00:00Z"
}
```

---

## Contracts

### Create Contract
**POST** `/api/clients/{clientId}/contracts`

Request:
```json
{
  "title": "Website Redesign",
  "description": "Marketing site overhaul",
  "type": "FixedPrice"
}
```
Response (201):
```json
{
  "id": "guid",
  "clientId": "guid",
  "status": "Draft"
}
```

---

### Activate Contract
**POST** `/api/contracts/{id}/activate`

Response (200):
```json
{
  "id": "guid",
  "status": "Active"
}
```
Errors:
* 400 if no deliverables exist
* 409 if already active or archived

---

## Deliverables

### Add Deliverable
**POST** `/api/contracts/{contractId}/deliverables`

Request:
```json
{
  "title": "Landing Page",
  "description": "Homepage redesign",
  "dueDate": "2026-02-01T00:00:00Z"
}
```
Response (201):
```json
{
  "id": "guid",
  "status": "Planned"
}
```

---

### Change Deliverable Status
**PATCH** `/api/deliverables/{id}/status`

Request:
```json
{
  "status": "InProgress"
}
```
Response (200):
```json
{
  "id": "guid",
  "status": "InProgress"
}
```
Invalid transitions return 400.

---

## Activity Logs (Read-Only)

### Get Activity for Entity
**GET** `/api/activity?entityType=Contract&entityId={id}`

Response:
```json
[
  {
    "activityType": "Activated",
    "description": "Contract activated",
    "occurredAt": "2026-01-14T10:00:00Z"
  }
]
```
