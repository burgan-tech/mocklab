import { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { InputTextarea } from 'primereact/inputtextarea';
import { InputNumber } from 'primereact/inputnumber';
import { Dropdown } from 'primereact/dropdown';
import { Checkbox } from 'primereact/checkbox';
import { Tag } from 'primereact/tag';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { Message } from 'primereact/message';
import { TabView, TabPanel } from 'primereact/tabview';
import { Divider } from 'primereact/divider';
import { Badge } from 'primereact/badge';
import { IconField } from 'primereact/iconfield';
import { InputIcon } from 'primereact/inputicon';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { mockService } from '../services/mockService';
import { collectionService } from '../services/collectionService';

export default function MockManagementPage() {
  const [mocks, setMocks] = useState([]);
  const [collections, setCollections] = useState([]);
  const [loading, setLoading] = useState(false);
  const [mockDialog, setMockDialog] = useState(false);
  const [mock, setMock] = useState(null);
  const [selectedMocks, setSelectedMocks] = useState(null);
  const [globalFilter, setGlobalFilter] = useState('');
  const [isEditMode, setIsEditMode] = useState(false);
  const [curlDialog, setCurlDialog] = useState(false);
  const [curlCommand, setCurlCommand] = useState('');
  const [curlLoading, setCurlLoading] = useState(false);
  const [openApiDialog, setOpenApiDialog] = useState(false);
  const [openApiJson, setOpenApiJson] = useState('');
  const [openApiLoading, setOpenApiLoading] = useState(false);
  const [showTemplateHelp, setShowTemplateHelp] = useState(false);
  const [activeTabIndex, setActiveTabIndex] = useState(0);
  const [collectionFilter, setCollectionFilter] = useState(null);
  const toast = useRef(null);

  const emptyMock = {
    httpMethod: 'GET',
    route: '',
    queryString: null,
    requestBody: null,
    statusCode: 200,
    responseBody: '',
    contentType: 'application/json',
    description: '',
    delayMs: null,
    collectionId: null,
    isSequential: false,
    isActive: true,
    rules: [],
    sequenceItems: []
  };

  const emptyRule = {
    conditionField: '',
    conditionOperator: 'equals',
    conditionValue: '',
    statusCode: 200,
    responseBody: '',
    contentType: 'application/json',
    priority: 0
  };

  const emptySequenceItem = {
    order: 0,
    statusCode: 200,
    responseBody: '',
    contentType: 'application/json',
    delayMs: null
  };

  const httpMethodOptions = [
    { label: 'GET', value: 'GET' },
    { label: 'POST', value: 'POST' },
    { label: 'PUT', value: 'PUT' },
    { label: 'DELETE', value: 'DELETE' },
    { label: 'PATCH', value: 'PATCH' },
    { label: 'HEAD', value: 'HEAD' },
    { label: 'OPTIONS', value: 'OPTIONS' }
  ];

  const contentTypeOptions = [
    { label: 'application/json', value: 'application/json' },
    { label: 'application/xml', value: 'application/xml' },
    { label: 'text/plain', value: 'text/plain' },
    { label: 'text/html', value: 'text/html' },
    { label: 'application/x-www-form-urlencoded', value: 'application/x-www-form-urlencoded' }
  ];

  const statusCodeOptions = [
    { label: '200 - OK', value: 200 },
    { label: '201 - Created', value: 201 },
    { label: '204 - No Content', value: 204 },
    { label: '400 - Bad Request', value: 400 },
    { label: '401 - Unauthorized', value: 401 },
    { label: '403 - Forbidden', value: 403 },
    { label: '404 - Not Found', value: 404 },
    { label: '500 - Internal Server Error', value: 500 },
    { label: '503 - Service Unavailable', value: 503 }
  ];

  const operatorOptions = [
    { label: 'Equals', value: 'equals' },
    { label: 'Contains', value: 'contains' },
    { label: 'Starts With', value: 'startsWith' },
    { label: 'Ends With', value: 'endsWith' },
    { label: 'Regex', value: 'regex' },
    { label: 'Exists', value: 'exists' },
    { label: 'Not Exists', value: 'notExists' },
    { label: 'Greater Than', value: 'greaterThan' },
    { label: 'Less Than', value: 'lessThan' }
  ];

  const conditionFieldSuggestions = [
    { label: 'header.Authorization', value: 'header.Authorization' },
    { label: 'header.Content-Type', value: 'header.Content-Type' },
    { label: 'header.X-Api-Key', value: 'header.X-Api-Key' },
    { label: 'query.page', value: 'query.page' },
    { label: 'query.limit', value: 'query.limit' },
    { label: 'query.search', value: 'query.search' },
    { label: 'body.id', value: 'body.id' },
    { label: 'body.type', value: 'body.type' },
    { label: 'body.status', value: 'body.status' },
    { label: 'method', value: 'method' },
    { label: 'path', value: 'path' }
  ];

  useEffect(() => {
    loadMocks();
    loadCollections();
  }, []);

  const loadCollections = async () => {
    try {
      const data = await collectionService.getAllCollections();
      setCollections(data);
    } catch {
      // Silently fail - collections are optional
    }
  };

  const loadMocks = async () => {
    setLoading(true);
    try {
      const data = await mockService.getAllMocks();
      setMocks(data);
    } catch (error) {
      toast.current.show({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to load mocks: ' + error.message,
        life: 3000
      });
    } finally {
      setLoading(false);
    }
  };

  const openNew = () => {
    setMock(emptyMock);
    setIsEditMode(false);
    setActiveTabIndex(0);
    setShowTemplateHelp(false);
    setMockDialog(true);
  };

  const hideDialog = () => {
    setMockDialog(false);
    setActiveTabIndex(0);
  };

  const saveMock = async () => {
    if (!mock.route.trim()) {
      toast.current.show({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please enter a route',
        life: 3000
      });
      return;
    }

    try {
      // Clean up sequence items ordering before save
      const mockToSave = { ...mock };
      if (mockToSave.sequenceItems) {
        mockToSave.sequenceItems = mockToSave.sequenceItems.map((item, idx) => ({
          ...item,
          order: idx
        }));
      }

      if (isEditMode) {
        await mockService.updateMock(mock.id, mockToSave);
        toast.current.show({
          severity: 'success',
          summary: 'Success',
          detail: 'Mock updated successfully',
          life: 3000
        });
      } else {
        await mockService.createMock(mockToSave);
        toast.current.show({
          severity: 'success',
          summary: 'Success',
          detail: 'Mock created successfully',
          life: 3000
        });
      }

      setMockDialog(false);
      setMock(emptyMock);
      setActiveTabIndex(0);
      loadMocks();
    } catch (error) {
      toast.current.show({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to save mock: ' + error.message,
        life: 3000
      });
    }
  };

  const editMock = async (mockRow) => {
    try {
      // Fetch full mock details (with rules and sequence items)
      const fullMock = await mockService.getMock(mockRow.id);
      setMock({ ...fullMock });
      setIsEditMode(true);
      setActiveTabIndex(0);
      setShowTemplateHelp(false);
      setMockDialog(true);
    } catch (error) {
      toast.current.show({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to load mock details: ' + error.message,
        life: 3000
      });
    }
  };

  const deleteMock = (mock) => {
    confirmDialog({
      message: `Are you sure you want to delete this mock response?`,
      header: 'Delete Confirmation',
      icon: 'pi pi-exclamation-triangle',
      acceptClassName: 'p-button-danger',
      accept: async () => {
        try {
          await mockService.deleteMock(mock.id);
          toast.current.show({
            severity: 'success',
            summary: 'Success',
            detail: 'Mock deleted successfully',
            life: 3000
          });
          loadMocks();
        } catch (error) {
          toast.current.show({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to delete mock: ' + error.message,
            life: 3000
          });
        }
      }
    });
  };

  const toggleMock = async (mock) => {
    try {
      await mockService.toggleMock(mock.id);
      toast.current.show({
        severity: 'success',
        summary: 'Success',
        detail: `Mock ${mock.isActive ? 'deactivated' : 'activated'} successfully`,
        life: 3000
      });
      loadMocks();
    } catch (error) {
      toast.current.show({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to toggle mock: ' + error.message,
        life: 3000
      });
    }
  };

  const resetSequence = async (mockRow) => {
    try {
      await mockService.resetSequence(mockRow.id);
      toast.current.show({
        severity: 'success',
        summary: 'Success',
        detail: 'Sequence counter reset',
        life: 3000
      });
    } catch (error) {
      toast.current.show({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to reset sequence: ' + error.message,
        life: 3000
      });
    }
  };

  const clearAllMocks = () => {
    confirmDialog({
      message: 'Are you sure you want to delete ALL mock responses? This action cannot be undone!',
      header: 'Clear All Confirmation',
      icon: 'pi pi-exclamation-triangle',
      acceptClassName: 'p-button-danger',
      accept: async () => {
        try {
          const result = await mockService.clearAllMocks();
          toast.current.show({
            severity: 'success',
            summary: 'Success',
            detail: result.message,
            life: 3000
          });
          loadMocks();
        } catch (error) {
          toast.current.show({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to clear mocks: ' + error.message,
            life: 3000
          });
        }
      }
    });
  };

  const importFromCurl = async () => {
    if (!curlCommand.trim()) {
      toast.current.show({ severity: 'warn', summary: 'Warning', detail: 'Please enter a cURL command', life: 3000 });
      return;
    }
    setCurlLoading(true);
    try {
      const result = await mockService.importCurl(curlCommand);
      toast.current.show({
        severity: 'success',
        summary: 'Success',
        detail: `Mock imported: ${result.httpMethod} ${result.route} (${result.statusCode})`,
        life: 4000
      });
      setCurlDialog(false);
      setCurlCommand('');
      loadMocks();
    } catch (error) {
      toast.current.show({ severity: 'error', summary: 'Error', detail: error.message, life: 5000 });
    } finally {
      setCurlLoading(false);
    }
  };

  const importFromOpenApi = async () => {
    if (!openApiJson.trim()) {
      toast.current.show({ severity: 'warn', summary: 'Warning', detail: 'Please enter OpenAPI JSON', life: 3000 });
      return;
    }
    setOpenApiLoading(true);
    try {
      const result = await mockService.importOpenApi(openApiJson);
      toast.current.show({
        severity: 'success',
        summary: 'Success',
        detail: result.message,
        life: 4000
      });
      setOpenApiDialog(false);
      setOpenApiJson('');
      loadMocks();
    } catch (error) {
      toast.current.show({ severity: 'error', summary: 'Error', detail: error.message, life: 5000 });
    } finally {
      setOpenApiLoading(false);
    }
  };

  const onInputChange = (e, name) => {
    const val = (e.target && e.target.value) || '';
    let _mock = { ...mock };
    _mock[`${name}`] = val;
    setMock(_mock);
  };

  // ========== Rules Helpers ==========
  const addRule = () => {
    const _mock = { ...mock };
    const newPriority = _mock.rules.length > 0
      ? Math.max(..._mock.rules.map(r => r.priority)) + 1
      : 0;
    _mock.rules = [..._mock.rules, { ...emptyRule, priority: newPriority }];
    setMock(_mock);
  };

  const removeRule = (index) => {
    const _mock = { ...mock };
    _mock.rules = _mock.rules.filter((_, i) => i !== index);
    setMock(_mock);
  };

  const updateRule = (index, field, value) => {
    const _mock = { ...mock };
    _mock.rules = _mock.rules.map((r, i) => i === index ? { ...r, [field]: value } : r);
    setMock(_mock);
  };

  // ========== Sequence Helpers ==========
  const addSequenceItem = () => {
    const _mock = { ...mock };
    const newOrder = _mock.sequenceItems.length;
    _mock.sequenceItems = [..._mock.sequenceItems, { ...emptySequenceItem, order: newOrder }];
    setMock(_mock);
  };

  const removeSequenceItem = (index) => {
    const _mock = { ...mock };
    _mock.sequenceItems = _mock.sequenceItems.filter((_, i) => i !== index);
    setMock(_mock);
  };

  const updateSequenceItem = (index, field, value) => {
    const _mock = { ...mock };
    _mock.sequenceItems = _mock.sequenceItems.map((s, i) => i === index ? { ...s, [field]: value } : s);
    setMock(_mock);
  };

  const moveSequenceItem = (index, direction) => {
    const _mock = { ...mock };
    const items = [..._mock.sequenceItems];
    const targetIndex = index + direction;
    if (targetIndex < 0 || targetIndex >= items.length) return;
    [items[index], items[targetIndex]] = [items[targetIndex], items[index]];
    _mock.sequenceItems = items;
    setMock(_mock);
  };

  // ========== Column Templates ==========
  const statusBodyTemplate = (rowData) => (
    <Tag
      value={rowData.isActive ? 'Active' : 'Inactive'}
      severity={rowData.isActive ? 'success' : 'danger'}
    />
  );

  const httpMethodBodyTemplate = (rowData) => {
    const methodColors = {
      GET: 'info', POST: 'success', PUT: 'warning', DELETE: 'danger',
      PATCH: 'help', HEAD: 'secondary', OPTIONS: 'secondary'
    };
    return <Tag value={rowData.httpMethod} severity={methodColors[rowData.httpMethod] || 'info'} rounded />;
  };

  const statusCodeBodyTemplate = (rowData) => {
    const getSeverity = (code) => {
      if (code >= 200 && code < 300) return 'success';
      if (code >= 300 && code < 400) return 'info';
      if (code >= 400 && code < 500) return 'warning';
      if (code >= 500) return 'danger';
      return 'secondary';
    };
    return <Tag value={rowData.statusCode} severity={getSeverity(rowData.statusCode)} />;
  };

  const featuresBodyTemplate = (rowData) => {
    const features = [];
    if (rowData.rules && rowData.rules.length > 0) {
      features.push(
        <Tag key="rules" value={`${rowData.rules.length} Rules`} severity="info" icon="pi pi-sitemap" className="mr-1" />
      );
    }
    if (rowData.isSequential && rowData.sequenceItems && rowData.sequenceItems.length > 0) {
      features.push(
        <Tag key="seq" value={`${rowData.sequenceItems.length} Steps`} severity="help" icon="pi pi-replay" className="mr-1" />
      );
    }
    return features.length > 0 ? <div className="flex flex-wrap gap-1">{features}</div> : <span className="text-color-secondary">-</span>;
  };

  const actionBodyTemplate = (rowData) => (
    <div className="flex gap-2">
      <Button
        icon={rowData.isActive ? 'pi pi-eye-slash' : 'pi pi-eye'}
        rounded outlined
        severity={rowData.isActive ? 'warning' : 'success'}
        className="p-button-sm"
        onClick={() => toggleMock(rowData)}
        tooltip={rowData.isActive ? 'Deactivate' : 'Activate'}
        tooltipOptions={{ position: 'top' }}
      />
      <Button
        icon="pi pi-pencil"
        rounded outlined
        className="p-button-sm"
        onClick={() => editMock(rowData)}
        tooltip="Edit"
        tooltipOptions={{ position: 'top' }}
      />
      {rowData.isSequential && rowData.sequenceItems && rowData.sequenceItems.length > 0 && (
        <Button
          icon="pi pi-replay"
          rounded outlined
          severity="help"
          className="p-button-sm"
          onClick={() => resetSequence(rowData)}
          tooltip="Reset Sequence"
          tooltipOptions={{ position: 'top' }}
        />
      )}
      <Button
        icon="pi pi-trash"
        rounded outlined
        severity="danger"
        className="p-button-sm"
        onClick={() => deleteMock(rowData)}
        tooltip="Delete"
        tooltipOptions={{ position: 'top' }}
      />
    </div>
  );

  const leftToolbarTemplate = () => (
    <div className="flex flex-wrap gap-2">
      <Button label="New Mock" icon="pi pi-plus" severity="success" onClick={openNew} />
      <Button label="Import cURL" icon="pi pi-download" severity="help" outlined onClick={() => setCurlDialog(true)} className="hidden-label-sm" />
      <Button label="Import OpenAPI" icon="pi pi-file-import" severity="info" outlined onClick={() => setOpenApiDialog(true)} className="hidden-label-sm" />
    </div>
  );

  const collectionFilterOptions = [
    { label: 'All Collections', value: null },
    { label: 'No Collection', value: -1 },
    ...collections.map(c => ({ label: c.name, value: c.id }))
  ];

  const filteredMocks = collectionFilter === null
    ? mocks
    : collectionFilter === -1
      ? mocks.filter(m => !m.collectionId)
      : mocks.filter(m => m.collectionId === collectionFilter);

  const collectionFilterTemplate = (option) => {
    if (option.value === null || option.value === -1) return <span>{option.label}</span>;
    const col = collections.find(c => c.id === option.value);
    return (
      <div className="flex align-items-center gap-2">
        {col?.color && <div style={{ width: '0.75rem', height: '0.75rem', borderRadius: '50%', backgroundColor: col.color }} />}
        <span>{option.label}</span>
      </div>
    );
  };

  const rightToolbarTemplate = () => (
    <div className="flex flex-wrap gap-2 align-items-center">
      {collections.length > 0 && (
        <Dropdown
          value={collectionFilter}
          options={collectionFilterOptions}
          onChange={(e) => setCollectionFilter(e.value)}
          placeholder="Collection"
          itemTemplate={collectionFilterTemplate}
          valueTemplate={(option) => {
            if (!option || option.value === null) return <span className="text-color-secondary"><i className="pi pi-filter mr-2" style={{ fontSize: '0.875rem' }}></i>All Collections</span>;
            if (option.value === -1) return <span><i className="pi pi-filter mr-2" style={{ fontSize: '0.875rem' }}></i>No Collection</span>;
            const col = collections.find(c => c.id === option.value);
            return (
              <span className="flex align-items-center gap-2">
                {col?.color && <div style={{ width: '0.625rem', height: '0.625rem', borderRadius: '50%', backgroundColor: col.color }} />}
                {option.label}
              </span>
            );
          }}
          style={{ width: '12rem' }}
        />
      )}
      <Button icon="pi pi-refresh" outlined onClick={loadMocks} tooltip="Refresh" tooltipOptions={{ position: 'top' }} />
      <Button icon="pi pi-trash" severity="danger" outlined onClick={clearAllMocks} tooltip="Clear All" tooltipOptions={{ position: 'top' }} />
    </div>
  );

  const header = (
    <div className="flex flex-wrap gap-2 align-items-center justify-content-between">
      <h4 className="m-0">Manage Mock Responses</h4>
      <IconField iconPosition="left">
        <InputIcon className="pi pi-search" />
        <InputText type="search" onInput={(e) => setGlobalFilter(e.target.value)} placeholder="Search..." />
      </IconField>
    </div>
  );

  const mockDialogFooter = (
    <>
      <Button label="Cancel" icon="pi pi-times" outlined severity="secondary" onClick={hideDialog} />
      <Button label="Save" icon="pi pi-check" onClick={saveMock} />
    </>
  );

  // ========== Rule Editor Tab Content ==========
  const renderRulesTab = () => (
    <div>
      <div className="flex align-items-center justify-content-between mb-3">
        <div>
          <p className="text-color-secondary m-0 text-sm">
            Rules are evaluated in priority order (ascending). The first matching rule overrides the default response.
          </p>
        </div>
        <Button label="Add Rule" icon="pi pi-plus" size="small" severity="info" onClick={addRule} />
      </div>

      {(!mock?.rules || mock.rules.length === 0) && (
        <Message severity="info" text="No rules defined. The default response will always be used." className="w-full" />
      )}

      {mock?.rules?.map((rule, index) => (
        <div key={index} className="surface-ground border-round p-3 mb-3">
          <div className="flex align-items-center justify-content-between mb-2">
            <span className="font-semibold text-sm">Rule {index + 1}</span>
            <Button icon="pi pi-trash" rounded text severity="danger" size="small" onClick={() => removeRule(index)} tooltip="Remove Rule" />
          </div>

          <div className="grid">
            <div className="col-12 md:col-4">
              <div className="field mb-2">
                <label className="text-sm font-medium mb-1 block">Condition Field</label>
                <Dropdown
                  value={rule.conditionField}
                  options={conditionFieldSuggestions}
                  onChange={(e) => updateRule(index, 'conditionField', e.value)}
                  editable
                  placeholder="e.g., header.Authorization"
                  className="w-full"
                  style={{ fontSize: '0.85rem' }}
                />
              </div>
            </div>
            <div className="col-12 md:col-3">
              <div className="field mb-2">
                <label className="text-sm font-medium mb-1 block">Operator</label>
                <Dropdown
                  value={rule.conditionOperator}
                  options={operatorOptions}
                  onChange={(e) => updateRule(index, 'conditionOperator', e.value)}
                  className="w-full"
                  style={{ fontSize: '0.85rem' }}
                />
              </div>
            </div>
            <div className="col-12 md:col-3">
              <div className="field mb-2">
                <label className="text-sm font-medium mb-1 block">Value</label>
                <InputText
                  value={rule.conditionValue || ''}
                  onChange={(e) => updateRule(index, 'conditionValue', e.target.value)}
                  placeholder="Match value"
                  disabled={rule.conditionOperator === 'exists' || rule.conditionOperator === 'notExists'}
                  className="w-full"
                  style={{ fontSize: '0.85rem' }}
                />
              </div>
            </div>
            <div className="col-12 md:col-2">
              <div className="field mb-2">
                <label className="text-sm font-medium mb-1 block">Priority</label>
                <InputNumber
                  value={rule.priority}
                  onValueChange={(e) => updateRule(index, 'priority', e.value ?? 0)}
                  min={0}
                  max={999}
                  className="w-full"
                  inputStyle={{ fontSize: '0.85rem' }}
                />
              </div>
            </div>
          </div>

          <Divider className="my-2" />

          <div className="grid">
            <div className="col-12 md:col-3">
              <div className="field mb-2">
                <label className="text-sm font-medium mb-1 block">Status Code</label>
                <Dropdown
                  value={rule.statusCode}
                  options={statusCodeOptions}
                  onChange={(e) => updateRule(index, 'statusCode', e.value)}
                  className="w-full"
                  style={{ fontSize: '0.85rem' }}
                />
              </div>
            </div>
            <div className="col-12 md:col-4">
              <div className="field mb-2">
                <label className="text-sm font-medium mb-1 block">Content Type</label>
                <Dropdown
                  value={rule.contentType}
                  options={contentTypeOptions}
                  onChange={(e) => updateRule(index, 'contentType', e.value)}
                  editable
                  className="w-full"
                  style={{ fontSize: '0.85rem' }}
                />
              </div>
            </div>
            <div className="col-12">
              <div className="field mb-0">
                <label className="text-sm font-medium mb-1 block">Response Body</label>
                <InputTextarea
                  value={rule.responseBody || ''}
                  onChange={(e) => updateRule(index, 'responseBody', e.target.value)}
                  rows={3}
                  autoResize
                  style={{ fontFamily: 'monospace', fontSize: '0.8rem' }}
                  className="w-full"
                  placeholder='{"error": "Unauthorized"}'
                />
              </div>
            </div>
          </div>
        </div>
      ))}
    </div>
  );

  // ========== Sequence Editor Tab Content ==========
  const renderSequenceTab = () => (
    <div>
      <div className="flex align-items-center justify-content-between mb-3">
        <div className="flex align-items-center gap-3">
          <div className="flex align-items-center">
            <Checkbox
              inputId="isSequential"
              checked={mock?.isSequential || false}
              onChange={(e) => {
                let _mock = { ...mock };
                _mock.isSequential = e.checked;
                setMock(_mock);
              }}
            />
            <label htmlFor="isSequential" className="ml-2 font-semibold">Enable Sequential Mode</label>
          </div>
        </div>
        <Button
          label="Add Step"
          icon="pi pi-plus"
          size="small"
          severity="help"
          onClick={addSequenceItem}
          disabled={!mock?.isSequential}
        />
      </div>

      <p className="text-color-secondary text-sm mt-0 mb-3">
        When enabled, each request cycles through steps in order instead of returning the default response.
        After the last step, it wraps back to the first.
      </p>

      {!mock?.isSequential && (
        <Message severity="info" text="Enable sequential mode to configure response steps." className="w-full" />
      )}

      {mock?.isSequential && (!mock?.sequenceItems || mock.sequenceItems.length === 0) && (
        <Message severity="warn" text="No steps defined. Add at least one step to use sequential mode." className="w-full" />
      )}

      {mock?.isSequential && mock?.sequenceItems?.map((item, index) => (
        <div key={index} className="surface-ground border-round p-3 mb-3">
          <div className="flex align-items-center justify-content-between mb-2">
            <div className="flex align-items-center gap-2">
              <Badge value={index + 1} severity="help" />
              <span className="font-semibold text-sm">Step {index + 1}</span>
            </div>
            <div className="flex gap-1">
              <Button
                icon="pi pi-arrow-up"
                rounded text size="small"
                disabled={index === 0}
                onClick={() => moveSequenceItem(index, -1)}
                tooltip="Move Up"
              />
              <Button
                icon="pi pi-arrow-down"
                rounded text size="small"
                disabled={index === mock.sequenceItems.length - 1}
                onClick={() => moveSequenceItem(index, 1)}
                tooltip="Move Down"
              />
              <Button icon="pi pi-trash" rounded text severity="danger" size="small" onClick={() => removeSequenceItem(index)} tooltip="Remove Step" />
            </div>
          </div>

          <div className="grid">
            <div className="col-12 md:col-3">
              <div className="field mb-2">
                <label className="text-sm font-medium mb-1 block">Status Code</label>
                <Dropdown
                  value={item.statusCode}
                  options={statusCodeOptions}
                  onChange={(e) => updateSequenceItem(index, 'statusCode', e.value)}
                  className="w-full"
                  style={{ fontSize: '0.85rem' }}
                />
              </div>
            </div>
            <div className="col-12 md:col-4">
              <div className="field mb-2">
                <label className="text-sm font-medium mb-1 block">Content Type</label>
                <Dropdown
                  value={item.contentType}
                  options={contentTypeOptions}
                  onChange={(e) => updateSequenceItem(index, 'contentType', e.value)}
                  editable
                  className="w-full"
                  style={{ fontSize: '0.85rem' }}
                />
              </div>
            </div>
            <div className="col-12 md:col-3">
              <div className="field mb-2">
                <label className="text-sm font-medium mb-1 block">Delay (ms)</label>
                <InputNumber
                  value={item.delayMs}
                  onValueChange={(e) => updateSequenceItem(index, 'delayMs', e.value)}
                  min={0}
                  max={30000}
                  placeholder="Optional"
                  className="w-full"
                  inputStyle={{ fontSize: '0.85rem' }}
                />
              </div>
            </div>
            <div className="col-12">
              <div className="field mb-0">
                <label className="text-sm font-medium mb-1 block">Response Body</label>
                <InputTextarea
                  value={item.responseBody || ''}
                  onChange={(e) => updateSequenceItem(index, 'responseBody', e.target.value)}
                  rows={3}
                  autoResize
                  style={{ fontFamily: 'monospace', fontSize: '0.8rem' }}
                  className="w-full"
                  placeholder='{"status": "pending"}'
                />
              </div>
            </div>
          </div>
        </div>
      ))}
    </div>
  );

  return (
    <div>
      <Toast ref={toast} />
      <ConfirmDialog />

      {/* Page Header */}
      <div className="page-header">
        <div className="page-header-icon">
          <i className="pi pi-database"></i>
        </div>
        <div className="page-header-text">
          <h1>Mock Response Management</h1>
          <p>Create and manage mock API responses</p>
        </div>
      </div>

      {/* Main Content */}
      <div className="card">
        <Toolbar className="mb-4" start={leftToolbarTemplate} end={rightToolbarTemplate} />

        <Message
          severity="info"
          text="Mock responses will be matched against incoming requests based on HTTP method, route, and query string"
          className="mb-4 w-full"
        />

        <DataTable
          value={filteredMocks}
          selection={selectedMocks}
          onSelectionChange={(e) => setSelectedMocks(e.value)}
          dataKey="id"
          paginator
          rows={10}
          rowsPerPageOptions={[5, 10, 25, 50]}
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
          currentPageReportTemplate="Showing {first} to {last} of {totalRecords} mocks"
          globalFilter={globalFilter}
          header={header}
          loading={loading}
          stripedRows
          rowHover
          emptyMessage="No mock responses found."
          size="small"
          scrollable
          scrollHeight="flex"
          tableStyle={{ minWidth: '70rem' }}
        >
          <Column selectionMode="multiple" exportable={false} frozen style={{ width: '3rem' }} />
          <Column field="httpMethod" header="Method" sortable body={httpMethodBodyTemplate} style={{ width: '6rem' }} />
          <Column field="route" header="Route" sortable style={{ minWidth: '14rem' }} />
          <Column field="statusCode" header="Status" sortable body={statusCodeBodyTemplate} style={{ width: '5.5rem' }} />
          <Column
            field="delayMs"
            header="Delay"
            sortable
            body={(rowData) => rowData.delayMs ? (
              <Tag value={`${rowData.delayMs}ms`} severity="warning" />
            ) : (
              <span className="text-color-secondary">-</span>
            )}
            style={{ width: '5.5rem' }}
          />
          <Column
            header="Features"
            body={featuresBodyTemplate}
            style={{ minWidth: '9rem' }}
          />
          <Column
            field="description"
            header="Description"
            body={(rowData) => rowData.description || <span className="text-color-secondary">-</span>}
            style={{ minWidth: '10rem' }}
          />
          <Column
            field="collectionId"
            header="Collection"
            body={(rowData) => {
              const col = collections.find(c => c.id === rowData.collectionId);
              return col ? (
                <Tag value={col.name} style={{ backgroundColor: col.color || '#6366f1', color: '#fff' }} />
              ) : (
                <span className="text-color-secondary">-</span>
              );
            }}
            style={{ minWidth: '7rem' }}
          />
          <Column field="isActive" header="Active" sortable body={statusBodyTemplate} style={{ width: '5.5rem' }} />
          <Column body={actionBodyTemplate} exportable={false} frozen alignFrozen="right" style={{ width: '11rem' }} />
        </DataTable>
      </div>

      {/* Mock Dialog with TabView */}
      <Dialog
        visible={mockDialog}
        style={{ width: 'min(900px, 95vw)' }}
        header={isEditMode ? 'Edit Mock Response' : 'New Mock Response'}
        modal
        className="p-fluid"
        footer={mockDialogFooter}
        onHide={hideDialog}
        breakpoints={{ '960px': '90vw', '641px': '95vw' }}
      >
        <TabView activeIndex={activeTabIndex} onTabChange={(e) => setActiveTabIndex(e.index)}>
          {/* Tab 1: Basic Settings */}
          <TabPanel header="Basic" leftIcon="pi pi-cog mr-2">
            <div className="grid">
              <div className="col-12 md:col-6">
                <div className="field">
                  <label htmlFor="httpMethod">HTTP Method <span className="text-red-500">*</span></label>
                  <Dropdown
                    id="httpMethod"
                    value={mock?.httpMethod || ''}
                    options={httpMethodOptions}
                    onChange={(e) => onInputChange(e, 'httpMethod')}
                    placeholder="Select HTTP method"
                  />
                </div>
              </div>

              <div className="col-12 md:col-6">
                <div className="field">
                  <label htmlFor="statusCode">Status Code <span className="text-red-500">*</span></label>
                  <Dropdown
                    id="statusCode"
                    value={mock?.statusCode || 200}
                    options={statusCodeOptions}
                    onChange={(e) => onInputChange(e, 'statusCode')}
                    placeholder="Select status code"
                  />
                </div>
              </div>

              <div className="col-12 md:col-6">
                <div className="field">
                  <label htmlFor="delayMs">Response Delay (ms)</label>
                  <InputNumber
                    id="delayMs"
                    value={mock?.delayMs}
                    onValueChange={(e) => {
                      let _mock = { ...mock };
                      _mock.delayMs = e.value;
                      setMock(_mock);
                    }}
                    placeholder="e.g., 500"
                    min={0}
                    max={30000}
                  />
                  <small>Optional. Simulate network latency (0-30000ms)</small>
                </div>
              </div>

              <div className="col-12 md:col-6">
                <div className="field">
                  <label htmlFor="collectionId">Collection</label>
                  <Dropdown
                    id="collectionId"
                    value={mock?.collectionId}
                    options={collections.map(c => ({ label: c.name, value: c.id }))}
                    onChange={(e) => {
                      let _mock = { ...mock };
                      _mock.collectionId = e.value;
                      setMock(_mock);
                    }}
                    placeholder="No collection"
                    showClear
                  />
                </div>
              </div>

              <div className="col-12">
                <div className="field">
                  <label htmlFor="route">Route Pattern <span className="text-red-500">*</span></label>
                  <InputText
                    id="route"
                    value={mock?.route || ''}
                    onChange={(e) => onInputChange(e, 'route')}
                    placeholder="e.g., /api/users/{id}"
                    required
                  />
                  <small>Use {'{parameter}'} for path parameters</small>
                </div>
              </div>

              <div className="col-12 md:col-6">
                <div className="field">
                  <label htmlFor="queryString">Query String</label>
                  <InputText
                    id="queryString"
                    value={mock?.queryString || ''}
                    onChange={(e) => onInputChange(e, 'queryString')}
                    placeholder="e.g., ?page=1&size=10"
                  />
                </div>
              </div>

              <div className="col-12 md:col-6">
                <div className="field">
                  <label htmlFor="contentType">Content Type</label>
                  <Dropdown
                    id="contentType"
                    value={mock?.contentType || 'application/json'}
                    options={contentTypeOptions}
                    onChange={(e) => onInputChange(e, 'contentType')}
                    placeholder="Select content type"
                    editable
                  />
                </div>
              </div>

              <div className="col-12">
                <div className="field">
                  <label htmlFor="description">Description</label>
                  <InputText
                    id="description"
                    value={mock?.description || ''}
                    onChange={(e) => onInputChange(e, 'description')}
                    placeholder="Brief description of this mock response"
                  />
                </div>
              </div>

              <div className="col-12">
                <div className="field">
                  <label htmlFor="requestBody">Request Body (optional)</label>
                  <InputTextarea
                    id="requestBody"
                    value={mock?.requestBody || ''}
                    onChange={(e) => onInputChange(e, 'requestBody')}
                    rows={3}
                    autoResize
                    placeholder="Expected request body (for POST/PUT requests)"
                  />
                </div>
              </div>

              <div className="col-12">
                <div className="field">
                  <label htmlFor="responseBody">Response Body <span className="text-red-500">*</span></label>
                  <InputTextarea
                    id="responseBody"
                    value={mock?.responseBody || ''}
                    onChange={(e) => onInputChange(e, 'responseBody')}
                    rows={6}
                    autoResize
                    placeholder='e.g., {"id": "{{$randomUUID}}", "name": "{{$randomName}}"}'
                    style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}
                  />
                  <small>
                    Supports template variables.{' '}
                    <a onClick={() => setShowTemplateHelp(!showTemplateHelp)} style={{ cursor: 'pointer', color: 'var(--primary-color)' }}>
                      {showTemplateHelp ? 'Hide' : 'Show'} available variables
                    </a>
                  </small>
                  {showTemplateHelp && (
                    <div className="mt-2 p-3 border-round surface-ground text-sm" style={{ lineHeight: '1.8' }}>
                      <div className="font-semibold mb-2">Random Data:</div>
                      <code>{'{{$randomUUID}}'}</code> - UUID &nbsp;|&nbsp;
                      <code>{'{{$randomName}}'}</code> - Name &nbsp;|&nbsp;
                      <code>{'{{$randomEmail}}'}</code> - Email &nbsp;|&nbsp;
                      <code>{'{{$randomInt}}'}</code> - Integer &nbsp;|&nbsp;
                      <code>{'{{$randomInt(1,100)}}'}</code> - Range &nbsp;|&nbsp;
                      <code>{'{{$randomFloat}}'}</code> - Float &nbsp;|&nbsp;
                      <code>{'{{$randomBool}}'}</code> - Boolean
                      <div className="font-semibold mb-2 mt-3">Timestamps:</div>
                      <code>{'{{$timestamp}}'}</code> - Unix timestamp &nbsp;|&nbsp;
                      <code>{'{{$isoTimestamp}}'}</code> - ISO 8601
                      <div className="font-semibold mb-2 mt-3">Request Data:</div>
                      <code>{'{{$request.path}}'}</code> - Path &nbsp;|&nbsp;
                      <code>{'{{$request.method}}'}</code> - Method &nbsp;|&nbsp;
                      <code>{'{{$request.body}}'}</code> - Body &nbsp;|&nbsp;
                      <code>{'{{$request.query.paramName}}'}</code> - Query param &nbsp;|&nbsp;
                      <code>{'{{$request.header.headerName}}'}</code> - Header
                    </div>
                  )}
                </div>
              </div>

              <div className="col-12">
                <div className="field-checkbox">
                  <Checkbox
                    inputId="isActive"
                    checked={mock?.isActive || false}
                    onChange={(e) => {
                      let _mock = { ...mock };
                      _mock.isActive = e.checked;
                      setMock(_mock);
                    }}
                  />
                  <label htmlFor="isActive" className="ml-2 font-semibold">Active</label>
                </div>
              </div>
            </div>
          </TabPanel>

          {/* Tab 2: Response Rules */}
          <TabPanel
            header={
              <span>
                Response Rules
                {mock?.rules?.length > 0 && <Badge value={mock.rules.length} severity="info" className="ml-2" />}
              </span>
            }
            leftIcon="pi pi-sitemap mr-2"
          >
            {renderRulesTab()}
          </TabPanel>

          {/* Tab 3: Sequence */}
          <TabPanel
            header={
              <span>
                Sequence
                {mock?.isSequential && mock?.sequenceItems?.length > 0 && <Badge value={mock.sequenceItems.length} severity="help" className="ml-2" />}
              </span>
            }
            leftIcon="pi pi-replay mr-2"
          >
            {renderSequenceTab()}
          </TabPanel>
        </TabView>
      </Dialog>

      {/* cURL Import Dialog */}
      <Dialog
        visible={curlDialog}
        style={{ width: 'min(700px, 95vw)' }}
        header="Import from cURL"
        modal
        className="p-fluid"
        onHide={() => { setCurlDialog(false); setCurlCommand(''); }}
        breakpoints={{ '960px': '90vw', '641px': '95vw' }}
        footer={
          <>
            <Button label="Cancel" icon="pi pi-times" outlined severity="secondary" onClick={() => { setCurlDialog(false); setCurlCommand(''); }} />
            <Button label="Import" icon="pi pi-download" onClick={importFromCurl} loading={curlLoading} />
          </>
        }
      >
        <div className="field">
          <label htmlFor="curlInput">cURL Command</label>
          <InputTextarea
            id="curlInput"
            value={curlCommand}
            onChange={(e) => setCurlCommand(e.target.value)}
            rows={8}
            autoResize
            placeholder={"curl -X GET https://api.example.com/users \\\n  -H 'Accept: application/json'"}
            style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}
          />
          <small>Paste a cURL command. It will be executed and the response will be saved as a mock.</small>
        </div>
      </Dialog>

      {/* OpenAPI Import Dialog */}
      <Dialog
        visible={openApiDialog}
        style={{ width: 'min(800px, 95vw)' }}
        header="Import from OpenAPI (Swagger) JSON"
        modal
        className="p-fluid"
        onHide={() => { setOpenApiDialog(false); setOpenApiJson(''); }}
        breakpoints={{ '960px': '90vw', '641px': '95vw' }}
        footer={
          <>
            <Button label="Cancel" icon="pi pi-times" outlined severity="secondary" onClick={() => { setOpenApiDialog(false); setOpenApiJson(''); }} />
            <Button label="Import" icon="pi pi-file-import" onClick={importFromOpenApi} loading={openApiLoading} />
          </>
        }
      >
        <div className="field">
          <label htmlFor="openApiInput">OpenAPI JSON</label>
          <InputTextarea
            id="openApiInput"
            value={openApiJson}
            onChange={(e) => setOpenApiJson(e.target.value)}
            rows={12}
            autoResize
            placeholder='{"openapi": "3.0.0", "paths": { ... }}'
            style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}
          />
          <small>Paste an OpenAPI/Swagger JSON specification. A mock will be created for each path + method combination.</small>
        </div>
      </Dialog>
    </div>
  );
}
