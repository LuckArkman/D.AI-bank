import { useState, useEffect } from 'react';
import axios from 'axios';
import { useAuthStore } from '../store/useAuthStore';
import { Landmark, TrendingUp, ArrowRight, Loader2, CheckCircle2, AlertCircle, Sparkles } from 'lucide-react';

import Modal from '../components/Modal';

interface Loan {
    id: string;
    principalAmount: number;
    interestRate: number;
    installments: number;
    monthlyPayment: number;
    remainingBalance: number;
    status: number; // Pending=0, Active=1, Paid=2
    createdAt: string;
}

const LoansPage = () => {
    const { token } = useAuthStore();
    const [loans, setLoans] = useState<Loan[]>([]);
    const [loading, setLoading] = useState(true);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [requestData, setRequestData] = useState({ amount: '', installments: 12 });
    const [submitting, setSubmitting] = useState(false);
    const [step, setStep] = useState<'form' | 'success'>('form');

    const fetchLoans = async () => {
        try {
            const response = await axios.get('/api/v1/loans', {
                headers: { Authorization: `Bearer ${token}` }
            });
            setLoans(response.data);
        } catch (err) {
            console.error('Error fetching loans', err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchLoans();
    }, []);

    const handleRequestLoan = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitting(true);
        try {
            await axios.post('/api/v1/loans/request', {
                amount: parseFloat(requestData.amount),
                installments: requestData.installments
            }, {
                headers: { Authorization: `Bearer ${token}` }
            });
            setStep('success');
            fetchLoans();
        } catch (err) {
            console.error('Error requesting loan', err);
        } finally {
            setSubmitting(false);
        }
    };

    const handleClose = () => {
        setIsModalOpen(false);
        setStep('form');
        setRequestData({ amount: '', installments: 12 });
    };

    return (
        <div className="space-y-10">
            <div className="flex justify-between items-center">
                <div>
                    <h2 className="text-3xl font-display font-bold text-white">Créditos e Empréstimos</h2>
                    <p className="text-surface-400">Linhas de crédito pré-aprovadas com as melhores taxas do mercado.</p>
                </div>
                <button
                    onClick={() => setIsModalOpen(true)}
                    className="btn-primary flex items-center gap-2"
                >
                    <TrendingUp className="w-5 h-5" />
                    Simular Empréstimo
                </button>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                <div className="lg:col-span-2 space-y-8">
                    <h3 className="text-xl font-bold">Contratos Ativos</h3>
                    {loading ? (
                        <div className="glass-card p-10 text-center"><Loader2 className="w-8 h-8 animate-spin mx-auto" /></div>
                    ) : loans.length === 0 ? (
                        <div className="glass-card p-12 text-center text-surface-500">Nenhum contrato ativo no momento.</div>
                    ) : (
                        <div className="space-y-4">
                            {loans.map((loan) => (
                                <div key={loan.id} className="glass-card flex justify-between items-center group">
                                    <div className="flex items-center gap-4">
                                        <div className="p-3 bg-brand-600/10 rounded-xl text-brand-400">
                                            <Landmark className="w-6 h-6" />
                                        </div>
                                        <div>
                                            <p className="font-bold">Crédito Pessoal - {loan.installments}x</p>
                                            <p className="text-xs text-surface-500">Contratado em {new Date(loan.createdAt).toLocaleDateString()}</p>
                                        </div>
                                    </div>
                                    <div className="text-right">
                                        <p className="font-bold text-white text-lg">
                                            R$ {loan.remainingBalance.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                                        </p>
                                        <p className="text-[10px] text-brand-500 bg-brand-500/10 px-2 py-0.5 rounded-full inline-block mt-1">
                                            {loan.status === 0 ? 'ANÁLISE' : loan.status === 1 ? 'ATIVO' : 'QUITADO'}
                                        </p>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                <div className="space-y-8">
                    <div className="glass-card bg-gradient-to-br from-brand-900/20 to-surface-900 border-brand-500/20">
                        <div className="flex items-center gap-2 text-brand-400 mb-4">
                            <Sparkles className="w-5 h-5" />
                            <span className="text-xs font-black uppercase tracking-widest">D.AI Score</span>
                        </div>
                        <h4 className="text-2xl font-display font-bold mb-4 text-white">Excelente!</h4>
                        <p className="text-sm text-surface-400 mb-8 leading-relaxed">
                            Baseado no seu histórico, você possui uma linha de crédito pré-aprovada de até <strong>R$ 80.000,00</strong>.
                        </p>
                        <div className="h-2 bg-surface-800 rounded-full overflow-hidden mb-8">
                            <div className="h-full w-[85%] bg-gradient-to-r from-brand-500 to-emerald-500 rounded-full shadow-[0_0_15px_rgba(139,92,246,0.5)]" />
                        </div>
                        <button onClick={() => setIsModalOpen(true)} className="btn-primary w-full">Solicitar Agora</button>
                    </div>
                </div>
            </div>

            <Modal isOpen={isModalOpen} onClose={handleClose} title="Simulação de Crédito">
                {step === 'form' ? (
                    <form onSubmit={handleRequestLoan} className="space-y-8">
                        <div className="space-y-4">
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-surface-300">Quanto você precisa?</label>
                                <div className="relative">
                                    <span className="absolute left-4 top-1/2 -translate-y-1/2 text-surface-500 font-bold">R$</span>
                                    <input
                                        type="number"
                                        className="input-field w-full pl-12 text-2xl font-bold"
                                        placeholder="0,00"
                                        required
                                        value={requestData.amount}
                                        onChange={(e) => setRequestData({ ...requestData, amount: e.target.value })}
                                    />
                                </div>
                            </div>

                            <div className="space-y-2">
                                <label className="text-sm font-medium text-surface-300">Número de Parcelas</label>
                                <div className="grid grid-cols-4 gap-2">
                                    {[6, 12, 24, 48].map((n) => (
                                        <button
                                            key={n}
                                            type="button"
                                            onClick={() => setRequestData({ ...requestData, installments: n })}
                                            className={`py-2 rounded-xl text-xs font-bold transition-all border ${requestData.installments === n ? 'bg-brand-600 border-brand-600 text-white' : 'bg-surface-900 border-white/5 text-surface-500 hover:border-brand-500/50'}`}
                                        >
                                            {n}x
                                        </button>
                                    ))}
                                </div>
                            </div>
                        </div>

                        <div className="glass-card !bg-white/5 space-y-4">
                            <div className="flex justify-between text-sm">
                                <span className="text-surface-400">Taxa de Juros</span>
                                <span className="text-emerald-500 font-bold">2.5% ao mês</span>
                            </div>
                            <div className="flex justify-between text-sm">
                                <span className="text-surface-400">Valor da Parcela (estimado)</span>
                                <span className="text-white font-bold">
                                    R$ {requestData.amount ? (parseFloat(requestData.amount) * 1.3 / requestData.installments).toLocaleString('pt-BR', { maximumFractionDigits: 2 }) : '0,00'}
                                </span>
                            </div>
                        </div>

                        <div className="flex items-center gap-3 p-4 bg-amber-500/10 border border-amber-500/20 rounded-xl">
                            <AlertCircle className="w-5 h-5 text-amber-500 flex-shrink-0" />
                            <p className="text-[10px] text-amber-200/80 leading-tight">
                                Ao clicar em solicitar, sua proposta passará por uma análise Instantânea de crédito pela nossa IA. Sujeito à aprovação.
                            </p>
                        </div>

                        <button
                            disabled={submitting || !requestData.amount}
                            className="btn-primary w-full py-4 text-lg flex items-center justify-center gap-2"
                        >
                            {submitting ? <Loader2 className="w-5 h-5 animate-spin" /> : <>Contratar Crédito <ArrowRight className="w-5 h-5" /></>}
                        </button>
                    </form>
                ) : (
                    <div className="text-center py-10 space-y-6">
                        <div className="w-24 h-24 bg-emerald-500/20 rounded-full flex items-center justify-center mx-auto mb-6">
                            <CheckCircle2 className="w-12 h-12 text-emerald-500" />
                        </div>
                        <div>
                            <h4 className="text-3xl font-display font-bold mb-2">Solicitação Enviada!</h4>
                            <p className="text-surface-400">Nossa IA está analisando sua proposta. Você receberá uma notificação em instantes.</p>
                        </div>
                        <button onClick={handleClose} className="btn-secondary w-full py-4">Voltar ao Painel</button>
                    </div>
                )}
            </Modal>
        </div>
    );
};

export default LoansPage;
