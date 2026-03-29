/**
 * Oturum nesnesinden sayısal kullanıcı id (API camelCase userId / PascalCase UserId / id).
 */
export function getSessionUserId(user) {
    if (!user || typeof user !== 'object') return undefined;
    const raw = user.id ?? user.UserId ?? user.userId;
    if (raw == null || raw === '') return undefined;
    const n = Number(raw);
    return Number.isFinite(n) && n > 0 ? n : undefined;
}
