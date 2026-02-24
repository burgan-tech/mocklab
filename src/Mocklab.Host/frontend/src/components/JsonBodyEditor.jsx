import { useRef, useEffect } from 'react';
import Editor from '@monaco-editor/react';
import { Button } from 'primereact/button';

function tryBeautify(str) {
  if (!str || !str.trim()) return null;
  try {
    const parsed = JSON.parse(str);
    return JSON.stringify(parsed, null, 2);
  } catch {
    return null;
  }
}

/**
 * JSON body editor with syntax highlighting and Beautify. Used for response/request body (and rule/sequence body).
 * Auto-beautifies valid JSON on first render. Beautify button is small and right-aligned.
 */
export function JsonBodyEditor({ value = '', onChange, height = 200, placeholder, autoBeautify = true, 'data-testid': dataTestId, toastRef }) {
  const editorRef = useRef(null);
  const autoBeautifyDoneRef = useRef(false);

  useEffect(() => {
    if (!autoBeautify || autoBeautifyDoneRef.current || !value?.trim()) return;
    const formatted = tryBeautify(value);
    if (formatted && formatted !== value) {
      autoBeautifyDoneRef.current = true;
      onChange(formatted);
    }
  }, [autoBeautify, value, onChange]);

  const handleEditorDidMount = (editor) => {
    editorRef.current = editor;
  };

  const handleBeautify = () => {
    const str = value || '';
    if (!str.trim()) {
      onChange('{\n  \n}\n');
      return;
    }
    const formatted = tryBeautify(str);
    if (formatted != null) {
      onChange(formatted);
      toastRef?.current?.show({ severity: 'success', summary: 'Formatted', detail: 'JSON beautified.', life: 2000 });
    } else {
      toastRef?.current?.show({ severity: 'warn', summary: 'Invalid JSON', detail: 'Could not parse JSON.', life: 4000 });
    }
  };

  return (
    <div className="json-body-editor border-1 surface-border border-round overflow-hidden" data-testid={dataTestId}>
      <div className="flex justify-content-end align-items-center mb-1">
        <Button
          type="button"
          icon="pi pi-align-left"
          size="small"
          text
          rounded
          onClick={handleBeautify}
          title="Beautify JSON"
          className="p-button-sm"
        />
      </div>
      <Editor
        height={height}
        defaultLanguage="json"
        language="json"
        value={value || ''}
        onChange={(v) => onChange(v ?? '')}
        onMount={handleEditorDidMount}
        options={{
          minimap: { enabled: false },
          scrollBeyondLastLine: false,
          wordWrap: 'on',
          lineNumbers: 'on',
          folding: true,
          formatOnPaste: true,
          formatOnType: true,
          tabSize: 2,
          suggest: { showKeywords: false },
        }}
        loading={<span className="text-color-secondary">Loading editor…</span>}
      />
    </div>
  );
}

export default JsonBodyEditor;
