import apiConfig from '../config/apiConfig';

class MockService {
  async getAllMocks(isActive = null) {
    const url = isActive !== null 
      ? `${apiConfig.adminPath}?isActive=${isActive}`
      : apiConfig.adminPath;
    
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error('Failed to fetch mocks');
    }

    return await response.json();
  }

  async getMock(id) {
    const response = await fetch(`${apiConfig.adminPath}/${id}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error('Failed to fetch mock');
    }

    return await response.json();
  }

  async createMock(mockData) {
    const response = await fetch(apiConfig.adminPath, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(mockData),
    });

    if (!response.ok) {
      throw new Error('Failed to create mock');
    }

    return await response.json();
  }

  async updateMock(id, mockData) {
    const response = await fetch(`${apiConfig.adminPath}/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(mockData),
    });

    if (!response.ok) {
      throw new Error('Failed to update mock');
    }

    return await response.json();
  }

  async deleteMock(id) {
    const response = await fetch(`${apiConfig.adminPath}/${id}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error('Failed to delete mock');
    }

    return response.status === 204 ? null : await response.json();
  }

  async toggleMock(id) {
    const response = await fetch(`${apiConfig.adminPath}/${id}/toggle`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error('Failed to toggle mock');
    }

    return await response.json();
  }

  async clearAllMocks() {
    const response = await fetch(`${apiConfig.adminPath}/clear`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error('Failed to clear mocks');
    }

    return await response.json();
  }

  async importCurl(curlCommand) {
    const response = await fetch(`${apiConfig.adminPath}/import/curl`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ curl: curlCommand }),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({}));
      throw new Error(error.error || 'Failed to import from cURL');
    }

    return await response.json();
  }

  async importOpenApi(openApiJson) {
    const response = await fetch(`${apiConfig.adminPath}/import/openapi`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ openApiJson }),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({}));
      throw new Error(error.error || 'Failed to import from OpenAPI');
    }

    return await response.json();
  }

  async resetSequence(id) {
    const response = await fetch(`${apiConfig.adminPath}/${id}/sequence/reset`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error('Failed to reset sequence');
    }

    return await response.json();
  }

  async resetAllSequences() {
    const response = await fetch(`${apiConfig.adminPath}/sequence/reset-all`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error('Failed to reset all sequences');
    }

    return await response.json();
  }
}

export const mockService = new MockService();
