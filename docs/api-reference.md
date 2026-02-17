# API Reference

## Admin Endpoints

All admin endpoints are under `/_admin/mocks`.

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/_admin/mocks` | List all mocks |
| `GET` | `/_admin/mocks?isActive=true` | List active mocks only |
| `GET` | `/_admin/mocks/{id}` | Get a specific mock |
| `POST` | `/_admin/mocks` | Create a new mock |
| `PUT` | `/_admin/mocks/{id}` | Update a mock |
| `DELETE` | `/_admin/mocks/{id}` | Delete a mock |
| `PATCH` | `/_admin/mocks/{id}/toggle` | Toggle active/inactive |
| `DELETE` | `/_admin/mocks/clear` | Delete all mocks |
| `POST` | `/_admin/mocks/import/curl` | Import from cURL command |
| `POST` | `/_admin/mocks/import/openapi` | Import from OpenAPI spec |

## Mock Response Model

```json
{
  "id": 1,
  "httpMethod": "GET",
  "route": "/api/users",
  "queryString": "?page=1",
  "requestBody": null,
  "statusCode": 200,
  "responseBody": "{\"users\": []}",
  "contentType": "application/json",
  "description": "User list",
  "isActive": true,
  "createdAt": "2026-01-30T10:00:00Z",
  "updatedAt": null
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `httpMethod` | `string` | Yes | HTTP method: `GET`, `POST`, `PUT`, `DELETE`, `PATCH`, `HEAD`, `OPTIONS` |
| `route` | `string` | Yes | Route pattern to match (e.g. `/api/users`) |
| `queryString` | `string` | No | Query string filter (e.g. `?category=electronics`) |
| `requestBody` | `string` | No | Expected request body for matching |
| `statusCode` | `int` | Yes | HTTP status code to return |
| `responseBody` | `string` | Yes | Response body to return |
| `contentType` | `string` | Yes | Content-Type header (e.g. `application/json`) |
| `description` | `string` | No | Human-readable description |
| `isActive` | `bool` | Yes | Whether the mock is active |

## CRUD Examples

### Create a Mock

```http
POST /_admin/mocks
Content-Type: application/json

{
  "httpMethod": "GET",
  "route": "/api/products",
  "statusCode": 200,
  "responseBody": "{\"products\": [{\"id\": 1, \"name\": \"Laptop\"}]}",
  "contentType": "application/json",
  "description": "Product list",
  "isActive": true
}
```

### Update a Mock

```http
PUT /_admin/mocks/1
Content-Type: application/json

{
  "httpMethod": "GET",
  "route": "/api/products",
  "statusCode": 200,
  "responseBody": "{\"products\": [{\"id\": 1, \"name\": \"Updated Laptop\"}]}",
  "contentType": "application/json",
  "description": "Updated product list",
  "isActive": true
}
```

### Toggle Active/Inactive

```http
PATCH /_admin/mocks/1/toggle
```

Response:
```json
{
  "id": 1,
  "isActive": false,
  "message": "Mock deactivated"
}
```

### Delete a Mock

```http
DELETE /_admin/mocks/1
```

### Clear All Mocks

```http
DELETE /_admin/mocks/clear
```

Response:
```json
{
  "message": "All mock responses deleted",
  "deletedCount": 5
}
```

## Import

### From cURL

Parses a cURL command and creates a mock response from it:

```http
POST /_admin/mocks/import/curl
Content-Type: application/json

{
  "curl": "curl -X GET https://api.example.com/users -H 'Accept: application/json'"
}
```

### From OpenAPI / Swagger

Parses an OpenAPI JSON spec and creates mock responses for each endpoint:

```http
POST /_admin/mocks/import/openapi
Content-Type: application/json

{
  "openApiJson": "{ ... OpenAPI 3.0 spec ... }"
}
```

Response:
```json
{
  "message": "Successfully imported 12 mock response(s) from OpenAPI specification.",
  "importedCount": 12,
  "mocks": [ ... ]
}
```

## Using Mock Responses

Any request that is not an admin route is handled by the catch-all controller and matched against active mocks.

If `RoutePrefix` is configured (e.g. `"mock"`), only requests under that prefix are intercepted. The prefix is stripped before matching.

| RoutePrefix | Request | Matches Mock Route |
|---|---|---|
| `""` | `GET /api/users` | `/api/users` |
| `"mock"` | `GET /mock/api/users` | `/api/users` |

### Examples

**List endpoint:**

```http
GET /api/users
```
```json
{
  "users": [
    { "id": 1, "name": "John Doe" },
    { "id": 2, "name": "Jane Smith" }
  ]
}
```

**With query string:**

```http
GET /api/products?category=electronics
```
```json
{
  "products": [
    { "id": 1, "name": "Laptop", "category": "electronics" }
  ]
}
```

**POST request:**

```http
POST /api/users
Content-Type: application/json

{ "name": "New User", "email": "new@example.com" }
```

Response (201):
```json
{
  "id": 3,
  "name": "New User",
  "message": "User created successfully"
}
```

**No match found:**

If no mock matches the request, a 404 is returned with details:

```json
{
  "error": "Mock response not found",
  "request": {
    "method": "GET",
    "path": "/api/unknown",
    "queryString": "",
    "body": null
  },
  "message": "No mock response found for this request. Please add a mock response to the database.",
  "timestamp": "2026-02-16T10:00:00Z"
}
```

## Route Matching Logic

Matching is performed in two phases:

### Phase 1: Exact Match

The request path is compared directly to the mock's `route` field.

```
Request:  GET /api/users
Mock:     route = "/api/users"  →  Match
```

### Phase 2: Contains Fallback

If no exact match is found, mocks whose `route` is **contained within** the request path are matched.

```
Request:  GET /api/users/123/orders
Mock:     route = "/api/users"  →  Match (path contains route)
```

### Additional Filters

After a route match is found, these filters are applied in order:

1. **HTTP Method** — Must match exactly
2. **Query String** — If defined on the mock, must be present in the request
3. **Request Body** — If defined on the mock, must match the request body
4. **IsActive** — Must be `true`

The first mock that passes all filters is returned.
