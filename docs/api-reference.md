# API Reference

## Admin Endpoints

### Mock Management (`/_admin/mocks`)

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/_admin/mocks` | List all mocks |
| `GET` | `/_admin/mocks?isActive=true` | List active mocks only |
| `GET` | `/_admin/mocks?collectionId=1` | List mocks by collection |
| `GET` | `/_admin/mocks/{id}` | Get a specific mock (includes rules & sequences) |
| `POST` | `/_admin/mocks` | Create a new mock |
| `PUT` | `/_admin/mocks/{id}` | Update a mock |
| `DELETE` | `/_admin/mocks/{id}` | Delete a mock |
| `PATCH` | `/_admin/mocks/{id}/toggle` | Toggle active/inactive |
| `DELETE` | `/_admin/mocks/clear` | Delete all mocks |
| `POST` | `/_admin/mocks/import/curl` | Import from cURL command |
| `POST` | `/_admin/mocks/import/openapi` | Import from OpenAPI spec |
| `POST` | `/_admin/mocks/{id}/sequence/reset` | Reset sequence counter for a mock |
| `POST` | `/_admin/mocks/sequence/reset-all` | Reset all sequence counters |

### Collection Management (`/_admin/collections`)

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/_admin/collections` | List all collections with mock counts |
| `GET` | `/_admin/collections/{id}` | Get a collection with its mocks |
| `POST` | `/_admin/collections` | Create a new collection |
| `PUT` | `/_admin/collections/{id}` | Update a collection |
| `DELETE` | `/_admin/collections/{id}` | Delete a collection (mocks keep, CollectionId set to null) |
| `POST` | `/_admin/collections/{id}/export` | Export collection as JSON |
| `POST` | `/_admin/collections/import` | Import collection from JSON |

### Request Log Management (`/_admin/logs`)

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/_admin/logs` | List request logs (paginated) |
| `GET` | `/_admin/logs?method=POST&isMatched=false` | Filter logs |
| `GET` | `/_admin/logs/{id}` | Get a specific log entry |
| `GET` | `/_admin/logs/count?minutes=5` | Count recent logs |
| `DELETE` | `/_admin/logs/clear` | Clear all logs |

**Log query parameters:**

| Param | Type | Description |
|---|---|---|
| `method` | string | Filter by HTTP method (GET, POST, etc.) |
| `statusCode` | int | Filter by response status code |
| `isMatched` | bool | Filter matched/unmatched requests |
| `from` | datetime | Start date filter |
| `to` | datetime | End date filter |
| `page` | int | Page number (default: 1) |
| `pageSize` | int | Items per page (default: 50) |

---

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
  "delayMs": null,
  "collectionId": null,
  "isSequential": false,
  "isActive": true,
  "createdAt": "2026-01-30T10:00:00Z",
  "updatedAt": null,
  "rules": [],
  "sequenceItems": []
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `httpMethod` | `string` | Yes | HTTP method: `GET`, `POST`, `PUT`, `DELETE`, `PATCH`, `HEAD`, `OPTIONS` |
| `route` | `string` | Yes | Route pattern to match (e.g. `/api/users`) |
| `queryString` | `string` | No | Query string filter (e.g. `?category=electronics`) |
| `requestBody` | `string` | No | Expected request body for matching |
| `statusCode` | `int` | Yes | HTTP status code to return |
| `responseBody` | `string` | Yes | Response body to return (supports template variables) |
| `contentType` | `string` | Yes | Content-Type header (e.g. `application/json`) |
| `description` | `string` | No | Human-readable description |
| `delayMs` | `int?` | No | Response delay in milliseconds |
| `collectionId` | `int?` | No | Parent collection ID |
| `isSequential` | `bool` | No | Enable sequential response mode |
| `isActive` | `bool` | Yes | Whether the mock is active |

---

## Collections

Collections group related mocks for organization. Each collection has a name, description, and color for visual identification in the UI.

### Create a Collection

```http
POST /_admin/collections
Content-Type: application/json

{
  "name": "Payment APIs",
  "description": "All payment-related endpoints",
  "color": "#6366f1"
}
```

### Import a Collection

Import a collection with its mocks in a single request:

```http
POST /_admin/collections/import
Content-Type: application/json

