import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { InputTextarea } from 'primereact/inputtextarea';
import { Tag } from 'primereact/tag';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { FileUpload } from 'primereact/fileupload';
import { Message } from 'primereact/message';
import { Dropdown } from 'primereact/dropdown';
import { Checkbox } from 'primereact/checkbox';
import { IconField } from 'primereact/iconfield';
import { InputIcon } from 'primereact/inputicon';
import { collectionService } from '../services/collectionService';
import { mockService } from '../services/mockService';

export default function CollectionsPage() {
  const navigate = useNavigate();
  const [collections, setCollections] = useState([]);
  const [loading, setLoading] = useState(false);
  const [collectionDialog, setCollectionDialog] = useState(false);
  const [collection, setCollection] = useState(null);
  const [isEditMode, setIsEditMode] = useState(false);
  const [importDialog, setImportDialog] = useState(false);
  const [importJson, setImportJson] = useState('');
  const [importLoading, setImportLoading] = useState(false);
  const [saveLoading, setSaveLoading] = useState(false);
  const [globalFilter, setGlobalFilter] = useState('');
  const [deleteCollectionDialog, setDeleteCollectionDialog] = useState(false);
  const [deleteCollectionId, setDeleteCollectionId] = useState(null);
  const [deleteCollectionName, setDeleteCollectionName] = useState('');
  const [deleteCollectionMockCount, setDeleteCollectionMockCount] = useState(0);
  const [deleteCollectionMockIds, setDeleteCollectionMockIds] = useState([]);
  const [deleteCollectionMoveToCollectionId, setDeleteCollectionMoveToCollectionId] = useState(null);
  const [deleteCollectionMoveToFolderId, setDeleteCollectionMoveToFolderId] = useState(null);
  const [deleteCollectionAlsoDeleteMocks, setDeleteCollectionAlsoDeleteMocks] = useState(false);
  const [deleteCollectionLoading, setDeleteCollectionLoading] = useState(false);
  const toast = useRef(null);

  const COLOR_PALETTE = [
    '#6366f1', '#8b5cf6', '#a855f7', '#d946ef', '#ec4899',
    '#f43f5e', '#ef4444', '#f97316', '#eab308', '#84cc16',
    '#22c55e', '#14b8a6', '#06b6d4', '#0ea5e9', '#3b82f6',
    '#64748b', '#78716c', '#000000'
  ];

  const emptyCollection = {
    name: '',
    description: '',
    color: ''
  };

  useEffect(() => {
    loadCollections();
  }, []);

  const loadCollections = async (includeFolders = true) => {
    setLoading(true);
    try {
      const data = await collectionService.getAllCollections(includeFolders);
      setCollections(data);
    } catch (error) {
      toast.current.show({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to load collections: ' + error.message,
        life: 3000
      });
    } finally {
      setLoading(false);
    }
  };

  const openNew = () => {
    setCollection(emptyCollection);
    setIsEditMode(false);
    setCollectionDialog(true);
  };

  const hideDialog = () => {
    setCollectionDialog(false);
  };

  const saveCollection = async () => {
    if (!collection.name.trim()) {
      toast.current.show({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please enter a collection name',
        life: 3000
      });
      return;
    }

    setSaveLoading(true);
    try {
      const hexColor = collection.color && (collection.color.startsWith('#') ? collection.color : '#' + collection.color);
      const data = {
        name: collection.name.trim(),
        description: collection.description?.trim() || null,
        color: hexColor || null
      };

      if (isEditMode) {
        await collectionService.updateCollection(collection.id, data);
        toast.current.show({ severity: 'success', summary: 'Success', detail: 'Collection updated successfully', life: 3000 });
      } else {
        await collectionService.createCollection(data);
        toast.current.show({ severity: 'success', summary: 'Success', detail: 'Collection created successfully', life: 3000 });
      }

      setCollectionDialog(false);
      setCollection(emptyCollection);
      loadCollections();
    } catch (error) {
      toast.current.show({ severity: 'error', summary: 'Error', detail: 'Failed to save collection: ' + error.message, life: 3000 });
    } finally {
      setSaveLoading(false);
    }
  };

  const editCollection = (col) => {
    setCollection({ ...col, color: col.color ?? '' });
    setIsEditMode(true);
    setCollectionDialog(true);
  };

  const openDeleteCollectionDialog = async (col) => {
    setDeleteCollectionId(col.id);
    setDeleteCollectionName(col.name);
    setDeleteCollectionMockCount(col.mockCount ?? 0);
    setDeleteCollectionMoveToCollectionId(null);
    setDeleteCollectionMoveToFolderId(null);
    setDeleteCollectionAlsoDeleteMocks(false);
    setDeleteCollectionDialog(true);
    try {
      const mocks = await mockService.getAllMocks(null, col.id, null);
      setDeleteCollectionMockIds(mocks.map((m) => m.id));
    } catch {
      setDeleteCollectionMockIds([]);
    }
  };

  const confirmDeleteCollection = async () => {
    if (!deleteCollectionId) return;
    setDeleteCollectionLoading(true);
    try {
      if (deleteCollectionMockIds.length > 0 && !deleteCollectionAlsoDeleteMocks) {
        await mockService.bulkUpdateMocks(
          deleteCollectionMockIds,
          deleteCollectionMoveToCollectionId ?? null,
          deleteCollectionMoveToFolderId ?? null
        );
      }
      await collectionService.deleteCollection(deleteCollectionId, deleteCollectionAlsoDeleteMocks);
      toast.current.show({ severity: 'success', summary: 'Success', detail: 'Collection deleted', life: 3000 });
      setDeleteCollectionDialog(false);
      loadCollections();
    } catch (error) {
      toast.current.show({ severity: 'error', summary: 'Error', detail: error.message, life: 3000 });
    } finally {
      setDeleteCollectionLoading(false);
    }
  };

  const exportCollection = async (col) => {
    try {
      const data = await collectionService.exportCollection(col.id);
      const json = JSON.stringify(data, null, 2);
      const blob = new Blob([json], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${col.name.toLowerCase().replace(/\s+/g, '-')}-collection.json`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
      toast.current.show({ severity: 'success', summary: 'Success', detail: 'Collection exported successfully', life: 3000 });
    } catch (error) {
      toast.current.show({ severity: 'error', summary: 'Error', detail: 'Failed to export: ' + error.message, life: 3000 });
    }
  };

  const importCollection = async () => {
    if (!importJson.trim()) {
      toast.current.show({ severity: 'warn', summary: 'Warning', detail: 'Please paste collection JSON or upload a file', life: 3000 });
      return;
    }
    setImportLoading(true);
    try {
      const data = JSON.parse(importJson);
      const result = await collectionService.importCollection(data);
      toast.current.show({ severity: 'success', summary: 'Success', detail: result.message, life: 4000 });
      setImportDialog(false);
      setImportJson('');
      loadCollections();
    } catch (error) {
      toast.current.show({ severity: 'error', summary: 'Error', detail: error.message, life: 5000 });
    } finally {
      setImportLoading(false);
    }
  };

  const onFileUpload = (e) => {
    const file = e.files && e.files[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = (event) => {
      try {
        const text = event.target.result;
        // Validate it's valid JSON
        JSON.parse(text);
        setImportJson(text);
        toast.current.show({ severity: 'info', summary: 'File Loaded', detail: `"${file.name}" loaded. Click Import to proceed.`, life: 3000 });
      } catch {
        toast.current.show({ severity: 'error', summary: 'Invalid JSON', detail: 'The uploaded file does not contain valid JSON.', life: 4000 });
      }
    };
    reader.readAsText(file);
    // Clear the file upload component so the same file can be re-selected
    e.options.clear();
  };

  const colorBodyTemplate = (rowData) => {
    return rowData.color ? (
      <div style={{ width: '1.5rem', height: '1.5rem', borderRadius: '50%', backgroundColor: rowData.color, border: '2px solid var(--surface-border)' }} />
    ) : (
      <span className="text-color-secondary">-</span>
    );
  };

  const actionBodyTemplate = (rowData) => (
    <div className="flex gap-2">
      <Button icon="pi pi-database" rounded outlined className="p-button-sm" onClick={() => navigate(`/data-buckets/${rowData.id}`)} tooltip="Data Buckets" tooltipOptions={{ position: 'top' }} />
      <Button icon="pi pi-download" rounded outlined severity="info" className="p-button-sm" onClick={() => exportCollection(rowData)} tooltip="Export" tooltipOptions={{ position: 'top' }} />
      <Button icon="pi pi-pencil" rounded outlined className="p-button-sm" onClick={() => editCollection(rowData)} tooltip="Edit" tooltipOptions={{ position: 'top' }} />
      <Button icon="pi pi-trash" rounded outlined severity="danger" className="p-button-sm" onClick={() => openDeleteCollectionDialog(rowData)} tooltip="Delete" tooltipOptions={{ position: 'top' }} />
    </div>
  );

  const leftToolbarTemplate = () => (
    <div className="flex gap-2">
      <Button label="New Collection" icon="pi pi-plus" severity="success" onClick={openNew} />
      <Button label="Import Collection" icon="pi pi-upload" severity="help" outlined onClick={() => setImportDialog(true)} />
    </div>
  );

  const rightToolbarTemplate = () => (
    <Button label="Refresh" icon="pi pi-refresh" outlined onClick={loadCollections} />
  );

  const header = (
    <div className="flex flex-wrap gap-2 align-items-center justify-content-between">
      <h4 className="m-0">Manage Collections</h4>
      <IconField iconPosition="left">
        <InputIcon className="pi pi-search" />
        <InputText type="search" onInput={(e) => setGlobalFilter(e.target.value)} placeholder="Search..." />
      </IconField>
    </div>
  );

  const dialogFooter = (
    <>
      <Button label="Cancel" icon="pi pi-times" outlined onClick={hideDialog} />
      <Button label="Save" icon="pi pi-check" loading={saveLoading} onClick={saveCollection} />
    </>
  );

  return (
    <div>
      <Toast ref={toast} />

      <div className="page-header">
        <div className="page-header-icon">
          <i className="pi pi-folder"></i>
        </div>
        <div className="page-header-text">
          <h1>Mock Collections</h1>
          <p>Group, organize and share your mock responses</p>
        </div>
      </div>

      <div className="card">
        <Toolbar className="mb-4" start={leftToolbarTemplate} end={rightToolbarTemplate} />

        <DataTable
          value={collections}
          dataKey="id"
          paginator
          rows={10}
          rowsPerPageOptions={[5, 10, 25]}
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
          currentPageReportTemplate="Showing {first} to {last} of {totalRecords} collections"
          globalFilter={globalFilter}
          header={header}
          loading={loading}
          stripedRows
          rowHover
          emptyMessage="No collections found."
          size="small"
          scrollable
          scrollHeight="flex"
          tableStyle={{ minWidth: '50rem' }}
        >
          <Column field="id" header="ID" sortable style={{ width: '5rem' }} />
          <Column field="color" header="Color" body={colorBodyTemplate} style={{ width: '5rem' }} />
          <Column field="name" header="Name" sortable style={{ minWidth: '12rem' }} body={(rowData) => <span className="font-semibold">{rowData.name}</span>} />
          <Column field="description" header="Description" body={(rowData) => rowData.description || <span className="text-color-secondary">-</span>} style={{ minWidth: '14rem' }} />
          <Column field="mockCount" header="Mocks" sortable body={(rowData) => <Tag value={rowData.mockCount} severity={rowData.mockCount > 0 ? 'info' : 'secondary'} />} style={{ width: '7rem' }} />
          <Column field="createdAt" header="Created" sortable body={(rowData) => new Date(rowData.createdAt).toLocaleDateString('tr-TR')} style={{ width: '8rem' }} />
          <Column body={actionBodyTemplate} exportable={false} style={{ width: '12rem' }} />
        </DataTable>
      </div>

      {/* Create/Edit Dialog — same style as MockManagementPage collection modals */}
      <Dialog
        visible={collectionDialog}
        style={{ width: '22rem' }}
        header={isEditMode ? 'Edit Collection' : 'New Collection'}
        modal
        className="p-fluid"
        footer={dialogFooter}
        onHide={hideDialog}
      >
        <div className="field">
          <label htmlFor="name">Name</label>
          <InputText
            id="name"
            value={collection?.name || ''}
            onChange={(e) => setCollection({ ...collection, name: e.target.value })}
            placeholder="e.g. Payment API, Auth Service"
            className="w-full"
          />
        </div>
        <div className="field">
          <label htmlFor="description">Description</label>
          <InputTextarea
            id="description"
            value={collection?.description || ''}
            onChange={(e) => setCollection({ ...collection, description: e.target.value })}
            rows={2}
            placeholder="Brief description"
            className="w-full"
          />
        </div>
        <div className="field">
          <label>Color</label>
          <div className="flex flex-wrap gap-2 align-items-center mt-1">
            <button
              type="button"
              className="border-circle w-2rem h-2rem border-2 surface-border flex-shrink-0"
              style={{ backgroundColor: 'transparent' }}
              onClick={() => setCollection({ ...collection, color: '' })}
              title="No color"
            />
            {COLOR_PALETTE.map((hex) => (
              <button
                key={hex}
                type="button"
                className="border-circle w-2rem h-2rem border-2 flex-shrink-0"
                style={{
                  backgroundColor: hex,
                  borderColor: (collection?.color && (collection.color === hex || collection.color === hex.replace('#', ''))) ? 'var(--primary-color)' : 'var(--surface-border)'
                }}
                onClick={() => setCollection({ ...collection, color: hex })}
                title={hex}
              />
            ))}
          </div>
        </div>
      </Dialog>

      {/* Delete Collection Dialog — same behaviour as MockManagementPage */}
      <Dialog
        visible={deleteCollectionDialog}
        header="Delete Collection"
        modal
        className="p-fluid"
        style={{ width: '28rem' }}
        onHide={() => setDeleteCollectionDialog(false)}
        footer={
          <>
            <Button label="Cancel" icon="pi pi-times" outlined onClick={() => setDeleteCollectionDialog(false)} />
            <Button label="Delete" icon="pi pi-trash" severity="danger" loading={deleteCollectionLoading} onClick={confirmDeleteCollection} />
          </>
        }
      >
        {deleteCollectionMockCount > 0 ? (
          <>
            <p className="mb-3">This collection has <strong>{deleteCollectionMockCount}</strong> route(s).</p>
            <div className="field mb-3">
              <label>Move routes to</label>
              <div className="grid">
                <div className="col-12 md:col-6">
                  <Dropdown
                    value={deleteCollectionMoveToCollectionId}
                    options={[
                      { label: 'Uncategorized', value: null },
                      ...collections.filter((c) => c.id !== deleteCollectionId).map((c) => ({ label: c.name, value: c.id }))
                    ]}
                    onChange={(e) => { setDeleteCollectionMoveToCollectionId(e.value); setDeleteCollectionMoveToFolderId(null); }}
                    placeholder="Collection"
                    className="w-full"
                  />
                </div>
                <div className="col-12 md:col-6">
                  <Dropdown
                    value={deleteCollectionMoveToFolderId}
                    options={[
                      { label: 'Uncategorized', value: null },
                      ...(collections.find((c) => c.id === deleteCollectionMoveToCollectionId)?.folders || []).map((f) => ({ label: f.name, value: f.id }))
                    ]}
                    onChange={(e) => setDeleteCollectionMoveToFolderId(e.value)}
                    placeholder="Folder"
                    className="w-full"
                    disabled={!deleteCollectionMoveToCollectionId}
                  />
                </div>
              </div>
            </div>
            <div className="field-checkbox mb-0">
              <Checkbox
                inputId="deleteCollectionAlsoDeleteMocks"
                checked={deleteCollectionAlsoDeleteMocks}
                onChange={(e) => setDeleteCollectionAlsoDeleteMocks(e.checked)}
              />
              <label htmlFor="deleteCollectionAlsoDeleteMocks">Delete routes instead</label>
            </div>
          </>
        ) : (
          <p>Delete this collection?</p>
        )}
      </Dialog>

      {/* Import Dialog */}
      <Dialog
        visible={importDialog}
        style={{ width: 'min(800px, 95vw)' }}
        header="Import Collection"
        modal
        className="p-fluid"
        onHide={() => { setImportDialog(false); setImportJson(''); }}
        breakpoints={{ '960px': '90vw', '641px': '95vw' }}
        footer={
          <>
            <Button label="Cancel" icon="pi pi-times" outlined severity="secondary" onClick={() => { setImportDialog(false); setImportJson(''); }} />
            <Button label="Import" icon="pi pi-upload" onClick={importCollection} loading={importLoading} disabled={!importJson.trim()} />
          </>
        }
      >
        <div className="field mb-3">
          <label className="font-semibold mb-2 block">Upload JSON File</label>
          <FileUpload
            mode="basic"
            accept=".json,application/json"
            maxFileSize={5000000}
            auto
            chooseLabel="Choose JSON File"
            chooseOptions={{ icon: 'pi pi-file', className: 'p-button-outlined' }}
            customUpload
            uploadHandler={onFileUpload}
          />
          <small>Upload a .json file exported from Mocklab</small>
        </div>

        <div className="flex align-items-center gap-2 my-3">
          <hr className="flex-1 border-top-1 surface-border" />
          <span className="text-color-secondary text-sm font-semibold">OR</span>
          <hr className="flex-1 border-top-1 surface-border" />
        </div>

        <div className="field">
          <label htmlFor="importJson" className="font-semibold mb-2 block">Paste JSON</label>
          <InputTextarea
            id="importJson"
            value={importJson}
            onChange={(e) => setImportJson(e.target.value)}
            rows={10}
            autoResize
            placeholder='{"collection": {"name": "...", "description": "..."}, "mocks": [...]}'
            style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}
          />
          <small>Paste the exported collection JSON here</small>
        </div>

        {importJson.trim() && (
          <Message severity="success" text="JSON loaded and ready for import." className="w-full mt-2" />
        )}
      </Dialog>
    </div>
  );
}
