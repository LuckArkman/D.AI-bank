import { useState } from 'react';
import axios from 'axios';
import { useAuthStore } from '../store/useAuthStore';
import { Loader2, CheckCircle2, AlertCircle, Zap, ArrowRight, User } from 'lucide-react';
import Modal from './Modal';

interface PixTransferModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSuccess: () => void;
}

const PixTransferModal = ({ isOpen, onClose, onSuccess }: PixTransferModalProps) => {
    const { token } = useAuthStore();
    const [formData, setFormData] = useState({ key: '', amount: '' });
    const [loading, setLoading] = useState(false);
    const [step, setStep] = useState<'form' | 'success'>('form');
    const [error, setError] = useState('');

    const handleTransfer = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError('');

        try {
            await axios.post('/api/v1/operations/transfer/pix', {
                key: formData.key,
                amount: parseFloat(formData.amount)
            }, {
                headers: { Authorization: `Bearer ${token}` }
            });

            setStep('success');
            onSuccess();
        } catch (err: any) {
            setError(err.response?.data?.error || 'Erro ao processar transferência Pix.');
        } finally {
            setLoading(false);
        }
    };

    const handleClose = () => {
        setStep('form');
        setFormData({ key: '', amount: '' });
        setError('');
        onClose();
    };

    return (
        <Modal isOpen={isOpen} onClose={handleClose} title="Enviar Pix">
            {step === 'form' ? (
                <form onSubmit={handleTransfer} className="space-y-6">
                    <div className="space-y-2">
                        <label className="text-sm font-medium text-surface-300">Chave Pix do Destinatário</label>
                        <div className="relative">
                            <User className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-surface-600" />
                            <input
                                type="text"
                                required
                                className="input-field w-full pl-12"
                                placeholder="E-mail, CPF ou Celular"
                                value={formData.key}
                                onChange={(e) => setFormData({ ...formData, key: e.target.value })}
                            />
                        </div>
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-surface-300">Valor da Transferência</label>
                        <div className="relative">
                            <span className="absolute left-4 top-1/2 -translate-y-1/2 text-surface-500 font-bold">R$</span>
                            <input
                                type="number"
                                step="0.01"
                                min="0.01"
                                required
                                className="input-field w-full pl-12 text-2xl font-bold"
                                placeholder="0,00"
                                value={formData.amount}
                                onChange={(e) => setFormData({ ...formData, amount: e.target.value })}
                            />
                        </div>
                    </div>

                    {error && (
                        <div className="p-4 bg-red-500/10 text-red-500 border border-red-500/20 rounded-xl text-sm flex items-center gap-3">
                            <AlertCircle className="w-5 h-5" />
                            {error}
                        </div>
                    )}

                    <button
                        type="submit"
                        disabled={loading || !formData.amount || !formData.key}
                        className="btn-primary w-full py-4 text-lg flex items-center justify-center gap-2 group"
                    >
                        {loading ? (
                            <Loader2 className="w-5 h-5 animate-spin" />
                        ) : (
                            <>
                                Enviar Agora
                                <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
                            </>
                        )}
                    </button>
                </form>
            ) : (
                <div className="text-center py-10 space-y-6">
                    <div className="w-24 h-24 bg-brand-500/20 rounded-full flex items-center justify-center mx-auto mb-6">
                        <Zap className="w-12 h-12 text-brand-500 fill-brand-500" />
                    </div>
                    <div>
                        <h4 className="text-2xl font-display font-bold mb-2">Pix Enviado!</h4>
                        <p className="text-surface-400 tracking-wide">
                            Sua transferência de <span className="text-white font-bold">R$ {parseFloat(formData.amount).toLocaleString('pt-BR', { minimumFractionDigits: 2 })}</span> para <span className="text-white font-bold">{formData.key}</span> está sendo processada.
                        </p>
                    </div>
                    <button
                        onClick={handleClose}
                        className="btn-secondary w-full py-4"
                    >
                        Fechar
                    </button>
                </div>
            )}
        </Modal>
    );
};

export default PixTransferModal;
