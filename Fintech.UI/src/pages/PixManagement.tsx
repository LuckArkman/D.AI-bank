import { useState, useEffect } from 'react';
import axios from 'axios';
import { useAuthStore } from '../store/useAuthStore';
import { Plus, Trash2, Key, Loader2, CheckCircle2, AlertCircle } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import Modal from '../components/Modal';

interface PixKey {
    key: string;
    type: string;
    createdAt: string;
}

const PixManagement = () => {
    const { token } = useAuthStore();
    const [keys, setKeys] = useState<PixKey[]>([]);
    const [loading, setLoading] = useState(true);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [newKey, setNewKey] = useState({ key: '', type: 'EMAIL' });
    const [submitting, setSubmitting] = useState(false);
    const [message, setMessage] = useState({ type: '', text: '' });

    const fetchKeys = async () => {
        try {
            const response = await axios.get('/api/v1/pix/keys', {
                headers: { Authorization: `Bearer ${token}` }
            });
            setKeys(response.data);
        } catch (err) {
            console.error('Error fetching keys', err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchKeys();
    }, []);

    const handleCreateKey = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitting(true);
        setMessage({ type: '', text: '' });

        try {
            await axios.post('/api/v1/pix/keys', newKey, {
                headers: { Authorization: `Bearer ${token}` }
            });
            setMessage({ type: 'success', text: 'Chave Pix cadastrada com sucesso!' });
            setNewKey({ key: '', type: 'EMAIL' });
            fetchKeys();
            setTimeout(() => setIsModalOpen(false), 2000);
        } catch (err: any) {
            setMessage({ type: 'error', text: err.response?.data?.error || 'Erro ao cadastrar chave.' });
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className="space-y-8">
            <div className="flex justify-between items-center">
                <div>
                    <h2 className="text-3xl font-display font-bold">Minhas Chaves Pix</h2>
                    <p className="text-surface-400">Gerencie como você recebe transferências instantâneas.</p>
                </div>
                <button
                    onClick={() => setIsModalOpen(true)}
                    className="btn-primary flex items-center gap-2"
                >
                    <Plus className="w-5 h-5" />
                    Nova Chave
                </button>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                <AnimatePresence>
                    {loading ? (
                        <div className="col-span-full py-20 text-center text-surface-500">
                            <Loader2 className="w-8 h-8 animate-spin mx-auto mb-4" />
                            Carregando chaves...
                        </div>
                    ) : keys.length === 0 ? (
                        <div className="col-span-full glass-card p-12 text-center">
                            <Key className="w-12 h-12 text-surface-600 mx-auto mb-4" />
                            <p className="text-surface-400">Você ainda não tem chaves Pix cadastradas.</p>
                            <button
                                onClick={() => setIsModalOpen(true)}
                                className="text-brand-400 hover:text-brand-300 font-bold mt-4"
                            >
                                Criar minha primeira chave
                            </button>
                        </div>
                    ) : (
                        keys.map((k) => (
                            <motion.div
                                key={k.key}
                                initial={{ opacity: 0, scale: 0.9 }}
                                animate={{ opacity: 1, scale: 1 }}
                                exit={{ opacity: 0, scale: 0.9 }}
                                className="glass-card flex flex-col justify-between group"
                            >
                                <div>
                                    <div className="flex justify-between items-start mb-4">
                                        <div className="p-3 bg-brand-600/10 rounded-xl text-brand-400">
                                            <Key className="w-6 h-6" />
                                        </div>
                                        <span className="text-xs font-bold uppercase tracking-wider text-surface-500 bg-white/5 px-2 py-1 rounded">
                                            {k.type}
                                        </span>
                                    </div>
                                    <p className="text-lg font-bold truncate mb-1">{k.key}</p>
                                    <p className="text-xs text-surface-500">Cadastrada em {new Date(k.createdAt).toLocaleDateString()}</p>
                                </div>

                                <div className="mt-6 pt-6 border-t border-white/5 flex gap-2">
                                    <button className="flex-1 btn-secondary py-2 text-sm">Copiar</button>
                                    <button className="p-2 hover:bg-red-500/10 hover:text-red-500 rounded-xl transition-colors text-surface-500">
                                        <Trash2 className="w-5 h-5" />
                                    </button>
                                </div>
                            </motion.div>
                        ))
                    )}
                </AnimatePresence>
            </div>

            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title="Cadastrar Nova Chave"
            >
                <form onSubmit={handleCreateKey} className="space-y-6">
                    <div className="space-y-2">
                        <label className="text-sm font-medium text-surface-300">Tipo de Chave</label>
                        <select
                            className="input-field w-full appearance-none"
                            value={newKey.type}
                            onChange={(e) => setNewKey({ ...newKey, type: e.target.value })}
                        >
                            <option value="EMAIL">Email</option>
                            <option value="CPF">CPF</option>
                            <option value="PHONE">Telefone</option>
                            <option value="RANDOM">Chave Aleatória</option>
                        </select>
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-surface-300">Sua Chave</label>
                        <input
                            type="text"
                            required
                            disabled={newKey.type === 'RANDOM'}
                            placeholder={newKey.type === 'RANDOM' ? 'Será gerada automaticamente' : 'Insira o valor da chave'}
                            className="input-field w-full disabled:opacity-50"
                            value={newKey.key}
                            onChange={(e) => setNewKey({ ...newKey, key: e.target.value })}
                        />
                    </div>

                    {message.text && (
                        <div className={cn(
                            "p-4 rounded-xl text-sm flex items-center gap-3",
                            message.type === 'success' ? "bg-emerald-500/10 text-emerald-500 border border-emerald-500/20" : "bg-red-500/10 text-red-500 border border-red-500/20"
                        )}>
                            {message.type === 'success' ? <CheckCircle2 className="w-5 h-5" /> : <AlertCircle className="w-5 h-5" />}
                            {message.text}
                        </div>
                    )}

                    <button
                        type="submit"
                        disabled={submitting}
                        className="btn-primary w-full py-4 flex items-center justify-center gap-2"
                    >
                        {submitting ? <Loader2 className="w-5 h-5 animate-spin" /> : 'Confirmar Cadastro'}
                    </button>
                </form>
            </Modal>
        </div>
    );
};

// Helper for classes locally if not available globably correctly
function cn(...inputs: any[]) {
    return inputs.filter(Boolean).join(' ');
}

export default PixManagement;
