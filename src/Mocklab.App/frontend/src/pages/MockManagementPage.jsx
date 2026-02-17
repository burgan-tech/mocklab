import { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { InputTextarea } from 'primereact/inputtextarea';
import { Dropdown } from 'primereact/dropdown';
import { Checkbox } from 'primereact/checkbox';
import { Tag } from 'primereact/tag';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { Message } from 'primereact/message';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { mockService } from '../services/mockService';

export default function MockManagementPage() {
  const [mocks, setMocks] = useState([]);
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
    isActive: true
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

  useEffect(() => {
    loadMocks();
  }, []);

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
    setMockDialog(true);
  };

  const hideDialog = () => {
    setMockDialog(false);
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
      if (isEditMode) {
        await mockService.updateMock(mock.id, mock);
        toast.current.show({
          severity: 'success',
          summary: 'Success',
          detail: 'Mock updated successfully',
          life: 3000
        });
      } else {
        await mockService.createMock(mock);
        toast.current.show({
          severity: 'success',
          summary: 'Success',
          detail: 'Mock created successfully',
          life: 3000
        });
      }
      
      setMockDialog(false);
      setMock(emptyMock);
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

  const editMock = (mock) => {
    setMock({ ...mock });
    setIsEditMode(true);
    setMockDialog(true);
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

  const statusBodyTemplate = (rowData) => {
    return (
      <Tag 
        value={rowData.isActive ? 'Active' : 'Inactive'} 
        severity={rowData.isActive ? 'success' : 'danger'} 
      />
    );
  };

  const httpMethodBodyTemplate = (rowData) => {
    const methodColors = {
      GET: 'info',
      POST: 'success',
      PUT: 'warning',
      DELETE: 'danger',
      PATCH: 'help',
      HEAD: 'secondary',
      OPTIONS: 'secondary'
    };

    return (
      <Tag 
        value={rowData.httpMethod} 
        severity={methodColors[rowData.httpMethod] || 'info'}
        rounded
      />
    );
  };

  const statusCodeBodyTemplate = (rowData) => {
    const getSeverity = (code) => {
      if (code >= 200 && code < 300) return 'success';
      if (code >= 300 && code < 400) return 'info';
      if (code >= 400 && code < 500) return 'warning';
      if (code >= 500) return 'danger';
      return 'secondary';
    };

    return (
      <Tag 
        value={rowData.statusCode} 
        severity={getSeverity(rowData.statusCode)}
      />
    );
  };

  const actionBodyTemplate = (rowData) => {
    return (
      <div className="flex gap-2">
        <Button
          icon={rowData.isActive ? 'pi pi-eye-slash' : 'pi pi-eye'}
          rounded
          outlined
          severity={rowData.isActive ? 'warning' : 'success'}
          className="p-button-sm"
          onClick={() => toggleMock(rowData)}
          tooltip={rowData.isActive ? 'Deactivate' : 'Activate'}
          tooltipOptions={{ position: 'top' }}
        />
        <Button
          icon="pi pi-pencil"
          rounded
          outlined
          className="p-button-sm"
          onClick={() => editMock(rowData)}
          tooltip="Edit"
          tooltipOptions={{ position: 'top' }}
        />
        <Button
          icon="pi pi-trash"
          rounded
          outlined
          severity="danger"
          className="p-button-sm"
          onClick={() => deleteMock(rowData)}
          tooltip="Delete"
          tooltipOptions={{ position: 'top' }}
        />
      </div>
    );
  };

  const leftToolbarTemplate = () => {
    return (
      <div className="flex gap-2">
        <Button
          label="New Mock"
          icon="pi pi-plus"
          severity="success"
          onClick={openNew}
        />
        <Button
          label="Import cURL"
          icon="pi pi-download"
          severity="help"
          outlined
          onClick={() => setCurlDialog(true)}
        />
        <Button
          label="Import OpenAPI"
          icon="pi pi-file-import"
          severity="info"
          outlined
          onClick={() => setOpenApiDialog(true)}
        />
      </div>
    );
  };

  const rightToolbarTemplate = () => {
    return (
      <div className="flex gap-2">
        <Button
          label="Refresh"
          icon="pi pi-refresh"
          outlined
          onClick={loadMocks}
        />
        <Button
          label="Clear All"
          icon="pi pi-trash"
          severity="danger"
          outlined
          onClick={clearAllMocks}
        />
      </div>
    );
  };

  const header = (
    <div className="flex flex-wrap gap-2 align-items-center justify-content-between">
      <h4 className="m-0">Manage Mock Responses</h4>
      <span className="p-input-icon-left">
        <i className="pi pi-search" />
        <InputText
          type="search"
          onInput={(e) => setGlobalFilter(e.target.value)}
          placeholder="Search..."
        />
      </span>
    </div>
  );

  const mockDialogFooter = (
    <>
      <Button
        label="Cancel"
        icon="pi pi-times"
        outlined
        severity="secondary"
        onClick={hideDialog}
      />
      <Button
        label="Save"
        icon="pi pi-check"
        onClick={saveMock}
      />
    </>
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
        <Toolbar
          className="mb-4"
          start={leftToolbarTemplate}
          end={rightToolbarTemplate}
        />

        <Message
          severity="info"
          text="Mock responses will be matched against incoming requests based on HTTP method, route, and query string"
          className="mb-4 w-full"
        />

        <DataTable
          value={mocks}
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
        >
          <Column selectionMode="multiple" exportable={false} style={{ width: '3rem' }} />
          <Column field="id" header="ID" sortable style={{ width: '5rem' }} />
          <Column
            field="httpMethod"
            header="Method"
            sortable
            body={httpMethodBodyTemplate}
            style={{ width: '7rem' }}
          />
          <Column
            field="route"
            header="Route"
            sortable
            style={{ minWidth: '14rem' }}
          />
          <Column
            field="statusCode"
            header="Status"
            sortable
            body={statusCodeBodyTemplate}
            style={{ width: '7rem' }}
          />
          <Column
            field="contentType"
            header="Content Type"
            body={(rowData) => (
              <span className="text-color-secondary text-sm">{rowData.contentType}</span>
            )}
            style={{ minWidth: '10rem' }}
          />
          <Column
            field="description"
            header="Description"
            body={(rowData) => rowData.description || <span className="text-color-secondary">-</span>}
            style={{ minWidth: '12rem' }}
          />
          <Column
            field="isActive"
            header="Active"
            sortable
            body={statusBodyTemplate}
            style={{ width: '6rem' }}
          />
          <Column
            body={actionBodyTemplate}
            exportable={false}
            style={{ width: '10rem' }}
          />
        </DataTable>
      </div>

      {/* Mock Dialog */}
      <Dialog
        visible={mockDialog}
        style={{ width: 'min(800px, 95vw)' }}
        header={isEditMode ? 'Edit Mock Response' : 'New Mock Response'}
        modal
        className="p-fluid"
        footer={mockDialogFooter}
        onHide={hideDialog}
        breakpoints={{ '960px': '90vw', '641px': '95vw' }}
      >
        <div className="grid">
          <div className="col-12 md:col-6">
            <div className="field">
              <label htmlFor="httpMethod">
                HTTP Method <span className="text-red-500">*</span>
              </label>
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
              <label htmlFor="statusCode">
                Status Code <span className="text-red-500">*</span>
              </label>
              <Dropdown
                id="statusCode"
                value={mock?.statusCode || 200}
                options={statusCodeOptions}
                onChange={(e) => onInputChange(e, 'statusCode')}
                placeholder="Select status code"
              />
            </div>
          </div>

          <div className="col-12">
            <div className="field">
              <label htmlFor="route">
                Route Pattern <span className="text-red-500">*</span>
              </label>
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
                rows={4}
                autoResize
                placeholder="Expected request body (for POST/PUT requests)"
              />
            </div>
          </div>

          <div className="col-12">
            <div className="field">
              <label htmlFor="responseBody">
                Response Body <span className="text-red-500">*</span>
              </label>
              <InputTextarea
                id="responseBody"
                value={mock?.responseBody || ''}
                onChange={(e) => onInputChange(e, 'responseBody')}
                rows={8}
                autoResize
                placeholder='e.g., {"message": "Success", "data": []}'
              />
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
              <label htmlFor="isActive" className="ml-2 font-semibold">
                Active
              </label>
            </div>
          </div>
        </div>
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
            <Button
              label="Cancel"
              icon="pi pi-times"
              outlined
              severity="secondary"
              onClick={() => { setCurlDialog(false); setCurlCommand(''); }}
            />
            <Button
              label="Import"
              icon="pi pi-download"
              onClick={importFromCurl}
              loading={curlLoading}
            />
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
          <small>
            Paste a cURL command. It will be executed and the response will be saved as a mock.
          </small>
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
            <Button
              label="Cancel"
              icon="pi pi-times"
              outlined
              severity="secondary"
              onClick={() => { setOpenApiDialog(false); setOpenApiJson(''); }}
            />
            <Button
              label="Import"
              icon="pi pi-file-import"
              onClick={importFromOpenApi}
              loading={openApiLoading}
            />
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
          <small>
            Paste an OpenAPI/Swagger JSON specification. A mock will be created for each path + method combination.
          </small>
        </div>
      </Dialog>
    </div>
  );
}
