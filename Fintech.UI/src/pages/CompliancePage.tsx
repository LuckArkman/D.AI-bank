import { useState, useEffect } from 'react';
import { adminApi, type BusinessRule } from '../api/adminApi';
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
                <div className="space-y-6 animate-in fade-in slide-in-from-bottom-4 duration-500">
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                        <ReportStatCard title="Total Transactions" value="128,432" icon={<FileText className="text-brand-400" />} trend="+12%" />
                        <ReportStatCard title="Flagged Events" value="14" icon={<AlertTriangle className="text-orange-400" />} trend="-3%" />
                        <ReportStatCard title="Compliance Health" value="98.2%" icon={<ShieldCheck className="text-emerald-400" />} trend="+0.5%" />
                    </div>

                    <div className="glass-card">
                        <div className="flex justify-between items-center mb-8">
                            <h3 className="text-xl font-bold">Recent Regulatory Reports</h3>
                            <button className="btn-secondary text-xs py-2 px-4">Generate On-Demand</button>
                        </div>

                        <div className="overflow-x-auto">
                            <table className="w-full text-left border-collapse">
                                <thead>
                                    <tr className="border-b border-white/5 text-surface-400 text-sm">
                                        <th className="pb-4 font-medium">Report Name</th>
                                        <th className="pb-4 font-medium">Jurisdiction</th>
                                        <th className="pb-4 font-medium">Status</th>
                                        <th className="pb-4 font-medium">Generated</th>
                                        <th className="pb-4 font-medium text-right">Action</th>
                                    </tr>
                                </thead>
                                <tbody className="divide-y divide-white/5">
                                    <ReportRow name="Monthly AML Summary" jurisdiction="Brazil" status="Completed" date="2026-01-15" />
                                    <ReportRow name="Quarterly Tax Evidence" jurisdiction="USA" status="Processing" date="2026-01-10" />
                                    <ReportRow name="KYC Audit Trail" jurisdiction="Europe" status="Completed" date="2026-01-05" />
                                    <ReportRow name="Faster Payments Audit" jurisdiction="UK" status="Failed" date="2025-12-28" />
                                </tbody>
                            </table>
                        </div>
                    </div>

                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                        <div className="glass-card h-80 flex flex-col items-center justify-center relative overflow-hidden">
                            <h4 className="absolute top-6 left-6 font-bold text-surface-400 text-sm uppercase tracking-wider">Risk Distribution</h4>
                            <div className="w-48 h-48 rounded-full border-[16px] border-surface-900 flex items-center justify-center relative">
                                <div className="absolute inset-0 rounded-full border-[16px] border-emerald-500 border-t-transparent border-r-transparent -rotate-45" />
                                <div className="text-center">
                                    <p className="text-3xl font-bold">Low</p>
                                    <p className="text-xs text-surface-500">Global Score</p>
                                </div>
                            </div>
                        </div>

                        <div className="glass-card h-80 flex flex-col">
                            <h4 className="font-bold text-surface-400 text-sm uppercase tracking-wider mb-6">Upcoming Deadlines</h4>
                            <div className="space-y-4">
                                <DeadlineItem title="BACEN Reporting" date="In 3 days" severity="red" />
                                <DeadlineItem title="GDPR Renewal" date="In 12 days" severity="yellow" />
                                <DeadlineItem title="FCA Quarterly Audit" date="In 15 days" severity="blue" />
                            </div>
                        </div>
                    </div>
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

const ReportStatCard = ({ title, value, icon, trend }: { title: string, value: string, icon: React.ReactNode, trend: string }) => (
    <div className="glass-card p-6 flex flex-col justify-between group hover:border-brand-500/30 transition-all">
        <div className="flex justify-between items-start">
            <div className="w-12 h-12 rounded-2xl bg-white/5 flex items-center justify-center group-hover:scale-110 transition-transform">
                {icon}
            </div>
            <span className={trend.startsWith('+') ? "text-emerald-400 text-xs font-bold" : "text-red-400 text-xs font-bold"}>
                {trend}
            </span>
        </div>
        <div className="mt-4">
            <p className="text-surface-400 text-sm font-medium">{title}</p>
            <h3 className="text-2xl font-bold mt-1">{value}</h3>
        </div>
    </div>
);

const ReportRow = ({ name, jurisdiction, status, date }: { name: string, jurisdiction: string, status: string, date: string }) => (
    <tr className="group hover:bg-white/5 transition-colors">
        <td className="py-4 font-medium text-surface-50">
            <div className="flex items-center gap-3">
                <div className="w-8 h-8 rounded-lg bg-surface-900 border border-white/5 flex items-center justify-center">
                    <FileText size={14} className="text-surface-400" />
                </div>
                {name}
            </div>
        </td>
        <td className="py-4 text-surface-400 text-sm">{jurisdiction}</td>
        <td className="py-4">
            <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold uppercase ${status === 'Completed' ? 'bg-emerald-500/10 text-emerald-400' :
                status === 'Processing' ? 'bg-indigo-500/10 text-indigo-400' :
                    'bg-red-500/10 text-red-400'
                }`}>
                {status}
            </span>
        </td>
        <td className="py-4 text-surface-400 text-sm">{date}</td>
        <td className="py-4 text-right">
            <button className="text-brand-400 hover:text-brand-300 text-xs font-bold opacity-0 group-hover:opacity-100 transition-all">Download</button>
        </td>
    </tr>
);

const DeadlineItem = ({ title, date, severity }: { title: string, date: string, severity: 'red' | 'yellow' | 'blue' }) => (
    <div className="flex items-center justify-between p-4 bg-white/5 rounded-xl border border-white/5">
        <div className="flex items-center gap-3">
            <div className={`w-2 h-2 rounded-full ${severity === 'red' ? 'bg-red-500 shadow-[0_0_8px_rgba(239,68,68,0.5)]' :
                severity === 'yellow' ? 'bg-yellow-500 shadow-[0_0_8px_rgba(234,179,8,0.5)]' :
                    'bg-blue-500 shadow-[0_0_8px_rgba(59,130,246,0.5)]'
                }`} />
            <span className="font-medium text-sm">{title}</span>
        </div>
        <span className="text-xs text-surface-400">{date}</span>
    </div>
);
