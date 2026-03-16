import axios from 'axios';

const apiClient = axios.create({
    baseURL: '/api',
    timeout: 15000,
});

// ---- User ----
export const registerUser = async ({ username, email, password }) => {
    const { data } = await apiClient.post('/User/register', {
        Username: username,
        Email: email,
        Password: password,
    });
    return data;
};

export const loginUser = async (username, password) => {
    const { data } = await apiClient.post('/User/login', { EmailOrUsername: username, password });
    return data;
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
    const { data } = await apiClient.post(`/Library/user/${userId}/add`, game, {
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

// ---- Games (RAWG) ----
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

export const getRecommendations = async (games) => {
    if (!games || games.length === 0) return [];
    const { data } = await apiClient.post('/Library/recommendations', games);
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
