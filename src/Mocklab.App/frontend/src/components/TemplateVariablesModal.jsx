import { Dialog } from 'primereact/dialog';

/**
 * Modal that shows Scriban template variables and examples (aligned with project API/docs).
 * Contract: request.*, headers["..."], helpers.*, request.json.
 */
export function TemplateVariablesModal({ visible, onHide }) {
  return (
    <Dialog
      header="Template variables (Scriban)"
      visible={visible}
      onHide={onHide}
      style={{ width: 'min(600px, 95vw)' }}
      className="template-variables-modal"
      dismissableMask
    >
      <div className="text-sm line-height-3">
        <p className="mt-0 mb-2">
          Use <code>{'{{ }}'}</code> for output. Legacy <code>{'{{$...}}'}</code> is also supported.
        </p>

        <div className="font-semibold mb-1">Helpers (recommended)</div>
        <div className="surface-ground p-2 mb-3 border-round">
          <code>{'{{ helpers.guid() }}'}</code> <code>{'{{ helpers.rand_int(1, 100) }}'}</code>{' '}
          <code>{'{{ helpers.alphanum(12) }}'}</code> <code>{'{{ helpers.username() }}'}</code>{' '}
          <code>{'{{ helpers.email() }}'}</code> <code>{'{{ helpers.email("my.domain.com") }}'}</code>
        </div>

        <div className="font-semibold mb-1">Request</div>
        <div className="surface-ground p-2 mb-3 border-round">
          <code>{'{{ request.method }}'}</code> <code>{'{{ request.path }}'}</code> <code>{'{{ request.body }}'}</code>{' '}
          <code>{'{{ request.json }}'}</code> <code>{'{{ request.query["tier"] }}'}</code>{' '}
          <code>{'{{ request.headers["X-Api-Key"] }}'}</code> <code>{'{{ request.route.id }}'}</code>
        </div>

        <div className="font-semibold mb-1">Headers (top-level, case-insensitive)</div>
        <div className="surface-ground p-2 mb-3 border-round">
          <code>{'{{ headers["x-correlation-id"] }}'}</code> <code>{'{{ headers["x-correlation-id"] | helpers.guid() }}'}</code>
        </div>

        <div className="font-semibold mb-1">Data buckets (collection)</div>
        <div className="surface-ground p-2 mb-3 border-round">
          <code>{'{{ persons[0].name }}'}</code> or <code>{'{{ random_item("persons").name }}'}</code>
        </div>

        <div className="font-semibold mb-1">Example (headers, request, helpers, loop)</div>
        <pre className="surface-ground p-2 border-round overflow-auto text-xs m-0" style={{ maxHeight: '14rem' }}>
{`{
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
}`}
        </pre>
      </div>
    </Dialog>
  );
}

export default TemplateVariablesModal;
