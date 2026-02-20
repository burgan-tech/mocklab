import apiConfig, { apiUrl, parseErrorResponse } from '../config/apiConfig';

class CollectionService {
  async getAllCollections(includeFolders = false) {
    const path = includeFolders ? `${apiConfig.adminCollectionsPath}?includeFolders=true` : apiConfig.adminCollectionsPath;
    const response = await fetch(apiUrl(path), {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }

  async getCollection(id) {
    const response = await fetch(apiUrl(`${apiConfig.adminCollectionsPath}/${id}`), {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }

  async createCollection(data) {
    const response = await fetch(apiUrl(apiConfig.adminCollectionsPath), {
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

  async updateCollection(id, data) {
    const response = await fetch(apiUrl(`${apiConfig.adminCollectionsPath}/${id}`), {
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

  async deleteCollection(id, deleteMocks = false) {
    const path = deleteMocks ? `${apiConfig.adminCollectionsPath}/${id}?deleteMocks=true` : `${apiConfig.adminCollectionsPath}/${id}`;
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

  async exportCollection(id) {
    const response = await fetch(apiUrl(`${apiConfig.adminCollectionsPath}/${id}/export`), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }

  async importCollection(importData) {
    const response = await fetch(apiUrl(`${apiConfig.adminCollectionsPath}/import`), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(importData),
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }
}

export const collectionService = new CollectionService();
