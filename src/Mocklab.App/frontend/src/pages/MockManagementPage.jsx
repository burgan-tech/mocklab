import { useState, useEffect, useRef, useMemo } from 'react';
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
import { Tree } from 'primereact/tree';
import { ContextMenu } from 'primereact/contextmenu';
import { mockService } from '../services/mockService';
import { collectionService } from '../services/collectionService';
import { folderService } from '../services/folderService';

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
  const [treeSelectionKey, setTreeSelectionKey] = useState('all');
  const [treeSelectionData, setTreeSelectionData] = useState({ type: 'all' });
  const [folderDialog, setFolderDialog] = useState(false);
  const [newFolderCollectionId, setNewFolderCollectionId] = useState(null);
  const [newFolderName, setNewFolderName] = useState('');
  const [newFolderColor, setNewFolderColor] = useState('');
  const [folderDialogLoading, setFolderDialogLoading] = useState(false);
  const [contextMenuNodeKey, setContextMenuNodeKey] = useState(null);
  const [editCollectionDialog, setEditCollectionDialog] = useState(false);
  const [editCollectionId, setEditCollectionId] = useState(null);
  const [editCollectionName, setEditCollectionName] = useState('');
  const [editCollectionDescription, setEditCollectionDescription] = useState('');
  const [editCollectionColor, setEditCollectionColor] = useState('');
  const [editCollectionLoading, setEditCollectionLoading] = useState(false);
  const [editFolderDialog, setEditFolderDialog] = useState(false);
  const [editFolderId, setEditFolderId] = useState(null);
  const [editFolderName, setEditFolderName] = useState('');
  const [editFolderColor, setEditFolderColor] = useState('');
  const [editFolderCollectionId, setEditFolderCollectionId] = useState(null);
  const [editFolderParentId, setEditFolderParentId] = useState(null);
  const [editFolderLoading, setEditFolderLoading] = useState(false);
  const [newCollectionDialog, setNewCollectionDialog] = useState(false);
  const [newCollectionName, setNewCollectionName] = useState('');
  const [newCollectionDescription, setNewCollectionDescription] = useState('');
  const [newCollectionColor, setNewCollectionColor] = useState('');
  const [newCollectionLoading, setNewCollectionLoading] = useState(false);
  const [deleteFolderDialog, setDeleteFolderDialog] = useState(false);
  const [deleteFolderId, setDeleteFolderId] = useState(null);
  const [deleteFolderCollectionId, setDeleteFolderCollectionId] = useState(null);
  const [deleteFolderMockCount, setDeleteFolderMockCount] = useState(0);
  const [deleteFolderMoveToCollectionId, setDeleteFolderMoveToCollectionId] = useState(null);
  const [deleteFolderMoveToFolderId, setDeleteFolderMoveToFolderId] = useState(null);
  const [deleteFolderAlsoDeleteMocks, setDeleteFolderAlsoDeleteMocks] = useState(false);
  const [deleteFolderLoading, setDeleteFolderLoading] = useState(false);
  const [deleteCollectionDialog, setDeleteCollectionDialog] = useState(false);
  const [deleteCollectionId, setDeleteCollectionId] = useState(null);
  const [deleteCollectionMockCount, setDeleteCollectionMockCount] = useState(0);
  const [deleteCollectionMoveToCollectionId, setDeleteCollectionMoveToCollectionId] = useState(null);
  const [deleteCollectionMoveToFolderId, setDeleteCollectionMoveToFolderId] = useState(null);
  const [deleteCollectionAlsoDeleteMocks, setDeleteCollectionAlsoDeleteMocks] = useState(false);
  const [deleteCollectionLoading, setDeleteCollectionLoading] = useState(false);
  const [bulkMoveDialog, setBulkMoveDialog] = useState(false);
  const [bulkMoveCollectionId, setBulkMoveCollectionId] = useState(null);
  const [bulkMoveFolderId, setBulkMoveFolderId] = useState(null);
  const [bulkMoveLoading, setBulkMoveLoading] = useState(false);
  const toast = useRef(null);
  const contextMenuRef = useRef(null);

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
    folderId: null,
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

  const FOLDER_COLOR_PALETTE = [
    '#6366f1', '#8b5cf6', '#a855f7', '#d946ef', '#ec4899',
    '#f43f5e', '#ef4444', '#f97316', '#eab308', '#84cc16',
    '#22c55e', '#14b8a6', '#06b6d4', '#0ea5e9', '#3b82f6',
    '#64748b', '#78716c', '#000000'
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
    loadCollections();
  }, []);

  useEffect(() => {
    if (collections.length === 0 && treeSelectionData.type === 'all') {
      loadMocksForSelection({ type: 'all' });
      return;
    }
    loadMocksForSelection(treeSelectionData);
  }, [treeSelectionKey, treeSelectionData.type, treeSelectionData.collectionId, treeSelectionData.folderId]);

  const loadCollections = async () => {
    try {
      const data = await collectionService.getAllCollections(true);
      setCollections(data);
    } catch {
      // Silently fail - collections are optional
    }
  };

  const loadMocksForSelection = async (selection) => {
    setLoading(true);
    try {
      let data;
      if (selection.type === 'all' || selection.type === 'uncategorized') {
        data = await mockService.getAllMocks(null, null, null);
      } else if (selection.type === 'collectionAll' || selection.type === 'collectionUncategorized') {
        data = await mockService.getAllMocks(null, selection.collectionId, null);
      } else if (selection.type === 'folder') {
        data = await mockService.getAllMocks(null, null, selection.folderId);
      } else {
        data = await mockService.getAllMocks(null, selection.collectionId ?? null, null);
      }
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

  const loadMocks = () => loadMocksForSelection(treeSelectionData);

  const openFolderDialog = (presetCollectionId = null) => {
    setNewFolderCollectionId(presetCollectionId ?? (collections.length ? collections[0].id : null));
    setNewFolderName('');
    setNewFolderColor('');
    setFolderDialog(true);
  };

  const openEditCollectionDialog = (collectionId) => {
    const c = collections.find((x) => x.id === collectionId);
    if (!c) return;
    setEditCollectionId(c.id);
    setEditCollectionName(c.name);
    setEditCollectionDescription(c.description ?? '');
    setEditCollectionColor(c.color ?? '');
    setEditCollectionDialog(true);
  };

  const saveEditCollection = async () => {
    if (!editCollectionName.trim()) {
      toast.current.show({ severity: 'warn', summary: 'Warning', detail: 'Name is required', life: 3000 });
      return;
    }
    setEditCollectionLoading(true);
    try {
      await collectionService.updateCollection(editCollectionId, {
        name: editCollectionName.trim(),
        description: editCollectionDescription.trim() || null,
        color: editCollectionColor.trim() || null
      });
      toast.current.show({ severity: 'success', summary: 'Success', detail: 'Collection updated', life: 3000 });
      setEditCollectionDialog(false);
      loadCollections();
    } catch (error) {
      toast.current.show({ severity: 'error', summary: 'Error', detail: error.message, life: 3000 });
    } finally {
      setEditCollectionLoading(false);
    }
  };

  const openEditFolderDialog = (folderId, collectionId) => {
    const col = collections.find((c) => c.id === collectionId);
    const folder = col?.folders?.find((f) => f.id === folderId);
    if (!folder) return;
    setEditFolderId(folder.id);
    setEditFolderName(folder.name);
    setEditFolderColor(folder.color ?? '');
    setEditFolderCollectionId(collectionId);
    setEditFolderParentId(folder.parentFolderId ?? null);
    setEditFolderDialog(true);
  };

  const saveEditFolder = async () => {
    if (!editFolderName.trim()) {
      toast.current.show({ severity: 'warn', summary: 'Warning', detail: 'Name is required', life: 3000 });
      return;
    }
    setEditFolderLoading(true);
    try {
      await folderService.updateFolder(editFolderId, {
        name: editFolderName.trim(),
        color: editFolderColor.trim() || null,
        parentFolderId: editFolderParentId
      });
      toast.current.show({ severity: 'success', summary: 'Success', detail: 'Folder updated', life: 3000 });
      setEditFolderDialog(false);
      loadCollections();
    } catch (error) {
      toast.current.show({ severity: 'error', summary: 'Error', detail: error.message, life: 3000 });
    } finally {
      setEditFolderLoading(false);
    }
  };

  const openDeleteFolderDialog = (folderId, collectionId) => {
    const col = collections.find((c) => c.id === collectionId);
    const folder = col?.folders?.find((f) => f.id === folderId);
    const mockCount = folder?.mockCount ?? 0;
    setDeleteFolderId(folderId);
    setDeleteFolderCollectionId(collectionId);
    setDeleteFolderMockCount(mockCount);
    setDeleteFolderMoveToCollectionId(null);
    setDeleteFolderMoveToFolderId(null);
    setDeleteFolderAlsoDeleteMocks(false);
    setDeleteFolderDialog(true);
  };

  const confirmDeleteFolder = async () => {
    if (!deleteFolderId) return;
    setDeleteFolderLoading(true);
    try {
      if (deleteFolderMockCount > 0 && !deleteFolderAlsoDeleteMocks) {
        const mockIds = mocks.filter((m) => m.folderId === deleteFolderId).map((m) => m.id);
        if (mockIds.length > 0) {
          await mockService.bulkUpdateMocks(mockIds, deleteFolderMoveToCollectionId ?? null, deleteFolderMoveToFolderId ?? null);
        }
      }
      await folderService.deleteFolder(deleteFolderId, deleteFolderAlsoDeleteMocks);
      toast.current.show({ severity: 'success', summary: 'Success', detail: 'Folder deleted', life: 3000 });
      setDeleteFolderDialog(false);
      loadCollections();
      loadMocks();
    } catch (error) {
      toast.current.show({ severity: 'error', summary: 'Error', detail: error.message, life: 3000 });
    } finally {
      setDeleteFolderLoading(false);
    }
  };

  const openDeleteCollectionDialog = (collectionId) => {
    const col = collections.find((c) => c.id === collectionId);
    const mockCount = col?.mockCount ?? 0;
    setDeleteCollectionId(collectionId);
    setDeleteCollectionMockCount(mockCount);
    setDeleteCollectionMoveToCollectionId(collections.filter((c) => c.id !== collectionId)[0]?.id ?? null);
    setDeleteCollectionMoveToFolderId(null);
    setDeleteCollectionAlsoDeleteMocks(false);
    setDeleteCollectionDialog(true);
  };

  const confirmDeleteCollection = async () => {
    if (!deleteCollectionId) return;
    setDeleteCollectionLoading(true);
    try {
      if (deleteCollectionMockCount > 0 && !deleteCollectionAlsoDeleteMocks) {
        const mockIds = mocks.filter((m) => m.collectionId === deleteCollectionId).map((m) => m.id);
        if (mockIds.length > 0) {
          await mockService.bulkUpdateMocks(mockIds, deleteCollectionMoveToCollectionId ?? null, deleteCollectionMoveToFolderId ?? null);
        }
      }
      await collectionService.deleteCollection(deleteCollectionId, deleteCollectionAlsoDeleteMocks);
      toast.current.show({ severity: 'success', summary: 'Success', detail: 'Collection deleted', life: 3000 });
      setDeleteCollectionDialog(false);
      loadCollections();
      loadMocks();
    } catch (error) {
      toast.current.show({ severity: 'error', summary: 'Error', detail: error.message, life: 3000 });
    } finally {
      setDeleteCollectionLoading(false);
    }
  };

  const openBulkMoveDialog = () => {
    setBulkMoveCollectionId(null);
    setBulkMoveFolderId(null);
    setBulkMoveDialog(true);
  };

  const saveBulkMove = async () => {
    if (!selectedMocks?.length) return;
    setBulkMoveLoading(true);
    try {
      await mockService.bulkUpdateMocks(
        selectedMocks.map((m) => m.id),
        bulkMoveCollectionId,
        bulkMoveFolderId
      );
      toast.current.show({ severity: 'success', summary: 'Success', detail: `${selectedMocks.length} route(s) updated`, life: 3000 });
      setBulkMoveDialog(false);
      setSelectedMocks(null);
      loadMocks();
      loadCollections();
    } catch (error) {
      toast.current.show({ severity: 'error', summary: 'Error', detail: error.message, life: 3000 });
    } finally {
      setBulkMoveLoading(false);
    }
  };

  const saveNewCollection = async () => {
    if (!newCollectionName.trim()) {
      toast.current.show({ severity: 'warn', summary: 'Warning', detail: 'Name is required', life: 3000 });
      return;
    }
    setNewCollectionLoading(true);
    try {
      await collectionService.createCollection({
        name: newCollectionName.trim(),
        description: newCollectionDescription.trim() || null,
        color: newCollectionColor.trim() || null
      });
      toast.current.show({ severity: 'success', summary: 'Success', detail: 'Collection created', life: 3000 });
      setNewCollectionDialog(false);
      setNewCollectionName('');
      setNewCollectionDescription('');
      setNewCollectionColor('');
      loadCollections();
    } catch (error) {
      toast.current.show({ severity: 'error', summary: 'Error', detail: error.message, life: 3000 });
    } finally {
      setNewCollectionLoading(false);
    }
  };

  const saveFolder = async () => {
    if (!newFolderName.trim() || !newFolderCollectionId) {
      toast.current.show({ severity: 'warn', summary: 'Warning', detail: 'Select a collection and enter folder name', life: 3000 });
      return;
    }
    setFolderDialogLoading(true);
    try {
      await folderService.createFolder(newFolderCollectionId, { name: newFolderName.trim(), color: newFolderColor.trim() || null });
      toast.current.show({ severity: 'success', summary: 'Success', detail: 'Folder created', life: 3000 });
      setFolderDialog(false);
      loadCollections();
    } catch (error) {
      toast.current.show({ severity: 'error', summary: 'Error', detail: error.message, life: 3000 });
    } finally {
      setFolderDialogLoading(false);
    }
  };

  function findNodeByKey(nodes, key) {
    if (!nodes || !key) return null;
    for (const node of nodes) {
      if (node.key === key) return node.data ? { ...node.data, key: node.key } : { key: node.key };
      if (node.children) {
        const found = findNodeByKey(node.children, key);
        if (found) return found;
      }
    }
    return null;
  }

  const treeNodes = useMemo(() => {
    const nodes = [
      { key: 'all', label: 'All', data: { type: 'all' }, icon: 'pi pi-fw pi-th-large' },
      { key: 'uncategorized', label: 'Uncategorized', data: { type: 'uncategorized' }, icon: 'pi pi-fw pi-folder-open' }
    ];
    (collections || []).forEach((c) => {
      const folderList = c.folders || [];
      const children = [
        { key: `collection-${c.id}-all`, label: 'All', data: { type: 'collectionAll', collectionId: c.id }, icon: 'pi pi-fw pi-list' },
        { key: `collection-${c.id}-uncategorized`, label: 'Uncategorized', data: { type: 'collectionUncategorized', collectionId: c.id }, icon: 'pi pi-fw pi-folder-open' }
      ];
      folderList.forEach((f) => {
        children.push({
          key: `folder-${f.id}`,
          label: f.mockCount != null ? `${f.name} (${f.mockCount})` : f.name,
          data: { type: 'folder', folderId: f.id, collectionId: c.id, color: f.color },
          icon: 'pi pi-fw pi-folder'
        });
      });
      nodes.push({
        key: `collection-${c.id}`,
        label: c.mockCount != null ? `${c.name} (${c.mockCount})` : c.name,
        data: { type: 'collection', collectionId: c.id, color: c.color },
        icon: 'pi pi-fw pi-folder',
        children
      });
    });
    return nodes;
  }, [collections]);

  const onTreeSelectionChange = (e) => {
    const key = e.value && (typeof e.value === 'string' ? e.value : Object.keys(e.value || {})[0]);
    if (!key) return;
    const data = findNodeByKey(treeNodes, key);
    setTreeSelectionKey(key);
    setTreeSelectionData(data || { type: 'all' });
  };

  const contextMenuNodeData = useMemo(
    () => (contextMenuNodeKey ? findNodeByKey(treeNodes, contextMenuNodeKey) : null),
    [treeNodes, contextMenuNodeKey]
  );

  const contextMenuModel = useMemo(() => {
    if (!contextMenuNodeData) return [];
    const { type, collectionId, folderId } = contextMenuNodeData;
    const items = [];
    if (type === 'collection' && collectionId) {
      items.push({ label: 'Edit collection', icon: 'pi pi-pencil', command: () => openEditCollectionDialog(collectionId) });
      items.push({ label: 'New folder', icon: 'pi pi-folder-plus', command: () => openFolderDialog(collectionId) });
      items.push({ label: 'Delete collection', icon: 'pi pi-trash', command: () => openDeleteCollectionDialog(collectionId) });
    }
    if ((type === 'collectionAll' || type === 'collectionUncategorized') && collectionId) {
      items.push({ label: 'New folder', icon: 'pi pi-folder-plus', command: () => openFolderDialog(collectionId) });
    }
    if (type === 'folder' && folderId && collectionId) {
      items.push({ label: 'Edit folder', icon: 'pi pi-pencil', command: () => openEditFolderDialog(folderId, collectionId) });
      items.push({ label: 'Delete folder', icon: 'pi pi-trash', command: () => openDeleteFolderDialog(folderId, collectionId) });
    }
    return items;
  }, [contextMenuNodeData]);

  const onTreeContextMenu = (e) => {
    setContextMenuNodeKey(e.node?.key ?? null);
    if (e.originalEvent && contextMenuRef.current) contextMenuRef.current.show(e.originalEvent);
  };

  const filteredMocks = useMemo(() => {
    if (treeSelectionData.type === 'uncategorized') return mocks.filter((m) => !m.collectionId);
    if (treeSelectionData.type === 'collectionUncategorized') return mocks.filter((m) => m.collectionId === treeSelectionData.collectionId && !m.folderId);
    return mocks;
  }, [mocks, treeSelectionData.type, treeSelectionData.collectionId]);

  const showCollectionColumn = treeSelectionData.type === 'all' || treeSelectionData.type === 'uncategorized';

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
      // Clean up sequence items ordering before save; ensure folderId is null when uncategorized
      const mockToSave = { ...mock };
      mockToSave.folderId = mock.folderId ?? null;
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
      {selectedMocks?.length > 0 && (
        <Button label={`Move to collection (${selectedMocks.length})`} icon="pi pi-folder-open" severity="secondary" outlined onClick={openBulkMoveDialog} />
      )}
      <Button label="Import cURL" icon="pi pi-download" severity="help" outlined onClick={() => setCurlDialog(true)} className="hidden-label-sm" />
      <Button label="Import OpenAPI" icon="pi pi-file-import" severity="info" outlined onClick={() => setOpenApiDialog(true)} className="hidden-label-sm" />
    </div>
  );

  const rightToolbarTemplate = () => (
    <div className="flex flex-wrap gap-2 align-items-center">
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

      <Dialog
        visible={folderDialog}
        header="New Folder"
        modal
        className="p-fluid"
        style={{ width: '22rem' }}
        onHide={() => setFolderDialog(false)}
        footer={
          <>
            <Button label="Cancel" icon="pi pi-times" outlined onClick={() => setFolderDialog(false)} />
            <Button label="Create" icon="pi pi-check" loading={folderDialogLoading} onClick={saveFolder} />
          </>
        }
      >
        <div className="field">
          <label htmlFor="newFolderCollection">Collection</label>
          <Dropdown
            id="newFolderCollection"
            value={newFolderCollectionId}
            options={collections.map(c => ({ label: c.name, value: c.id }))}
            onChange={(e) => setNewFolderCollectionId(e.value)}
            placeholder="Select collection"
            className="w-full"
          />
        </div>
        <div className="field">
          <label htmlFor="newFolderName">Folder name</label>
          <InputText
            id="newFolderName"
            value={newFolderName}
            onChange={(e) => setNewFolderName(e.target.value)}
            placeholder="e.g. Auth, Users"
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
              onClick={() => setNewFolderColor('')}
              title="No color"
            />
            {FOLDER_COLOR_PALETTE.map((hex) => (
              <button
                key={hex}
                type="button"
                className="border-circle w-2rem h-2rem border-2 flex-shrink-0"
                style={{
                  backgroundColor: hex,
                  borderColor: newFolderColor === hex ? 'var(--primary-color)' : 'var(--surface-border)'
                }}
                onClick={() => setNewFolderColor(hex)}
                title={hex}
              />
            ))}
          </div>
        </div>
      </Dialog>

      <Dialog
        visible={editCollectionDialog}
        header="Edit Collection"
        modal
        className="p-fluid"
        style={{ width: '22rem' }}
        onHide={() => setEditCollectionDialog(false)}
        footer={
          <>
            <Button label="Cancel" icon="pi pi-times" outlined onClick={() => setEditCollectionDialog(false)} />
            <Button label="Save" icon="pi pi-check" loading={editCollectionLoading} onClick={saveEditCollection} />
          </>
        }
      >
        <div className="field">
          <label htmlFor="editCollectionName">Name</label>
          <InputText
            id="editCollectionName"
            value={editCollectionName}
            onChange={(e) => setEditCollectionName(e.target.value)}
            className="w-full"
          />
        </div>
        <div className="field">
          <label htmlFor="editCollectionDescription">Description</label>
          <InputTextarea
            id="editCollectionDescription"
            value={editCollectionDescription}
            onChange={(e) => setEditCollectionDescription(e.target.value)}
            rows={2}
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
              onClick={() => setEditCollectionColor('')}
              title="No color"
            />
            {FOLDER_COLOR_PALETTE.map((hex) => (
              <button
                key={hex}
                type="button"
                className="border-circle w-2rem h-2rem border-2 flex-shrink-0"
                style={{
                  backgroundColor: hex,
                  borderColor: editCollectionColor === hex ? 'var(--primary-color)' : 'var(--surface-border)'
                }}
                onClick={() => setEditCollectionColor(hex)}
                title={hex}
              />
            ))}
          </div>
        </div>
      </Dialog>

      <Dialog
        visible={editFolderDialog}
        header="Edit Folder"
        modal
        className="p-fluid"
        style={{ width: '22rem' }}
        onHide={() => setEditFolderDialog(false)}
        footer={
          <>
            <Button label="Cancel" icon="pi pi-times" outlined onClick={() => setEditFolderDialog(false)} />
            <Button label="Save" icon="pi pi-check" loading={editFolderLoading} onClick={saveEditFolder} />
          </>
        }
      >
        <div className="field">
          <label htmlFor="editFolderName">Folder name</label>
          <InputText
            id="editFolderName"
            value={editFolderName}
            onChange={(e) => setEditFolderName(e.target.value)}
            className="w-full"
          />
        </div>
        <div className="field">
          <label htmlFor="editFolderParentId">Parent folder</label>
          <Dropdown
            id="editFolderParentId"
            value={editFolderParentId}
            options={[
              { label: 'None (root)', value: null },
              ...(collections.find((c) => c.id === editFolderCollectionId)?.folders || [])
                .filter((f) => f.id !== editFolderId)
                .map((f) => ({ label: f.name, value: f.id }))
            ]}
            onChange={(e) => setEditFolderParentId(e.value)}
            placeholder="None"
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
              onClick={() => setEditFolderColor('')}
              title="No color"
            />
            {FOLDER_COLOR_PALETTE.map((hex) => (
              <button
                key={hex}
                type="button"
                className="border-circle w-2rem h-2rem border-2 flex-shrink-0"
                style={{
                  backgroundColor: hex,
                  borderColor: editFolderColor === hex ? 'var(--primary-color)' : 'var(--surface-border)'
                }}
                onClick={() => setEditFolderColor(hex)}
                title={hex}
              />
            ))}
          </div>
        </div>
      </Dialog>

      <Dialog
        visible={newCollectionDialog}
        header="New Collection"
        modal
        className="p-fluid"
        style={{ width: '22rem' }}
        onHide={() => setNewCollectionDialog(false)}
        footer={
          <>
            <Button label="Cancel" icon="pi pi-times" outlined onClick={() => setNewCollectionDialog(false)} />
            <Button label="Create" icon="pi pi-check" loading={newCollectionLoading} onClick={saveNewCollection} />
          </>
        }
      >
        <div className="field">
          <label htmlFor="newCollectionName">Name</label>
          <InputText
            id="newCollectionName"
            value={newCollectionName}
            onChange={(e) => setNewCollectionName(e.target.value)}
            placeholder="Collection name"
            className="w-full"
          />
        </div>
        <div className="field">
          <label htmlFor="newCollectionDescription">Description</label>
          <InputTextarea
            id="newCollectionDescription"
            value={newCollectionDescription}
            onChange={(e) => setNewCollectionDescription(e.target.value)}
            rows={2}
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
              onClick={() => setNewCollectionColor('')}
              title="No color"
            />
            {FOLDER_COLOR_PALETTE.map((hex) => (
              <button
                key={hex}
                type="button"
                className="border-circle w-2rem h-2rem border-2 flex-shrink-0"
                style={{
                  backgroundColor: hex,
                  borderColor: newCollectionColor === hex ? 'var(--primary-color)' : 'var(--surface-border)'
                }}
                onClick={() => setNewCollectionColor(hex)}
                title={hex}
              />
            ))}
          </div>
        </div>
      </Dialog>

      <Dialog
        visible={deleteFolderDialog}
        header="Delete Folder"
        modal
        className="p-fluid"
        style={{ width: '28rem' }}
        onHide={() => setDeleteFolderDialog(false)}
        footer={
          <>
            <Button label="Cancel" icon="pi pi-times" outlined onClick={() => setDeleteFolderDialog(false)} />
            <Button label="Delete" icon="pi pi-trash" severity="danger" loading={deleteFolderLoading} onClick={confirmDeleteFolder} />
          </>
        }
      >
        {deleteFolderMockCount > 0 ? (
          <>
            <p className="mb-3">This folder has <strong>{deleteFolderMockCount}</strong> route(s).</p>
            <div className="field mb-3">
              <label>Move routes to</label>
              <div className="grid">
                <div className="col-12 md:col-6">
                  <Dropdown
                    value={deleteFolderMoveToCollectionId}
                    options={[
                      { label: 'Uncategorized', value: null },
                      ...collections.filter((c) => c.id !== deleteFolderCollectionId).map((c) => ({ label: c.name, value: c.id }))
                    ]}
                    onChange={(e) => { setDeleteFolderMoveToCollectionId(e.value); setDeleteFolderMoveToFolderId(null); }}
                    placeholder="Collection"
                    className="w-full"
                  />
                </div>
                <div className="col-12 md:col-6">
                  <Dropdown
                    value={deleteFolderMoveToFolderId}
                    options={[
                      { label: 'Uncategorized', value: null },
                      ...(collections.find((c) => c.id === deleteFolderMoveToCollectionId)?.folders || []).map((f) => ({ label: f.name, value: f.id }))
                    ]}
                    onChange={(e) => setDeleteFolderMoveToFolderId(e.value)}
                    placeholder="Folder"
                    className="w-full"
                    disabled={!deleteFolderMoveToCollectionId}
                  />
                </div>
              </div>
            </div>
            <div className="field-checkbox mb-0">
              <Checkbox
                inputId="deleteFolderAlsoDeleteMocks"
                checked={deleteFolderAlsoDeleteMocks}
                onChange={(e) => setDeleteFolderAlsoDeleteMocks(e.checked)}
              />
              <label htmlFor="deleteFolderAlsoDeleteMocks">Delete routes instead</label>
            </div>
          </>
        ) : (
          <p>Delete this folder? Child folders will become root folders.</p>
        )}
      </Dialog>

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

      <Dialog
        visible={bulkMoveDialog}
        header={`Move ${selectedMocks?.length ?? 0} route(s) to collection`}
        modal
        className="p-fluid"
        style={{ width: '22rem' }}
        onHide={() => setBulkMoveDialog(false)}
        footer={
          <>
            <Button label="Cancel" icon="pi pi-times" outlined onClick={() => setBulkMoveDialog(false)} />
            <Button label="Update" icon="pi pi-check" loading={bulkMoveLoading} onClick={saveBulkMove} />
          </>
        }
      >
        <div className="field">
          <label>Collection</label>
          <Dropdown
            value={bulkMoveCollectionId}
            options={collections.map((c) => ({ label: c.name, value: c.id }))}
            onChange={(e) => { setBulkMoveCollectionId(e.value); setBulkMoveFolderId(null); }}
            placeholder="Select collection (or leave uncategorized)"
            className="w-full"
            showClear
          />
        </div>
        <div className="field">
          <label>Folder</label>
          <Dropdown
            value={bulkMoveFolderId}
            options={[
              { label: 'Uncategorized', value: null },
              ...(collections.find((c) => c.id === bulkMoveCollectionId)?.folders || []).map((f) => ({ label: f.name, value: f.id }))
            ]}
            onChange={(e) => setBulkMoveFolderId(e.value)}
            placeholder="Uncategorized"
            className="w-full"
            disabled={!bulkMoveCollectionId}
            showClear
          />
        </div>
      </Dialog>

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

      {/* Main Content: split layout — collection tree (left) + route table (right) */}
      <div className="grid">
        <div className="col-12 md:col-4 lg:col-3">
          <div className="card h-full">
            <div className="flex align-items-center justify-content-between mb-3">
              <h5 className="mt-0 mb-0">Collections</h5>
              <Button icon="pi pi-plus" rounded text size="small" onClick={() => { setNewCollectionName(''); setNewCollectionDescription(''); setNewCollectionColor(''); setNewCollectionDialog(true); }} tooltip="New collection" tooltipOptions={{ position: 'bottom' }} />
            </div>
            <Tree
              value={treeNodes}
              selectionMode="single"
              selectionKeys={treeSelectionKey}
              onSelectionChange={onTreeSelectionChange}
              onContextMenu={onTreeContextMenu}
              contextMenuSelectionKey={contextMenuNodeKey}
              onContextMenuSelectionChange={(e) => setContextMenuNodeKey(e.value ?? null)}
              className="w-full border-none"
              filter
              filterPlaceholder="Search..."
              nodeTemplate={(node) => (
                <div className="flex align-items-center gap-2">
                  {node.data?.color ? (
                    <span
                      className="flex-shrink-0 border-circle border-1 border-400"
                      style={{ width: '0.75rem', height: '0.75rem', backgroundColor: node.data.color }}
                      title={node.data.color}
                    />
                  ) : null}
                  <span>{node.label}</span>
                </div>
              )}
            />
            <ContextMenu model={contextMenuModel} ref={contextMenuRef} />
          </div>
        </div>
        <div className="col-12 md:col-8 lg:col-9">
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
              {showCollectionColumn && (
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
              )}
              <Column field="isActive" header="Active" sortable body={statusBodyTemplate} style={{ width: '5.5rem' }} />
              <Column body={actionBodyTemplate} exportable={false} frozen alignFrozen="right" style={{ width: '11rem' }} />
            </DataTable>
          </div>
        </div>
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
                      _mock.folderId = null;
                      setMock(_mock);
                    }}
                    placeholder="No collection"
                    showClear
                  />
                </div>
              </div>

              <div className="col-12 md:col-6">
                <div className="field">
                  <label htmlFor="folderId">Folder</label>
                  <Dropdown
                    id="folderId"
                    value={mock?.folderId ?? null}
                    options={[
                      { label: 'Uncategorized', value: null },
                      ...(collections.find(c => c.id === mock?.collectionId)?.folders || []).map(f => ({ label: f.name, value: f.id }))
                    ]}
                    onChange={(e) => {
                      let _mock = { ...mock };
                      _mock.folderId = e.value !== undefined && e.value !== null ? e.value : null;
                      setMock(_mock);
                    }}
                    placeholder="Uncategorized (optional)"
                    showClear
                    disabled={!mock?.collectionId}
                  />
                  <small>Optional. Leave as Uncategorized for no folder.</small>
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
