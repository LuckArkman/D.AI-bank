import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Zap, Mail, Lock, Loader2, ArrowLeft } from 'lucide-react';
import { useAuthStore } from '../store/useAuthStore';
import axios from 'axios';

const LoginPage = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const navigate = useNavigate();
    const { login } = useAuthStore();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError('');

        try {
            // Endpoint real conforme Program.cs do backend
            const response = await axios.post('http://localhost:5222/api/v1/auth/login', {
                email,
                password,
            });

            const { token, name, email: userEmail, accountId } = response.data;
            login({ id: accountId, name, email: userEmail, accountId }, token);
            navigate('/dashboard');
        } catch (err: any) {
            setError(err.response?.data?.error || 'Falha na autenticação. Verifique seu email e senha.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen bg-gradient-mesh flex items-center justify-center p-4">
            <Link
                to="/"
                className="fixed top-8 left-8 text-surface-400 hover:text-white flex items-center gap-2 transition-colors"
            >
                <ArrowLeft className="w-5 h-5" />
                Voltar para a home
            </Link>

            <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                className="w-full max-w-md"
            >
                <div className="glass-card p-8 md:p-10">
                    <div className="flex flex-col items-center mb-10">
                        <div className="w-16 h-16 bg-brand-600 rounded-2xl flex items-center justify-center shadow-2xl shadow-brand-600/30 mb-6 rotate-3">
                            <Zap className="text-white w-10 h-10 fill-white" />
                        </div>
                        <h1 className="text-3xl font-display font-bold text-center">Acessar Conta</h1>
                        <p className="text-surface-400 text-center mt-2">Bem-vindo de volta ao D.AI Bank</p>
                    </div>

                    <form onSubmit={handleSubmit} className="space-y-6">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-surface-300 ml-1">E-mail</label>
                            <div className="relative">
                                <Mail className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-surface-600" />
                                <input
                                    type="email"
                                    required
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    className="input-field w-full pl-12"
                                    placeholder="name@example.com"
                                />
                            </div>
                        </div>

                        <div className="space-y-2">
                            <label className="text-sm font-medium text-surface-300 ml-1">Senha</label>
                            <div className="relative">
                                <Lock className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-surface-600" />
                                <input
                                    type="password"
                                    required
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    className="input-field w-full pl-12"
                                    placeholder="••••••••"
                                />
                            </div>
                        </div>

                        {error && (
                            <div className="bg-red-500/10 border border-red-500/20 text-red-500 p-4 rounded-xl text-sm">
                                {error}
                            </div>
                        )}

                        <button
                            disabled={loading}
                            type="submit"
                            className="btn-primary w-full py-4 text-lg flex items-center justify-center gap-2"
                        >
                            {loading ? (
                                <>
                                    <Loader2 className="w-5 h-5 animate-spin" />
                                    Autenticando...
                                </>
                            ) : (
                                'Entrar'
                            )}
                        </button>
                    </form>

                    <div className="mt-8 text-center text-surface-500 text-sm">
                        Ainda não tem conta?{' '}
                        <Link to="/register" className="text-brand-400 hover:text-brand-300 font-semibold underline underline-offset-4">
                            Crie uma agora
                        </Link>
                    </div>
                </div>
            </motion.div>
        </div>
    );
};

export default LoginPage;
