import axios from 'axios';

const api = axios.create({
    baseURL: 'http://localhost:5222/api/v1',
});

// Interceptor para adicionar Token e TenantId
api.interceptors.request.use((config) => {
    // Tenta pegar o token do localStorage (ou do store se preferir)
    const authStorage = localStorage.getItem('fintech-auth');
    if (authStorage) {
        const { state } = JSON.parse(authStorage);
        if (state.token) {
            config.headers.Authorization = `Bearer ${state.token}`;
        }
        if (state.tenantId) {
            config.headers['X-Tenant-Id'] = state.tenantId;
        }
    }
    return config;
});

export default api;
