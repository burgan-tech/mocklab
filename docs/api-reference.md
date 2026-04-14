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

### Data Buckets (`/_admin/collections/{collectionId}/data-buckets`)

Data buckets are named JSON datasets attached to a collection, for use in Scriban templates (e.g. `{{ persons[0].name }}`, `{{ random_item("persons") }}`).

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/_admin/collections/{id}/data-buckets` | List data buckets for the collection |
| `GET` | `/_admin/collections/{id}/data-buckets/{bucketId}` | Get one bucket (includes `data` JSON) |
| `POST` | `/_admin/collections/{id}/data-buckets` | Create a bucket (body: `name`, `description?`, `data?` JSON string) |
| `PUT` | `/_admin/collections/{id}/data-buckets/{bucketId}` | Update a bucket |
| `DELETE` | `/_admin/collections/{id}/data-buckets/{bucketId}` | Delete a bucket |

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

**Default / catch-all rule:** A rule with an empty `conditionField` always matches. Use it as the last rule (highest priority number) to act as an `else` branch — it fires when no earlier conditional rule matched.

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

### Condition Fields (FieldScope + CustomField)

Rules target a value using a logical **condition field**. In the UI this is configured as **Field Scope** (where to look) plus **Custom Field** (path or key within that scope). The API stores a single `conditionField` string derived from both.

| conditionField format | Field Scope | Custom Field | Description |
|---|---|---|---|
| `body.propertyPath` | Body | JSON path (dot-notation) | Request body, e.g. `body.amount`, `body.user.name` |
| `header.HeaderName` | Header | Header name | Request header, e.g. `header.Authorization`, `header.X-Api-Key` |
| `query.paramName` | Query parameter | Query key | Query string, e.g. `query.page`, `query.filter` |
| `route.paramName` | Route parameter | Route param name | From mock route template (e.g. `/api/users/{id}` → `route.id`) |
| `method` | Method | (none) | HTTP method: `GET`, `POST`, etc. |
| `path` | Path | (none) | Full request path |
| `cookie.name` | Cookie | Cookie name | Request cookie, e.g. `cookie.sessionId` |

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

### Example: If/Else Pattern with Default Rule

A rule with an **empty `conditionField`** always matches. Place it last (highest priority number) to create an `else` branch that fires whenever no earlier conditional rule matched.

Create a mock for `POST /api/accounts/transfer` with these rules:

| Priority | conditionField | Operator | conditionValue | Response |
|---|---|---|---|---|
| 0 | `body.accountType` | equals | `SAVINGS` | 200 savings-specific response |
| 1 | *(empty — default)* | — | — | 200 general response |

```bash
# POST with accountType=SAVINGS -> rule 0 matches
curl -X POST http://localhost:5000/api/accounts/transfer \
  -H "Content-Type: application/json" \
  -d '{"accountType": "SAVINGS", "amount": 1000}'

# POST with any other accountType -> rule 0 skipped, default rule (priority 1) matches
curl -X POST http://localhost:5000/api/accounts/transfer \
  -H "Content-Type: application/json" \
  -d '{"accountType": "CHECKING", "amount": 500}'
