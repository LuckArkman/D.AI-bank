import { useState, useEffect } from 'react';
import { Routes, Route, NavLink, useNavigate } from 'react-router-dom';
import {
    LayoutDashboard,
    History,
    Zap,
    CreditCard,
    Settings,
    LogOut,
    Wallet,
    ArrowUpRight,
    ArrowDownLeft,
    Search,
    Plus,
    RefreshCcw,
    TrendingUp
} from 'lucide-react';
import { useAuthStore } from '../store/useAuthStore';
import { motion } from 'framer-motion';
import axios from 'axios';
import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';
import PixManagement from './PixManagement';
import CardsPage from './CardsPage';
import LoansPage from './LoansPage';
import InvestmentsPage from './InvestmentsPage';
import DepositModal from '../components/DepositModal';
import PixTransferModal from '../components/PixTransferModal';
import { Landmark, PieChart } from 'lucide-react';

function cn(...inputs: ClassValue[]) {
    return twMerge(clsx(inputs));
}

const Dashboard = () => {
    const { user, logout, token } = useAuthStore();
    const navigate = useNavigate();
    const [balance, setBalance] = useState({ amount: 0, currency: 'BRL' });
    const [transactions, setTransactions] = useState<any[]>([]);
    const [cards, setCards] = useState<any[]>([]);
    const [goals, setGoals] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);

    // Modals state
    const [isDepositOpen, setIsDepositOpen] = useState(false);
    const [isPixTransferOpen, setIsPixTransferOpen] = useState(false);

    const fetchData = async () => {
        setLoading(true);
        try {
            const [balanceRes, transactionsRes, cardsRes, goalsRes] = await Promise.all([
                axios.get(`/api/v1/open-banking/accounts/${user?.accountId}/balance`, {
                    headers: { Authorization: `Bearer ${token}` }
                }),
                axios.get(`/api/v1/open-banking/accounts/${user?.accountId}/transactions`, {
                    headers: { Authorization: `Bearer ${token}` }
                }),
                axios.get(`/api/v1/cards`, {
                    headers: { Authorization: `Bearer ${token}` }
                }),
                axios.get(`/api/v1/investments/goals`, {
                    headers: { Authorization: `Bearer ${token}` }
                })
            ]);

            setBalance(balanceRes.data);
            setTransactions(transactionsRes.data);
            setCards(cardsRes.data);
            setGoals(goalsRes.data);

        } catch (err) {
            console.error('Error fetching dashboard data', err);
        } finally {
            setLoading(false);
        }
    };


    useEffect(() => {
        fetchData();
    }, []);

    const handleLogout = () => {
        logout();
        navigate('/');
    };

    return (
        <div className="flex min-h-screen bg-surface-950 text-surface-50">
            {/* Sidebar */}
            <aside className="w-72 glass border-r border-white/5 flex flex-col p-6 fixed h-full z-40">
                <div className="flex items-center gap-2 mb-12">
                    <Zap className="text-brand-500 w-8 h-8 fill-brand-500" />
                    <span className="text-xl font-display font-bold">D.AI Bank</span>
                </div>

                <nav className="flex-1 space-y-2">
                    <MenuLink to="/dashboard" icon={<LayoutDashboard />} label="Dashboard" end />
                    <MenuLink to="/dashboard/history" icon={<History />} label="Extrato" />
                    <MenuLink to="/dashboard/pix" icon={<Zap />} label="Área Pix" />
                    <MenuLink to="/dashboard/cards" icon={<CreditCard />} label="Meus Cartões" />
                    <MenuLink to="/dashboard/loans" icon={<Landmark />} label="Créditos" />
                    <MenuLink to="/dashboard/investments" icon={<PieChart />} label="Investimentos" />
                    <MenuLink to="/dashboard/settings" icon={<Settings />} label="Configurações" />


                </nav>

                <button
                    onClick={handleLogout}
                    className="flex items-center gap-3 text-surface-400 hover:text-red-400 p-4 rounded-xl transition-colors mt-auto"
                >
                    <LogOut className="w-5 h-5" />
                    <span className="font-medium">Sair da conta</span>
                </button>
            </aside>

            {/* Main Content */}
            <main className="flex-1 ml-72 p-10 max-w-7xl mx-auto w-full">
                <header className="flex justify-between items-start mb-10">
                    <div>
                        <h1 className="text-3xl font-display font-bold">Olá, {user?.name.split(' ')[0]} 👋</h1>
                        <p className="text-surface-400">Aqui está o resumo da sua vida financeira hoje.</p>
                    </div>

                    <div className="flex items-center gap-4">
                        <button
                            onClick={fetchData}
                            className="p-2.5 bg-surface-900 border border-white/5 rounded-xl text-surface-400 hover:text-white transition-all hover:bg-surface-800"
                            title="Sincronizar dados"
                        >
                            <RefreshCcw className={cn("w-5 h-5", loading && "animate-spin")} />
                        </button>
                        <div className="relative">
                            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-surface-500" />
                            <input
                                type="text"
                                placeholder="Pesquisar..."
                                className="bg-surface-900 border border-white/5 rounded-xl pl-10 pr-4 py-2 text-sm outline-none focus:border-brand-500 transition-all"
                            />
                        </div>
                        <div className="w-10 h-10 bg-brand-600 rounded-full flex items-center justify-center font-bold shadow-lg shadow-brand-600/20">
                            {user?.name.charAt(0)}
                        </div>
                    </div>
                </header>

                <Routes>
                    <Route path="/" element={
                        <div className="space-y-8">
                            {/* Balance & Cards */}
                            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                                <div className="lg:col-span-2 space-y-8">
                                    {/* Balance Card */}
                                    <div className="bg-brand-600 rounded-3xl p-8 relative overflow-hidden shadow-2xl shadow-brand-600/30">
                                        <div className="absolute top-0 right-0 p-8 opacity-20">
                                            <Wallet className="w-32 h-32" />
                                        </div>
                                        <div className="relative z-10">
                                            <p className="text-brand-100 font-medium mb-1">Saldo Total</p>
                                            <h2 className="text-5xl font-display font-bold mb-8">
                                                {new Intl.NumberFormat('pt-BR', { style: 'currency', currency: balance.currency }).format(balance.amount)}
                                            </h2>
                                            <div className="flex gap-4">
                                                <button
                                                    onClick={() => setIsPixTransferOpen(true)}
                                                    className="bg-white text-brand-600 hover:bg-brand-50 px-8 py-3 rounded-xl text-sm font-bold transition-all shadow-xl shadow-brand-900/20 active:scale-95"
                                                >
                                                    Transferir Pix
                                                </button>
                                                <button
                                                    onClick={() => setIsDepositOpen(true)}
                                                    className="bg-white/10 hover:bg-white/20 px-8 py-3 rounded-xl text-sm font-bold transition-all border border-white/10 active:scale-95"
                                                >
                                                    Depositar
                                                </button>
                                            </div>
                                        </div>
                                    </div>

                                    {/* Quick Actions */}
                                    <div className="grid grid-cols-4 gap-4">
                                        <QuickAction onClick={() => navigate('/dashboard/pix')} icon={<Zap />} label="Pix" color="bg-brand-500" />
                                        <QuickAction onClick={() => setIsPixTransferOpen(true)} icon={<ArrowUpRight />} label="Pagar" color="bg-orange-500" />
                                        <QuickAction onClick={() => setIsDepositOpen(true)} icon={<Plus />} label="Adicionar" color="bg-emerald-500" />
                                        <QuickAction onClick={() => navigate('/dashboard/investments')} icon={<TrendingUp />} label="Investir" color="bg-indigo-500" />
                                    </div>


                                    {/* Transactions List */}
                                    <div className="glass-card !p-0 overflow-hidden">
                                        <div className="p-6 border-b border-white/5 flex justify-between items-center">
                                            <h3 className="text-xl font-bold">Últimas Atividades</h3>
                                            <NavLink to="/dashboard/history" className="text-brand-400 text-sm font-medium hover:underline">Ver tudo</NavLink>
                                        </div>
                                        <div className="divide-y divide-white/5">
                                            {loading ? (
                                                <div className="p-10 text-center text-surface-500">Caregando transações...</div>
                                            ) : transactions.length === 0 ? (
                                                <div className="p-10 text-center text-surface-500">Nenhuma transação encontrada.</div>
                                            ) : (
                                                transactions.slice(0, 5).map((tx) => (
                                                    <div key={tx.id} className="p-5 flex justify-between items-center hover:bg-white/5 transition-colors group">
                                                        <div className="flex items-center gap-4">
                                                            <div className={cn(
                                                                "w-12 h-12 rounded-2xl flex items-center justify-center transition-all group-hover:scale-110",
                                                                tx.type.includes('CREDIT') || tx.type.includes('RECEIVED') || tx.type.includes('CREATED') || tx.type.includes('DEPOSIT')
                                                                    ? "bg-emerald-500/10 text-emerald-500"
                                                                    : "bg-red-500/10 text-red-500"
                                                            )}>
                                                                {tx.type.includes('CREDIT') || tx.type.includes('RECEIVED') || tx.type.includes('CREATED') || tx.type.includes('DEPOSIT')
                                                                    ? <ArrowDownLeft className="w-6 h-6" />
                                                                    : <ArrowUpRight className="w-6 h-6" />
                                                                }
                                                            </div>
                                                            <div>
                                                                <p className="font-bold">{tx.type.replace('_', ' ')}</p>
                                                                <p className="text-xs text-surface-500">{new Date(tx.timestamp).toLocaleDateString()} • {new Date(tx.timestamp).toLocaleTimeString()}</p>
                                                            </div>
                                                        </div>
                                                        <p className={cn(
                                                            "text-lg font-bold",
                                                            tx.type.includes('CREDIT') || tx.type.includes('RECEIVED') || tx.type.includes('CREATED') || tx.type.includes('DEPOSIT')
                                                                ? "text-emerald-500"
                                                                : "text-white"
                                                        )}>
                                                            {tx.type.includes('CREDIT') || tx.type.includes('RECEIVED') || tx.type.includes('CREATED') || tx.type.includes('DEPOSIT') ? '+' : '-'}
                                                            {new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(tx.amount)}
                                                        </p>
                                                    </div>
                                                ))
                                            )}
                                        </div>
                                    </div>
                                </div>

                                {/* Cards / Right Sidebar info */}
                                <div className="space-y-8">
                                    {cards.length > 0 ? (
                                        <div className={cn(
                                            "glass-card h-64 relative overflow-hidden flex flex-col justify-between group cursor-pointer transition-all hover:scale-[1.02]",
                                            cards[0].isVirtual ? "bg-gradient-to-br from-indigo-600 to-violet-800 border-none" : ""
                                        )} onClick={() => navigate('/dashboard/cards')}>
                                            <div className="absolute top-0 right-0 p-6 opacity-30 group-hover:scale-110 transition-transform">
                                                <CreditCard className="w-24 h-24" />
                                            </div>
                                            <div className="relative z-10">
                                                <div className="flex justify-between items-start mb-6">
                                                    <p className="text-sm font-medium text-white/60">
                                                        {cards[0].isVirtual ? 'Cartão Virtual' : 'Cartão Principal'}
                                                    </p>
                                                    {cards[0].status === 2 && (
                                                        <span className="bg-red-500 text-[8px] font-black px-2 py-0.5 rounded-full">BLOQUEADO</span>
                                                    )}
                                                </div>
                                                <p className="text-xl font-mono tracking-[0.2em] mb-2 text-white">•••• •••• •••• {cards[0].lastFourDigits}</p>
                                                <p className="text-xs uppercase text-white/40">Exp: {new Date(cards[0].expiryDate).toLocaleDateString(undefined, { month: '2-digit', year: '2-digit' })}</p>
                                            </div>
                                            <div className="flex justify-between items-end relative z-10">
                                                <div>
                                                    <p className="text-xs text-white/40">Titular</p>
                                                    <p className="font-bold uppercase tracking-wider text-sm">{cards[0].holderName}</p>
                                                </div>
                                                <div className="w-12 h-8 bg-white/10 rounded-md border border-white/10 backdrop-blur-md" />
                                            </div>
                                        </div>
                                    ) : (
                                        <div
                                            onClick={() => navigate('/dashboard/cards')}
                                            className="glass-card h-64 border-dashed border-white/10 flex flex-col items-center justify-center p-8 text-center group cursor-pointer hover:border-brand-500/50 transition-all"
                                        >
                                            <div className="w-16 h-16 bg-white/5 rounded-2xl flex items-center justify-center mb-4 group-hover:scale-110 transition-transform">
                                                <Plus className="w-8 h-8 text-surface-500 group-hover:text-brand-400" />
                                            </div>
                                            <p className="font-bold mb-1">Nenhum cartão</p>
                                            <p className="text-xs text-surface-500">Toque para solicitar seu primeiro cartão físico ou virtual.</p>
                                        </div>
                                    )}


                                    <div className="glass-card">
                                        <h4 className="font-bold mb-4 flex justify-between items-center">
                                            Metas de Economia
                                            <span className="text-[10px] text-brand-400 cursor-pointer hover:underline" onClick={() => navigate('/dashboard/investments')}>Ver todas</span>
                                        </h4>
                                        <div className="space-y-6">
                                            {goals.length === 0 ? (
                                                <div className="text-center py-4">
                                                    <p className="text-xs text-surface-500 italic">Nenhuma meta ativa.</p>
                                                </div>
                                            ) : (
                                                goals.slice(0, 3).map(goal => (
                                                    <GoalItem
                                                        key={goal.id}
                                                        label={goal.name}
                                                        progress={Math.round((goal.currentAmount / goal.targetAmount) * 100)}
                                                        color={goal.color}
                                                    />
                                                ))
                                            )}
                                        </div>
                                    </div>


                                    <div className="bg-gradient-to-br from-indigo-600 to-violet-700 p-6 rounded-2xl shadow-xl">
                                        <h4 className="font-bold mb-2">Invite Friends</h4>
                                        <p className="text-sm text-indigo-100 mb-4 leading-relaxed">Refer a friend and get R$ 50,00 off your next invoice.</p>
                                        <button className="w-full bg-white text-indigo-700 py-3 rounded-xl font-bold text-sm">Convidar agora</button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    } />

                    <Route path="/history" element={
                        <div className="glass-card p-0 overflow-hidden">
                            <div className="p-8 border-b border-white/5">
                                <h2 className="text-2xl font-bold">Extrato Completo</h2>
                            </div>
                            <div className="divide-y divide-white/5">
                                {transactions.map((tx) => (
                                    <div key={tx.id} className="p-6 flex justify-between items-center">
                                        <div className="flex items-center gap-4">
                                            <p className="text-sm text-surface-500">{new Date(tx.timestamp).toLocaleDateString()}</p>
                                            <p className="font-bold">{tx.type}</p>
                                        </div>
                                        <p className="font-bold">
                                            {new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(tx.amount)}
                                        </p>
                                    </div>
                                ))}
                            </div>
                        </div>
                    } />

                    <Route path="/pix" element={<PixManagement />} />
                    <Route path="/cards" element={<CardsPage />} />
                    <Route path="/loans" element={<LoansPage />} />
                    <Route path="/investments" element={<InvestmentsPage />} />

                    <Route path="*" element={
                        <div className="p-20 text-center text-surface-400 flex flex-col items-center gap-4">
                            <Settings className="w-12 h-12 animate-spin-slow text-surface-700" />
                            <p className="text-xl font-medium">Recurso em desenvolvimento</p>
                            <button onClick={() => navigate('/dashboard')} className="btn-secondary">Voltar ao Início</button>
                        </div>
                    } />

                </Routes>
            </main>

            <DepositModal
                isOpen={isDepositOpen}
                onClose={() => setIsDepositOpen(false)}
                onSuccess={fetchData}
            />
            <PixTransferModal
                isOpen={isPixTransferOpen}
                onClose={() => setIsPixTransferOpen(false)}
                onSuccess={fetchData}
            />
        </div>
    );
};