{
  "collection": {
    "name": "User APIs",
    "description": "User management endpoints",
    "color": "#22c55e"
  },
  "mocks": [
    {
      "httpMethod": "GET",
      "route": "/api/users/profile",
      "statusCode": 200,
      "responseBody": "{\"id\": 1, \"name\": \"Mehmet\"}",
      "contentType": "application/json",
      "description": "User profile",
      "isActive": true
    }
  ]
}
```

### Export a Collection

```http
POST /_admin/collections/3/export
```

Returns the collection and its mocks in the same format as import, making it easy to share between environments.

---

## Conditional Response Rules

Rules let a single mock endpoint return different responses based on the incoming request. Rules are evaluated in **priority order** (ascending) and the **first match wins**. If no rule matches, the default mock response is returned.

> Rules and sequential mode are mutually exclusive. If `isSequential` is true, rules are not evaluated.

### Rule Model

```json
{
  "conditionField": "header.Authorization",
  "conditionOperator": "notExists",
  "conditionValue": null,
  "statusCode": 401,
  "responseBody": "{\"error\": \"Token required\"}",
  "contentType": "application/json",
  "priority": 0
}
```

### Condition Fields

| Field Format | Source | Example |
|---|---|---|
| `header.HeaderName` | Request header | `header.Authorization`, `header.X-Api-Key` |
| `query.paramName` | Query string parameter | `query.page`, `query.category` |
| `body.propertyPath` | JSON body (dot-notation) | `body.amount`, `body.user.address.city` |
| `method` | HTTP method | Matches against `GET`, `POST`, etc. |
| `path` | Request path | Matches against the URL path |

### Condition Operators

| Operator | Description | Example |
|---|---|---|
| `equals` | Exact match (case-insensitive) | `header.Authorization` equals `Bearer valid-token` |
| `contains` | Substring match | `body.name` contains `John` |
| `startsWith` | Prefix match | `header.Authorization` startsWith `Bearer` |
| `endsWith` | Suffix match | `path` endsWith `/details` |
| `regex` | Regular expression | `header.User-Agent` regex `.*Chrome.*` |
| `exists` | Field is present | `header.Authorization` exists |
| `notExists` | Field is absent | `header.Authorization` notExists |
| `greaterThan` | Numeric comparison | `body.amount` greaterThan `10000` |
| `lessThan` | Numeric comparison | `body.amount` lessThan `0` |

### Example: Auth-Based Responses

Create a mock for `GET /api/secure-data` with these rules:

| Priority | Condition | Response |
|---|---|---|
| 0 | `header.Authorization` notExists | 401 `{"error": "Token required"}` |
| 1 | `header.Authorization` equals `Bearer expired` | 403 `{"error": "Token expired"}` |
| - | *(default, no rule match)* | 200 `{"data": "secret"}` |

```bash
# No token -> 401
curl http://localhost:5000/api/secure-data

# Expired token -> 403
curl -H "Authorization: Bearer expired" http://localhost:5000/api/secure-data

# Valid token -> 200
curl -H "Authorization: Bearer valid-token" http://localhost:5000/api/secure-data
```

### Example: Body-Based Responses

Create a mock for `POST /api/payments` with rules:

| Priority | Condition | Response |
|---|---|---|
| 0 | `body.amount` greaterThan `10000` | 400 `{"error": "Daily limit exceeded"}` |
| 1 | `body.currency` equals `BTC` | 422 `{"error": "Unsupported currency"}` |
| - | *(default)* | 200 `{"status": "approved"}` |

```bash
# Normal payment -> 200
curl -X POST http://localhost:5000/api/payments \
  -H "Content-Type: application/json" \
  -d '{"amount": 500, "currency": "TRY"}'

# High amount -> 400
curl -X POST http://localhost:5000/api/payments \
  -H "Content-Type: application/json" \
  -d '{"amount": 15000, "currency": "TRY"}'

# Unsupported currency -> 422
curl -X POST http://localhost:5000/api/payments \
  -H "Content-Type: application/json" \
  -d '{"amount": 500, "currency": "BTC"}'
