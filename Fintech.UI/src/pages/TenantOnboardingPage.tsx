import { useState } from 'react';
import { adminApi, TenantOnboardingRequest } from '../api/adminApi';
import { Building2, User, Palette, Globe, CheckCircle, AlertTriangle } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

const TenantOnboardingPage = () => {
    const navigate = useNavigate();
    const [loading, setLoading] = useState(false);
    const [successData, setSuccessData] = useState<any>(null);
    const [error, setError] = useState('');

    const [form, setForm] = useState<TenantOnboardingRequest>({
        name: '',
        identifier: '',
        adminEmail: '',
        adminPassword: '',
        adminName: '',
        themePrimaryColor: '#8B5CF6',
        jurisdiction: 'Brazil',
        businessMode: 'B2C'
    });

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError('');
        try {
            const response = await adminApi.onboardTenant(form);
            setSuccessData(response);
        } catch (err: any) {
            console.error('Onboarding failed', err);
            setError(err.response?.data?.Error || 'Failed to onboard tenant');
        } finally {
            setLoading(false);
        }
    };

    if (successData) {
        return (
            <div className="max-w-2xl mx-auto p-8 glass-card animate-in zoom-in-95 duration-500">
                <div className="text-center mb-8">
                    <div className="w-20 h-20 bg-green-500/20 text-green-400 rounded-full flex items-center justify-center mx-auto mb-4">
                        <CheckCircle size={40} />
                    </div>
                    <h2 className="text-3xl font-display font-bold">Tenant Created Successfully!</h2>
                    <p className="text-surface-400 mt-2">The new tenant environment has been initialized.</p>
                </div>

                <div className="space-y-4 bg-surface-950/50 p-6 rounded-xl border border-white/5">
                    <div className="flex justify-between border-b border-white/5 pb-2">
                        <span className="text-surface-400">Tenant ID</span>
                        <span className="font-mono text-sm">{successData.tenantId}</span>
                    </div>
                    <div className="flex justify-between border-b border-white/5 pb-2">
                        <span className="text-surface-400">API Key</span>
                        <span className="font-mono text-sm text-brand-300">{successData.apiKey}</span>
                    </div>
                    <div className="flex justify-between">
                        <span className="text-surface-400">Admin Email</span>
                        <span className="font-medium">{successData.adminUserEmail}</span>
                    </div>
                </div>

                <div className="mt-8 flex justify-center">
                    <button onClick={() => setSuccessData(null)} className="btn-secondary mr-4">Onboard Another</button>
                    <button onClick={() => navigate('/dashboard')} className="btn-primary">Go to Dashboard</button>
                </div>
            </div>
        );
    }

    return (
        <div className="max-w-4xl mx-auto">
            <header className="mb-8">
                <h1 className="text-3xl font-display font-bold">Tenant Onboarding</h1>
                <p className="text-surface-400 mt-1">Create a new bank environment (Tenant) with its own database and rules.</p>
            </header>

            <form onSubmit={handleSubmit} className="glass-card p-8 space-y-8 animate-in slide-in-from-bottom-4 duration-500">
                {/* Organization Details */}
                <div className="space-y-4">
                    <div className="flex items-center gap-2 text-brand-400 border-b border-white/5 pb-2">
                        <Building2 size={20} />
                        <h3 className="font-bold text-lg">Organization Details</h3>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                            <label className="label">Tenant Name</label>
                            <input
                                type="text"
                                required
                                className="input-field w-full"
                                placeholder="e.g. NeoBank X"
                                value={form.name}
                                onChange={e => setForm({ ...form, name: e.target.value })}
                            />
                        </div>
                        <div>
                            <label className="label">Identifier (Subdomain)</label>
                            <input
                                type="text"
                                required
                                className="input-field w-full"
                                placeholder="e.g. neobank-x"
                                value={form.identifier}
                                onChange={e => setForm({ ...form, identifier: e.target.value })}
                            />
                        </div>
                    </div>
                </div>

                {/* Admin User */}
                <div className="space-y-4">
                    <div className="flex items-center gap-2 text-brand-400 border-b border-white/5 pb-2">
                        <User size={20} />
                        <h3 className="font-bold text-lg">Initial Admin User</h3>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                            <label className="label">Admin Name</label>
                            <input
                                type="text"
                                required
                                className="input-field w-full"
                                placeholder="Full Name"
                                value={form.adminName}
                                onChange={e => setForm({ ...form, adminName: e.target.value })}
                            />
                        </div>
                        <div>
                            <label className="label">Admin Email</label>
                            <input
                                type="email"
                                required
                                className="input-field w-full"
                                placeholder="admin@neobank.com"
                                value={form.adminEmail}
                                onChange={e => setForm({ ...form, adminEmail: e.target.value })}
                            />
                        </div>
                        <div className="md:col-span-2">
                            <label className="label">Admin Password</label>
                            <input
                                type="password"
                                required
                                className="input-field w-full"
                                placeholder="Strong Password"
                                value={form.adminPassword}
                                onChange={e => setForm({ ...form, adminPassword: e.target.value })}
                            />
                        </div>
                    </div>
                </div>

                {/* Configuration */}
                <div className="space-y-4">
                    <div className="flex items-center gap-2 text-brand-400 border-b border-white/5 pb-2">
                        <Globe size={20} />
                        <h3 className="font-bold text-lg">Configuration</h3>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                        <div>
                            <label className="label">Jurisdiction</label>
                            <select
                                className="input-field w-full"
                                value={form.jurisdiction}
                                onChange={e => setForm({ ...form, jurisdiction: e.target.value })}
                            >
                                <option value="Brazil">Brazil</option>
                                <option value="USA">USA</option>
                                <option value="Europe">Europe</option>
                                <option value="UnitedKingdom">United Kingdom</option>
                            </select>
                        </div>
                        <div>
                            <label className="label">Business Mode</label>
                            <select
                                className="input-field w-full"
                                value={form.businessMode}
                                onChange={e => setForm({ ...form, businessMode: e.target.value })}
                            >
                                <option value="B2C">Retail (B2C)</option>
                                <option value="B2B">Corporate (B2B)</option>
                            </select>
                        </div>
                        <div>
                            <label className="label">Primary Color</label>
                            <div className="flex items-center gap-2">
                                <input
                                    type="color"
                                    className="h-10 w-10 rounded bg-transparent border-0 cursor-pointer"
                                    value={form.themePrimaryColor}
                                    onChange={e => setForm({ ...form, themePrimaryColor: e.target.value })}
                                />
                                <input
                                    type="text"
                                    className="input-field w-full"
                                    value={form.themePrimaryColor}
                                    onChange={e => setForm({ ...form, themePrimaryColor: e.target.value })}
                                />
                            </div>
                        </div>
                    </div>
                </div>

                {error && (
                    <div className="bg-red-500/10 border border-red-500/20 text-red-400 p-4 rounded-lg flex items-center gap-2">
                        <AlertTriangle size={20} />
                        {error}
                    </div>
                )}

                <div className="flex justify-end pt-4">
                    <button type="submit" disabled={loading} className="btn-primary w-full md:w-auto px-8 relative">
                        {loading ? (
                            <span className="flex items-center gap-2">Processing...</span>
                        ) : (
                            "Create Tenant"
                        )}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default TenantOnboardingPage;