const MenuLink = ({ to, icon, label, end = false }: { to: string, icon: React.ReactNode, label: string, end?: boolean }) => (
    <NavLink
        to={to}
        end={end}
        className={({ isActive }) => cn(
            "flex items-center gap-3 p-4 rounded-xl transition-all duration-300 group",
            isActive
                ? "bg-brand-600/10 text-brand-400 shadow-inner border border-brand-600/10"
                : "text-surface-400 hover:bg-white/5 hover:text-white"
        )}
    >
        {({ isActive }) => (
            <>
                <div className={cn(
                    "w-5 h-5 transition-transform group-hover:scale-110",
                    isActive ? "text-brand-400" : "text-surface-400"
                )}>
                    {icon}
                </div>
                <span className="font-medium">{label}</span>
            </>
        )}
    </NavLink>
);

const QuickAction = ({ icon, label, color, onClick }: { icon: React.ReactNode, label: string, color: string, onClick: () => void }) => (
    <button
        onClick={onClick}
        className="flex flex-col items-center gap-3 group"
    >
        <div className={cn(
            "w-16 h-16 rounded-2xl flex items-center justify-center text-white shadow-lg transition-all group-hover:-translate-y-1 active:scale-95",
            color
        )}>
            <div className="w-6 h-6">{icon}</div>
        </div>
        <span className="text-xs font-medium text-surface-400 group-hover:text-white transition-colors">{label}</span>
    </button>
);

const GoalItem = ({ label, progress, color }: { label: string, progress: number, color: string }) => (
    <div className="space-y-2">
        <div className="flex justify-between text-sm">
            <span className="text-surface-300">{label}</span>
            <span className="font-bold">{progress}%</span>
        </div>
        <div className="h-2 bg-surface-900 rounded-full overflow-hidden">
            <motion.div
                initial={{ width: 0 }}
                animate={{ width: `${progress}%` }}
                transition={{ duration: 1, ease: 'easeOut' }}
                className={cn("h-full rounded-full", color)}
            />
        </div>
    </div>
);

export default Dashboard;
