import { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { InputTextarea } from 'primereact/inputtextarea';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { FileUpload } from 'primereact/fileupload';
import { collectionService } from '../services/collectionService';
import { dataBucketService } from '../services/dataBucketService';

const MAX_IMPORT_FILE_SIZE_BYTES = 5 * 1024 * 1024; // 5 MB

function validateDataJson(str) {
  if (!str || !str.trim()) return { valid: true, value: '[]' };
  try {
    const parsed = JSON.parse(str);
    if (Array.isArray(parsed) || (typeof parsed === 'object' && parsed !== null)) {
      return { valid: true, value: JSON.stringify(parsed) };
    }
    return { valid: false, error: 'Data must be a JSON array or object.' };
  } catch (e) {
    return { valid: false, error: e.message || 'Invalid JSON' };
  }
}

export default function DataBucketsPage() {
  const { collectionId } = useParams();
  const navigate = useNavigate();
  const toast = useRef(null);
  const [buckets, setBuckets] = useState([]);
  const [collectionName, setCollectionName] = useState('');
  const [loading, setLoading] = useState(false);
  const [bucketDialog, setBucketDialog] = useState(false);
  const [bucket, setBucket] = useState(null);
  const [isEditMode, setIsEditMode] = useState(false);
  const [saveLoading, setSaveLoading] = useState(false);
  const [deleteDialog, setDeleteDialog] = useState(false);
  const [bucketToDelete, setBucketToDelete] = useState(null);
  const [deleteLoading, setDeleteLoading] = useState(false);
  const [importJson, setImportJson] = useState('');
  const [dataJsonError, setDataJsonError] = useState('');

  const emptyBucket = { name: '', description: '', data: '[]' };

  useEffect(() => {
    if (!collectionId) return;
    loadCollection();
    loadBuckets();
  }, [collectionId]);

  const loadCollection = async () => {
    try {
      const col = await collectionService.getCollection(collectionId);
      setCollectionName(col?.name ?? 'Collection');
    } catch {
      setCollectionName('Collection');
    }
  };

  const loadBuckets = async () => {
    setLoading(true);
    try {
      const data = await dataBucketService.getAll(collectionId);
      setBuckets(data);
    } catch (error) {
      toast.current.show({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to load data buckets: ' + error.message,
        life: 4000,
      });
    } finally {
      setLoading(false);
    }
  };

  const openNew = () => {
    setBucket(emptyBucket);
    setIsEditMode(false);
    setImportJson('');
    setDataJsonError('');
    setBucketDialog(true);
  };

  const editBucket = async (row) => {
    setBucket({ ...row, data: '[]' });
    setIsEditMode(true);
    setImportJson('');
    setDataJsonError('');
    setBucketDialog(true);
    try {
      const full = await dataBucketService.getOne(collectionId, row.id);
      setBucket((prev) => ({ ...prev, data: full.data ?? '[]' }));
    } catch (error) {
      toast.current.show({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to load bucket: ' + error.message,
        life: 4000,
      });
    }
  };

  const hideBucketDialog = () => {
    setBucketDialog(false);
    setBucket(null);
    setDataJsonError('');
    setImportJson('');
  };

  const applyImportJson = () => {
    if (!importJson.trim()) {
      toast.current.show({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Paste JSON first',
        life: 3000,
      });
      return;
    }
    const result = validateDataJson(importJson);
    if (result.valid) {
      setBucket((prev) => ({ ...prev, data: result.value }));
      setDataJsonError('');
      setImportJson('');
      toast.current.show({
        severity: 'success',
        summary: 'Applied',
        detail: 'Data replaced with pasted JSON',
        life: 3000,
      });
    } else {
      setDataJsonError(result.error);
      toast.current.show({
        severity: 'error',
        summary: 'Invalid JSON',
        detail: result.error,
        life: 4000,
      });
    }
  };

  const onImportFile = (e) => {
    const file = e.files && e.files[0];
    if (!file) return;
    if (file.size > MAX_IMPORT_FILE_SIZE_BYTES) {
      toast.current.show({
        severity: 'error',
        summary: 'File too large',
        detail: `Maximum size is ${MAX_IMPORT_FILE_SIZE_BYTES / 1024 / 1024} MB`,
        life: 4000,
      });
      e.options.clear();
      return;
    }
    const reader = new FileReader();
    reader.onload = (event) => {
      try {
        const text = event.target.result;
        const result = validateDataJson(text);
        if (result.valid) {
          setBucket((prev) => ({ ...prev, data: result.value }));
          setDataJsonError('');
          setImportJson('');
          toast.current.show({
            severity: 'success',
            summary: 'File loaded',
            detail: `"${file.name}" applied.`,
            life: 3000,
          });
        } else {
          setDataJsonError(result.error);
          toast.current.show({
            severity: 'error',
            summary: 'Invalid JSON',
            detail: result.error,
            life: 4000,
          });
        }
      } catch {
        toast.current.show({
          severity: 'error',
          summary: 'Invalid JSON',
          detail: 'The file does not contain valid JSON.',
          life: 4000,
        });
      }
    };
    reader.readAsText(file);
    e.options.clear();
  };

  const saveBucket = async () => {
    if (!bucket.name.trim()) {
      toast.current.show({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Name is required',
        life: 3000,
      });
      return;
    }
    const result = validateDataJson(bucket.data);
    if (!result.valid) {
      setDataJsonError(result.error);
      toast.current.show({
        severity: 'error',
        summary: 'Invalid JSON',
        detail: result.error,
        life: 4000,
      });
      return;
    }
    setDataJsonError('');
    setSaveLoading(true);
    try {
      const payload = {
        name: bucket.name.trim(),
        description: bucket.description?.trim() || null,
        data: result.value,
      };
      if (isEditMode) {
        await dataBucketService.update(collectionId, bucket.id, payload);
        toast.current.show({
          severity: 'success',
          summary: 'Success',
          detail: 'Data bucket updated',
          life: 3000,
        });
      } else {
        await dataBucketService.create(collectionId, payload);
        toast.current.show({
          severity: 'success',
          summary: 'Success',
          detail: 'Data bucket created',
          life: 3000,
        });
      }
      hideBucketDialog();
      loadBuckets();
    } catch (error) {
      toast.current.show({
        severity: 'error',
        summary: 'Error',
        detail: error.message,
        life: 4000,
      });
    } finally {
      setSaveLoading(false);
    }
  };

  const openDeleteDialog = (row) => {
    setBucketToDelete(row);
    setDeleteDialog(true);
  };

  const confirmDelete = async () => {
    if (!bucketToDelete) return;
    setDeleteLoading(true);
    try {
      await dataBucketService.remove(collectionId, bucketToDelete.id);
      toast.current.show({
        severity: 'success',
        summary: 'Success',
        detail: 'Data bucket deleted',
        life: 3000,
      });
      setDeleteDialog(false);
      setBucketToDelete(null);
      loadBuckets();
    } catch (error) {
      toast.current.show({
        severity: 'error',
        summary: 'Error',
        detail: error.message,
        life: 4000,
      });
    } finally {
      setDeleteLoading(false);
    }
  };

  const exportBucket = async (row) => {
    try {
      const data = await dataBucketService.exportBucket(collectionId, row.id);
      const json = JSON.stringify(data, null, 2);
      const blob = new Blob([json], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${(row.name || 'bucket').toLowerCase().replace(/\s+/g, '-')}-bucket.json`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
      toast.current.show({
        severity: 'success',
        summary: 'Exported',
        detail: `"${row.name}" downloaded`,
        life: 3000,
      });
    } catch (error) {
      toast.current.show({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to export: ' + error.message,
        life: 3000,
      });
    }
  };

  const exportAll = async () => {
    try {
      const data = await dataBucketService.exportAll(collectionId);
      const json = JSON.stringify(data, null, 2);
      const blob = new Blob([json], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${(collectionName || 'collection').toLowerCase().replace(/\s+/g, '-')}-data-buckets.json`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
      toast.current.show({
        severity: 'success',
        summary: 'Exported',
        detail: 'All data buckets downloaded',
        life: 3000,
      });
    } catch (error) {
      toast.current.show({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to export: ' + error.message,
        life: 3000,
      });
    }
  };

  const formatJson = () => {
    const result = validateDataJson(bucket?.data);
    if (result.valid) {
      try {
        const parsed = JSON.parse(result.value);
        setBucket((prev) => ({ ...prev, data: JSON.stringify(parsed, null, 2) }));
        setDataJsonError('');
      } catch {
        // no-op
      }
    }
  };

  const actionBodyTemplate = (rowData) => (
    <div className="flex gap-2">
      <Button
        icon="pi pi-pencil"
        rounded
        outlined
        className="p-button-sm"
        onClick={() => editBucket(rowData)}
        tooltip="Edit"
        tooltipOptions={{ position: 'top' }}
      />
      <Button
        icon="pi pi-download"
        rounded
        outlined
        severity="info"
        className="p-button-sm"
        onClick={() => exportBucket(rowData)}
        tooltip="Export"
        tooltipOptions={{ position: 'top' }}
      />
      <Button
        icon="pi pi-trash"
        rounded
        outlined
        severity="danger"
        className="p-button-sm"
        onClick={() => openDeleteDialog(rowData)}
        tooltip="Delete"
        tooltipOptions={{ position: 'top' }}
      />
    </div>
  );

  const leftToolbarTemplate = () => (
    <div className="flex gap-2">
      <Button label="Back to Collections" icon="pi pi-arrow-left" outlined onClick={() => navigate('/collections')} />
      <Button label="New Data Bucket" icon="pi pi-plus" severity="success" onClick={openNew} />
      <Button
        label="Export all"
        icon="pi pi-download"
        outlined
        severity="info"
        onClick={exportAll}
        disabled={buckets.length === 0}
      />
    </div>
  );

  const rightToolbarTemplate = () => (
    <Button label="Refresh" icon="pi pi-refresh" outlined onClick={loadBuckets} />
  );

  const bucketDialogFooter = (
    <>
      <Button label="Cancel" icon="pi pi-times" outlined onClick={hideBucketDialog} />
      <Button label="Save" icon="pi pi-check" loading={saveLoading} onClick={saveBucket} />
    </>
  );

  if (!collectionId) {
    return (
      <div className="p-4">
        <p>Invalid collection.</p>
        <Button label="Back to Collections" onClick={() => navigate('/collections')} />
      </div>
    );
  }

  return (
    <div>
      <Toast ref={toast} />
      <div className="page-header">
        <div className="page-header-icon">
          <i className="pi pi-database"></i>
        </div>
        <div className="page-header-text">
          <h1>Data Buckets</h1>
          <p>{collectionName ? `Data for collection "${collectionName}"` : 'Manage JSON data used in Scriban templates'}</p>
        </div>
      </div>

      <div className="card">
        <Toolbar className="mb-4" start={leftToolbarTemplate} end={rightToolbarTemplate} />
        <DataTable
          value={buckets}
          dataKey="id"
          loading={loading}
          stripedRows
          rowHover
          emptyMessage="No data buckets. Add one to use in templates (e.g. persons, products)."
          size="small"
          scrollable
          scrollHeight="flex"
          tableStyle={{ minWidth: '40rem' }}
        >
          <Column field="id" header="ID" sortable style={{ width: '4rem' }} />
          <Column field="name" header="Name" sortable style={{ minWidth: '10rem' }} />
          <Column
            field="description"
            header="Description"
            body={(row) => row.description || <span className="text-color-secondary">-</span>}
            style={{ minWidth: '12rem' }}
          />
          <Column
            field="updatedAt"
            header="Updated"
            sortable
            body={(row) => (row.updatedAt ? new Date(row.updatedAt).toLocaleString() : '-')}
            style={{ width: '10rem' }}
          />
          <Column body={actionBodyTemplate} exportable={false} style={{ width: '10rem' }} />
        </DataTable>
      </div>

      <Dialog
        visible={bucketDialog}
        header={isEditMode ? 'Edit Data Bucket' : 'New Data Bucket'}
        modal
        className="p-fluid"
        style={{ width: 'min(90vw, 42rem)' }}
        footer={bucketDialogFooter}
        onHide={hideBucketDialog}
      >
        <div className="field">
          <label htmlFor="bucket-name">Name (used as template variable)</label>
          <InputText
            id="bucket-name"
            value={bucket?.name ?? ''}
            onChange={(e) => setBucket((prev) => ({ ...prev, name: e.target.value }))}
            placeholder="e.g. persons, products"
            className="w-full"
          />
        </div>
        <div className="field">
          <label htmlFor="bucket-desc">Description (optional)</label>
          <InputText
            id="bucket-desc"
            value={bucket?.description ?? ''}
            onChange={(e) => setBucket((prev) => ({ ...prev, description: e.target.value }))}
            placeholder="Brief description"
            className="w-full"
          />
        </div>
        <div className="field">
          <div className="flex align-items-center justify-content-between mb-1">
            <label htmlFor="bucket-data">Data (JSON array or object)</label>
            <Button label="Format" icon="pi pi-align-left" text className="p-button-sm" onClick={formatJson} />
          </div>
          <InputTextarea
            id="bucket-data"
            value={bucket?.data ?? '[]'}
            onChange={(e) => {
              setBucket((prev) => ({ ...prev, data: e.target.value }));
              setDataJsonError('');
            }}
            rows={8}
            className="w-full font-monospace"
            style={{ fontFamily: 'ui-monospace, monospace' }}
          />
          {dataJsonError && <small className="text-red-500 block mt-1">{dataJsonError}</small>}
        </div>
        <div className="field">
          <label>Import from JSON</label>
          <InputTextarea
            value={importJson}
            onChange={(e) => setImportJson(e.target.value)}
            placeholder="Paste JSON here, then click Apply to replace data"
            rows={3}
            className="w-full font-monospace mb-2"
          />
          <Button label="Apply (replace data)" icon="pi pi-check" outlined onClick={applyImportJson} />
        </div>
        <div className="field">
          <label>Import from file (.json, max 5 MB)</label>
          <FileUpload
            mode="basic"
            accept=".json,application/json"
            maxFileSize={MAX_IMPORT_FILE_SIZE_BYTES}
            chooseLabel="Choose JSON file"
            chooseOptions={{ className: 'p-button-outlined' }}
            onSelect={onImportFile}
          />
        </div>
      </Dialog>

      <Dialog
        visible={deleteDialog}
        header="Delete Data Bucket"
        modal
        style={{ width: '24rem' }}
        onHide={() => setDeleteDialog(false)}
        footer={
          <>
            <Button label="Cancel" icon="pi pi-times" outlined onClick={() => setDeleteDialog(false)} />
            <Button label="Delete" icon="pi pi-trash" severity="danger" loading={deleteLoading} onClick={confirmDelete} />
          </>
        }
      >
        {bucketToDelete && (
          <p>
            Delete data bucket <strong>{bucketToDelete.name}</strong>? Templates using this bucket will get empty or
            null data.
          </p>
        )}
      </Dialog>
    </div>
  );
}
