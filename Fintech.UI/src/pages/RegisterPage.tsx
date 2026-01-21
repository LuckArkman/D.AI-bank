import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Zap, Mail, Lock, Loader2, ArrowLeft, User, Banknote } from 'lucide-react';
import { useAuthStore } from '../store/useAuthStore';
import axios from 'axios';

const RegisterPage = () => {
    const [formData, setFormData] = useState({
        name: '',
        email: '',
        password: '',
        initialDeposit: 0,
        profileType: 0, // StandardIndividual
    });
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const navigate = useNavigate();
    const { login } = useAuthStore();

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: name === 'initialDeposit' ? parseFloat(value) || 0 : (name === 'profileType' ? parseInt(value) : value)
        }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError('');

        try {
            const response = await axios.post('/api/v1/auth/register', {
                name: formData.name,
                email: formData.email,
                password: formData.password,
                initialDeposit: formData.initialDeposit,
                profileType: formData.profileType,
            });

            const { token, name, email, accountId } = response.data;
            login({ id: accountId, name, email, accountId }, token);
            navigate('/dashboard');
        } catch (err: any) {
            setError(err.response?.data?.error || 'Não foi possível criar sua conta. Tente novamente mais tarde.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen bg-gradient-mesh flex items-center justify-center p-4 py-20">
            <Link
                to="/"
                className="fixed top-8 left-8 text-surface-400 hover:text-white flex items-center gap-2 transition-colors z-50"
            >
                <ArrowLeft className="w-5 h-5" />
                Home
            </Link>

            <motion.div
                initial={{ opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                className="w-full max-w-xl"
            >
                <div className="glass-card p-8 md:p-12">
                    <div className="flex flex-col items-center mb-10 text-center">
                        <div className="w-16 h-16 bg-brand-600 rounded-2xl flex items-center justify-center shadow-2xl shadow-brand-600/30 mb-6 -rotate-3">
                            <Zap className="text-white w-10 h-10 fill-white" />
                        </div>
                        <h1 className="text-4xl font-display font-bold">Junte-se ao Futuro</h1>
                        <p className="text-surface-400 mt-2">Crie sua conta no D.AI Bank em menos de 2 minutos.</p>
                    </div>

                    <form onSubmit={handleSubmit} className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div className="space-y-2 col-span-2 md:col-span-1">
                            <label className="text-sm font-medium text-surface-300 ml-1">Nome Completo</label>
                            <div className="relative">
                                <User className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-surface-600" />
                                <input
                                    name="name"
                                    type="text"
                                    required
                                    value={formData.name}
                                    onChange={handleInputChange}
                                    className="input-field w-full pl-12"
                                    placeholder="Seu nome"
                                />
                            </div>
                        </div>

                        <div className="space-y-2 col-span-2 md:col-span-1">
                            <label className="text-sm font-medium text-surface-300 ml-1">E-mail</label>
                            <div className="relative">
                                <Mail className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-surface-600" />
                                <input
                                    name="email"
                                    type="email"
                                    required
                                    value={formData.email}
                                    onChange={handleInputChange}
                                    className="input-field w-full pl-12"
                                    placeholder="name@example.com"
                                />
                            </div>
                        </div>

                        <div className="space-y-2 col-span-2 md:col-span-1">
                            <label className="text-sm font-medium text-surface-300 ml-1">Senha</label>
                            <div className="relative">
                                <Lock className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-surface-600" />
                                <input
                                    name="password"
                                    type="password"
                                    required
                                    value={formData.password}
                                    onChange={handleInputChange}
                                    className="input-field w-full pl-12"
                                    placeholder="Mín. 6 caracteres"
                                />
                            </div>
                        </div>

                        <div className="space-y-2 col-span-2 md:col-span-1">
                            <label className="text-sm font-medium text-surface-300 ml-1">Perfil de Conta</label>
                            <select
                                name="profileType"
                                value={formData.profileType}
                                onChange={handleInputChange}
                                className="input-field w-full appearance-none bg-surface-900"
                            >
                                <option value={0}>Pessoa Física (Padrão)</option>
                                <option value={1}>Pessoa Física (Premium)</option>
                                <option value={2}>Empresarial (PJ)</option>
                                <option value={3}>Microempreendedor (MEI)</option>
                                <option value={4}>Conta Salário</option>
                                <option value={5}>Conta de Pagamento</option>
                            </select>
                        </div>

                        <div className="space-y-2 col-span-2 md:col-span-1">
                            <label className="text-sm font-medium text-surface-300 ml-1">Depósito Inicial (Opcional)</label>
                            <div className="relative">
                                <Banknote className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-surface-600" />
                                <input
                                    name="initialDeposit"
                                    type="number"
                                    min="0"
                                    step="0.01"
                                    value={formData.initialDeposit}
                                    onChange={handleInputChange}
                                    className="input-field w-full pl-12"
                                    placeholder="R$ 0,00"
                                />
                            </div>
                        </div>

                        {error && (
                            <div className="col-span-2 bg-red-500/10 border border-red-500/20 text-red-500 p-4 rounded-xl text-sm">
                                {error}
                            </div>
                        )}

                        <button
                            disabled={loading}
                            type="submit"
                            className="btn-primary col-span-2 py-4 text-lg flex items-center justify-center gap-2 mt-4"
                        >
                            {loading ? (
                                <>
                                    <Loader2 className="w-5 h-5 animate-spin" />
                                    Processando...
                                </>
                            ) : (
                                'Criar minha conta'
                            )}
                        </button>
                    </form>

                    <div className="mt-8 text-center text-surface-500 text-sm">
                        Já tem uma conta?{' '}
                        <Link to="/login" className="text-brand-400 hover:text-brand-300 font-semibold underline underline-offset-4">
                            Acessar agora
                        </Link>
                    </div>
                </div>
            </motion.div>
        </div>
    );
};

export default RegisterPage;
