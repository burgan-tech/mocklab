import { useState } from 'react';
import { Dialog } from 'primereact/dialog';
import { TabView, TabPanel } from 'primereact/tabview';

/**
 * Modal that shows Scriban template variables and examples (aligned with project API/docs).
 * Contract: request.*, headers["..."], helpers.*, request.body.field, pre-generated helpers.
 */
function HelperTable({ rows, note }) {
  return (
    <div>
      {note && <p className="text-xs text-color-secondary mt-0 mb-2">{note}</p>}
      <table className="w-full text-xs" style={{ borderCollapse: 'collapse' }}>
        <thead>
          <tr className="text-color-secondary">
            <th className="text-left pb-1 pr-3" style={{ width: '52%' }}>Expression</th>
            <th className="text-left pb-1">Description</th>
          </tr>
        </thead>
        <tbody>
          {rows.map(([expr, desc]) => (
            <tr key={expr} style={{ borderBottom: '1px solid var(--surface-border)' }}>
              <td className="py-1 pr-3"><code>{expr}</code></td>
              <td className="py-1 text-color-secondary">{desc}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function TemplateVariablesModal({ visible, onHide }) {
  const [activeTab, setActiveTab] = useState(0);

  const chip = (text) => (
    <code
      key={text}
      className="surface-ground border-round px-1 mr-1 mb-1"
      style={{ display: 'inline-block', fontSize: '0.78rem' }}
    >
      {text}
    </code>
  );

  return (
    <Dialog
      header="Template variables (Scriban)"
      visible={visible}
      onHide={onHide}
      style={{ width: 'min(780px, 96vw)' }}
      className="template-variables-modal"
      dismissableMask
    >
      <div className="text-sm line-height-3">
        <p className="mt-0 mb-3 text-color-secondary">
          Use <code>{'{{ expression }}'}</code> for output. Full Scriban syntax supported:{' '}
          <code>{'{{ for x in items }}...{{ end }}'}</code>, <code>{'{{ if cond }}...{{ else }}...{{ end }}'}</code>.
          Legacy <code>{'{{$variable}}'}</code> placeholders are auto-converted.
        </p>

        <TabView activeIndex={activeTab} onTabChange={(e) => setActiveTab(e.index)}>

          {/* ── Request ── */}
          <TabPanel header="Request">
            <table className="w-full text-xs" style={{ borderCollapse: 'collapse' }}>
              <thead>
                <tr className="text-color-secondary">
                  <th className="text-left pb-1 pr-3" style={{ width: '50%' }}>Expression</th>
                  <th className="text-left pb-1">Description</th>
                </tr>
              </thead>
              <tbody>
                {[
                  ['{{ request.method }}', 'HTTP method (GET, POST, …)'],
                  ['{{ request.path }}', 'Request path'],
                  ['{{ request.body }}', 'Parsed JSON object when body is valid JSON — navigate fields directly'],
                  ['{{ request.body.accountName }}', 'Field from JSON body (dot-navigation)'],
                  ['{{ request.body.user.name }}', 'Nested field from JSON body'],
                  ['{{ request.body_raw }}', 'Always the raw body string'],
                  ['{{ request.json }}', 'Alias for parsed body (backward compat)'],
                  ['{{ request.query.page }}', 'Query parameter'],
                  ['{{ request.query["tier"] }}', 'Query parameter (bracket syntax)'],
                  ['{{ request.headers["X-Api-Key"] }}', 'Request header'],
                  ['{{ request.cookies.sessionId }}', 'Request cookie'],
                  ['{{ request.route.id }}', 'Route parameter (e.g. /api/users/{id})'],
                  ['{{ headers["x-correlation-id"] }}', 'Top-level header (case-insensitive)'],
                  ['{{ headers["x-id"] | helpers.guid() }}', 'Header with fallback if missing'],
                ].map(([expr, desc]) => (
                  <tr key={expr} style={{ borderBottom: '1px solid var(--surface-border)' }}>
                    <td className="py-1 pr-3"><code>{expr}</code></td>
                    <td className="py-1 text-color-secondary">{desc}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </TabPanel>

          {/* ── Helpers ── */}
          <TabPanel header="Helpers">
            <HelperTable rows={[
              ['{{ helpers.guid() }}', 'Random UUID v4'],
              ['{{ helpers.rand_int(1, 100) }}', 'Random integer in [min, max]'],
              ['{{ helpers.alphanum(12) }}', 'Random alphanumeric string (length 12)'],
              ['{{ helpers.username() }}', 'Random username (e.g. swift_eagle42)'],
              ['{{ helpers.email() }}', 'Random email'],
              ['{{ helpers.email("my.domain.com") }}', 'Random email with custom domain'],
              ['{{ upper "hello" }}', 'Convert to uppercase → HELLO'],
              ['{{ lower "HELLO" }}', 'Convert to lowercase → hello'],
              ['{{ upper request.body.accountName }}', 'Uppercase a body field'],
              ['{{ random_int 18 65 }}', 'Random int with range'],
              ['{{ random_name }}', 'Random full name'],
              ['{{ random_email }}', 'Random email'],
              ['{{ random_phone }}', 'Random phone number'],
              ['{{ random_bool }}', 'true or false'],
              ['{{ random_float }}', 'Random float (0–1000)'],
              ['{{ timestamp }}', 'Unix timestamp (seconds)'],
              ['{{ iso_timestamp }}', 'ISO 8601 timestamp'],
              ['{{ now }}', 'Current UTC DateTime — use inside date_time_add'],
              ["{{ now_fmt 'yyyy-MM-dd' }}", 'Today as formatted string'],
              ["{{ now_fmt 'yyyy-MM-ddTHH:mm:ss' }}", 'Today with time as formatted string'],
              ['{{ random_string 8 }}', 'Random string of length 8'],
              ['{{ random_alpha_numeric 10 }}', 'Random alphanumeric string of length 10'],
              ['{{ random_number_string 6 }}', 'Random numeric string of length 6'],
            ]} note="All helpers work both as {{ helpers.xxx() }} and as top-level {{ xxx }}." />
          </TabPanel>

          {/* ── Arithmetic ── */}
          <TabPanel header="Arithmetic">
            <HelperTable rows={[
              ['{{ add 10 5 }}', 'Addition → 15'],
              ['{{ subtract 10 3 }}', 'Subtraction → 7'],
              ['{{ multiply 4 2.5 }}', 'Multiplication → 10'],
              ['{{ divide 10 4 }}', 'Division → 2.5'],
              ["{{ body 'fieldName' }}", 'Read a top-level field from JSON request body'],
              ["{{ subtract 3 (body 'retryCount') }}", 'Subtract body field from a literal'],
              ["{{ add (body 'quantity') (body 'bonus') }}", 'Add two body fields together'],
            ]} note="Operator comes first, then the two operands. body('field') reads a top-level field from the JSON request body." />
          </TabPanel>

          {/* ── Date & Time ── */}
          <TabPanel header="Date &amp; Time">
            <HelperTable rows={[
              ["{{ now_fmt 'yyyy-MM-dd' }}", 'Today as formatted string'],
              ["{{ now_fmt 'yyyy-MM-ddTHH:mm:ss' }}", 'Today with time as formatted string'],
              ["{{ date_time_add (now) 1 'days' }}", 'Tomorrow — ISO 8601 string'],
              ["{{ date_time_add (now) -7 'days' }}", '7 days ago — ISO 8601 string'],
              ["{{ date_time_add (now) 3 'months' }}", '3 months from now — ISO 8601 string'],
              ["{{ date_time_add_fmt (now) 1 'days' 'yyyy-MM-dd' }}", 'Tomorrow as formatted string'],
              ["{{ date_time_add_fmt (now) 30 'days' 'yyyy-MM-dd' }}", '+30 days as formatted string'],
              ["{{ date_time_add_fmt (now) -1 'months' 'yyyy-MM-dd' }}", '1 month ago as formatted string'],
              ["{{ date_time_add_fmt (now) 1 'months' 'yyyy-MM-dd HH:mm' }}", '+1 month with time'],
            ]} note="Supported units: years, months, weeks, days, hours, minutes, seconds. Use date_time_add_fmt to get a formatted string directly." />
          </TabPanel>

          {/* ── Faker ── */}
          <TabPanel header="Faker">
            <HelperTable rows={[
              ["{{ faker 'number.int' 1 100 }}", 'Random integer between 1 and 100'],
              ["{{ faker 'number.float' 0.5 5.0 2 }}", 'Random float in [0.5, 5.0] with 2 decimals'],
              ["{{ faker 'person.firstName' }}", 'Random first name'],
              ["{{ faker 'person.lastName' }}", 'Random last name'],
              ["{{ faker 'person.fullName' }}", 'Random full name'],
              ["{{ faker 'internet.email' }}", 'Random email address'],
              ["{{ faker 'internet.url' }}", 'Random URL'],
              ["{{ faker 'internet.ip' }}", 'Random IP address'],
              ["{{ faker 'internet.color' }}", 'Random hex color'],
              ["{{ faker 'location.city' }}", 'Random city'],
              ["{{ faker 'location.country' }}", 'Random country'],
              ["{{ faker 'location.latitude' }}", 'Random latitude'],
              ["{{ faker 'location.longitude' }}", 'Random longitude'],
              ["{{ faker 'location.zipCode' }}", 'Random zip code'],
              ["{{ faker 'finance.iban' }}", 'Random IBAN'],
              ["{{ faker 'finance.bic' }}", 'Random BIC / SWIFT code'],
              ["{{ faker 'finance.currencyCode' }}", 'Random currency code'],
              ["{{ faker 'finance.amount' }}", 'Random price'],
              ["{{ faker 'company.name' }}", 'Random company name'],
              ["{{ faker 'date.future' }}", 'Random future date (ISO 8601)'],
              ["{{ faker 'date.past' }}", 'Random past date (ISO 8601)'],
              ["{{ faker 'string.uuid' }}", 'Random UUID'],
              ["{{ faker 'lorem.word' }}", 'Random word'],
              ["{{ faker 'phone.number' }}", 'Random phone number'],
              ["{{ faker 'system.mimeType' }}", 'Random MIME type'],
              ["{{ faker 'system.fileExt' }}", 'Random file extension'],
            ]} note="Syntax: {{ faker 'category.type' ...args }}. Compatible with Mockoon faker helper naming." />
          </TabPanel>

          {/* ── Pre-generated ── */}
          <TabPanel header="Pre-generated">
            <p className="text-xs text-color-secondary mt-0 mb-2">
              Ready-made domain data. Available both as <code>{'{{ random_xxx }}'}</code> and <code>{'{{ helpers.random_xxx() }}'}</code>.
            </p>

            <div className="font-medium text-xs mb-1 mt-2">Location & Identity</div>
            <div className="mb-2 flex flex-wrap gap-1">
              {['random_company_name','random_city','random_country','random_address',
                'random_zip_code','random_continent','random_timezone',
                'random_latitude','random_longitude','random_language_code'].map(chip)}
            </div>

            <div className="font-medium text-xs mb-1 mt-2">People & Auth</div>
            <div className="mb-2 flex flex-wrap gap-1">
              {['random_job_title','random_department','random_username','random_password',
                'random_age','random_birthdate','random_role'].map(chip)}
            </div>

            <div className="font-medium text-xs mb-1 mt-2">Finance & Commerce</div>
            <div className="mb-2 flex flex-wrap gap-1">
              {['random_currency_code','random_iban','random_account_number','random_swift_code',
                'random_credit_card_number','random_price','random_stock_symbol',
                'random_transaction_type','random_product_name'].map(chip)}
            </div>

            <div className="font-medium text-xs mb-1 mt-2">Business & Workflow</div>
            <div className="mb-2 flex flex-wrap gap-1">
              {['random_category','random_status','random_priority',
                'random_order_status','random_ticket_status'].map(chip)}
            </div>

            <div className="font-medium text-xs mb-1 mt-2">System & Technical</div>
            <div className="mb-2 flex flex-wrap gap-1">
              {['random_ip','random_mac_address','random_url','random_http_status_code',
                'random_color','random_hex_color','random_file_extension','random_mime_type'].map(chip)}
            </div>
          </TabPanel>

          {/* ── Examples ── */}
          <TabPanel header="Examples">
            <div className="font-medium text-xs mb-1">Body field navigation + helpers</div>
            <pre className="surface-ground p-2 border-round overflow-auto text-xs mb-3" style={{ maxHeight: '10rem' }}>
{`{
  "greeting": "Hello, {{ request.body.accountName }}!",
  "accountType": "{{ upper request.body.accountType }}",
  "transferId": "{{ helpers.guid() }}",
  "currency": "{{ request.body.currency }}",
  "echoAmount": {{ request.body.amount }},
  "rawPayload": "{{ request.body_raw }}"
}`}
            </pre>

            <div className="font-medium text-xs mb-1">Pre-generated — user profile</div>
            <pre className="surface-ground p-2 border-round overflow-auto text-xs mb-3" style={{ maxHeight: '10rem' }}>
{`{
  "id": "{{ helpers.guid() }}",
  "username": "{{ random_username }}",
  "email": "{{ helpers.email() }}",
  "role": "{{ random_role }}",
  "age": {{ random_age }},
  "department": "{{ random_department }}",
  "jobTitle": "{{ random_job_title }}",
  "address": {
    "city": "{{ random_city }}",
    "country": "{{ random_country }}",
    "timezone": "{{ random_timezone }}"
  },
  "bankAccount": {
    "iban": "{{ random_iban }}",
    "swift": "{{ random_swift_code }}",
    "currency": "{{ random_currency_code }}"
  }
}`}
            </pre>

            <div className="font-medium text-xs mb-1">Loop + conditional</div>
            <pre className="surface-ground p-2 border-round overflow-auto text-xs mb-3" style={{ maxHeight: '10rem' }}>
{`{
  "correlationId": "{{ headers["x-correlation-id"] | helpers.guid() }}",
  "isPremium": {{ request.query["tier"] == "premium" }},
  "items": [
    {{ for i in 0..2 }}
      { "id": "{{ helpers.guid() }}", "amount": {{ helpers.rand_int(10, 500) }} }{{ if !for.last }},{{ end }}
    {{ end }}
  ]
}`}
            </pre>

            <div className="font-medium text-xs mb-1">Scriban helpers (arithmetic, date, faker)</div>
            <pre className="surface-ground p-2 border-round overflow-auto text-xs mb-3" style={{ maxHeight: '10rem' }}>
{`{
  "retriesLeft": {{ subtract 3 (body 'retryCount') }},
  "expiresAt": "{{ date_time_add_fmt (now) 1 'days' 'yyyy-MM-dd' }}",
  "score": {{ faker 'number.float' 0.5 5.0 2 }},
  "today": "{{ now_fmt 'yyyy-MM-dd' }}",
  "nextMonth": "{{ date_time_add_fmt (now) 1 'months' 'yyyy-MM-dd' }}"
}`}
            </pre>

            <div className="font-medium text-xs mb-1">Data buckets (collection)</div>
            <pre className="surface-ground p-2 border-round overflow-auto text-xs" style={{ maxHeight: '7rem' }}>
{`{{ persons[0].name }}
{{ random_item("persons").name }}
{{ random_item("products").price }}`}
            </pre>
            <p className="text-color-secondary text-xs mt-1 mb-0">
              Bucket name is set under <strong>Collections → Data Buckets</strong>.
            </p>
          </TabPanel>

        </TabView>
      </div>
    </Dialog>
  );
}

export default TemplateVariablesModal;
