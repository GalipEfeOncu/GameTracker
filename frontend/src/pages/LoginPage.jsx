import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { Gamepad2, Loader2, AlertCircle } from 'lucide-react';
import { loginUser } from '../api/apiClient';
import { useUser } from '../context/UserContext';
import { usePreferences } from '../context/PreferencesContext';

export default function LoginPage() {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [rememberMe, setRememberMe] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');

    const { login } = useUser();
    const { startPage } = usePreferences();
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setIsLoading(true);

        try {
            // rememberMe → API’de daha uzun ömürlü access JWT (refresh token yok; bkz. backend JwtTokenService)
            const user = await loginUser(username, password, rememberMe);
            const uid = user?.UserId ?? user?.id ?? user?.userId;
            const accessToken = user?.AccessToken ?? user?.accessToken;
            if (user && typeof user === 'object' && uid != null) {
                login({ ...user, id: uid, accessToken });
            } else {
                login(user);
            }
            const to = startPage === 'Library' ? '/library' : '/popular';
            navigate(to);
        } catch (err) {
            const status = err.response?.status;
            const data = err.response?.data;
            const backendMsg = data?.message;
            const isLoginFailure = status === 401 || status === 400;
            const isGenericHttpError = typeof err.message === 'string' && /request failed with status code \d+/i.test(err.message);

            if (data?.code === 'EmailNotVerified' || (typeof backendMsg === 'string' && backendMsg.toLowerCase().includes('verify')))
                setError('E-posta adresinizi doğrulayın. Kayıt sonrası gönderilen kodu kullandığınızdan emin olun.');
            else if (isLoginFailure || isGenericHttpError)
                setError('Kullanıcı adı veya şifre hatalı.');
            else if (typeof backendMsg === 'string')
                setError(backendMsg);
            else
                setError('Kullanıcı adı veya şifre hatalı.');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="flex h-screen w-full items-center justify-center bg-[#0f111a]">
            <div className="w-full max-w-sm border border-[#1f2334] bg-[#141722] px-8 py-10">
                <div className="mb-8 flex flex-col items-center">
                    <div className="mb-4 flex h-12 w-12 items-center justify-center border border-[#1f2334] bg-[#1a1e2d]">
                        <Gamepad2 size={22} className="text-blue-400" />
                    </div>
                    <h1 className="text-xl font-semibold tracking-tight text-white">Hoş Geldiniz</h1>
                    <p className="mt-1.5 text-sm text-gray-500">GameTracker hesabınıza giriş yapın.</p>
                </div>

                <form onSubmit={handleSubmit} className="space-y-4">
                    {error && (
                        <div className="flex items-center gap-2 border border-red-500/30 bg-red-500/10 px-3 py-2.5 text-sm text-red-400">
                            <AlertCircle size={14} /> {error}
                        </div>
                    )}

                    <div className="space-y-1">
                        <label htmlFor="login-username" className="text-xs font-medium uppercase tracking-wider text-gray-500">Kullanıcı Adı</label>
                        <input
                            id="login-username"
                            type="text"
                            name="username"
                            autoComplete="username"
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                            className="w-full border border-[#1f2334] bg-[#1a1e2d] px-3 py-2.5 text-sm text-white outline-none transition-colors focus:border-blue-500 placeholder:text-gray-500"
                            placeholder="Kullanıcı adınız..."
                            required
                        />
                    </div>

                    <div className="space-y-1">
                        <div className="flex items-center justify-between">
                            <label htmlFor="login-password" className="text-xs font-medium uppercase tracking-wider text-gray-500">Şifre</label>
                            <Link to="/forgot-password" className="text-xs text-blue-400 hover:text-blue-300">
                                Şifremi unuttum
                            </Link>
                        </div>
                        <input
                            id="login-password"
                            type="password"
                            name="password"
                            autoComplete="current-password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            className="w-full border border-[#1f2334] bg-[#1a1e2d] px-3 py-2.5 text-sm text-white outline-none transition-colors focus:border-blue-500 placeholder:text-gray-500"
                            placeholder="••••••••"
                            required
                        />
                    </div>

                    <label className="flex cursor-pointer flex-col gap-1 sm:flex-row sm:items-center sm:gap-2">
                        <span className="flex items-center gap-2 text-sm text-gray-300">
                            <input
                                type="checkbox"
                                checked={rememberMe}
                                onChange={(e) => setRememberMe(e.target.checked)}
                                className="h-4 w-4 rounded-none border border-[#1f2334] bg-[#1a1e2d] text-blue-600 focus:ring-2 focus:ring-blue-500 focus:ring-offset-0"
                            />
                            Beni hatırla
                        </span>
                        <span className="text-xs text-gray-500 sm:pl-1">Bu cihazda oturum süresini uzatır.</span>
                    </label>

                    <button
                        type="submit"
                        disabled={isLoading}
                        className="mt-4 w-full border border-[#2a2f45] bg-blue-600 py-2.5 text-sm font-medium text-white transition-colors hover:bg-blue-500 disabled:cursor-not-allowed disabled:opacity-50 flex items-center justify-center gap-2"
                    >
                        {isLoading ? <Loader2 size={18} className="animate-spin" /> : 'Giriş Yap'}
                    </button>
                </form>

                <p className="mt-6 text-center text-sm text-gray-500">
                    Hesabınız yok mu? <Link to="/register" className="text-blue-400 hover:text-blue-300 underline underline-offset-2">Kayıt olun</Link>
                </p>
            </div>
        </div>
    );
}
