import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { Settings as SettingsIcon, User, Lock, Home, Eye, Loader2, CheckCircle2, AlertCircle, Trash2 } from 'lucide-react';
import { useUser } from '../context/UserContext';
import { usePreferences } from '../context/PreferencesContext';
import { updateUsername, updatePassword, requestDeleteAccount, confirmDeleteAccount } from '../api/apiClient';

export default function SettingsPage() {
    const { user, updateUser, logout } = useUser();
    const navigate = useNavigate();
    const { startPage, showNsfw, setStartPage, setShowNsfw } = usePreferences();
    const userId = user?.id ?? user?.UserId ?? user?.userId;
    const displayName = user?.username ?? user?.Username ?? '';

    const [newUsername, setNewUsername] = useState('');
    const [usernameMessage, setUsernameMessage] = useState({ type: '', text: '' });
    const [currentPassword, setCurrentPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [newPasswordAgain, setNewPasswordAgain] = useState('');
    const [passwordMessage, setPasswordMessage] = useState({ type: '', text: '' });
    const [deleteStep, setDeleteStep] = useState('idle'); // 'idle' | 'requested' | 'done'
    const [deleteCode, setDeleteCode] = useState('');
    const [deleteMessage, setDeleteMessage] = useState({ type: '', text: '' });
    const [deleteLoading, setDeleteLoading] = useState(false);

    const usernameMutation = useMutation({
        mutationFn: () => updateUsername(userId, newUsername),
        onSuccess: () => {
            updateUser({ username: newUsername.trim(), Username: newUsername.trim() });
            setUsernameMessage({ type: 'success', text: 'Kullanıcı adı güncellendi.' });
            setNewUsername('');
        },
        onError: (err) => {
            const msg = err.response?.data ?? err.message ?? 'Güncellenemedi.';
            setUsernameMessage({ type: 'error', text: typeof msg === 'string' ? msg : 'Bir hata oluştu.' });
        },
    });

    const passwordMutation = useMutation({
        mutationFn: () => updatePassword(userId, {
            currentPassword,
            newPassword,
            newPasswordAgain,
        }),
        onSuccess: () => {
            setPasswordMessage({ type: 'success', text: 'Şifre güncellendi.' });
            setCurrentPassword('');
            setNewPassword('');
            setNewPasswordAgain('');
        },
        onError: (err) => {
            const msg = err.response?.data ?? err.message ?? 'Güncellenemedi.';
            setPasswordMessage({ type: 'error', text: typeof msg === 'string' ? msg : 'Bir hata oluştu.' });
        },
    });

    const handleUsernameSubmit = (e) => {
        e.preventDefault();
        setUsernameMessage({ type: '', text: '' });
        const trimmed = newUsername.trim();
        if (!trimmed) {
            setUsernameMessage({ type: 'error', text: 'Yeni kullanıcı adı girin.' });
            return;
        }
        if (trimmed === displayName) {
            setUsernameMessage({ type: 'error', text: 'Mevcut adınızla aynı.' });
            return;
        }
        usernameMutation.mutate();
    };

    const handlePasswordSubmit = (e) => {
        e.preventDefault();
        setPasswordMessage({ type: '', text: '' });
        if (!currentPassword || !newPassword || !newPasswordAgain) {
            setPasswordMessage({ type: 'error', text: 'Tüm alanları doldurun.' });
            return;
        }
        if (newPassword.length < 8) {
            setPasswordMessage({ type: 'error', text: 'Yeni şifre en az 8 karakter olmalı.' });
            return;
        }
        if (newPassword !== newPasswordAgain) {
            setPasswordMessage({ type: 'error', text: 'Yeni şifreler eşleşmiyor.' });
            return;
        }
        passwordMutation.mutate();
    };

    const handleRequestDelete = async () => {
        try {
            setDeleteMessage({ type: '', text: '' });
            const uid = userId != null && userId !== '' ? Number(userId) : NaN;
            if (Number.isNaN(uid) || uid < 1) {
                setDeleteMessage({ type: 'error', text: 'Oturum bilgisi bulunamadı. Çıkış yapıp tekrar giriş yapın.' });
                return;
            }
            setDeleteLoading(true);
            const timeoutMs = 20000;
            await Promise.race([
                requestDeleteAccount(uid),
                new Promise((_, reject) => setTimeout(() => reject(new Error('İstek zaman aşımına uğradı. Tekrar deneyin.')), timeoutMs)),
            ]);
            setDeleteStep('requested');
            setDeleteMessage({ type: 'success', text: 'E-postanıza gönderilen 6 haneli kodu girin.' });
        } catch (err) {
            const msg = err.response?.data?.message ?? err.response?.data ?? err.message ?? 'Kod gönderilemedi.';
            setDeleteMessage({ type: 'error', text: typeof msg === 'string' ? msg : 'Bir hata oluştu.' });
        } finally {
            setDeleteLoading(false);
        }
    };

    const handleConfirmDelete = async (e) => {
        e.preventDefault();
        setDeleteMessage({ type: '', text: '' });
        if (!deleteCode.trim() || deleteCode.length !== 6) {
            setDeleteMessage({ type: 'error', text: '6 haneli kodu girin.' });
            return;
        }
        const uid = userId != null && userId !== '' ? Number(userId) : NaN;
        if (Number.isNaN(uid) || uid < 1) {
            setDeleteMessage({ type: 'error', text: 'Oturum bilgisi bulunamadı.' });
            return;
        }
        setDeleteLoading(true);
        try {
            await confirmDeleteAccount(uid, deleteCode);
            setDeleteStep('done');
            setDeleteMessage({ type: 'success', text: 'Hesabınız silindi. Çıkış yapılıyor...' });
            logout();
            setTimeout(() => navigate('/'), 1500);
        } catch (err) {
            const msg = err.response?.data?.message ?? err.response?.data ?? err.message ?? 'Hesap silinemedi.';
            setDeleteMessage({ type: 'error', text: typeof msg === 'string' ? msg : 'Kod geçersiz veya süresi dolmuş.' });
        } finally {
            setDeleteLoading(false);
        }
    };

    if (!user) {
        return (
            <div className="h-full flex flex-col items-center justify-center py-40 text-gray-500">
                <SettingsIcon size={48} className="mb-4 opacity-50" />
                <p className="font-medium">Ayarlara erişmek için giriş yapın.</p>
            </div>
        );
    }

    return (
        <div className="h-full overflow-y-auto px-8 pt-8 pb-20 scroll-smooth max-w-3xl mx-auto">
            <div className="space-y-10">
                {/* Profil özeti */}
                <div className="p-6 rounded-none bg-[#141722] border border-[#1f2334] flex items-center gap-6 shadow-md">
                    <div className="w-16 h-16 rounded-none bg-gradient-to-tr from-blue-600 to-indigo-600 flex items-center justify-center text-3xl font-bold text-white shadow-sm">
                        {displayName?.[0]?.toUpperCase() ?? '?'}
                    </div>
                    <div>
                        <h2 className="text-2xl font-bold text-white tracking-tight">{displayName}</h2>
                        <div className="mt-1 flex items-center gap-2 text-gray-500 font-mono text-sm">
                            ID: {userId} <span className="w-1.5 h-1.5 rounded-full bg-gray-500" /> Profil Aktif
                        </div>
                    </div>
                </div>

                {/* Hesap: Kullanıcı adı */}
                <section className="p-6 rounded-none bg-[#141722] border border-[#1f2334]">
                    <h3 className="flex items-center gap-2 text-lg font-bold text-white mb-4">
                        <User size={20} className="text-blue-500" /> Kullanıcı adı
                    </h3>
                    <form onSubmit={handleUsernameSubmit} className="space-y-3">
                        <input
                            type="text"
                            value={newUsername}
                            onChange={(e) => setNewUsername(e.target.value)}
                            placeholder="Yeni kullanıcı adı"
                            className="w-full px-4 py-2.5 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-white placeholder-gray-500 outline-none focus:border-blue-500/50"
                        />
                        {usernameMessage.text && (
                            <p className={`flex items-center gap-2 text-sm ${usernameMessage.type === 'success' ? 'text-green-400' : 'text-red-400'}`}>
                                {usernameMessage.type === 'success' ? <CheckCircle2 size={16} /> : <AlertCircle size={16} />}
                                {usernameMessage.text}
                            </p>
                        )}
                        <button
                            type="submit"
                            disabled={usernameMutation.isPending || !newUsername.trim()}
                            className="px-4 py-2 bg-blue-600 hover:bg-blue-500 disabled:opacity-50 text-white rounded-none font-semibold text-sm flex items-center gap-2"
                        >
                            {usernameMutation.isPending ? <Loader2 size={16} className="animate-spin" /> : null}
                            Güncelle
                        </button>
                    </form>
                </section>

                {/* Hesap: Şifre */}
                <section className="p-6 rounded-none bg-[#141722] border border-[#1f2334]">
                    <h3 className="flex items-center gap-2 text-lg font-bold text-white mb-4">
                        <Lock size={20} className="text-blue-500" /> Şifre değiştir
                    </h3>
                    <form onSubmit={handlePasswordSubmit} className="space-y-3">
                        <input
                            type="password"
                            value={currentPassword}
                            onChange={(e) => setCurrentPassword(e.target.value)}
                            placeholder="Mevcut şifre"
                            className="w-full px-4 py-2.5 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-white placeholder-gray-500 outline-none focus:border-blue-500/50"
                        />
                        <input
                            type="password"
                            value={newPassword}
                            onChange={(e) => setNewPassword(e.target.value)}
                            placeholder="Yeni şifre (en az 8 karakter)"
                            className="w-full px-4 py-2.5 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-white placeholder-gray-500 outline-none focus:border-blue-500/50"
                        />
                        <input
                            type="password"
                            value={newPasswordAgain}
                            onChange={(e) => setNewPasswordAgain(e.target.value)}
                            placeholder="Yeni şifre (tekrar)"
                            className="w-full px-4 py-2.5 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-white placeholder-gray-500 outline-none focus:border-blue-500/50"
                        />
                        {passwordMessage.text && (
                            <p className={`flex items-center gap-2 text-sm ${passwordMessage.type === 'success' ? 'text-green-400' : 'text-red-400'}`}>
                                {passwordMessage.type === 'success' ? <CheckCircle2 size={16} /> : <AlertCircle size={16} />}
                                {passwordMessage.text}
                            </p>
                        )}
                        <button
                            type="submit"
                            disabled={passwordMutation.isPending}
                            className="px-4 py-2 bg-blue-600 hover:bg-blue-500 disabled:opacity-50 text-white rounded-none font-semibold text-sm flex items-center gap-2"
                        >
                            {passwordMutation.isPending ? <Loader2 size={16} className="animate-spin" /> : null}
                            Şifreyi güncelle
                        </button>
                    </form>
                </section>

                {/* Tercihler */}
                <section className="p-6 rounded-none bg-[#141722] border border-[#1f2334]">
                    <h3 className="flex items-center gap-2 text-lg font-bold text-white mb-4">
                        <Home size={20} className="text-blue-500" /> Tercihler
                    </h3>
                    <div className="space-y-6">
                        <div>
                            <label className="block text-sm font-medium text-gray-400 mb-2">Başlangıç sayfası</label>
                            <select
                                value={startPage}
                                onChange={(e) => setStartPage(e.target.value)}
                                className="w-full max-w-xs px-4 py-2.5 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-white outline-none focus:border-blue-500/50"
                            >
                                <option value="Home">Ana sayfa (Popüler)</option>
                                <option value="Library">Kütüphane</option>
                            </select>
                            <p className="mt-1.5 text-xs text-gray-500">Giriş sonrası ve ana adres açılışında kullanılır.</p>
                        </div>
                        <div className="flex items-center justify-between gap-4">
                            <div className="flex items-center gap-2">
                                <Eye size={20} className="text-gray-400" />
                                <div>
                                    <p className="text-sm font-medium text-white">NSFW içerik göster</p>
                                    <p className="text-xs text-gray-500">Arama ve listelerde yetişkin etiketli oyunlar.</p>
                                </div>
                            </div>
                            <button
                                type="button"
                                role="switch"
                                aria-checked={showNsfw}
                                onClick={() => setShowNsfw(!showNsfw)}
                                className={`relative w-12 h-6 rounded-full transition-colors ${showNsfw ? 'bg-blue-600' : 'bg-[#1a1e2d] border border-[#1f2334]'}`}
                            >
                                <span className={`absolute top-1 w-4 h-4 rounded-full bg-white transition-transform ${showNsfw ? 'left-7' : 'left-1'}`} />
                            </button>
                        </div>
                    </div>
                </section>

                {/* Hesap sil */}
                <section className="p-6 rounded-none bg-[#141722] border border-red-500/20">
                    <h3 className="flex items-center gap-2 text-lg font-bold text-white mb-4">
                        <Trash2 size={20} className="text-red-500" /> Hesabı sil
                    </h3>
                    <p className="text-gray-400 text-sm mb-4">Hesabınızı kalıcı olarak silmek için e-postanıza gönderilen onay kodunu girmeniz gerekir.</p>
                    {deleteStep === 'idle' && deleteMessage.text && (
                        <p className={`flex items-center gap-2 text-sm mb-3 ${deleteMessage.type === 'success' ? 'text-green-400' : 'text-red-400'}`}>
                            {deleteMessage.type === 'success' ? <CheckCircle2 size={16} /> : <AlertCircle size={16} />}
                            {deleteMessage.text}
                        </p>
                    )}
                    {deleteStep === 'idle' && (
                        <button
                            type="button"
                            onClick={handleRequestDelete}
                            disabled={deleteLoading}
                            className="px-4 py-2 bg-red-600/20 hover:bg-red-600/30 text-red-400 border border-red-500/30 rounded-none font-medium text-sm flex items-center gap-2"
                        >
                            {deleteLoading ? <Loader2 size={16} className="animate-spin" /> : null}
                            Hesap silme kodu gönder
                        </button>
                    )}
                    {deleteStep === 'requested' && (
                        <form onSubmit={handleConfirmDelete} className="space-y-3">
                            {deleteMessage.text && (
                                <p className={`flex items-center gap-2 text-sm ${deleteMessage.type === 'success' ? 'text-green-400' : 'text-red-400'}`}>
                                    {deleteMessage.type === 'success' ? <CheckCircle2 size={16} /> : <AlertCircle size={16} />}
                                    {deleteMessage.text}
                                </p>
                            )}
                            <input
                                type="text"
                                value={deleteCode}
                                onChange={(e) => setDeleteCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                                placeholder="000000"
                                maxLength={6}
                                className="w-full max-w-[140px] px-4 py-2.5 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-white text-center tracking-widest"
                            />
                            <div className="flex gap-2">
                                <button type="button" onClick={() => { setDeleteStep('idle'); setDeleteCode(''); setDeleteMessage({ type: '', text: '' }); }} className="px-4 py-2 rounded-none border border-[#1f2334] text-gray-400 hover:bg-[#1a1e2d]">
                                    İptal
                                </button>
                                <button type="submit" disabled={deleteLoading || deleteCode.length !== 6} className="px-4 py-2 bg-red-600 hover:bg-red-500 disabled:opacity-50 text-white rounded-none font-medium flex items-center gap-2">
                                    {deleteLoading ? <Loader2 size={16} className="animate-spin" /> : null}
                                    Hesabı kalıcı olarak sil
                                </button>
                            </div>
                        </form>
                    )}
                    {deleteStep === 'done' && deleteMessage.text && (
                        <p className="flex items-center gap-2 text-green-400 text-sm"><CheckCircle2 size={16} /> {deleteMessage.text}</p>
                    )}
                </section>
            </div>
        </div>
    );
}
