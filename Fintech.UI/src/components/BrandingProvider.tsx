import { useEffect } from 'react';
import { useAuthStore } from '../store/useAuthStore';
import api from '../api/api';

export const BrandingProvider = ({ children }: { children: React.ReactNode }) => {
    const { tenantId } = useAuthStore();

    useEffect(() => {
        const loadBranding = async () => {
            if (!tenantId) return;

            try {
                const response = await api.get(`/tenants/${tenantId}`);
                const tenant = response.data;

                if (tenant.branding) {
                    const { primaryColor } = tenant.branding;
                    if (primaryColor) {
                        applyColor(primaryColor);
                    }
                }
            } catch (error) {
                console.error('Failed to load branding', error);
            }
        };

        loadBranding();
    }, [tenantId]);

    const applyColor = (hex: string) => {
        // Convert hex to HSL or RGB if needed by tailwind
        // Simplified: apply directly to --brand-500 and its variations
        const root = document.documentElement;

        // Helper to get RGB from Hex
        const r = parseInt(hex.slice(1, 3), 16);
        const g = parseInt(hex.slice(3, 5), 16);
        const b = parseInt(hex.slice(5, 7), 16);

        // Update CSS variables
        root.style.setProperty('--brand-500', `${r} ${g} ${b}`);
        root.style.setProperty('--brand-600', `${Math.max(0, r - 20)} ${Math.max(0, g - 20)} ${Math.max(0, b - 20)}`);
    };

    return <>{children}</>;
};
