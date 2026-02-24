import apiConfig, { apiUrl, parseErrorResponse } from '../config/apiConfig';

class MockService {
  async getAllMocks(isActive = null, collectionId = null, folderId = null) {
    const params = new URLSearchParams();
    if (isActive !== null) params.set('isActive', isActive);
    if (collectionId != null) params.set('collectionId', collectionId);
    if (folderId != null) params.set('folderId', folderId);
    const qs = params.toString();
    const path = qs ? `${apiConfig.adminPath}?${qs}` : apiConfig.adminPath;

    const response = await fetch(apiUrl(path));

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }

  async getMock(id) {
    const response = await fetch(apiUrl(`${apiConfig.adminPath}/${id}`));

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }

  async createMock(mockData) {
    const response = await fetch(apiUrl(apiConfig.adminPath), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(mockData),
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }

  async updateMock(id, mockData) {
    const response = await fetch(apiUrl(`${apiConfig.adminPath}/${id}`), {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(mockData),
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }

  async bulkUpdateMocks(mockIds, collectionId, folderId) {
    const body = {
      mockIds,
      collectionId: collectionId === undefined || collectionId === null ? null : collectionId,
      folderId: folderId === undefined || folderId === null ? null : folderId,
    };
    const response = await fetch(apiUrl(`${apiConfig.adminPath}/bulk-update`), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    if (response.status === 204) return { updatedCount: mockIds.length };
    const ct = response.headers.get('content-type');
    if (ct && ct.includes('application/json')) return await response.json();
    return { updatedCount: mockIds.length };
  }

  async duplicateMock(id, targetCollectionId = null, targetFolderId = null) {
    const body = {};
    if (targetCollectionId !== null && targetCollectionId !== undefined) body.collectionId = targetCollectionId;
    if (targetFolderId !== null && targetFolderId !== undefined) body.folderId = targetFolderId;

    const response = await fetch(apiUrl(`${apiConfig.adminPath}/${id}/duplicate`), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }

  async deleteMock(id) {
    const response = await fetch(apiUrl(`${apiConfig.adminPath}/${id}`), {
      method: 'DELETE',
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

  async toggleMock(id) {
    const response = await fetch(apiUrl(`${apiConfig.adminPath}/${id}/toggle`), {
      method: 'PATCH',
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }

  async clearAllMocks() {
    const response = await fetch(apiUrl(`${apiConfig.adminPath}/clear`), {
      method: 'DELETE',
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }

  async importCurl(curlCommand) {
    const response = await fetch(apiUrl(`${apiConfig.adminPath}/import/curl`), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ curl: curlCommand }),
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }

  async importOpenApi(openApiJson) {
    const response = await fetch(apiUrl(`${apiConfig.adminPath}/import/openapi`), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ openApiJson }),
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }

  async resetSequence(id) {
    const response = await fetch(apiUrl(`${apiConfig.adminPath}/${id}/sequence/reset`), {
      method: 'POST',
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }

  async resetAllSequences() {
    const response = await fetch(apiUrl(`${apiConfig.adminPath}/sequence/reset-all`), {
      method: 'POST',
    });

    if (!response.ok) {
      const msg = await parseErrorResponse(response);
      throw new Error(msg);
    }

    return await response.json();
  }
}

export const mockService = new MockService();
