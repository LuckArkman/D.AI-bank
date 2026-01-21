import { useState, useEffect } from 'react';
import { adminApi, BusinessRule } from '../api/adminApi';
import { Plus, Trash2, ShieldCheck, AlertTriangle, Info, XCircle, FileText } from 'lucide-react';
import Modal from '../components/Modal';

const CompliancePage = () => {
    const [activeTab, setActiveTab] = useState<'rules' | 'reports'>('rules');
    const [rules, setRules] = useState<BusinessRule[]>([]);
    const [loading, setLoading] = useState(false);
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

    // Form State
    const [newRule, setNewRule] = useState<Partial<BusinessRule>>({
        name: '',
        description: '',
        conditionExpression: '',
        errorMessage: '',
        severity: 2 // Warning by default
    });

    useEffect(() => {
        if (activeTab === 'rules') {
            loadRules();
        }
    }, [activeTab]);

    const loadRules = async () => {
        setLoading(true);
        try {
            const data = await adminApi.getRules();
            setRules(data);
        } catch (error) {
            console.error('Failed to load rules', error);
        } finally {
            setLoading(false);
        }
    };

    const handleCreateRule = async () => {
        try {
            await adminApi.createRule(newRule as BusinessRule);
            setIsCreateModalOpen(false);
            setNewRule({ name: '', description: '', conditionExpression: '', errorMessage: '', severity: 2 });
            loadRules();
        } catch (error) {
            console.error('Failed to create rule', error);
            alert('Error creating rule');
        }
    };

    const handleDeleteRule = async (id: string) => {
        if (!confirm('Are you sure you want to delete this rule?')) return;
        try {
            await adminApi.deleteRule(id);
            loadRules();
        } catch (error) {
            console.error('Failed to delete rule', error);
        }
    };

    const getSeverityBadge = (severity: number) => {
        switch (severity) {
            case 1: return <span className="px-2 py-1 rounded-full text-xs font-bold bg-blue-500/10 text-blue-400 border border-blue-500/20 flex items-center gap-1"><Info size={12} /> Info</span>;
            case 2: return <span className="px-2 py-1 rounded-full text-xs font-bold bg-yellow-500/10 text-yellow-400 border border-yellow-500/20 flex items-center gap-1"><AlertTriangle size={12} /> Warning</span>;
            case 3: return <span className="px-2 py-1 rounded-full text-xs font-bold bg-orange-500/10 text-orange-400 border border-orange-500/20 flex items-center gap-1"><XCircle size={12} /> Error</span>;
            case 4: return <span className="px-2 py-1 rounded-full text-xs font-bold bg-red-500/10 text-red-400 border border-red-500/20 flex items-center gap-1"><ShieldCheck size={12} /> Blocking</span>;
            default: return null;
        }
    };

    return (
        <div className="space-y-6">
            <header className="flex items-center justify-between">
                <div>
                    <h1 className="text-3xl font-display font-bold">Compliance & Rules</h1>
                    <p className="text-surface-400 mt-1">Manage business rules and view compliance reports.</p>
                </div>
                <div className="flex bg-surface-900/50 p-1 rounded-xl border border-white/5">
                    <button
                        onClick={() => setActiveTab('rules')}
                        className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${activeTab === 'rules' ? 'bg-brand-600 text-white shadow-lg' : 'text-surface-400 hover:text-white'}`}
                    >
                        Business Rules
                    </button>
                    <button
                        onClick={() => setActiveTab('reports')}
                        className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${activeTab === 'reports' ? 'bg-brand-600 text-white shadow-lg' : 'text-surface-400 hover:text-white'}`}
                    >
                        Reports
                    </button>
                </div>
            </header>

            {activeTab === 'rules' && (
                <div className="glass-card p-0 overflow-hidden animate-in fade-in slide-in-from-bottom-4 duration-500">
                    <div className="p-6 border-b border-white/5 flex justify-between items-center">
                        <h2 className="text-xl font-bold flex items-center gap-2">
                            <ShieldCheck className="text-brand-400" />
                            Active Rules
                        </h2>
                        <button
                            onClick={() => setIsCreateModalOpen(true)}
                            className="btn-primary flex items-center gap-2"
                        >
                            <Plus size={18} />
                            New Rule
                        </button>
                    </div>

                    {loading ? (
                        <div className="p-12 text-center text-surface-400">Loading rules...</div>
                    ) : rules.length === 0 ? (
                        <div className="p-12 text-center text-surface-400">No rules found. Create one to get started.</div>
                    ) : (
                        <div className="divide-y divide-white/5">
                            {rules.map((rule) => (
                                <div key={rule.id} className="p-6 hover:bg-white/5 transition-colors group">
                                    <div className="flex justify-between items-start">
                                        <div className="space-y-2">
                                            <div className="flex items-center gap-3">
                                                <h3 className="font-bold text-lg">{rule.name}</h3>
                                                {getSeverityBadge(rule.severity)}
                                            </div>
                                            <p className="text-surface-400 text-sm max-w-2xl">{rule.description}</p>
                                            <div className="bg-surface-950/50 rounded-lg p-3 font-mono text-xs text-brand-300 border border-white/5 inline-block">
                                                {rule.conditionExpression}
                                            </div>
                                            <div className="text-xs text-red-300/80 flex items-center gap-1">
                                                <AlertTriangle size={12} />
                                                Error Message: "{rule.errorMessage}"
                                            </div>
                                        </div>
                                        <button
                                            onClick={() => rule.id && handleDeleteRule(rule.id)}
                                            className="p-2 hover:bg-red-500/10 text-surface-500 hover:text-red-400 rounded-lg transition-colors opacity-0 group-hover:opacity-100"
                                            title="Delete Rule"
                                        >
                                            <Trash2 size={18} />
                                        </button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            )}

            {activeTab === 'reports' && (
                <div className="glass-card p-12 text-center text-surface-400 animate-in fade-in slide-in-from-bottom-4 duration-500">
                    <FileText className="w-16 h-16 mx-auto mb-4 text-surface-600" />
                    <h3 className="text-xl font-bold text-white mb-2">Compliance Reports</h3>
                    <p>Report generation module is currently under construction.</p>
                </div>
            )}

            <Modal
                isOpen={isCreateModalOpen}
                onClose={() => setIsCreateModalOpen(false)}
                title="Create Business Rule"
            >
                <div className="space-y-4">
                    <div>
                        <label className="block text-sm font-medium text-surface-300 mb-1">Rule Name</label>
                        <input
                            type="text"
                            className="input-field w-full"
                            placeholder="e.g., High Value Transaction Check"
                            value={newRule.name}
                            onChange={(e) => setNewRule({ ...newRule, name: e.target.value })}
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-surface-300 mb-1">Description</label>
                        <textarea
                            className="input-field w-full h-20 resize-none"
                            placeholder="Describe what this rule validates..."
                            value={newRule.description}
                            onChange={(e) => setNewRule({ ...newRule, description: e.target.value })}
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-surface-300 mb-1">C# Condition Expression</label>
                        <input
                            type="text"
                            className="input-field w-full font-mono text-sm"
                            placeholder="e.g., amount > 10000 && currency == 'USD'"
                            value={newRule.conditionExpression}
                            onChange={(e) => setNewRule({ ...newRule, conditionExpression: e.target.value })}
                        />
                        <p className="text-xs text-surface-500 mt-1">
                            Available variables: <code>amount</code>, <code>currency</code>, <code>operationType</code> (via context).
                        </p>
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium text-surface-300 mb-1">Severity</label>
                            <select
                                className="input-field w-full"
                                value={newRule.severity}
                                onChange={(e) => setNewRule({ ...newRule, severity: Number(e.target.value) })}
                            >
                                <option value={1}>Information</option>
                                <option value={2}>Warning</option>
                                <option value={3}>Error</option>
                                <option value={4}>Blocking</option>
                            </select>
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-surface-300 mb-1">Error Message</label>
                            <input
                                type="text"
                                className="input-field w-full"
                                placeholder="Message to show if rule fails"
                                value={newRule.errorMessage}
                                onChange={(e) => setNewRule({ ...newRule, errorMessage: e.target.value })}
                            />
                        </div>
                    </div>

                    <div className="pt-4 flex justify-end gap-3">
                        <button
                            onClick={() => setIsCreateModalOpen(false)}
                            className="px-4 py-2 rounded-xl hover:bg-white/5 transition-colors font-medium text-surface-300 hover:text-white"
                        >
                            Cancel
                        </button>
                        <button
                            onClick={handleCreateRule}
                            disabled={!newRule.name || !newRule.conditionExpression}
                            className="btn-primary"
                        >
                            Create Rule
                        </button>
                    </div>
                </div>
            </Modal>
        </div>
    );
};

export default CompliancePage;
