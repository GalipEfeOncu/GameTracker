import { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { logoutUser } from '../api/apiClient';
import { setDesktopSession, isDesktop } from '../desktop/bridge';

// Vite build'inde gömülü API kökü; desktop tracker backend'e aynı URL ile konuşur.
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '';

const UserContext = createContext(null);

export function UserProvider({ children }) {
    const [user, setUser] = useState(() => {
        try {
            const stored = localStorage.getItem('gt_user');
            return stored ? JSON.parse(stored) : null;
        } catch { return null; }
    });

    const login = useCallback((userData) => {
        localStorage.setItem('gt_user', JSON.stringify(userData));
        setUser(userData);
        if (isDesktop) {
            const uid = userData?.id ?? userData?.userId ?? userData?.UserId;
            const token = userData?.accessToken ?? userData?.AccessToken;
            setDesktopSession({ userId: uid, accessToken: token, apiBaseUrl: API_BASE_URL }).catch(() => {});
        }
    }, []);

    // Sayfa yenilendiğinde / desktop restart sonrası kaydı geri yükleyince de sync et.
    useEffect(() => {
        if (!isDesktop || !user) return;
        const uid = user?.id ?? user?.userId ?? user?.UserId;
        const token = user?.accessToken ?? user?.AccessToken;
        if (uid && token) {
            setDesktopSession({ userId: uid, accessToken: token, apiBaseUrl: API_BASE_URL }).catch(() => {});
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    // UI hemen güncellenir; refresh token iptali arka planda (fire-and-forget). Ağ hatası UX'i bloklamaz.
    const logout = useCallback(() => {
        let refreshToken = null;
        try {
            const stored = localStorage.getItem('gt_user');
            if (stored) refreshToken = JSON.parse(stored)?.refreshToken ?? null;
        } catch { /* ignore */ }

        localStorage.removeItem('gt_user');
        setUser(null);

        if (isDesktop) {
            setDesktopSession({ userId: null, accessToken: null, apiBaseUrl: API_BASE_URL }).catch(() => {});
        }

        if (refreshToken) {
            logoutUser(refreshToken).catch(() => { /* ignore */ });
        }
    }, []);

    const updateUser = useCallback((partial) => {
        setUser((prev) => {
            if (!prev) return prev;
            const next = { ...prev, ...partial };
            localStorage.setItem('gt_user', JSON.stringify(next));
            return next;
        });
    }, []);

    return (
        <UserContext.Provider value={{ user, login, logout, updateUser, isLoggedIn: !!user }}>
            {children}
        </UserContext.Provider>
    );
}

export const useUser = () => useContext(UserContext);
