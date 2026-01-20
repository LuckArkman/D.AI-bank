import { useState } from 'react';
import axios from 'axios';
import { useAuthStore } from '../store/useAuthStore';
import { Loader2, CheckCircle2, AlertCircle, Banknote, ClipboardCheck } from 'lucide-react';
import Modal from './Modal';

interface DepositModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSuccess: () => void;
}

const DepositModal = ({ isOpen, onClose, onSuccess }: DepositModalProps) => {
    const { token } = useAuthStore();
    const [amount, setAmount] = useState('');
    const [loading, setLoading] = useState(false);
    const [step, setStep] = useState<'form' | 'success'>('form');
    const [error, setError] = useState('');

    const handleDeposit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError('');

        try {
            await axios.post('/api/v1/operations/deposit', {
                amount: parseFloat(amount)
            }, {
                headers: { Authorization: `Bearer ${token}` }
            });

            setStep('success');
            onSuccess();
        } catch (err: any) {
            setError(err.response?.data?.error || 'Erro ao processar depósito.');
        } finally {
            setLoading(false);
        }
    };

    const handleClose = () => {
        setStep('form');
        setAmount('');
        setError('');
        onClose();
    };

    return (
        <Modal isOpen={isOpen} onClose={handleClose} title="Adicionar Saldo">
            {step === 'form' ? (
                <form onSubmit={handleDeposit} className="space-y-6">
                    <div className="p-4 bg-brand-600/10 rounded-2xl flex items-center gap-4 mb-6">
                        <div className="w-12 h-12 bg-brand-600 rounded-xl flex items-center justify-center text-white shadow-lg">
                            <Banknote className="w-6 h-6" />
                        </div>
                        <div>
                            <p className="text-sm text-surface-400">Sandbox Mode</p>
                            <p className="text-white font-bold">Depósito Instantâneo</p>
                        </div>
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-surface-300">Quanto deseja depositar?</label>
                        <div className="relative">
                            <span className="absolute left-4 top-1/2 -translate-y-1/2 text-surface-500 font-bold">R$</span>
                            <input
                                type="number"
                                step="0.01"
                                min="0.01"
                                required
                                autoFocus
                                className="input-field w-full pl-12 text-2xl font-bold"
                                placeholder="0,00"
                                value={amount}
                                onChange={(e) => setAmount(e.target.value)}
                            />
                        </div>
                        <p className="text-xs text-surface-500 mt-2">O valor será creditado imediatamente em sua conta.</p>
                    </div>

                    {error && (
                        <div className="p-4 bg-red-500/10 text-red-500 border border-red-500/20 rounded-xl text-sm flex items-center gap-3">
                            <AlertCircle className="w-5 h-5" />
                            {error}
                        </div>
                    )}

                    <button
                        type="submit"
                        disabled={loading || !amount}
                        className="btn-primary w-full py-4 text-lg flex items-center justify-center gap-2"
                    >
                        {loading ? <Loader2 className="w-5 h-5 animate-spin" /> : 'Confirmar Depósito'}
                    </button>
                </form>
            ) : (
                <div className="text-center py-10 space-y-6">
                    <div className="w-24 h-24 bg-emerald-500/20 rounded-full flex items-center justify-center mx-auto mb-6">
                        <CheckCircle2 className="w-12 h-12 text-emerald-500" />
                    </div>
                    <div>
                        <h4 className="text-2xl font-display font-bold mb-2">Depósito Confirmado!</h4>
                        <p className="text-surface-400 tracking-wide">
                            Seu saldo de <span className="text-white font-bold">R$ {parseFloat(amount).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}</span> já está disponível.
                        </p>
                    </div>
                    <button
                        onClick={handleClose}
                        className="btn-secondary w-full py-4 flex items-center justify-center gap-2"
                    >
                        <ClipboardCheck className="w-5 h-5" />
                        Entendido
                    </button>
                </div>
            )}
        </Modal>
    );
};

export default DepositModal;
