import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Mail, Loader2, KeyRound, AlertCircle, CheckCircle2, ArrowLeft } from 'lucide-react';
import { requestPasswordReset, resetPasswordWithCode } from '../api/apiClient';

export default function ForgotPasswordPage() {
    const [step, setStep] = useState('email');
    const [email, setEmail] = useState('');
    const [code, setCode] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [newPasswordAgain, setNewPasswordAgain] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [message, setMessage] = useState({ type: '', text: '' });
    const navigate = useNavigate();

    const handleRequestCode = async (e) => {
        e.preventDefault();
        setMessage({ type: '', text: '' });
        const trimmed = email.trim();
        if (!trimmed) {
            setMessage({ type: 'error', text: 'E-posta adresinizi girin.' });
            return;
        }
        setIsLoading(true);
        try {
            await requestPasswordReset(trimmed);
            setMessage({ type: 'success', text: 'E-posta gönderildi. Gelen 6 haneli kodu girin.' });
            setStep('reset');
        } catch (err) {
            const msg = err.response?.data?.message ?? err.response?.data ?? err.message ?? 'Kod gönderilemedi.';
            setMessage({ type: 'error', text: typeof msg === 'string' ? msg : 'Bir hata oluştu.' });
        } finally {
            setIsLoading(false);
        }
    };

    const handleResetPassword = async (e) => {
        e.preventDefault();
        setMessage({ type: '', text: '' });
        if (!code.trim()) {
            setMessage({ type: 'error', text: 'Doğrulama kodunu girin.' });
            return;
        }
        if (newPassword.length < 8) {
            setMessage({ type: 'error', text: 'Yeni şifre en az 8 karakter olmalı.' });
            return;
        }
        if (newPassword !== newPasswordAgain) {
            setMessage({ type: 'error', text: 'Şifreler eşleşmiyor.' });
            return;
        }
        setIsLoading(true);
        try {
            await resetPasswordWithCode({
                email: email.trim(),
                code: code.trim(),
                newPassword,
                newPasswordAgain,
            });
            setMessage({ type: 'success', text: 'Şifreniz güncellendi. Giriş sayfasına yönlendiriliyorsunuz...' });
            setTimeout(() => navigate('/login'), 2000);
        } catch (err) {
            const msg = err.response?.data?.message ?? err.response?.data ?? err.message ?? 'Şifre sıfırlanamadı.';
            setMessage({ type: 'error', text: typeof msg === 'string' ? msg : 'Kod geçersiz veya süresi dolmuş olabilir.' });
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="flex h-screen w-full items-center justify-center bg-[#0f111a] p-4">
            <div className="w-full max-w-sm rounded-none bg-[#141722] border border-[#1f2334] px-8 py-10">
                <Link
                    to="/login"
                    className="inline-flex items-center gap-2 text-sm text-gray-400 hover:text-white mb-6 transition-colors"
                >
                    <ArrowLeft size={16} /> Girişe dön
                </Link>

                <div className="flex items-center gap-3 mb-6">
                    <div className="w-12 h-12 rounded-none bg-blue-600/20 flex items-center justify-center">
                        <KeyRound size={24} className="text-blue-500" />
                    </div>
                    <div>
                        <h1 className="text-xl font-bold text-white tracking-tight">Şifremi Unuttum</h1>
                        <p className="text-gray-500 text-sm mt-0.5">
                            {step === 'email' ? 'E-posta adresinize sıfırlama kodu göndereceğiz.' : 'Kodu ve yeni şifrenizi girin.'}
                        </p>
                    </div>
                </div>

                {message.text && (
                    <div
                        className={`flex items-center gap-2 p-3 rounded-none text-sm font-medium mb-4 ${
                            message.type === 'success' ? 'bg-green-500/10 text-green-400 border border-green-500/20' : 'bg-red-500/10 text-red-400 border border-red-500/20'
                        }`}
                    >
                        {message.type === 'success' ? <CheckCircle2 size={18} /> : <AlertCircle size={18} />}
                        {message.text}
                    </div>
                )}

                {step === 'email' ? (
                    <form onSubmit={handleRequestCode} className="space-y-4">
                        <div>
                            <label className="block text-xs font-bold text-gray-400 uppercase tracking-wider mb-1.5">E-posta</label>
                            <div className="relative">
                                <Mail size={18} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" />
                                <input
                                    type="email"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    placeholder="kayitli@eposta.com"
                                    className="w-full pl-10 pr-4 py-3 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-white placeholder-gray-500 outline-none focus:border-blue-500/50"
                                    required
                                />
                            </div>
                        </div>
                        <button
                            type="submit"
                            disabled={isLoading}
                            className="w-full py-3 bg-blue-600 hover:bg-blue-500 disabled:opacity-50 text-white rounded-none font-bold flex items-center justify-center gap-2"
                        >
                            {isLoading ? <Loader2 size={20} className="animate-spin" /> : null}
                            Kod Gönder
                        </button>
                    </form>
                ) : (
                    <form onSubmit={handleResetPassword} className="space-y-4">
                        <div>
                            <label className="block text-xs font-bold text-gray-400 uppercase tracking-wider mb-1.5">E-posta</label>
                            <input
                                type="email"
                                value={email}
                                readOnly
                                className="w-full px-4 py-2.5 bg-[#1a1e2d]/50 border border-[#1f2334] rounded-none text-gray-400 cursor-not-allowed"
                            />
                        </div>
                        <div>
                            <label className="block text-xs font-bold text-gray-400 uppercase tracking-wider mb-1.5">6 haneli kod</label>
                            <input
                                type="text"
                                value={code}
                                onChange={(e) => setCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                                placeholder="000000"
                                maxLength={6}
                                className="w-full px-4 py-3 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-white placeholder-gray-500 outline-none focus:border-blue-500/50 text-center text-lg tracking-widest"
                            />
                        </div>
                        <div>
                            <label className="block text-xs font-bold text-gray-400 uppercase tracking-wider mb-1.5">Yeni şifre (en az 8 karakter)</label>
                            <input
                                type="password"
                                value={newPassword}
                                onChange={(e) => setNewPassword(e.target.value)}
                                placeholder="••••••••"
                                className="w-full px-4 py-3 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-white placeholder-gray-500 outline-none focus:border-blue-500/50"
                                required
                            />
                        </div>
                        <div>
                            <label className="block text-xs font-bold text-gray-400 uppercase tracking-wider mb-1.5">Yeni şifre (tekrar)</label>
                            <input
                                type="password"
                                value={newPasswordAgain}
                                onChange={(e) => setNewPasswordAgain(e.target.value)}
                                placeholder="••••••••"
                                className="w-full px-4 py-3 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-white placeholder-gray-500 outline-none focus:border-blue-500/50"
                                required
                            />
                        </div>
                        <div className="flex gap-2">
                            <button
                                type="button"
                                onClick={() => setStep('email')}
                                className="px-4 py-3 rounded-none border border-[#1f2334] text-gray-400 hover:bg-[#1a1e2d] hover:text-white transition-colors font-medium"
                            >
                                Geri
                            </button>
                            <button
                                type="submit"
                                disabled={isLoading}
                                className="flex-1 py-3 bg-blue-600 hover:bg-blue-500 disabled:opacity-50 text-white rounded-none font-bold flex items-center justify-center gap-2"
                            >
                                {isLoading ? <Loader2 size={20} className="animate-spin" /> : null}
                                Şifreyi Sıfırla
                            </button>
                        </div>
                    </form>
                )}

                <p className="mt-6 text-center text-gray-500 text-sm">
                    <Link to="/login" className="text-blue-500 hover:text-blue-400 font-medium">Giriş sayfasına dön</Link>
                </p>
            </div>
        </div>
    );
}
