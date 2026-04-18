import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Gamepad2, Loader2, AlertCircle, CheckCircle2, Mail } from 'lucide-react';
import { registerUser, verifyEmail } from '../api/apiClient';
import { useI18n } from '../i18n/useI18n';

export default function RegisterPage() {
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [verificationCode, setVerificationCode] = useState('');
    const [step, setStep] = useState('register');
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');

    const navigate = useNavigate();
    const { t } = useI18n();

    const handleRegister = async (e) => {
        e.preventDefault();
        setError('');
        setIsLoading(true);
        try {
            const res = await registerUser({ username, email, password });
            if (res?.requireVerification) {
                setStep('verify');
            } else {
                setStep('success');
                setTimeout(() => navigate('/login'), 2000);
            }
        } catch (err) {
            const message = err.response?.data?.message ?? err.response?.data ?? err.message ?? t('register.errRegister');
            setError(typeof message === 'string' ? message : t('common.genericError'));
        } finally {
            setIsLoading(false);
        }
    };

    const handleVerify = async (e) => {
        e.preventDefault();
        setError('');
        if (!verificationCode.trim() || verificationCode.length !== 6) {
            setError(t('register.errVerifyCode'));
            return;
        }
        setIsLoading(true);
        try {
            await verifyEmail(email.trim(), verificationCode.trim());
            setStep('success');
            setTimeout(() => navigate('/login'), 2000);
        } catch (err) {
            const message = err.response?.data?.message ?? err.response?.data ?? err.message ?? t('register.errVerifyFail');
            setError(typeof message === 'string' ? message : t('register.errVerifyFail'));
        } finally {
            setIsLoading(false);
        }
    };

    const cardClass = 'w-full max-w-sm border border-[#1f2334] bg-[#141722] px-8 py-10';
    const labelClass = 'text-xs font-medium uppercase tracking-wider text-gray-500';
    const inputClass = 'w-full border border-[#1f2334] bg-[#1a1e2d] px-3 py-2.5 text-sm text-white outline-none transition-colors focus:border-blue-500 placeholder:text-gray-500';
    const btnClass = 'w-full border border-[#2a2f45] bg-blue-600 py-2.5 text-sm font-medium text-white transition-colors hover:bg-blue-500 disabled:cursor-not-allowed disabled:opacity-50 flex items-center justify-center gap-2';
    const linkClass = 'text-blue-400 hover:text-blue-300 underline underline-offset-2';

    return (
        <div className="flex h-screen w-full items-center justify-center bg-[#0f111a]">
            <div className={cardClass}>
                <div className="mb-8 flex flex-col items-center">
                    <div className="mb-4 flex h-12 w-12 items-center justify-center border border-[#1f2334] bg-[#1a1e2d]">
                        <Gamepad2 size={22} className="text-blue-400" />
                    </div>
                    <h1 className="text-xl font-semibold tracking-tight text-white">{t('register.title')}</h1>
                    <p className="mt-1.5 text-center text-sm text-gray-500">{t('register.subtitle')}</p>
                </div>

                {step === 'success' ? (
                    <div className="flex flex-col items-center py-10">
                        <div className="mb-4 flex h-14 w-14 items-center justify-center border border-[#1f2334] bg-[#1a1e2d]">
                            <CheckCircle2 size={28} className="text-green-500" />
                        </div>
                        <h2 className="text-lg font-semibold text-white">{t('register.successVerified')}</h2>
                        <p className="mt-1 text-center text-sm text-gray-500">{t('register.successRedirect')}</p>
                    </div>
                ) : step === 'verify' ? (
                    <form onSubmit={handleVerify} className="space-y-4">
                        <div className="flex items-center gap-3 border border-[#1f2334] bg-[#1a1e2d] px-3 py-2.5 text-sm text-gray-400">
                            <Mail size={18} />
                            <span>{t('register.verifyBanner')}</span>
                        </div>
                        {error && (
                            <div className="flex items-center gap-2 border border-red-500/30 bg-red-500/10 px-3 py-2.5 text-sm text-red-400">
                                <AlertCircle size={14} /> {error}
                            </div>
                        )}
                        <div className="space-y-1">
                            <label className={labelClass}>{t('register.email')}</label>
                            <input type="email" value={email} readOnly className="w-full border border-[#1f2334] bg-[#1a1e2d]/80 px-3 py-2.5 text-sm text-gray-500 cursor-not-allowed" />
                        </div>
                        <div className="space-y-1">
                            <label className={labelClass}>{t('register.codeLabel')}</label>
                            <input
                                type="text"
                                value={verificationCode}
                                onChange={(e) => setVerificationCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                                placeholder="000000"
                                maxLength={6}
                                className={`${inputClass} text-center tracking-[0.25em]`}
                            />
                        </div>
                        <button type="submit" disabled={isLoading || verificationCode.length !== 6} className={`mt-4 ${btnClass}`}>
                            {isLoading ? <Loader2 size={18} className="animate-spin" /> : t('register.verifySubmit')}
                        </button>
                        <p className="mt-6 text-center text-sm text-gray-500">
                            {t('register.haveAccount')}{' '}
                            <Link to="/login" className={linkClass}>{t('nav.login')}</Link>
                        </p>
                    </form>
                ) : (
                    <form onSubmit={handleRegister} className="space-y-4">
                        {error && (
                            <div className="flex items-center gap-2 border border-red-500/30 bg-red-500/10 px-3 py-2.5 text-sm text-red-400">
                                <AlertCircle size={14} /> {error}
                            </div>
                        )}

                        <div className="space-y-1">
                            <label className={labelClass}>{t('register.username')}</label>
                            <input type="text" value={username} onChange={(e) => setUsername(e.target.value)} className={inputClass} placeholder={t('register.usernamePh')} required />
                        </div>
                        <div className="space-y-1">
                            <label className={labelClass}>{t('register.email')}</label>
                            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} className={inputClass} placeholder={t('register.emailPh')} required />
                        </div>
                        <div className="space-y-1">
                            <label className={labelClass}>{t('register.password')}</label>
                            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} className={inputClass} placeholder="••••••••" required />
                        </div>

                        <button type="submit" disabled={isLoading} className={`mt-4 ${btnClass}`}>
                            {isLoading ? <Loader2 size={18} className="animate-spin" /> : t('register.submit')}
                        </button>

                        <p className="mt-6 text-center text-sm text-[#6b6d76]">
                            {t('register.haveAccount')}{' '}
                            <Link to="/login" className={linkClass}>{t('nav.login')}</Link>
                        </p>
                    </form>
                )}
            </div>
        </div>
    );
}
