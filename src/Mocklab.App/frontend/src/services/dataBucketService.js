import apiConfig, { apiUrl, parseErrorResponse } from '../config/apiConfig';

class DataBucketService {
  getAll(collectionId) {
    const path = apiConfig.dataBucketsPath(collectionId);
    return fetch(apiUrl(path), {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    }).then(async (response) => {
      if (!response.ok) {
        const msg = await parseErrorResponse(response);
        throw new Error(msg);
      }
      return response.json();
    });
  }

  getOne(collectionId, bucketId) {
    const path = `${apiConfig.dataBucketsPath(collectionId)}/${bucketId}`;
    return fetch(apiUrl(path), {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    }).then(async (response) => {
      if (!response.ok) {
        const msg = await parseErrorResponse(response);
        throw new Error(msg);
      }
      return response.json();
    });
  }

  create(collectionId, data) {
    const path = apiConfig.dataBucketsPath(collectionId);
    return fetch(apiUrl(path), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name: data.name,
        description: data.description ?? null,
        data: data.data ?? '[]',
      }),
    }).then(async (response) => {
      if (!response.ok) {
        const msg = await parseErrorResponse(response);
        throw new Error(msg);
      }
      return response.json();
    });
  }

  update(collectionId, bucketId, data) {
    const path = `${apiConfig.dataBucketsPath(collectionId)}/${bucketId}`;
    return fetch(apiUrl(path), {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name: data.name,
        description: data.description ?? null,
        data: data.data ?? '[]',
      }),
    }).then(async (response) => {
      if (!response.ok) {
        const msg = await parseErrorResponse(response);
        throw new Error(msg);
      }
      return response.json();
    });
  }

  remove(collectionId, bucketId) {
    const path = `${apiConfig.dataBucketsPath(collectionId)}/${bucketId}`;
    return fetch(apiUrl(path), {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' },
    }).then(async (response) => {
      if (!response.ok) {
        const msg = await parseErrorResponse(response);
        throw new Error(msg);
      }
      if (response.status === 204) return null;
      const ct = response.headers.get('content-type');
      if (ct && ct.includes('application/json')) return response.json();
      return null;
    });
  }

  exportBucket(collectionId, bucketId) {
    const path = `${apiConfig.dataBucketsPath(collectionId)}/${bucketId}/export`;
    return fetch(apiUrl(path), {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    }).then(async (response) => {
      if (!response.ok) {
        const msg = await parseErrorResponse(response);
        throw new Error(msg);
      }
      return response.json();
    });
  }

  exportAll(collectionId) {
    const path = `${apiConfig.dataBucketsPath(collectionId)}/export`;
    return fetch(apiUrl(path), {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    }).then(async (response) => {
      if (!response.ok) {
        const msg = await parseErrorResponse(response);
        throw new Error(msg);
      }
      return response.json();
    });
  }
}

export const dataBucketService = new DataBucketService();
