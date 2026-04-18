/** Kart ve izleme satırları için kısa süre metni (dk / sa). */
export function formatTrackedMinutes(minutes, t) {
    const m = Number(minutes);
    if (!Number.isFinite(m) || m <= 0) return null;
    if (m < 60) return t('time.minutes', { n: m });
    const h = Math.floor(m / 60);
    const rem = m % 60;
    return rem ? t('time.hoursMinutes', { h, m: rem }) : t('time.hoursOnly', { h });
}
