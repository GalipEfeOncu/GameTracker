import axios from 'axios';
import { emitToast } from '../utils/toastEvents';
import { getResolvedApiRoot } from '../utils/apiBaseUrl';
import { translate, getStoredLocale } from '../i18n/t';
import { isDesktop, setDesktopSession } from '../desktop/bridge';

// Üretim: .env içinde tam API kökü (örn. https://api.siteniz.com/api). Boşsa geliştirmede /api + Vite proxy kullanılır.
const rawBase = import.meta.env.VITE_API_BASE_URL;
const baseURL =
    typeof rawBase === 'string' && rawBase.trim() !== ''
        ? rawBase.trim().replace(/\/+$/, '')
        : '/api';

// Electron/file:// modunda BrowserRouter yerine HashRouter kullanılır; path tabanlı redirect kırılır.
const isDesktopBuild = import.meta.env.VITE_DESKTOP === '1' || import.meta.env.VITE_DESKTOP === 'true';

function redirectToLogin() {
    if (typeof window === 'undefined') return;
    if (isDesktopBuild) {
        if (window.location.hash !== '#/login') window.location.hash = '#/login';
        return;
    }
    if (!window.location.pathname.includes('/login')) window.location.assign('/login');
}

const apiClient = axios.create({
    baseURL,
    timeout: 15000,
});

function readStoredUser() {
    try {
        const raw = localStorage.getItem('gt_user');
        return raw ? JSON.parse(raw) : null;
    } catch {
        return null;
    }
}

function readStoredAccessToken() {
    const u = readStoredUser();
    return u?.accessToken ?? u?.AccessToken ?? null;
}

function readStoredRefreshToken() {
    const u = readStoredUser();
    return u?.refreshToken ?? u?.RefreshToken ?? null;
}

function updateStoredTokens({ accessToken, refreshToken }) {
    try {
        const u = readStoredUser();
        if (!u) return;
        const next = { ...u };
        if (accessToken) next.accessToken = accessToken;
        if (refreshToken) next.refreshToken = refreshToken;
        localStorage.setItem('gt_user', JSON.stringify(next));
    } catch { /* ignore */ }
}

apiClient.interceptors.request.use((config) => {
    const t = readStoredAccessToken();
    if (t) {
        const headers = config.headers ?? {};
        headers.Authorization = `Bearer ${t}`;
        config.headers = headers;
    }
    return config;
});

// Paralel 401'ler tek refresh çağrısına düşsün diye singleton promise.
let refreshInFlight = null;

async function refreshAccessToken() {
    if (refreshInFlight) return refreshInFlight;
    const refreshToken = readStoredRefreshToken();
    if (!refreshToken) return Promise.reject(new Error('no_refresh_token'));

    refreshInFlight = axios
        .post(`${baseURL}/User/refresh`, { RefreshToken: refreshToken }, { timeout: 15000 })
        .then((res) => {
            const data = res?.data ?? {};
            const newAccess = data.AccessToken ?? data.accessToken;
            const newRefresh = data.RefreshToken ?? data.refreshToken;
            if (!newAccess || !newRefresh) throw new Error('invalid_refresh_response');
            updateStoredTokens({ accessToken: newAccess, refreshToken: newRefresh });
            if (isDesktop) {
                const u = readStoredUser();
                const uid = u?.id ?? u?.userId ?? u?.UserId;
                if (uid) {
                    setDesktopSession({
                        userId: uid,
                        accessToken: newAccess,
                        apiBaseUrl: getResolvedApiRoot() || baseURL,
                    }).catch(() => {});
                }
            }
            return newAccess;
        })
        .finally(() => { refreshInFlight = null; });

    return refreshInFlight;
}

function urlLikelyRequiresSession(url) {
    const u = String(url ?? '');
    return (
        u.includes('/Library/user/') ||
        (u.includes('/User/') &&
            (u.includes('/profile/') ||
                u.includes('/request-delete-account') ||
                u.includes('/confirm-delete-account')))
    );
}

apiClient.interceptors.response.use(
    (r) => r,
    async (err) => {
        if (err.response?.status === 429) {
            emitToast(translate(getStoredLocale(), 'api.rateLimited'), 'error');
        }
        if (err.response?.status === 401) {
            const originalRequest = err.config || {};
            const url = String(originalRequest.url ?? '');

            // Refresh / login uçlarını tekrar denemeyiz — sonsuz döngü olur.
            if (url.includes('/User/login') || url.includes('/User/refresh')) {
                if (url.includes('/User/refresh')) {
                    try { localStorage.removeItem('gt_user'); } catch { /* ignore */ }
                    redirectToLogin();
                }
                return Promise.reject(err);
            }

            const hadAuth = !!originalRequest.headers?.Authorization;
            const hasStoredUser = !!readStoredUser();

            // Sadece oturum gerektiren uçlarda refresh dene. Anonim public uçlarda 401 dönüyorsa refresh'e girmeyiz.
            const shouldTryRefresh =
                !originalRequest._retry && (hadAuth || (hasStoredUser && urlLikelyRequiresSession(url)));

            if (shouldTryRefresh && readStoredRefreshToken()) {
                try {
                    const newAccess = await refreshAccessToken();
                    originalRequest._retry = true;
                    originalRequest.headers = originalRequest.headers ?? {};
                    originalRequest.headers.Authorization = `Bearer ${newAccess}`;
                    return apiClient(originalRequest);
                } catch {
                    try { localStorage.removeItem('gt_user'); } catch { /* ignore */ }
                    redirectToLogin();
                    return Promise.reject(err);
                }
            }

            if (hadAuth || (hasStoredUser && urlLikelyRequiresSession(url))) {
                try { localStorage.removeItem('gt_user'); } catch { /* ignore */ }
                redirectToLogin();
            }
        }
        return Promise.reject(err);
    }
);

