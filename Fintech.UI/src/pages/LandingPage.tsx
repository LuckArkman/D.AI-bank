import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { CreditCard, Shield, Zap, ArrowRight, Smartphone, Globe, BarChart3 } from 'lucide-react';

const LandingPage = () => {
    return (
        <div className="min-h-screen bg-gradient-mesh font-sans">
            {/* Navbar */}
            <nav className="fixed top-0 w-full z-50 glass border-b border-white/5">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                    <div className="flex justify-between h-20 items-center">
                        <div className="flex items-center gap-2">
                            <div className="w-10 h-10 bg-brand-600 rounded-xl flex items-center justify-center shadow-lg shadow-brand-600/30">
                                <Zap className="text-white w-6 h-6 fill-white" />
                            </div>
                            <span className="text-2xl font-display font-bold bg-clip-text text-transparent bg-gradient-to-r from-white to-brand-400">
                                D.AI Bank
                            </span>
                        </div>

                        <div className="hidden md:flex items-center gap-8 text-surface-400 font-medium">
                            <a href="#features" className="hover:text-white transition-colors">Funcionalidades</a>
                            <a href="#security" className="hover:text-white transition-colors">Segurança</a>
                            <a href="#benefits" className="hover:text-white transition-colors">Benefícios</a>
                        </div>

                        <div className="flex items-center gap-4">
                            <Link to="/login" className="text-surface-300 hover:text-white font-medium px-4">
                                Entrar
                            </Link>
                            <Link to="/register" className="btn-primary py-2.5 px-6">
                                Abrir Conta
                            </Link>
                        </div>
                    </div>
                </div>
            </nav>

            <main>
                {/* Hero Section */}
                <section className="pt-40 pb-20 px-4">
                    <div className="max-w-7xl mx-auto text-center">
                        <motion.div
                            initial={{ opacity: 0, y: 20 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ duration: 0.6 }}
                        >
                            <h1 className="text-5xl md:text-7xl font-display font-bold tracking-tight mb-6">
                                O futuro das finanças <br />
                                <span className="text-brand-500">inteligentes chegou.</span>
                            </h1>
                            <p className="text-xl text-surface-400 max-w-2xl mx-auto mb-10 leading-relaxed">
                                Bem-vindo ao D.AI Bank. A plataforma financeira que une inteligência artificial, segurança de nível bancário e uma experiência digital sem precedentes.
                            </p>

                            <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
                                <Link to="/register" className="btn-primary text-lg w-full sm:w-auto flex items-center justify-center gap-2 group">
                                    Começar agora
                                    <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
                                </Link>
                                <Link to="/features" className="btn-secondary text-lg w-full sm:w-auto">
                                    Conheça os recursos
                                </Link>
                            </div>
                        </motion.div>

                        {/* Hero Image / Mockup Placeholder */}
                        <motion.div
                            initial={{ opacity: 0, scale: 0.95 }}
                            animate={{ opacity: 1, scale: 1 }}
                            transition={{ delay: 0.3, duration: 0.8 }}
                            className="mt-20 relative px-4"
                        >
                            <div className="max-w-5xl mx-auto glass rounded-3xl p-4 shadow-2xl relative overflow-hidden group">
                                <div className="absolute inset-0 bg-gradient-to-tr from-brand-600/20 to-transparent pointer-events-none" />
                                <div className="aspect-[16/9] bg-surface-900 rounded-2xl flex items-center justify-center">
                                    <BarChart3 className="w-40 h-40 text-brand-500/20" />
                                    <div className="absolute bottom-10 right-10 flex flex-col gap-4">
                                        <div className="glass p-4 rounded-xl flex items-center gap-4 shadow-xl">
                                            <div className="w-10 h-10 bg-green-500/20 rounded-full flex items-center justify-center">
                                                <Zap className="text-green-500 w-5 h-5" />
                                            </div>
                                            <div className="text-left">
                                                <p className="text-sm text-surface-400">Saldo Disponível</p>
                                                <p className="text-xl font-bold">R$ 48.250,00</p>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </motion.div>
                    </div>
                </section>

                {/* Features Grid */}
                <section id="features" className="py-24 px-4 bg-surface-950/50">
                    <div className="max-w-7xl mx-auto">
                        <div className="text-center mb-16">
                            <h2 className="text-3xl md:text-5xl font-display font-bold mb-4">Tudo o que você precisa</h2>
                            <p className="text-surface-400 text-lg">Um ecossistema completo para sua vida financeira.</p>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
                            <FeatureCard
                                icon={<CreditCard className="w-8 h-8 text-brand-400" />}
                                title="Cartões Personalizados"
                                desc="Cartões de crédito e débito sem anuidade, com controle total pelo app."
                            />
                            <FeatureCard
                                icon={<Shield className="w-8 h-8 text-blue-400" />}
                                title="Segurança Máxima"
                                desc="Autenticação biométrica, MFA e monitoramento de fraudes com IA."
                            />
                            <FeatureCard
                                icon={<Zap className="w-8 h-8 text-amber-400" />}
                                title="Pix Instantâneo"
                                desc="Envie e receba dinheiro em segundos, com gestão simplificada de chaves."
                            />
                            <FeatureCard
                                icon={<Smartphone className="w-8 h-8 text-emerald-400" />}
                                title="Experiência Mobile"
                                desc="Navegação fluida e intuitiva, pensada para o seu smartphone."
                            />
                            <FeatureCard
                                icon={<Globe className="w-8 h-8 text-pink-400" />}
                                title="Sempre Conectado"
                                desc="Acesse sua conta de qualquer lugar do mundo com total suporte."
                            />
                            <FeatureCard
                                icon={<BarChart3 className="w-8 h-8 text-cyan-400" />}
                                title="Gestão de Gastos"
                                desc="Gráficos inteligentes que ajudam você a entender para onde vai seu dinheiro."
                            />
                        </div>
                    </div>
                </section>
            </main>

            <footer className="py-12 border-t border-white/5 bg-surface-950 px-4">
                <div className="max-w-7xl mx-auto flex flex-col md:flex-row justify-between items-center gap-8">
                    <div className="flex items-center gap-2">
                        <Zap className="text-brand-500 w-6 h-6" />
                        <span className="text-xl font-bold">D.AI Bank</span>
                    </div>
                    <div className="text-surface-500 text-sm">
                        © 2026 D.AI Bank Financial. Todos os direitos reservados.
                    </div>
                    <div className="flex gap-6 text-surface-400 text-sm">
                        <a href="#" className="hover:text-white">Privacidade</a>
                        <a href="#" className="hover:text-white">Termos</a>
                        <a href="#" className="hover:text-white">Contato</a>
                    </div>
                </div>
            </footer>
        </div>
    );
};

const FeatureCard = ({ icon, title, desc }: { icon: React.ReactNode, title: string, desc: string }) => (
    <motion.div
        whileHover={{ y: -5 }}
        className="glass-card flex flex-col items-start gap-4"
    >
        <div className="w-14 h-14 bg-surface-900 rounded-2xl flex items-center justify-center border border-white/5 shadow-inner">
            {icon}
        </div>
        <h3 className="text-xl font-bold">{title}</h3>
        <p className="text-surface-400 leading-relaxed text-sm">
            {desc}
        </p>
    </motion.div>
);

export default LandingPage;
