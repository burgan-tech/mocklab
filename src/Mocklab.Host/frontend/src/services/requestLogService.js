import apiConfig from '../config/apiConfig';

class RequestLogService {
  async getLogs({ method, statusCode, isMatched, from, to, route, search, page, pageSize } = {}) {
    const params = new URLSearchParams();

    if (method) params.append('method', method);
    if (statusCode !== undefined && statusCode !== null) params.append('statusCode', statusCode);
    if (isMatched !== undefined && isMatched !== null) params.append('isMatched', isMatched);
    if (from) params.append('from', from);
    if (to) params.append('to', to);
    if (route) params.append('route', route);
    if (search) params.append('search', search);
    if (page) params.append('page', page);
    if (pageSize) params.append('pageSize', pageSize);

    const queryString = params.toString();
    const url = queryString
      ? `${apiConfig.adminLogsPath}?${queryString}`
      : apiConfig.adminLogsPath;

    const response = await fetch(url);

    if (!response.ok) {
      let detail = '';
      try { detail = await response.text(); } catch {}
      throw new Error(`Server error ${response.status}: ${detail || response.statusText}`);
    }

    return await response.json();
  }

  async getLog(id) {
    const response = await fetch(`${apiConfig.adminLogsPath}/${id}`);

    if (!response.ok) {
      throw new Error('Failed to fetch request log');
    }

    return await response.json();
  }

  async getRecentCount(minutes = 5) {
    const response = await fetch(`${apiConfig.adminLogsPath}/count?minutes=${minutes}`);

    if (!response.ok) {
      throw new Error('Failed to fetch log count');
    }

    return await response.json();
  }

  async clearLogs() {
    const response = await fetch(`${apiConfig.adminLogsPath}/clear`, {
      method: 'DELETE',
    });

    if (!response.ok) {
      throw new Error('Failed to clear logs');
    }

    return await response.json();
  }
}

export const requestLogService = new RequestLogService();
