import { useState, useEffect, useRef, useCallback } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { Dropdown } from 'primereact/dropdown';
import { InputText } from 'primereact/inputtext';
import { Calendar } from 'primereact/calendar';
import { Tag } from 'primereact/tag';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { IconField } from 'primereact/iconfield';
import { InputIcon } from 'primereact/inputicon';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { requestLogService } from '../services/requestLogService';

export default function RequestLogsPage() {
  const [logs, setLogs] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [detailDialog, setDetailDialog] = useState(false);
  const [selectedLog, setSelectedLog] = useState(null);

  // Filters
  const [methodFilter, setMethodFilter] = useState(null);
  const [matchedFilter, setMatchedFilter] = useState(null);
  const [statusCodeFilter, setStatusCodeFilter] = useState(null);
  const [dateRange, setDateRange] = useState(null);
  const [routeFilter, setRouteFilter] = useState('');
  const [searchFilter, setSearchFilter] = useState('');
  const [debouncedRoute, setDebouncedRoute] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');

  // Pagination
  const [lazyParams, setLazyParams] = useState({
    first: 0,
    rows: 20,
    page: 1,
  });

  const toast = useRef(null);
  const routeDebounceRef = useRef(null);
  const searchDebounceRef = useRef(null);

  const httpMethodOptions = [
    { label: 'All Methods', value: 'ALL' },
    { label: 'GET', value: 'GET' },
    { label: 'POST', value: 'POST' },
    { label: 'PUT', value: 'PUT' },
    { label: 'DELETE', value: 'DELETE' },
    { label: 'PATCH', value: 'PATCH' },
    { label: 'HEAD', value: 'HEAD' },
    { label: 'OPTIONS', value: 'OPTIONS' }
  ];

  const matchedOptions = [
    { label: 'All', value: 'ALL' },
    { label: 'Matched', value: 'matched' },
    { label: 'Unmatched', value: 'unmatched' }
  ];

  const statusCodeOptions = [
    { label: 'All Status', value: 'ALL' },
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

  const onRouteFilterChange = (value) => {
    setRouteFilter(value);
    if (routeDebounceRef.current) clearTimeout(routeDebounceRef.current);
    routeDebounceRef.current = setTimeout(() => {
      setDebouncedRoute(value);
      setLazyParams(prev => ({ ...prev, first: 0, page: 1 }));
    }, 400);
  };

  const onSearchFilterChange = (value) => {
    setSearchFilter(value);
    if (searchDebounceRef.current) clearTimeout(searchDebounceRef.current);
    searchDebounceRef.current = setTimeout(() => {
      setDebouncedSearch(value);
      setLazyParams(prev => ({ ...prev, first: 0, page: 1 }));
    }, 400);
  };

  const loadLogs = useCallback(async () => {
    setLoading(true);
    try {
      const params = {
        page: lazyParams.page,
        pageSize: lazyParams.rows,
      };

      if (methodFilter && methodFilter !== 'ALL') params.method = methodFilter;
      if (matchedFilter && matchedFilter !== 'ALL') params.isMatched = matchedFilter === 'matched';
      if (statusCodeFilter && statusCodeFilter !== 'ALL') params.statusCode = statusCodeFilter;
      if (debouncedRoute) params.route = debouncedRoute;
      if (debouncedSearch) params.search = debouncedSearch;
      if (dateRange && dateRange[0]) params.from = dateRange[0].toISOString();
      if (dateRange && dateRange[1]) {
        const endOfDay = new Date(dateRange[1]);
        endOfDay.setHours(23, 59, 59, 999);
        params.to = endOfDay.toISOString();
      }

      const result = await requestLogService.getLogs(params);
      setLogs(result.data);
      setTotalCount(result.totalCount);
    } catch (error) {
      toast.current.show({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to load logs: ' + error.message,
        life: 5000
      });
    } finally {
      setLoading(false);
    }
  }, [methodFilter, matchedFilter, statusCodeFilter, dateRange, debouncedRoute, debouncedSearch, lazyParams]);

  useEffect(() => {
    loadLogs();
  }, [loadLogs]);

  useEffect(() => {
    return () => {
      if (routeDebounceRef.current) clearTimeout(routeDebounceRef.current);
      if (searchDebounceRef.current) clearTimeout(searchDebounceRef.current);
    };
  }, []);

  const onPage = (event) => {
    setLazyParams(prev => ({
      ...prev,
      first: event.first,
      rows: event.rows,
      page: Math.floor(event.first / event.rows) + 1,
    }));
  };

  const viewDetail = (log) => {
    setSelectedLog(log);
    setDetailDialog(true);
  };

  const clearAllLogs = () => {
    confirmDialog({
      message: 'Are you sure you want to clear ALL request logs? This action cannot be undone!',
      header: 'Clear Logs Confirmation',
      icon: 'pi pi-exclamation-triangle',
      acceptClassName: 'p-button-danger',
      accept: async () => {
        try {
          const result = await requestLogService.clearLogs();
          toast.current.show({
            severity: 'success',
            summary: 'Success',
            detail: result.message,
            life: 3000
          });
          loadLogs();
        } catch (error) {
          toast.current.show({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to clear logs: ' + error.message,
            life: 3000
          });
        }
      }
    });
  };

  // Column body templates
  const timestampBodyTemplate = (rowData) => {
    const date = new Date(rowData.timestamp);
    return (
      <span className="text-sm">
        {date.toLocaleDateString('tr-TR')} {date.toLocaleTimeString('tr-TR')}
      </span>
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
    return <Tag value={rowData.responseStatusCode} severity={getSeverity(rowData.responseStatusCode)} />;
  };

  const matchedBodyTemplate = (rowData) => {
    return rowData.isMatched
      ? <Tag value="Matched" severity="success" icon="pi pi-check" />
      : <Tag value="Unmatched" severity="danger" icon="pi pi-times" />;
  };

  const responseTimeBodyTemplate = (rowData) => {
    if (rowData.responseTimeMs === null || rowData.responseTimeMs === undefined) {
      return <span className="text-color-secondary">-</span>;
    }
    const severity = rowData.responseTimeMs > 1000 ? 'danger' : rowData.responseTimeMs > 500 ? 'warning' : 'success';
    return <Tag value={`${rowData.responseTimeMs}ms`} severity={severity} />;
  };

  const actionBodyTemplate = (rowData) => {
    return (
      <Button
        icon="pi pi-eye"
        rounded
        outlined
        className="p-button-sm"
        onClick={() => viewDetail(rowData)}
        tooltip="View Details"
        tooltipOptions={{ position: 'top' }}
      />
    );
  };

  const resetPage = () => {
    setLazyParams(prev => ({ ...prev, first: 0, page: 1 }));
  };

  const leftToolbarTemplate = () => (
    <div className="flex flex-wrap gap-2 align-items-center">
      <Button
        icon="pi pi-refresh"
        outlined
        onClick={loadLogs}
        tooltip="Refresh"
        tooltipOptions={{ position: 'top' }}
      />
      <Button
        icon="pi pi-trash"
        severity="danger"
        outlined
        onClick={clearAllLogs}
        tooltip="Clear All"
        tooltipOptions={{ position: 'top' }}
      />
    </div>
  );

  const rightToolbarTemplate = () => (
    <div className="flex flex-wrap gap-2 align-items-center">
      <IconField iconPosition="left">
        <InputIcon className="pi pi-search" />
        <InputText
          value={routeFilter}
          onChange={(e) => onRouteFilterChange(e.target.value)}
          placeholder="Search route..."
          style={{ width: '13rem' }}
        />
      </IconField>
      <Dropdown
        value={methodFilter}
        options={httpMethodOptions}
        onChange={(e) => { setMethodFilter(e.value); resetPage(); }}
        placeholder="Method"
        style={{ width: '10rem' }}
      />
      <Dropdown
        value={statusCodeFilter}
        options={statusCodeOptions}
        onChange={(e) => { setStatusCodeFilter(e.value); resetPage(); }}
        placeholder="Status Code"
        style={{ width: '11rem' }}
      />
      <Dropdown
        value={matchedFilter}
        options={matchedOptions}
        onChange={(e) => { setMatchedFilter(e.value); resetPage(); }}
        placeholder="Match Status"
        style={{ width: '10rem' }}
      />
      <Calendar
        value={dateRange}
        onChange={(e) => { setDateRange(e.value); resetPage(); }}
        selectionMode="range"
        placeholder="Date Range"
        showIcon
        showButtonBar
        dateFormat="dd/mm/yy"
        style={{ width: '14rem' }}
      />
    </div>
  );

  const header = (
    <div className="flex flex-wrap gap-2 align-items-center justify-content-between">
      <h4 className="m-0">Request Logs <span className="text-color-secondary text-sm font-normal ml-2">({totalCount})</span></h4>
      <IconField iconPosition="left">
        <InputIcon className="pi pi-search" />
        <InputText
          value={searchFilter}
          onChange={(e) => onSearchFilterChange(e.target.value)}
          placeholder="Search route, mock, body..."
          style={{ width: '18rem' }}
        />
      </IconField>
    </div>
  );

  const formatHeaders = (headersJson) => {
    try {
      const headers = JSON.parse(headersJson);
      return JSON.stringify(headers, null, 2);
    } catch {
      return headersJson || '-';
    }
  };

  const detailDialogFooter = (
    <Button
      label="Close"
      icon="pi pi-times"
      outlined
      severity="secondary"
      onClick={() => setDetailDialog(false)}
    />
  );

  return (
    <div>
      <Toast ref={toast} />
      <ConfirmDialog />

      {/* Page Header */}
      <div className="page-header">
        <div className="page-header-icon">
          <i className="pi pi-list"></i>
        </div>
        <div className="page-header-text">
          <h1>Request Logs</h1>
          <p>Monitor incoming requests and their matched responses</p>
        </div>
      </div>

      {/* Main Content */}
      <div className="card">
        <Toolbar
          className="mb-4"
          start={leftToolbarTemplate}
          end={rightToolbarTemplate}
        />

        <DataTable
          value={logs}
          dataKey="id"
          lazy
          paginator
          first={lazyParams.first}
          rows={lazyParams.rows}
          totalRecords={totalCount}
          onPage={onPage}
          rowsPerPageOptions={[10, 20, 50, 100]}
          paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
          currentPageReportTemplate="Showing {first} to {last} of {totalRecords} logs"
          header={header}
          loading={loading}
          stripedRows
          rowHover
          emptyMessage="No request logs found."
          size="small"
          scrollable
          scrollHeight="flex"
          tableStyle={{ minWidth: '60rem' }}
        >
          <Column
            field="timestamp"
            header="Timestamp"
            body={timestampBodyTemplate}
            style={{ minWidth: '11rem' }}
          />
          <Column
            field="httpMethod"
            header="Method"
            body={httpMethodBodyTemplate}
            style={{ width: '7rem' }}
          />
          <Column
            field="route"
            header="Route"
            style={{ minWidth: '14rem' }}
          />
          <Column
            field="responseStatusCode"
            header="Status"
            body={statusCodeBodyTemplate}
            style={{ width: '7rem' }}
          />
          <Column
            field="isMatched"
            header="Match"
            body={matchedBodyTemplate}
            style={{ width: '9rem' }}
          />
          <Column
            field="responseTimeMs"
            header="Time"
            body={responseTimeBodyTemplate}
            style={{ width: '7rem' }}
          />
          <Column
            field="matchedMockDescription"
            header="Mock"
            body={(rowData) => rowData.matchedMockDescription || <span className="text-color-secondary">-</span>}
            style={{ minWidth: '10rem' }}
          />
          <Column
            body={actionBodyTemplate}
            exportable={false}
            style={{ width: '5rem' }}
          />
        </DataTable>
      </div>

      {/* Detail Dialog */}
      <Dialog
        visible={detailDialog}
        style={{ width: 'min(800px, 95vw)' }}
        header="Request Log Detail"
        modal
        footer={detailDialogFooter}
        onHide={() => setDetailDialog(false)}
        breakpoints={{ '960px': '90vw', '641px': '95vw' }}
      >
        {selectedLog && (
          <div className="grid">
            <div className="col-12 md:col-6">
              <div className="field">
                <label className="font-semibold text-sm text-color-secondary">HTTP Method</label>
                <div className="mt-1">{httpMethodBodyTemplate(selectedLog)}</div>
              </div>
            </div>
            <div className="col-12 md:col-6">
              <div className="field">
                <label className="font-semibold text-sm text-color-secondary">Status Code</label>
                <div className="mt-1">{statusCodeBodyTemplate(selectedLog)}</div>
              </div>
            </div>
            <div className="col-12">
              <div className="field">
                <label className="font-semibold text-sm text-color-secondary">Route</label>
                <div className="mt-1" style={{ fontFamily: 'monospace', fontSize: '0.9rem' }}>
                  {selectedLog.route}
                </div>
              </div>
            </div>
            {selectedLog.queryString && (
              <div className="col-12">
                <div className="field">
                  <label className="font-semibold text-sm text-color-secondary">Query String</label>
                  <div className="mt-1" style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}>
                    {selectedLog.queryString}
                  </div>
                </div>
              </div>
            )}
            <div className="col-12 md:col-6">
              <div className="field">
                <label className="font-semibold text-sm text-color-secondary">Matched</label>
                <div className="mt-1">{matchedBodyTemplate(selectedLog)}</div>
              </div>
            </div>
            <div className="col-12 md:col-6">
              <div className="field">
                <label className="font-semibold text-sm text-color-secondary">Response Time</label>
                <div className="mt-1">{responseTimeBodyTemplate(selectedLog)}</div>
              </div>
            </div>
            {selectedLog.matchedMockId && (
              <div className="col-12">
                <div className="field">
                  <label className="font-semibold text-sm text-color-secondary">Matched Mock</label>
                  <div className="mt-1">
                    <Tag value={`ID: ${selectedLog.matchedMockId}`} severity="info" />
                    {selectedLog.matchedMockDescription && (
                      <span className="ml-2 text-sm">{selectedLog.matchedMockDescription}</span>
                    )}
                  </div>
                </div>
              </div>
            )}
            <div className="col-12">
              <div className="field">
                <label className="font-semibold text-sm text-color-secondary">Timestamp</label>
                <div className="mt-1 text-sm">
                  {new Date(selectedLog.timestamp).toLocaleString('tr-TR')}
                </div>
              </div>
            </div>
            {selectedLog.requestBody && (
              <div className="col-12">
                <div className="field">
                  <label className="font-semibold text-sm text-color-secondary">Request Body</label>
                  <pre className="mt-1 p-3 border-round surface-ground text-sm" style={{ fontFamily: 'monospace', whiteSpace: 'pre-wrap', wordBreak: 'break-all', maxHeight: '200px', overflow: 'auto' }}>
                    {selectedLog.requestBody}
                  </pre>
                </div>
              </div>
            )}
            {selectedLog.requestHeaders && (
              <div className="col-12">
                <div className="field">
                  <label className="font-semibold text-sm text-color-secondary">Request Headers</label>
                  <pre className="mt-1 p-3 border-round surface-ground text-sm" style={{ fontFamily: 'monospace', whiteSpace: 'pre-wrap', wordBreak: 'break-all', maxHeight: '300px', overflow: 'auto' }}>
                    {formatHeaders(selectedLog.requestHeaders)}
                  </pre>
                </div>
              </div>
            )}
          </div>
        )}
      </Dialog>
    </div>
  );
}
