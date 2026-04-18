import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { Settings as SettingsIcon, User, Lock, Home, Eye, Loader2, CheckCircle2, AlertCircle, Trash2, Gauge, HardDrive } from 'lucide-react';
import { useUser } from '../context/UserContext';
import { usePreferences } from '../context/PreferencesContext';
import { useToast } from '../context/ToastContext';
import { updateUsername, updatePassword, requestDeleteAccount, confirmDeleteAccount } from '../api/apiClient';
import { getSessionUserId } from '../utils/sessionUser';
import { isDesktop, getDesktopSettings, setDesktopSettings } from '../desktop/bridge';
import { useI18n } from '../i18n/useI18n';

export default function SettingsPage() {
    const { user, updateUser, logout } = useUser();
    const { showToast } = useToast();
    const navigate = useNavigate();
    const {
        startPage,
        showNsfw,
        setStartPage,
        setShowNsfw,
        popularListMode,
        setPopularListMode,
        locale,
        setLocale,
    } = usePreferences();
    const { t } = useI18n();
    const userId = getSessionUserId(user);
    const displayName = user?.username ?? user?.Username ?? '';

    const [newUsername, setNewUsername] = useState('');
    const [usernameMessage, setUsernameMessage] = useState({ type: '', text: '' });
    const [currentPassword, setCurrentPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [newPasswordAgain, setNewPasswordAgain] = useState('');
    const [passwordMessage, setPasswordMessage] = useState({ type: '', text: '' });
    const [deleteStep, setDeleteStep] = useState('idle');
    const [deleteCode, setDeleteCode] = useState('');
    const [deleteLoading, setDeleteLoading] = useState(false);

    const [desktopSettings, setDesktopSettingsState] = useState(null);
    useEffect(() => {
        if (!isDesktop) return;
        getDesktopSettings().then(setDesktopSettingsState).catch(() => {});
    }, []);
    const toggleDesktop = async (key) => {
        if (!desktopSettings) return;
        const next = await setDesktopSettings({ [key]: !desktopSettings[key] });
        if (next) setDesktopSettingsState(next);
    };

    const usernameMutation = useMutation({
        mutationFn: () => updateUsername(userId, newUsername),
        onSuccess: () => {
            updateUser({ username: newUsername.trim(), Username: newUsername.trim() });
            showToast(t('settings.toastUsernameOk'), 'success');
            setNewUsername('');
        },
        onError: (err) => {
            const msg = err.response?.data ?? err.message ?? t('common.notUpdated');
            showToast(typeof msg === 'string' ? msg : t('common.genericError'), 'error');
        },
    });

    const passwordMutation = useMutation({
        mutationFn: () => updatePassword(userId, {
            currentPassword,
            newPassword,
            newPasswordAgain,
        }),
        onSuccess: () => {
            showToast(t('settings.toastPasswordOk'), 'success');
            setCurrentPassword('');
            setNewPassword('');
            setNewPasswordAgain('');
        },
        onError: (err) => {
            const msg = err.response?.data ?? err.message ?? t('common.notUpdated');
            showToast(typeof msg === 'string' ? msg : t('common.genericError'), 'error');
        },
    });

    const handleUsernameSubmit = (e) => {
        e.preventDefault();
        setUsernameMessage({ type: '', text: '' });
        const trimmed = newUsername.trim();
        if (!trimmed) {
            setUsernameMessage({ type: 'error', text: t('settings.errUsernameEmpty') });
            return;
        }
        if (trimmed === displayName) {
            setUsernameMessage({ type: 'error', text: t('settings.errUsernameSame') });
            return;
        }
        usernameMutation.mutate();
    };

    const handlePasswordSubmit = (e) => {
        e.preventDefault();
        setPasswordMessage({ type: '', text: '' });
        if (!currentPassword || !newPassword || !newPasswordAgain) {
            setPasswordMessage({ type: 'error', text: t('settings.errPasswordFields') });
            return;
        }
        if (newPassword.length < 8) {
            setPasswordMessage({ type: 'error', text: t('settings.errPasswordShort') });
            return;
        }
        if (newPassword !== newPasswordAgain) {
            setPasswordMessage({ type: 'error', text: t('settings.errPasswordMatch') });
            return;
        }
        passwordMutation.mutate();
    };

    const handleRequestDelete = async () => {
        try {
            const uid = userId != null && userId !== '' ? Number(userId) : NaN;
            if (Number.isNaN(uid) || uid < 1) {
                showToast(t('settings.errSession'), 'error');
                return;
            }
            setDeleteLoading(true);
            const timeoutMs = 20000;
            await Promise.race([
                requestDeleteAccount(uid),
                new Promise((_, reject) => setTimeout(() => reject(new Error('timeout')), timeoutMs)),
            ]);
            setDeleteStep('requested');
            showToast(t('settings.toastDeleteCodeSent'), 'success');
        } catch (err) {
            if (err?.message === 'timeout') {
                showToast(t('settings.errTimeout'), 'error');
            } else {
                const msg = err.response?.data?.message ?? err.response?.data ?? err.message ?? t('common.genericError');
                showToast(typeof msg === 'string' ? msg : t('common.genericError'), 'error');
            }
        } finally {
            setDeleteLoading(false);
        }
    };

    const handleConfirmDelete = async (e) => {
        e.preventDefault();
        if (!deleteCode.trim() || deleteCode.length !== 6) {
            showToast(t('settings.errDeleteCode'), 'error');
            return;
        }
        const uid = userId != null && userId !== '' ? Number(userId) : NaN;
        if (Number.isNaN(uid) || uid < 1) {
            showToast(t('settings.errSessionShort'), 'error');
            return;
        }
        setDeleteLoading(true);
        try {
            await confirmDeleteAccount(uid, deleteCode);
            setDeleteStep('done');
            showToast(t('settings.toastDeleteOk'), 'success');
            logout();
            setTimeout(() => navigate('/'), 1500);
        } catch (err) {
            const msg = err.response?.data?.message ?? err.response?.data ?? err.message ?? t('common.genericError');
            showToast(typeof msg === 'string' ? msg : t('settings.errCodeInvalid'), 'error');
        } finally {
            setDeleteLoading(false);
        }
    };

    if (!user) {
        return (
            <div className="h-full flex flex-col items-center justify-center py-40 text-gray-500">
                <SettingsIcon size={48} className="mb-4 opacity-50" />
                <p className="font-medium">{t('settings.loginRequired')}</p>
            </div>
        );
    }

    return (
        <div className="h-full overflow-y-auto px-8 pt-8 pb-20 scroll-smooth max-w-3xl mx-auto">
            <div className="space-y-10">
                <div className="p-6 rounded-none bg-[#141722] border border-[#1f2334] flex items-center gap-6 shadow-md">
                    <div className="w-16 h-16 rounded-none bg-gradient-to-tr from-blue-600 to-indigo-600 flex items-center justify-center text-3xl font-bold text-white shadow-sm">
                        {displayName?.[0]?.toUpperCase() ?? '?'}
                    </div>
                    <div>
                        <h2 className="text-2xl font-bold text-white tracking-tight">{displayName}</h2>
                        <div className="mt-1 flex items-center gap-2 text-gray-500 font-mono text-sm">
                            ID: {userId} <span className="w-1.5 h-1.5 rounded-full bg-gray-500" /> {t('common.profileActive')}
                        </div>
                    </div>
                </div>

                <section className="p-6 rounded-none bg-[#141722] border border-[#1f2334]">
                    <h3 className="flex items-center gap-2 text-lg font-bold text-white mb-4">
                        <User size={20} className="text-blue-500" /> {t('settings.usernameSection')}
                    </h3>
                    <form onSubmit={handleUsernameSubmit} className="space-y-3">
                        <input
                            type="text"
                            value={newUsername}
                            onChange={(e) => setNewUsername(e.target.value)}
                            placeholder={t('settings.usernamePh')}
                            className="w-full px-4 py-2.5 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-white placeholder-gray-500 outline-none focus:border-blue-500/50"
                        />
                        {usernameMessage.text && usernameMessage.type === 'error' && (
                            <p className="flex items-center gap-2 text-sm text-red-400">
                                <AlertCircle size={16} />
                                {usernameMessage.text}
                            </p>
                        )}
                        <button
                            type="submit"
                            disabled={usernameMutation.isPending || !newUsername.trim()}
                            className="px-4 py-2 bg-blue-600 hover:bg-blue-500 disabled:opacity-50 text-white rounded-none font-semibold text-sm flex items-center gap-2"
                        >
                            {usernameMutation.isPending ? <Loader2 size={16} className="animate-spin" /> : null}
                            {t('common.update')}
                        </button>
                    </form>
                </section>

                <section className="p-6 rounded-none bg-[#141722] border border-[#1f2334]">
                    <h3 className="flex items-center gap-2 text-lg font-bold text-white mb-4">
                        <Lock size={20} className="text-blue-500" /> {t('settings.passwordSection')}
                    </h3>
                    <form onSubmit={handlePasswordSubmit} className="space-y-3">
                        <input
                            type="password"
                            value={currentPassword}
                            onChange={(e) => setCurrentPassword(e.target.value)}
                            placeholder={t('settings.currentPassword')}
                            className="w-full px-4 py-2.5 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-white placeholder-gray-500 outline-none focus:border-blue-500/50"
                        />
                        <input
                            type="password"
                            value={newPassword}
                            onChange={(e) => setNewPassword(e.target.value)}
                            placeholder={t('settings.newPassword')}
                            className="w-full px-4 py-2.5 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-white placeholder-gray-500 outline-none focus:border-blue-500/50"
                        />
                        <input
                            type="password"
                            value={newPasswordAgain}
                            onChange={(e) => setNewPasswordAgain(e.target.value)}
                            placeholder={t('settings.newPasswordAgain')}
                            className="w-full px-4 py-2.5 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-white placeholder-gray-500 outline-none focus:border-blue-500/50"
                        />
                        {passwordMessage.text && passwordMessage.type === 'error' && (
                            <p className="flex items-center gap-2 text-sm text-red-400">
                                <AlertCircle size={16} />
                                {passwordMessage.text}
                            </p>
                        )}
                        <button
                            type="submit"
                            disabled={passwordMutation.isPending}
                            className="px-4 py-2 bg-blue-600 hover:bg-blue-500 disabled:opacity-50 text-white rounded-none font-semibold text-sm flex items-center gap-2"
                        >
                            {passwordMutation.isPending ? <Loader2 size={16} className="animate-spin" /> : null}
                            {t('settings.passwordSubmit')}
                        </button>
                    </form>
                </section>

                <section className="p-6 rounded-none bg-[#141722] border border-[#1f2334]">
                    <h3 className="flex items-center gap-2 text-lg font-bold text-white mb-4">
                        <Home size={20} className="text-blue-500" /> {t('settings.prefsSection')}
                    </h3>
                    <div className="space-y-6">
                        <div>
                            <label className="block text-sm font-medium text-gray-400 mb-2">{t('settings.languageLabel')}</label>
                            <select
                                value={locale}
                                onChange={(e) => setLocale(e.target.value)}
                                className="w-full max-w-xs px-4 py-2.5 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-white outline-none focus:border-blue-500/50"
                            >
                                <option value="tr">{t('settings.languageTr')}</option>
                                <option value="en">{t('settings.languageEn')}</option>
                            </select>
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-400 mb-2">{t('settings.startPage')}</label>
                            <select
                                value={startPage}
                                onChange={(e) => setStartPage(e.target.value)}
                                className="w-full max-w-xs px-4 py-2.5 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-white outline-none focus:border-blue-500/50"
                            >
                                <option value="Home">{t('settings.startHome')}</option>
                                <option value="Library">{t('settings.startLibrary')}</option>
                            </select>
                            <p className="mt-1.5 text-xs text-gray-500">{t('settings.startPageHint')}</p>
                        </div>
                        <div className="flex items-center justify-between gap-4">
                            <div className="flex items-center gap-2">
                                <Eye size={20} className="text-gray-400" />
                                <div>
                                    <p className="text-sm font-medium text-white">{t('settings.nsfwTitle')}</p>
                                    <p className="text-xs text-gray-500">{t('settings.nsfwHint')}</p>
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
                        <div>
                            <p className="text-sm font-medium text-gray-400 mb-2">{t('settings.popularList')}</p>
                            <div className="space-y-2 max-w-md">
                                <label className="flex cursor-pointer items-start gap-3 rounded-none border border-[#1f2334] bg-[#1a1e2d]/50 p-3 hover:border-[#2a2f45]">
                                    <input
                                        type="radio"
                                        name="popularListMode"
                                        checked={popularListMode === 'scroll'}
                                        onChange={() => setPopularListMode('scroll')}
                                        className="mt-1"
                                    />
                                    <span>
                                        <span className="block text-sm font-medium text-white">{t('settings.popularScroll')}</span>
                                        <span className="block text-xs text-gray-500 mt-0.5">{t('settings.popularScrollHint')}</span>
                                    </span>
                                </label>
                                <label className="flex cursor-pointer items-start gap-3 rounded-none border border-[#1f2334] bg-[#1a1e2d]/50 p-3 hover:border-[#2a2f45]">
                                    <input
                                        type="radio"
                                        name="popularListMode"
                                        checked={popularListMode === 'paged'}
                                        onChange={() => setPopularListMode('paged')}
                                        className="mt-1"
                                    />
                                    <span>
                                        <span className="block text-sm font-medium text-white">{t('settings.popularPaged')}</span>
                                        <span className="block text-xs text-gray-500 mt-0.5">{t('settings.popularPagedHint')}</span>
                                    </span>
                                </label>
                            </div>
                        </div>
                    </div>
                </section>

                {isDesktop && desktopSettings && (
                    <section className="p-6 rounded-none bg-[#141722] border border-[#1f2334]">
                        <h3 className="flex items-center gap-2 text-lg font-bold text-white mb-4">
                            <HardDrive size={20} className="text-blue-500" /> {t('settings.desktopSection')}
                        </h3>
                        <div className="space-y-3">
                            <DesktopToggle
                                label={t('settings.trayLabel')}
                                hint={t('settings.trayHint')}
                                checked={desktopSettings.minimizeToTray}
                                onChange={() => toggleDesktop('minimizeToTray')}
                            />
                            <DesktopToggle
                                label={t('settings.autoStartLabel')}
                                hint={t('settings.autoStartHint')}
                                checked={desktopSettings.autoStart}
                                onChange={() => toggleDesktop('autoStart')}
                            />
                            <DesktopToggle
                                label={t('settings.startMinLabel')}
                                hint={t('settings.startMinHint')}
                                checked={desktopSettings.startMinimized}
                                onChange={() => toggleDesktop('startMinimized')}
                            />
                        </div>
                    </section>
                )}

                <section className="p-6 rounded-none bg-[#141722] border border-[#1f2334]">
                    <h3 className="flex items-center gap-2 text-lg font-bold text-white mb-4">
                        <Gauge size={20} className="text-blue-500" /> {t('settings.perfSection')}
                    </h3>
                    <p className="text-sm text-gray-400 leading-relaxed">
                        {t('settings.perfBody')}
                    </p>
                </section>

                <section className="p-6 rounded-none bg-[#141722] border border-red-500/20">
                    <h3 className="flex items-center gap-2 text-lg font-bold text-white mb-4">
                        <Trash2 size={20} className="text-red-500" /> {t('settings.deleteSection')}
                    </h3>
                    <p className="text-gray-400 text-sm mb-4">{t('settings.deleteIntro')}</p>
                    {deleteStep === 'idle' && (
                        <button
                            type="button"
                            onClick={handleRequestDelete}
                            disabled={deleteLoading}
                            className="px-4 py-2 bg-red-600/20 hover:bg-red-600/30 text-red-400 border border-red-500/30 rounded-none font-medium text-sm flex items-center gap-2"
                        >
                            {deleteLoading ? <Loader2 size={16} className="animate-spin" /> : null}
                            {t('settings.deleteSendCode')}
                        </button>
                    )}
                    {deleteStep === 'requested' && (
                        <form onSubmit={handleConfirmDelete} className="space-y-3">
                            <p className="text-sm text-gray-400">{t('settings.deleteCodeHint')}</p>
                            <input
                                type="text"
                                value={deleteCode}
                                onChange={(e) => setDeleteCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                                placeholder="000000"
                                maxLength={6}
                                className="w-full max-w-[140px] px-4 py-2.5 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-white text-center tracking-widest"
                            />
                            <div className="flex gap-2">
                                <button type="button" onClick={() => { setDeleteStep('idle'); setDeleteCode(''); }} className="px-4 py-2 rounded-none border border-[#1f2334] text-gray-400 hover:bg-[#1a1e2d]">
                                    {t('common.cancel')}
                                </button>
                                <button type="submit" disabled={deleteLoading || deleteCode.length !== 6} className="px-4 py-2 bg-red-600 hover:bg-red-500 disabled:opacity-50 text-white rounded-none font-medium flex items-center gap-2">
                                    {deleteLoading ? <Loader2 size={16} className="animate-spin" /> : null}
                                    {t('settings.deleteConfirm')}
                                </button>
                            </div>
                        </form>
                    )}
                    {deleteStep === 'done' && (
                        <p className="flex items-center gap-2 text-green-400 text-sm"><CheckCircle2 size={16} /> {t('settings.deleteDone')}</p>
                    )}
                </section>
            </div>
        </div>
    );
}

function DesktopToggle({ label, hint, checked, onChange }) {
    return (
        <label className="flex items-start gap-3 cursor-pointer py-2">
            <input
                type="checkbox"
                checked={!!checked}
                onChange={onChange}
                className="mt-0.5 h-4 w-4 border border-[#1f2334] bg-[#1a1e2d] text-blue-600"
            />
            <span className="flex-1">
                <span className="block text-sm font-medium text-gray-200">{label}</span>
                {hint && <span className="block text-xs text-gray-500 mt-0.5">{hint}</span>}
            </span>
        </label>
    );
}
