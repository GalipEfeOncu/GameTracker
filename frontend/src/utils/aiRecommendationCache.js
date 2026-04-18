/**
 * AI öneri sonuçlarını sayfa yenilemesinde / route değişiminde korumak ve
 * kütüphane değişene kadar yeniden Gemini çağrısını engellemek için sessionStorage.
 */

export function aiRecommendationCacheKey(userId, fingerprint) {
    return `gt_ai_cache_v1_${userId}_${fingerprint}`;
}

export function aiRecommendationLockKey(userId) {
    return `gt_ai_lock_fp_v1_${userId}`;
}

export function readAiRecommendationCache(userId, fingerprint) {
    if (userId == null || !fingerprint) return undefined;
    try {
        const raw = sessionStorage.getItem(aiRecommendationCacheKey(userId, fingerprint));
        if (raw == null) return undefined;
        return JSON.parse(raw);
    } catch {
        return undefined;
    }
}

export function writeAiRecommendationCache(userId, fingerprint, suggestionsPayload) {
    if (userId == null || !fingerprint) return;
    try {
        sessionStorage.setItem(aiRecommendationCacheKey(userId, fingerprint), JSON.stringify(suggestionsPayload));
        sessionStorage.setItem(aiRecommendationLockKey(userId), fingerprint);
    } catch {
        /* storage full or private mode */
    }
}

export function readLockedLibraryFingerprint(userId) {
    if (userId == null) return null;
    try {
        return sessionStorage.getItem(aiRecommendationLockKey(userId));
    } catch {
        return null;
    }
}
