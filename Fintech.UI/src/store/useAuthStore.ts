import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface User {
    id: string;
    name: string;
    email: string;
    accountId: string;
}

interface AuthState {
    user: User | null;
    token: string | null;
    tenantId: string | null;
    isAuthenticated: boolean;
    login: (user: User, token: string, tenantId?: string) => void;
    setTenant: (tenantId: string) => void;
    logout: () => void;
}

export const useAuthStore = create<AuthState>()(
    persist(
        (set) => ({
            user: null,
            token: null,
            tenantId: null,
            isAuthenticated: false,
            login: (user, token, tenantId) => set((state) => ({
                user,
                token,
                tenantId: tenantId || state.tenantId,
                isAuthenticated: true
            })),
            setTenant: (tenantId) => set({ tenantId }),
            logout: () => set({ user: null, token: null, isAuthenticated: false }),
        }),
        {
            name: 'fintech-auth',
        }
    )
);