```

---

## Sequential Responses

Sequential mode makes a mock cycle through a series of responses. Each request returns the next step in the sequence, wrapping around to the beginning when the end is reached. Sequence state is held **in-memory** and resets on application restart.

> Sequential mode and rules are mutually exclusive. When `isSequential` is true, rules are skipped.

### Sequence Item Model

```json
{
  "order": 0,
  "statusCode": 500,
  "responseBody": "{\"error\": \"Internal Server Error\"}",
  "contentType": "application/json",
  "delayMs": null
}
```

### Example: Retry Testing

A mock for `POST /api/orders` with `isSequential: true` and three steps:

| Step | Status | Body |
|---|---|---|
| 0 | 500 | `{"error": "Internal Server Error"}` |
| 1 | 503 | `{"error": "Service Unavailable"}` |
| 2 | 200 | `{"orderId": "ORD-123", "status": "created"}` |

```bash
curl -X POST http://localhost:5000/api/orders   # -> 500
curl -X POST http://localhost:5000/api/orders   # -> 503
curl -X POST http://localhost:5000/api/orders   # -> 200 (success!)
curl -X POST http://localhost:5000/api/orders   # -> 500 (wrap-around)
```

### Example: Rate Limiting

| Step | Status | Body |
|---|---|---|
| 0 | 200 | `{"data": "OK"}` |
| 1 | 200 | `{"data": "OK"}` |
| 2 | 429 | `{"error": "Too Many Requests", "retryAfter": 60}` |

### Resetting Sequences

```bash
# Reset a specific mock's sequence counter
curl -X POST http://localhost:5000/_admin/mocks/5/sequence/reset

# Reset all sequence counters
curl -X POST http://localhost:5000/_admin/mocks/sequence/reset-all
```

---

## Dynamic Template Variables

Response bodies support template variables using the `{{$variable}}` syntax. Variables are processed at request time, generating fresh values on every call.

### Built-in Variables

| Variable | Output Example | Description |
|---|---|---|
| `{{$randomUUID}}` | `a1b2c3d4-e5f6-7890-abcd-ef1234567890` | Random UUID v4 |
| `{{$timestamp}}` | `1708207800` | Unix timestamp (seconds) |
| `{{$isoTimestamp}}` | `2026-02-19T08:30:00.0000000Z` | ISO 8601 timestamp |
| `{{$randomInt}}` | `458231` | Random integer (1 - 1,000,000) |
| `{{$randomInt(1,100)}}` | `73` | Random integer in range |
| `{{$randomFloat}}` | `456.78` | Random float (2 decimal places) |
| `{{$randomBool}}` | `true` or `false` | Random boolean |
| `{{$randomName}}` | `Alice Johnson` | Random name from sample list |
| `{{$randomEmail}}` | `bob.smith@mock.io` | Random email |

### Request Variables

| Variable | Output Example | Description |
|---|---|---|
| `{{$request.path}}` | `/api/users/123` | Request path |
| `{{$request.method}}` | `POST` | HTTP method |
| `{{$request.body}}` | `{"key": "value"}` | Full request body |
| `{{$request.query.paramName}}` | `electronics` | Specific query parameter |
| `{{$request.header.headerName}}` | `Bearer token123` | Specific request header |

### Example: Dynamic User Response

Configure a mock's response body as:

```json
{
  "id": "{{$randomUUID}}",
  "name": "{{$randomName}}",
  "email": "{{$randomEmail}}",
  "age": {{$randomInt(18,65)}},
  "premium": {{$randomBool}},
  "createdAt": "{{$isoTimestamp}}"
}
```

Each request produces different values:

```bash
curl http://localhost:5000/api/users/random
# {"id":"42c178af-...","name":"Ivan Petrov","email":"rachel.green@sample.net","age":33,...}

