const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '';

export const apiConfig = {
  baseURL: API_BASE_URL,
  adminPath: '/_admin/mocks',
  adminLogsPath: '/_admin/logs',
  adminCollectionsPath: '/_admin/collections',
  adminFoldersPath: '/_admin/folders',
  timeout: 30000,
};

/** Build full URL for API calls (handles baseURL) */
export function apiUrl(path) {
  const base = (apiConfig.baseURL || '').replace(/\/$/, '');
  const p = (path || '').replace(/^\//, '');
  return base ? `${base}/${p}` : `/${p}`;
}

/** Parse error from response; never throws on parse (avoids HTML body breaking JSON parse) */
export async function parseErrorResponse(response) {
  const text = await response.text();
  try {
    const j = JSON.parse(text);
    return j.error || j.message || text || `Request failed (${response.status})`;
  } catch {
    return text && text.trim().length > 0 ? text : `Request failed (${response.status})`;
  }
}

export default apiConfig;