// ---- User ----
export const registerUser = async ({ username, email, password }) => {
    const { data } = await apiClient.post('/User/register', {
        Username: username,
        Email: email,
        Password: password,
    });
    return data;
};

/** Login: access token (15 dk) + refresh token döner. RememberMe yalnızca refresh ömrünü uzatır (30 → 90 gün). */
export const loginUser = async (username, password, rememberMe = false) => {
    const { data } = await apiClient.post('/User/login', {
        EmailOrUsername: username,
        password,
        RememberMe: !!rememberMe,
    });
    return data;
};

/** Sunucuda refresh token'ı iptal eder. İstek başarısız olsa bile sessizce geçer (çıkış yapan kullanıcı bloke edilmesin). */
export const logoutUser = async (refreshToken) => {
    if (!refreshToken) return;
    try {
        await apiClient.post('/User/logout', { RefreshToken: refreshToken });
    } catch { /* ignore */ }
};

export const updateUsername = async (userId, newUsername) => {
    if (!userId || !newUsername?.trim()) throw new Error('userId and newUsername required');
    const { data } = await apiClient.put(`/User/${userId}/profile/username`, { NewUsername: newUsername.trim() });
    return data;
};

export const updatePassword = async (userId, { currentPassword, newPassword, newPasswordAgain }) => {
    if (!userId) throw new Error('userId required');
    const { data } = await apiClient.put(`/User/${userId}/profile/password`, {
        CurrentPassword: currentPassword,
        NewPassword: newPassword,
        NewPasswordAgain: newPasswordAgain,
    });
    return data;
};

export const requestPasswordReset = async (email) => {
    const { data } = await apiClient.post('/User/forgot-password', { Email: email?.trim() });
    return data;
};

export const resetPasswordWithCode = async ({ email, code, newPassword, newPasswordAgain }) => {
    const { data } = await apiClient.post('/User/reset-password', {
        Email: email?.trim(),
        Code: code?.trim(),
        NewPassword: newPassword,
        NewPasswordAgain: newPasswordAgain,
    });
    return data;
};

export const verifyEmail = async (email, code) => {
    const { data } = await apiClient.post('/User/verify-email', {
        Email: email?.trim(),
        Code: code?.trim(),
    });
    return data;
};

export const requestDeleteAccount = async (userId) => {
    const { data } = await apiClient.post(`/User/${userId}/request-delete-account`);
    return data;
};

export const confirmDeleteAccount = async (userId, code) => {
    const { data } = await apiClient.post(`/User/${userId}/confirm-delete-account`, { Code: code?.trim() });
    return data;
};

// ---- Library (user games) ----
export const fetchUserLibrary = async (userId, status = null) => {
    if (!userId) return [];
    const { data } = await apiClient.get(`/Library/user/${userId}`, {
        params: status ? { status } : {},
    });
    return data;
};

export const addGameToLibrary = async (userId, game, status = 'PlanToPlay') => {
    if (!userId || !game) throw new Error('userId and game required');
    const id = Number(game.id);
    if (!Number.isFinite(id) || id <= 0) throw new Error('game.id required');
    // Tam RAWG gövdesi yerine sadece DB sütunları — büyük JSON / serileştirme sorunlarını önler
    const payload = {
        id,
        name: game.name ?? 'Unknown',
        background_image: game.background_image ?? '',
    };
    const { data } = await apiClient.post(`/Library/user/${userId}/add`, payload, {
        params: { status },
    });
    return data;
};

export const removeGameFromLibrary = async (userId, gameId) => {
    if (!userId || !gameId) throw new Error('userId and gameId required');
    const { data } = await apiClient.delete(`/Library/user/${userId}/remove/${gameId}`);
    return data;
};

export const updateGameStatus = async (userId, gameId, newStatus) => {
    if (!userId || !gameId || !newStatus) throw new Error('userId, gameId and newStatus required');
    const { data } = await apiClient.put(`/Library/user/${userId}/status/${gameId}`, {
        NewStatus: newStatus,
    });
    return data;
};

// ---- Oyun katalogu (liste/arama: IGDB; detay: IGDB + RAWG tamamlayıcı) ----
export const getGameDetails = async (id) => {
    if (!id) return null;
    const { data } = await apiClient.get(`/Library/game/${id}`);
    return data;
};

export const getGameScreenshots = async (gameId) => {
    if (!gameId) return [];
    const { data } = await apiClient.get(`/Library/game/${gameId}/screenshots`);
    return data ?? [];
};

// offset: kaç oyun atla; nsfw: tercihe göre (localStorage)
export const getPopularGames = async (offset = 0, nsfw = false) => {
    const { data } = await apiClient.get('/Library/popular', {
        params: { offset, nsfw },
    });
    return data;
};

/** Gemini + çoklu IGDB araması uzun sürebilir (varsayılan axios 15sn yetmeyebilir). */
export const getRecommendations = async (games) => {
    if (!games || games.length === 0) return [];
    const { data } = await apiClient.post('/Library/recommendations', games, { timeout: 120000 });
    return data;
};

export const searchGames = async (query, size = 40, nsfw = false) => {
    if (!query) return [];
    const { data } = await apiClient.get('/Library/search', {
        params: { query, size, nsfw },
    });
    return data;
};

export const getDiscoverGames = async ({ genre, mode, page }, nsfw = false) => {
    const { data } = await apiClient.get('/Library/discover', {
        params: { genre: genre ?? undefined, mode, page, nsfw },
    });
    return data;
};