```

> The default rule response body can also use template variables such as `{{ request.body.accountType }}` to echo fields from the incoming request.

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

## Dynamic Template Variables (Scriban)

Response bodies and rule response header values (when a rule matches) are processed with **Scriban**. You can use full Scriban syntax: `{{ expression }}` for output, `{{ for x in items }} ... {{ end }}`, `{{ if condition }} ... {{ else }} ... {{ end }}`, and any expression.

**Backward compatibility:** Legacy `{{$variable}}` placeholders are still supported and are converted to Scriban automatically (e.g. `{{$randomUUID}}` → `{{ guid }}`).

### Template contract (what you can use)

- **request:** `request.method`, `request.path`, `request.body` (parsed JSON object when body is valid JSON — supports field navigation like `request.body.accountName`; falls back to raw string for non-JSON bodies), `request.body_raw` (always the raw string body), `request.json` (alias for parsed body; kept for backward compatibility), `request.query`, `request.headers`, `request.cookies`, `request.route`.
- **headers (top-level):** Case-insensitive header access, e.g. `headers["x-correlation-id"]`, `headers["Authorization"]`. Use when the header might be missing: `headers["x-correlation-id"] | "default"` or null coalescing.
- **helpers:** `helpers.guid()`, `helpers.rand_int(min, maxInclusive)`, `helpers.alphanum(length)`, `helpers.username()` (e.g. fast_tiger42), `helpers.email(domain?)` (default domain example.com).

### Helpers (recommended: `helpers.*`)

| Expression | Description |
|---|---|
| `{{ helpers.guid() }}` | Random UUID v4 |
| `{{ helpers.rand_int(1, 100) }}` | Random integer in [min, maxInclusive] |
| `{{ helpers.alphanum(12) }}` | Random alphanumeric string (length 12) |
| `{{ helpers.username() }}` | Random username (e.g. fast_tiger42) |
| `{{ helpers.email() }}` or `{{ helpers.email("my.domain.com") }}` | Random email |

### String helpers

| Expression | Description |
|---|---|
| `{{ upper "hello" }}` | Convert string to uppercase |
| `{{ lower "HELLO" }}` | Convert string to lowercase |
| `{{ upper request.body.accountName }}` | Uppercase a field from the request body |

### Pre-generated data helpers

Ready-made random data for common domain objects — no external dependencies. All helpers return a single random value each time they are called.

#### Location & Identity

| Expression | Description | Example output |
|---|---|---|
| `{{ random_company_name }}` | Random company name | `Acme Corp` |
| `{{ random_city }}` | Random city name | `Istanbul` |
| `{{ random_country }}` | Random country name | `Germany` |
| `{{ random_address }}` | Random street address | `42 Oak Avenue, Berlin` |
| `{{ random_zip_code }}` | Random postal / zip code | `34100` |
| `{{ random_continent }}` | Random continent name | `Europe` |
| `{{ random_timezone }}` | Random IANA timezone | `Europe/Istanbul` |
| `{{ random_latitude }}` | Random latitude (decimal) | `41.0082` |
| `{{ random_longitude }}` | Random longitude (decimal) | `28.9784` |
| `{{ random_language_code }}` | Random ISO 639-1 language code | `tr` |

#### People & Authentication

| Expression | Description | Example output |
|---|---|---|
| `{{ random_job_title }}` | Random job title | `Software Engineer` |
| `{{ random_department }}` | Random department name | `Engineering` |
| `{{ random_username }}` | Random username | `swift_eagle42` |
| `{{ random_password }}` | Random mock password | `Xk9mP2!b` |
| `{{ random_age }}` | Random age (18–80) | `34` |
| `{{ random_birthdate }}` | Random birthdate (yyyy-MM-dd) | `1990-04-15` |
| `{{ random_role }}` | Random user role | `admin` |

#### Finance & Commerce

| Expression | Description | Example output |
|---|---|---|
| `{{ random_currency_code }}` | Random ISO 4217 currency code | `TRY` |
| `{{ random_iban }}` | Random IBAN-like account number | `TR8512344831957204839217` |
| `{{ random_account_number }}` | Random bank account number | `TR33000610051978645784` |
| `{{ random_swift_code }}` | Random SWIFT/BIC code | `ISBKTRISXXX` |
| `{{ random_credit_card_number }}` | Random Luhn-valid 16-digit card number | `4532015112830366` |
| `{{ random_price }}` | Random price (decimal string) | `249.99` |
| `{{ random_stock_symbol }}` | Random stock ticker symbol | `AAPL` |
| `{{ random_transaction_type }}` | Random transaction type | `credit` |
| `{{ random_product_name }}` | Random product name | `Ultra Gadget` |

#### Business & Workflow

| Expression | Description | Example output |
|---|---|---|
| `{{ random_category }}` | Random general category | `Electronics` |
| `{{ random_status }}` | Random entity status | `active` |
| `{{ random_priority }}` | Random priority level | `high` |
| `{{ random_order_status }}` | Random order status | `shipped` |
| `{{ random_ticket_status }}` | Random support ticket status | `open` |

#### System & Technical

| Expression | Description | Example output |
|---|---|---|
| `{{ random_ip }}` | Random IPv4 address | `192.168.1.42` |
| `{{ random_mac_address }}` | Random MAC address | `00:1A:2B:3C:4D:5E` |
| `{{ random_url }}` | Random API URL | `https://mock.io/api/v1/users` |
| `{{ random_http_status_code }}` | Random HTTP status code | `200` |
| `{{ random_color }}` | Random color name | `crimson` |
| `{{ random_hex_color }}` | Random hex color code | `#3A7BD5` |
| `{{ random_file_extension }}` | Random file extension | `pdf` |
| `{{ random_mime_type }}` | Random MIME type | `application/json` |

#### Example: Full user profile using pre-generated helpers

