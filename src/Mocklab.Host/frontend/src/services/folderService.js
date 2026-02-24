import apiConfig, { apiUrl, parseErrorResponse } from '../config/apiConfig';

class FolderService {
  async getFoldersByCollection(collectionId) {
    const response = await fetch(apiUrl(`${apiConfig.adminCollectionsPath}/${collectionId}/folders`), {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }

  async createFolder(collectionId, data) {
    const response = await fetch(apiUrl(`${apiConfig.adminCollectionsPath}/${collectionId}/folders`), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }

  async updateFolder(id, data) {
    const response = await fetch(apiUrl(`${apiConfig.adminFoldersPath}/${id}`), {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }

  async deleteFolder(id, deleteMocks = false) {
    const path = deleteMocks ? `${apiConfig.adminFoldersPath}/${id}?deleteMocks=true` : `${apiConfig.adminFoldersPath}/${id}`;
    const response = await fetch(apiUrl(path), {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' },
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    if (response.status === 204) return null;
    const ct = response.headers.get('content-type');
    if (ct && ct.includes('application/json')) return await response.json();
    return null;
  }
}

export const folderService = new FolderService();
