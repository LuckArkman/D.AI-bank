import api from './api';

export interface BusinessRule {
    id?: string;
    name: string;
    description: string;
    conditionExpression: string;
    errorMessage: string;
    severity: number;
    tenantId?: string;
}

export interface TenantOnboardingRequest {
    name: string;
    identifier: string;
    adminEmail: string;
    adminPassword: string;
    adminName: string;
    themePrimaryColor?: string;
    themeSecondaryColor?: string;
    themeLogoUrl?: string;
    supportEmail?: string;
    supportPhone?: string;
    businessMode?: string; // "B2C" | "B2B"
    jurisdiction?: string; // "Brazil" | "USA" | "Europe" | "UnitedKingdom"
}

export interface TenantOnboardingResponse {
    tenantId: string;
    apiKey: string;
    adminUserId: string;
    adminUserEmail: string;
}

export const adminApi = {
    // Rules
    getRules: async () => {
        const response = await api.get<BusinessRule[]>('/tenet/admin/rules');
        return response.data;
    },
    createRule: async (rule: BusinessRule) => {
        const response = await api.post<BusinessRule>('/tenet/admin/rules', rule);
        return response.data;
    },
    updateRule: async (id: string, rule: BusinessRule) => {
        await api.put(`/tenet/admin/rules/${id}`, rule);
    },
    deleteRule: async (id: string) => {
        await api.delete(`/tenet/admin/rules/${id}`);
    },

    // Onboarding
    onboardTenant: async (request: TenantOnboardingRequest) => {
        const response = await api.post<TenantOnboardingResponse>('/tenet/admin/onboard', request);
        return response.data;
    }
};