```json
{
  "id": "{{ helpers.guid() }}",
  "username": "{{ random_username }}",
  "email": "{{ helpers.email() }}",
  "role": "{{ random_role }}",
  "age": {{ random_age }},
  "birthdate": "{{ random_birthdate }}",
  "department": "{{ random_department }}",
  "jobTitle": "{{ random_job_title }}",
  "address": {
    "street": "{{ random_address }}",
    "city": "{{ random_city }}",
    "country": "{{ random_country }}",
    "zip": "{{ random_zip_code }}",
    "timezone": "{{ random_timezone }}"
  },
  "bankAccount": {
    "iban": "{{ random_iban }}",
    "swift": "{{ random_swift_code }}",
    "currency": "{{ random_currency_code }}"
  },
  "status": "{{ random_status }}",
  "language": "{{ random_language_code }}"
}
```

### Legacy global helpers (still supported)

| Scriban | Description |
|---|---|
| `{{ guid }}`, `{{ random_int }}`, `{{ random_int 18 65 }}`, `{{ random_name }}`, `{{ random_email }}`, `{{ timestamp }}`, `{{ iso_timestamp }}`, `{{ now }}`, `{{ random_bool }}`, etc. | Same as before; see legacy docs. |

### Request context

| Expression | Description |
|---|---|
| `{{ request.method }}`, `{{ request.path }}` | HTTP method and request path |
| `{{ request.body }}` | Parsed JSON object when body is valid JSON (use `request.body.accountName` to navigate fields). Falls back to raw string for non-JSON bodies. |
| `{{ request.body.accountName }}` | Navigate a specific field in a JSON request body |
| `{{ request.body_raw }}` | Always the raw request body string, regardless of content type |
| `{{ request.json }}` | Alias for parsed body — same as `request.body` (kept for backward compatibility) |
| `{{ request.query.page }}` or `{{ request.query["tier"] }}` | Query parameter |
| `{{ request.headers["X-Api-Key"] }}` | Request header |
| `{{ request.cookies.sessionId }}` | Request cookie |
| `{{ request.route.id }}` | Route parameter (e.g. route `/api/users/{id}`) |

### Top-level headers

| Expression | Description |
|---|---|
| `{{ headers["x-correlation-id"] }}` | Header value (case-insensitive). Use `\| "default"` if missing. |

### Data Buckets

Collections can have **data buckets**: named JSON data. In templates, bucket names are exposed as variables. For arrays use `random_item("bucketName")`.

Example: `{{ persons[0].name }}` or `{{ random_item("persons").name }}`.

Data bucket API: `GET/POST /_admin/collections/{collectionId}/data-buckets`, `GET/PUT/DELETE /_admin/collections/{collectionId}/data-buckets/{bucketId}`.

### Example: Full template (headers, request, helpers, loop)

```json
{
  "correlationId": "{{ headers["x-correlation-id"] | helpers.guid() }}",
  "path": "{{ request.path }}",
  "isPremium": {{ request.query["tier"] == "premium" }},
  "items": [
    {{ for i in 0..2 }}
      { "id": "{{ helpers.guid() }}", "amount": {{ helpers.rand_int(10, 500) }} }{{ if !for.last }},{{ end }}
    {{ end }}
  ],
  "user": {
    "username": "{{ helpers.username() }}",
    "email": "{{ helpers.email() }}"
  }
}
```

### Example: Request echo and request.json

```json
{
  "echo": {
    "method": "{{ request.method }}",
    "path": "{{ request.path }}",
    "userId": "{{ request.route.id }}",
    "auth": "{{ request.headers["Authorization"] }}"
  },
  "requestId": "{{ helpers.guid() }}",
  "bodyParsed": {{ request.json }}
}
```

### Example: JSON body field navigation (`request.body.field`)

When the request body is valid JSON, `request.body` is automatically parsed and fields can be navigated directly. Use `request.body_raw` when you need the original string.

```json
{
  "greeting": "Hello, {{ request.body.accountName }}!",
  "accountType": "{{ upper request.body.accountType }}",
  "transferId": "{{ helpers.guid() }}",
  "currency": "{{ request.body.currency }}",
  "echoAmount": {{ request.body.amount }},
  "rawPayload": "{{ request.body_raw }}"
}
```

For nested JSON: `{{ request.body.user.name }}`, `{{ request.body.address.city }}`.

> Multiple occurrences of the same helper in one response produce different values (e.g. two `{{ helpers.guid() }}` yield two different UUIDs).

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
