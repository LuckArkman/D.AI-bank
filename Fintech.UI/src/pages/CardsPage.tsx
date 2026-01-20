import { useState, useEffect } from 'react';
import axios from 'axios';
import { useAuthStore } from '../store/useAuthStore';
import { CreditCard, Plus, Shield, Lock, Eye, EyeOff, Loader2 } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import Modal from '../components/Modal';

interface Card {
    id: string;
    lastFourDigits: string;
    brand: string;
    holderName: string;
    type: number; // Debit=0, Credit=1
    status: number; // Active=0
    isVirtual: boolean;
    creditLimit: number;
    availableCredit: number;
    expiryDate: string;
}

const CardsPage = () => {
    const { token } = useAuthStore();
    const [cards, setCards] = useState<Card[]>([]);
    const [loading, setLoading] = useState(true);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [issuing, setIssuing] = useState(false);
    const [newCard, setNewCard] = useState({ brand: 'Visa', type: 0, isVirtual: false, creditLimit: 0 });

    const fetchCards = async () => {
        try {
            const response = await axios.get('/api/v1/cards', {
                headers: { Authorization: `Bearer ${token}` }
            });
            setCards(response.data);
        } catch (err) {
            console.error('Error fetching cards', err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchCards();
    }, []);

    const handleIssueCard = async (e: React.FormEvent) => {
        e.preventDefault();
        setIssuing(true);
        try {
            await axios.post('/api/v1/cards/issue', newCard, {
                headers: { Authorization: `Bearer ${token}` }
            });
            fetchCards();
            setIsModalOpen(false);
        } catch (err) {
            console.error('Error issuing card', err);
        } finally {
            setIssuing(false);
        }
    };

    return (
        <div className="space-y-10">
            <div className="flex justify-between items-center">
                <div>
                    <h2 className="text-3xl font-display font-bold text-white">Meus Cartões</h2>
                    <p className="text-surface-400">Gerencie seus cartões físicos e virtuais com total segurança.</p>
                </div>
                <button
                    onClick={() => setIsModalOpen(true)}
                    className="btn-primary flex items-center gap-2"
                >
                    <Plus className="w-5 h-5" />
                    Solicitar Novo Cartão
                </button>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                <AnimatePresence>
                    {loading ? (
                        <div className="col-span-full py-20 text-center">
                            <Loader2 className="w-10 h-10 animate-spin text-brand-500 mx-auto" />
                        </div>
                    ) : cards.length === 0 ? (
                        <motion.div
                            initial={{ opacity: 0 }}
                            animate={{ opacity: 1 }}
                            className="col-span-full glass-card p-16 text-center"
                        >
                            <CreditCard className="w-16 h-16 text-surface-700 mx-auto mb-6" />
                            <h3 className="text-xl font-bold mb-2">Nenhum cartão ativo</h3>
                            <p className="text-surface-500 mb-8">Você ainda não possui cartões vinculados a esta conta.</p>
                            <button onClick={() => setIsModalOpen(true)} className="btn-secondary">Emitir agora</button>
                        </motion.div>
                    ) : (
                        cards.map((card) => (
                            <CardWidget key={card.id} card={card} />
                        ))
                    )}
                </AnimatePresence>
            </div>

            <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title="Nova Emissão de Cartão">
                <form onSubmit={handleIssueCard} className="space-y-6">
                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-surface-300">Bandeira</label>
                            <select
                                className="input-field w-full appearance-none"
                                value={newCard.brand}
                                onChange={(e) => setNewCard({ ...newCard, brand: e.target.value })}
                            >
                                <option value="Visa">Visa</option>
                                <option value="Mastercard">MasterCard</option>
                                <option value="Elo">Elo</option>
                            </select>
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-surface-300">Tipo de Uso</label>
                            <select
                                className="input-field w-full appearance-none"
                                value={newCard.type}
                                onChange={(e) => setNewCard({ ...newCard, type: parseInt(e.target.value) })}
                            >
                                <option value={0}>Débito</option>
                                <option value={1}>Crédito</option>
                            </select>
                        </div>
                    </div>

                    <div className="flex items-center gap-4 bg-surface-900/50 p-4 rounded-xl border border-white/5">
                        <input
                            type="checkbox"
                            id="virtual"
                            className="w-5 h-5 accent-brand-500"
                            checked={newCard.isVirtual}
                            onChange={(e) => setNewCard({ ...newCard, isVirtual: e.target.checked })}
                        />
                        <label htmlFor="virtual" className="text-sm">
                            Emitir como <strong>Cartão Virtual</strong> (disponível na hora)
                        </label>
                    </div>

                    {newCard.type === 1 && (
                        <div className="space-y-2 animate-in fade-in slide-in-from-top-2">
                            <label className="text-sm font-medium text-surface-300">Limite Solicitado (Sujeito a análise)</label>
                            <input
                                type="number"
                                className="input-field w-full"
                                placeholder="R$ 0,00"
                                value={newCard.creditLimit}
                                onChange={(e) => setNewCard({ ...newCard, creditLimit: parseFloat(e.target.value) })}
                            />
                        </div>
                    )}

                    <button
                        disabled={issuing}
                        className="btn-primary w-full py-4 flex items-center justify-center gap-2"
                    >
                        {issuing ? <Loader2 className="w-5 h-5 animate-spin" /> : 'Confirmar Solicitação'}
                    </button>
                </form>
            </Modal>
        </div>
    );
};

const CardWidget = ({ card }: { card: Card }) => {
    const [showNumber, setShowNumber] = useState(false);

    return (
        <motion.div
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
            className="relative group h-60"
        >
            <div className={`absolute inset-0 rounded-3xl transition-all duration-500 overflow-hidden shadow-2xl ${card.isVirtual ? 'bg-gradient-to-br from-indigo-600 to-purple-800' : 'bg-gradient-to-br from-surface-800 to-surface-950 border border-white/10'}`}>
                {/* Chip & Logo */}
                <div className="p-8 h-full flex flex-col justify-between">
                    <div className="flex justify-between items-start">
                        <div className="w-12 h-10 bg-gradient-to-tr from-amber-400 to-amber-200 rounded-lg shadow-inner opacity-80" />
                        <div className="text-right">
                            <p className="text-[10px] font-bold uppercase tracking-widest opacity-60">D.AI Platinum</p>
                            <p className="text-xl font-bold italic">{card.brand}</p>
                        </div>
                    </div>

                    <div className="space-y-2">
                        <div className="flex items-center gap-4">
                            <p className="text-xl font-mono tracking-[0.3em] text-white">
                                {showNumber ? '4012 8821 0092' : '•••• •••• ••••'} {card.lastFourDigits}
                            </p>
                            <button onClick={() => setShowNumber(!showNumber)} className="text-white/40 hover:text-white transition-colors">
                                {showNumber ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                            </button>
                        </div>
                        <div className="flex justify-between items-end">
                            <div>
                                <p className="text-[8px] uppercase opacity-50 mb-1">Titular do Cartão</p>
                                <p className="text-sm font-bold tracking-widest uppercase">{card.holderName}</p>
                            </div>
                            <div className="text-right">
                                <p className="text-[8px] uppercase opacity-50 mb-1">Validade</p>
                                <p className="text-sm font-bold">08/30</p>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Type badge */}
                <div className="absolute top-0 right-0 p-4">
                    <span className={`text-[9px] font-black uppercase tracking-tighter px-2 py-0.5 rounded ${card.isVirtual ? 'bg-white/20' : 'bg-brand-600/20 text-brand-400'}`}>
                        {card.isVirtual ? 'Virtual' : 'Físico'}
                    </span>
                </div>
            </div>

            {/* Hover Info */}
            <div className="absolute -bottom-8 left-0 right-0 flex justify-center gap-4 opacity-0 group-hover:opacity-100 transition-opacity">
                <button className="text-[10px] font-bold py-1 px-3 glass rounded-full flex items-center gap-1 hover:text-brand-400">
                    <Lock className="w-3 h-3" /> Bloquear
                </button>
                <button className="text-[10px] font-bold py-1 px-3 glass rounded-full flex items-center gap-1 hover:text-brand-400">
                    <Shield className="w-3 h-3" /> Ajustar Limite
                </button>
            </div>
        </motion.div>
    );
};

export default CardsPage;
