import { useState, useEffect } from 'react';
import axios from 'axios';
import { useAuthStore } from '../store/useAuthStore';
import { TrendingUp, Plus, Target, ArrowRight, Loader2, Sparkles, PieChart as ChartIcon } from 'lucide-react';


import Modal from '../components/Modal';

interface Investment {
    id: string;
    name: string;
    type: number;
    principalAmount: number;
    currentAmount: number;
    expectedReturnRate: number;
}

interface SavingsGoal {
    id: string;
    name: string;
    targetAmount: number;
    currentAmount: number;
    color: string;
    status: number;
}

const InvestmentsPage = () => {
    const { token } = useAuthStore();
    const [investments, setInvestments] = useState<Investment[]>([]);
    const [goals, setGoals] = useState<SavingsGoal[]>([]);
    const [loading, setLoading] = useState(true);

    const [isInvestModalOpen, setIsInvestModalOpen] = useState(false);
    const [isGoalModalOpen, setIsGoalModalOpen] = useState(false);
    const [isDepositGoalOpen, setIsDepositGoalOpen] = useState(false);
    const [selectedGoal, setSelectedGoal] = useState<SavingsGoal | null>(null);

    const [investData, setInvestData] = useState({ name: '', amount: '', type: 1 }); // CDB=1
    const [goalData, setGoalData] = useState({ name: '', target: '', color: 'bg-brand-500' });
    const [depositAmount, setDepositAmount] = useState('');

    const [submitting, setSubmitting] = useState(false);

    const fetchData = async () => {
        try {
            const [invRes, goalRes] = await Promise.all([
                axios.get('/api/v1/investments', { headers: { Authorization: `Bearer ${token}` } }),
                axios.get('/api/v1/investments/goals', { headers: { Authorization: `Bearer ${token}` } })
            ]);
            setInvestments(invRes.data);
            setGoals(goalRes.data);
        } catch (err) {
            console.error('Error fetching investments', err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchData();
    }, []);

    const handleInvest = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitting(true);
        try {
            await axios.post('/api/v1/investments', {
                name: investData.name,
                amount: parseFloat(investData.amount),
                type: investData.type
            }, { headers: { Authorization: `Bearer ${token}` } });
            setIsInvestModalOpen(false);
            fetchData();
        } catch (err) {
            console.error(err);
        } finally {
            setSubmitting(false);
        }
    };

    const handleCreateGoal = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitting(true);
        try {
            await axios.post('/api/v1/investments/goals', {
                name: goalData.name,
                targetAmount: parseFloat(goalData.target),
                color: goalData.color
            }, { headers: { Authorization: `Bearer ${token}` } });
            setIsGoalModalOpen(false);
            fetchData();
        } catch (err) {
            console.error(err);
        } finally {
            setSubmitting(false);
        }
    };

    const handleDepositGoal = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!selectedGoal) return;
        setSubmitting(true);
        try {
            await axios.post(`/api/v1/investments/goals/${selectedGoal.id}/deposit`, {
                amount: parseFloat(depositAmount)
            }, { headers: { Authorization: `Bearer ${token}` } });
            setIsDepositGoalOpen(false);
            fetchData();
        } catch (err) {
            console.error(err);
        } finally {
            setSubmitting(false);
        }
    };

    const totalInvested = investments.reduce((acc, inv) => acc + inv.currentAmount, 0);
    const totalGoals = goals.reduce((acc, g) => acc + g.currentAmount, 0);

    return (
        <div className="space-y-10">
            <div className="flex justify-between items-center">
                <div>
                    <h2 className="text-3xl font-display font-bold text-white">Investimentos e Caixinhas</h2>
                    <p className="text-surface-400">Faça seu dinheiro trabalhar por você com inteligência artificial.</p>
                </div>
                <div className="flex gap-4">
                    <button
                        onClick={() => setIsGoalModalOpen(true)}
                        className="bg-white/5 hover:bg-white/10 text-white px-6 py-2.5 rounded-xl text-sm font-bold border border-white/5 transition-all flex items-center gap-2"
                    >
                        <Plus className="w-4 h-4 text-brand-400" />
                        Criar Caixinha
                    </button>
                    <button
                        onClick={() => setIsInvestModalOpen(true)}
                        className="btn-primary flex items-center gap-2"
                    >
                        <TrendingUp className="w-4 h-4" />
                        Novo Investimento
                    </button>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
                {/* Stats */}
                <div className="lg:col-span-1 space-y-6">
                    <div className="glass-card p-6 bg-gradient-to-br from-brand-600/20 to-surface-900 border-brand-500/20">
                        <p className="text-sm text-surface-400 mb-1">Total em Investimentos</p>
                        <h3 className="text-3xl font-display font-bold text-white">
                            R$ {totalInvested.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                        </h3>
                        <div className="flex items-center gap-2 text-emerald-500 text-xs mt-4">
                            <TrendingUp className="w-3 h-3" />
                            <span>+12.4% este ano</span>
                        </div>
                    </div>

                    <div className="glass-card p-6">
                        <p className="text-sm text-surface-400 mb-1">Total em Caixinhas</p>
                        <h3 className="text-2xl font-display font-bold text-white">
                            R$ {totalGoals.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                        </h3>
                    </div>

                    <div className="glass-card p-6 border-dashed border-white/10 opacity-50">
                        <div className="flex items-center gap-3 mb-4">
                            <Sparkles className="w-5 h-5 text-amber-500" />
                            <span className="text-xs font-bold uppercase tracking-wider">D.AI Advisor</span>
                        </div>
                        <p className="text-xs text-surface-400 leading-relaxed">
                            Percebi que você tem R$ 2.400 parado no saldo. Em um CDB de liquidez diária, isso renderia R$ 21,50 este mês.
                        </p>
                        <button className="text-brand-400 text-[10px] font-bold mt-4 hover:underline uppercase tracking-widest">Aplicar Agora</button>
                    </div>
                </div>

                {/* Main Content */}
                <div className="lg:col-span-3 space-y-12">
                    {/* Caixinhas Section */}
                    <section className="space-y-6">
                        <div className="flex justify-between items-end">
                            <h3 className="text-xl font-bold flex items-center gap-3">
                                <Target className="text-brand-400 w-6 h-6" /> Minhas Caixinhas
                            </h3>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            {loading ? (
                                <div className="col-span-full py-10 text-center"><Loader2 className="w-6 h-6 animate-spin mx-auto text-brand-500" /></div>
                            ) : goals.length === 0 ? (
                                <div className="col-span-full glass-card p-10 text-center text-surface-500 italic">Você ainda não tem caixinhas.</div>
                            ) : (
                                goals.map(goal => (
                                    <div key={goal.id} className="glass-card group hover:border-brand-500/30 transition-all p-6">
                                        <div className="flex justify-between items-start mb-6">
                                            <div className={`p-3 rounded-2xl ${goal.color}`}>
                                                <Target className="w-6 h-6 text-white" />
                                            </div>
                                            <div className="text-right">
                                                <p className="text-xs text-surface-500 font-bold uppercase tracking-tighter">Status</p>
                                                <p className="text-xs font-black text-brand-400">{goal.status === 1 ? 'CONCLUÍDO' : 'EM ANDAMENTO'}</p>
                                            </div>
                                        </div>
                                        <h4 className="text-lg font-bold mb-1">{goal.name}</h4>
                                        <p className="text-xs text-surface-500 mb-6 font-mono tracking-widest">
                                            R$ {goal.currentAmount.toLocaleString()} / R$ {goal.targetAmount.toLocaleString()}
                                        </p>

                                        <div className="space-y-2">
                                            <div className="h-2 bg-surface-900 rounded-full overflow-hidden">
                                                <div
                                                    className={`h-full transition-all duration-1000 ${goal.color.replace('bg-', 'bg-')}`}
                                                    style={{ width: `${Math.min(100, (goal.currentAmount / goal.targetAmount) * 100)}%` }}
                                                />
                                            </div>
                                            <div className="flex justify-between text-[10px] font-bold text-surface-500">
                                                <span>{Math.round((goal.currentAmount / goal.targetAmount) * 100)}%</span>
                                                <span>META</span>
                                            </div>
                                        </div>

                                        <button
                                            onClick={() => { setSelectedGoal(goal); setIsDepositGoalOpen(true); }}
                                            className="w-full bg-white/5 hover:bg-white/10 py-2.5 rounded-xl text-xs font-bold mt-6 opacity-0 group-hover:opacity-100 transition-all"
                                        >
                                            Guardar mais dinheiro
                                        </button>
                                    </div>
                                ))
                            )}
                        </div>
                    </section>

                    {/* Investments Section */}
                    <section className="space-y-6">
                        <div className="flex justify-between items-end">
                            <h3 className="text-xl font-bold flex items-center gap-3">
                                <ChartIcon className="text-brand-400 w-6 h-6" /> Portfólio de Ativos
                            </h3>
                        </div>

                        <div className="glass-card p-0 overflow-hidden border-white/5">
                            <table className="w-full text-left border-collapse">
                                <thead>
                                    <tr className="bg-white/5 text-[10px] font-black uppercase tracking-widest text-surface-500 border-b border-white/5">
                                        <th className="p-4">Produto</th>
                                        <th className="p-4">Tipo</th>
                                        <th className="p-4">Aplicado</th>
                                        <th className="p-4">Atual</th>
                                        <th className="p-4">Rentabilidade</th>
                                        <th className="p-4"></th>
                                    </tr>
                                </thead>
                                <tbody className="divide-y divide-white/5">
                                    {investments.map(inv => (
                                        <tr key={inv.id} className="hover:bg-white/[0.02] text-sm group transition-colors">
                                            <td className="p-4 font-bold">{inv.name}</td>
                                            <td className="p-4">
                                                <span className="bg-brand-500/10 text-brand-400 text-[10px] font-bold px-2 py-0.5 rounded">CDB</span>
                                            </td>
                                            <td className="p-4 text-surface-400">R$ {inv.principalAmount.toLocaleString()}</td>
                                            <td className="p-4 text-white font-mono font-bold">R$ {inv.currentAmount.toLocaleString()}</td>
                                            <td className="p-4">
                                                <span className="text-emerald-500 font-bold">+{inv.expectedReturnRate}% <span className="text-[10px] opacity-50">a.a</span></span>
                                            </td>
                                            <td className="p-4 text-right">
                                                <button className="opacity-0 group-hover:opacity-100 p-2 hover:bg-white/5 rounded-lg transition-all">
                                                    <ArrowRight className="w-4 h-4" />
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </section>
                </div>
            </div>

            {/* Modals */}
            <Modal isOpen={isInvestModalOpen} onClose={() => setIsInvestModalOpen(false)} title="Nova Aplicação">
                <form onSubmit={handleInvest} className="space-y-6">
                    <div className="space-y-2">
                        <label className="text-sm font-medium">Nome do Ativo</label>
                        <input
                            className="input-field w-full"
                            placeholder="Ex: CDB Liquidez D.AI"
                            required
                            value={investData.name}
                            onChange={e => setInvestData({ ...investData, name: e.target.value })}
                        />
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium">Tipo</label>
                            <select className="input-field w-full appearance-none">
                                <option value={1}>CDB</option>
                                <option value={2}>LCI / LCA</option>
                            </select>
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium">Valor</label>
                            <input
                                type="number"
                                className="input-field w-full"
                                placeholder="R$ 0,00"
                                required
                                value={investData.amount}
                                onChange={e => setInvestData({ ...investData, amount: e.target.value })}
                            />
                        </div>
                    </div>
                    <button disabled={submitting} className="btn-primary w-full py-4 flex justify-center items-center gap-2">
                        {submitting ? <Loader2 className="w-5 h-5 animate-spin" /> : 'Confirmar Aplicação'}
                    </button>
                </form>
            </Modal>

            <Modal isOpen={isGoalModalOpen} onClose={() => setIsGoalModalOpen(false)} title="Criar Nova Caixinha">
                <form onSubmit={handleCreateGoal} className="space-y-6">
                    <div className="space-y-2">
                        <label className="text-sm font-medium">Qual sua meta?</label>
                        <input
                            className="input-field w-full"
                            placeholder="Ex: Viagem de Férias"
                            required
                            value={goalData.name}
                            onChange={e => setGoalData({ ...goalData, name: e.target.value })}
                        />
                    </div>
                    <div className="space-y-2">
                        <label className="text-sm font-medium">Quanto deseja poupar?</label>
                        <input
                            type="number"
                            className="input-field w-full"
                            placeholder="R$ 0,00"
                            required
                            value={goalData.target}
                            onChange={e => setGoalData({ ...goalData, target: e.target.value })}
                        />
                    </div>
                    <div className="space-y-2">
                        <label className="text-sm font-medium">Estilo</label>
                        <div className="flex gap-2">
                            {['bg-brand-500', 'bg-emerald-500', 'bg-blue-500', 'bg-orange-500', 'bg-pink-500'].map(c => (
                                <button
                                    key={c}
                                    type="button"
                                    onClick={() => setGoalData({ ...goalData, color: c })}
                                    className={`w-10 h-10 rounded-xl ${c} border-2 ${goalData.color === c ? 'border-white' : 'border-transparent'}`}
                                />
                            ))}
                        </div>
                    </div>
                    <button disabled={submitting} className="btn-primary w-full py-4 flex justify-center items-center gap-2">
                        {submitting ? <Loader2 className="w-5 h-5 animate-spin" /> : 'Criar Caixinha'}
                    </button>
                </form>
            </Modal>

            <Modal isOpen={isDepositGoalOpen} onClose={() => setIsDepositGoalOpen(false)} title={`Guardar na: ${selectedGoal?.name}`}>
                <form onSubmit={handleDepositGoal} className="space-y-6">
                    <div className="space-y-2">
                        <label className="text-sm font-medium text-surface-400">Quanto deseja guardar agora?</label>
                        <input
                            type="number"
                            className="input-field w-full text-2xl font-bold"
                            placeholder="R$ 0,00"
                            required
                            value={depositAmount}
                            onChange={e => setDepositAmount(e.target.value)}
                        />
                    </div>
                    <button disabled={submitting} className="btn-primary w-full py-4 flex justify-center items-center gap-2">
                        {submitting ? <Loader2 className="w-5 h-5 animate-spin" /> : 'Confirmar Depósito'}
                    </button>
                </form>
            </Modal>
        </div>
    );
};

export default InvestmentsPage;
