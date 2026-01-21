import { useState, useEffect } from 'react';
import axios from 'axios';
import { useAuthStore } from '../store/useAuthStore';
import {
    Activity,
    Droplets,
    Globe,
    ShieldAlert,
    Percent,
    RefreshCcw,
    Zap
} from 'lucide-react';

interface LiquidityPool {
    id: string;
    network: string;
    currencyCode: string;
    totalBalance: number;
    reservedBalance: number;
    lastUpdated: string;
}

const TreasuryPage = () => {
    const { token } = useAuthStore();
    const [pools, setPools] = useState<LiquidityPool[]>([]);
    const [loading, setLoading] = useState(true);

    const fetchPools = async () => {
        setLoading(true);
        try {
            const res = await axios.get('/api/v1/admin/liquidity', {
                headers: { Authorization: `Bearer ${token}` }
            });
            setPools(res.data);
        } catch (err) {
            console.error('Error fetching liquidity pools', err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchPools();
    }, []);

    const totalLiquidityBRL = pools
        .filter(p => p.currencyCode === 'BRL')
        .reduce((acc, p) => acc + p.totalBalance, 0);

    const totalLiquidityUSD = pools
        .filter(p => p.currencyCode === 'USD')
        .reduce((acc, p) => acc + p.totalBalance, 0);

    return (
        <div className="space-y-8 pb-12">
            <header className="flex justify-between items-center">
                <div>
                    <h2 className="text-3xl font-display font-bold">Treasury & Liquidity</h2>
                    <p className="text-surface-400">Monitor and manage global settlement pools and AI fraud scores.</p>
                </div>
                <button
                    onClick={fetchPools}
                    className="flex items-center gap-2 bg-surface-900 hover:bg-surface-800 px-4 py-2 rounded-xl transition-all border border-white/5"
                >
                    <RefreshCcw className={loading ? "animate-spin w-4 h-4" : "w-4 h-4"} />
                    <span>Sync Nodes</span>
                </button>
            </header>

            {/* Stats Overview */}
            <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
                <StatCard
                    icon={<Droplets className="text-blue-400" />}
                    label="Global Liquidity (BRL)"
                    value={new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(totalLiquidityBRL)}
                    trend="+12.5%"
                />
                <StatCard
                    icon={<Globe className="text-emerald-400" />}
                    label="Global Liquidity (USD)"
                    value={new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(totalLiquidityUSD)}
                    trend="+5.2%"
                />
                <StatCard
                    icon={<ShieldAlert className="text-orange-400" />}
                    label="AI Fraud Interceptions"
                    value="1,284"
                    subLabel="Last 24 hours"
                />
                <StatCard
                    icon={<Percent className="text-indigo-400" />}
                    label="Tax Collection (Total)"
                    value="R$ 482k"
                    trend="+8.1%"
                />
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                {/* Liquidity Pools Table */}
                <div className="lg:col-span-2 glass-card !p-0">
                    <div className="p-6 border-b border-white/5 flex justify-between items-center">
                        <h3 className="text-xl font-bold">Active Settlement Pools</h3>
                        <div className="flex gap-2">
                            <div className="bg-emerald-500/10 text-emerald-500 text-[10px] font-bold px-2 py-1 rounded">SWIFT ONLINE</div>
                            <div className="bg-emerald-500/10 text-emerald-500 text-[10px] font-bold px-2 py-1 rounded">SEPA ONLINE</div>
                        </div>
                    </div>
                    <div className="overflow-x-auto">
                        <table className="w-full text-left">
                            <thead className="bg-white/5 text-surface-400 text-xs uppercase">
                                <tr>
                                    <th className="px-6 py-4 font-medium">Network / Currency</th>
                                    <th className="px-6 py-4 font-medium">Available Liquidity</th>
                                    <th className="px-6 py-4 font-medium">Reserved (In-Flight)</th>
                                    <th className="px-6 py-4 font-medium">Usage</th>
                                    <th className="px-6 py-4 font-medium">Status</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-white/5">
                                {pools.map((pool) => (
                                    <tr key={pool.id} className="hover:bg-white/5 transition-colors">
                                        <td className="px-6 py-4">
                                            <div className="flex items-center gap-3">
                                                <div className="w-8 h-8 rounded-lg bg-surface-800 flex items-center justify-center">
                                                    <Activity className="w-4 h-4 text-brand-400" />
                                                </div>
                                                <div>
                                                    <p className="font-bold">{pool.network}</p>
                                                    <p className="text-[10px] text-surface-500">{pool.currencyCode}</p>
                                                </div>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 font-mono">
                                            {pool.totalBalance.toLocaleString()} {pool.currencyCode}
                                        </td>
                                        <td className="px-6 py-4 text-surface-400 font-mono">
                                            {pool.reservedBalance.toLocaleString()} {pool.currencyCode}
                                        </td>
                                        <td className="px-6 py-4">
                                            <div className="w-24 h-1.5 bg-surface-800 rounded-full overflow-hidden">
                                                <div
                                                    className="h-full bg-brand-500"
                                                    style={{ width: `${(pool.totalBalance / 2000000) * 100}%` }}
                                                />
                                            </div>
                                        </td>
                                        <td className="px-6 py-4">
                                            <span className="flex items-center gap-2">
                                                <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse" />
                                                <span className="text-xs font-medium text-emerald-500">Active</span>
                                            </span>
                                        </td>
                                    </tr>
                                ))}
                                {pools.length === 0 && (
                                    <tr>
                                        <td colSpan={5} className="px-6 py-10 text-center text-surface-500">
                                            No liquidity pools found. Initiate a transfer to auto-seed.
                                        </td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>
                </div>

                {/* Right Column: AI Fraud Monitoring & Quick Seed */}
                <div className="space-y-8">
                    <div className="glass-card">
                        <h3 className="font-bold mb-6 flex items-center gap-2 text-red-400">
                            <Zap className="w-4 h-4 fill-red-400" />
                            Live Fraud Monitoring
                        </h3>
                        <div className="space-y-4">
                            <FraudAlert
                                account="0x48...f2e"
                                score={0.84}
                                reason="Geolocation mismatch (NY -> Moscow)"
                                time="2m ago"
                            />
                            <FraudAlert
                                account="0x21...a1b"
                                score={0.92}
                                reason="Extreme velocity (12 TX / 1m)"
                                time="14m ago"
                            />
                            <FraudAlert
                                account="0xef...33d"
                                score={0.71}
                                reason="High volume without KYC"
                                time="1h ago"
                            />
                        </div>
                    </div>

                    <div className="bg-gradient-to-br from-brand-600 to-indigo-700 p-8 rounded-3xl shadow-xl shadow-brand-900/40">
                        <h4 className="font-bold mb-4">Treasury Action</h4>
                        <p className="text-sm text-brand-100 mb-6 leading-relaxed">
                            Seed liquidity into cross-border pools to maintain settlement uptime.
                        </p>
                        <div className="space-y-3">
                            <button className="w-full bg-white text-brand-600 py-3 rounded-xl font-bold text-sm shadow-lg hover:translate-y-[-2px] transition-all">
                                Inject R$ 1,000,000.00
                            </button>
                            <button className="w-full bg-white/10 text-white py-3 rounded-xl font-bold text-sm border border-white/10 hover:bg-white/20 transition-all">
                                External Swap Node
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

const StatCard = ({ icon, label, value, trend, subLabel }: any) => (
    <div className="glass-card flex flex-col justify-between">
        <div className="flex justify-between items-start mb-4">
            <div className="p-2.5 bg-surface-900 rounded-xl border border-white/5">
                {icon}
            </div>
            {trend && (
                <div className="bg-emerald-500/10 text-emerald-500 text-[10px] font-bold px-2 py-1 rounded">
                    {trend}
                </div>
            )}
        </div>
        <div>
            <p className="text-sm text-surface-400 mb-1">{label}</p>
            <h4 className="text-2xl font-bold font-display">{value}</h4>
            {subLabel && <p className="text-[10px] text-surface-500 mt-1">{subLabel}</p>}
        </div>
    </div>
);

const FraudAlert = ({ account, score, reason, time }: any) => (
    <div className="p-4 bg-surface-950/50 border border-white/5 rounded-xl flex flex-col gap-2">
        <div className="flex justify-between items-center">
            <span className="text-xs font-mono text-surface-500 uppercase">{account}</span>
            <span className="text-xs font-bold text-red-400">{(score * 100).toFixed(0)}% RISK</span>
        </div>
        <p className="text-xs font-medium">{reason}</p>
        <div className="flex justify-between items-center">
            <span className="text-[10px] text-surface-600 uppercase font-bold">STATUS: INTERCEPTED</span>
            <span className="text-[10px] text-surface-600">{time}</span>
        </div>
    </div>
);

export default TreasuryPage;
