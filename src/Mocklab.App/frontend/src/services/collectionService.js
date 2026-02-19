import apiConfig from '../config/apiConfig';

class CollectionService {
  async getAllCollections() {
    const response = await fetch(apiConfig.adminCollectionsPath, {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    });

    if (!response.ok) {
      throw new Error('Failed to fetch collections');
    }

    return await response.json();
  }

  async getCollection(id) {
    const response = await fetch(`${apiConfig.adminCollectionsPath}/${id}`, {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    });

    if (!response.ok) {
      throw new Error('Failed to fetch collection');
    }

    return await response.json();
  }

  async createCollection(data) {
    const response = await fetch(apiConfig.adminCollectionsPath, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      throw new Error('Failed to create collection');
    }

    return await response.json();
  }

  async updateCollection(id, data) {
    const response = await fetch(`${apiConfig.adminCollectionsPath}/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      throw new Error('Failed to update collection');
    }

    return await response.json();
  }

  async deleteCollection(id) {
    const response = await fetch(`${apiConfig.adminCollectionsPath}/${id}`, {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' },
    });

    if (!response.ok) {
      throw new Error('Failed to delete collection');
    }

    return response.status === 204 ? null : await response.json();
  }

  async exportCollection(id) {
    const response = await fetch(`${apiConfig.adminCollectionsPath}/${id}/export`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
    });

    if (!response.ok) {
      throw new Error('Failed to export collection');
    }

    return await response.json();
  }

  async importCollection(importData) {
    const response = await fetch(`${apiConfig.adminCollectionsPath}/import`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(importData),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({}));
      throw new Error(error.error || 'Failed to import collection');
    }

    return await response.json();
  }
}

export const collectionService = new CollectionService();