curl http://localhost:5000/api/users/random
# {"id":"eb018745-...","name":"Laura Palmer","email":"charlie.brown@demo.dev","age":51,...}
```

### Example: Request Echo

```json
{
  "echo": {
    "method": "{{$request.method}}",
    "path": "{{$request.path}}",
    "auth": "{{$request.header.Authorization}}"
  },
  "requestId": "{{$randomUUID}}",
  "processedAt": "{{$isoTimestamp}}"
}
```

> Multiple occurrences of the same variable in one response produce different values. Two `{{$randomUUID}}` fields will have different UUIDs.

---

## Response Delays

Add artificial latency to mock responses for testing timeout handling, loading states, and slow network scenarios.

### Mock-Level Delay

Set `delayMs` on the mock to apply a delay to all requests:

```json
{
  "httpMethod": "GET",
  "route": "/api/reports/heavy",
  "statusCode": 200,
  "responseBody": "{\"report\": \"data\"}",
  "delayMs": 3000,
  "isActive": true
}
```

```bash
curl http://localhost:5000/api/reports/heavy
# Response arrives after ~3 seconds
```

### Sequence-Step Delay

Each sequence step can override the mock-level delay:

| Step | Status | DelayMs | Behavior |
|---|---|---|---|
| 0 | 200 | 100 | Fast |
| 1 | 200 | 2000 | Slower |
| 2 | 200 | 5000 | Very slow |

Sequence-step delay takes priority over mock-level delay. If a step has no `delayMs`, the mock-level delay is used as fallback.

---

## Request Logging

Every request hitting the mock server is logged to the database, whether or not it matches a mock. This is useful for debugging why requests aren't matching, verifying that requests are reaching the server, and measuring response times.

### Log Entry Model

```json
{
  "id": 49,
  "httpMethod": "GET",
  "route": "/api/users/random",
  "queryString": null,
  "requestBody": null,
  "requestHeaders": "{\"Accept\":\"*/*\",\"Host\":\"localhost:5000\"}",
  "matchedMockId": 22,
  "matchedMockDescription": "Dynamic user",
  "responseStatusCode": 200,
  "isMatched": true,
  "timestamp": "2026-02-19T08:10:32.078Z",
  "responseTimeMs": 45
}
```

### Querying Logs

```bash
# All logs (paginated)
curl "http://localhost:5000/_admin/logs?page=1&pageSize=20"

# Only unmatched requests (debug "why 404?")
curl "http://localhost:5000/_admin/logs?isMatched=false"

# Only POST requests
curl "http://localhost:5000/_admin/logs?method=POST"

# Count requests in last 5 minutes
curl "http://localhost:5000/_admin/logs/count?minutes=5"

# Clear all logs
curl -X DELETE "http://localhost:5000/_admin/logs/clear"
```

### Debugging with Logs

Common scenario: *"My test keeps getting 404, but I configured the mock."*

1. Check logs with `isMatched=false`
2. Compare the logged `route` with your mock's route
3. Common issues: typo in path, wrong HTTP method, query/body filter mismatch

---

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

---

## Using Mock Responses

Any request that is not an admin route is handled by the catch-all controller and matched against active mocks.

If `RoutePrefix` is configured (e.g. `"mock"`), only requests under that prefix are intercepted. The prefix is stripped before matching.

| RoutePrefix | Request | Matches Mock Route |
|---|---|---|
| `""` | `GET /api/users` | `/api/users` |
| `"mock"` | `GET /mock/api/users` | `/api/users` |

### Request Processing Pipeline

When a request arrives at the catch-all controller:

```
1. Route matching      -> Find mock by method + route
2. Sequential check    -> If isSequential, get next step
3. Rule evaluation     -> If not sequential, evaluate rules in priority order
4. Delay               -> Apply delayMs (sequence-step or mock-level)
5. Template processing -> Replace {{$variables}} in response body
6. Logging             -> Log request details to database
7. Response            -> Return processed response
```

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
Mock:     route = "/api/users"  ->  Match
```

### Phase 2: Contains Fallback

If no exact match is found, mocks whose `route` is **contained within** the request path are matched.

```
Request:  GET /api/users/123/orders
Mock:     route = "/api/users"  ->  Match (path contains route)
```

### Additional Filters

After a route match is found, these filters are applied in order:

1. **HTTP Method** -- Must match exactly
2. **Query String** -- If the request has a query string, mocks with a matching query string are preferred. Mocks without a query string defined will also match (wildcard behavior).
3. **Request Body** -- If the request has a body, mocks with a matching body are preferred. Mocks without a body defined will also match (wildcard behavior).
4. **IsActive** -- Must be `true`

The first mock that passes all filters is returned.
