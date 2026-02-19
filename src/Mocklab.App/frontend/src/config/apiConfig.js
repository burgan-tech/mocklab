const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '';

export const apiConfig = {
  baseURL: API_BASE_URL,
  adminPath: '/_admin/mocks',
  adminLogsPath: '/_admin/logs',
  adminCollectionsPath: '/_admin/collections',
  timeout: 30000,
};

export default apiConfig;
