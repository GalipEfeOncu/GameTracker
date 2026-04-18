/**
 * SPA'nın kullandığı API kökü (axios baseURL ile aynı mantık).
 * Electron ana süreç mutlak URL ister; .env boşken Vite proxy için origin + /api gerekir.
 */
export function getResolvedApiRoot() {
    const raw = typeof import.meta.env.VITE_API_BASE_URL === 'string'
        ? import.meta.env.VITE_API_BASE_URL.trim()
        : '';
    if (raw) return raw.replace(/\/+$/, '');
    if (typeof window !== 'undefined' && window.location?.protocol?.startsWith('http')) {
        return `${window.location.origin.replace(/\/+$/, '')}/api`;
    }
    return '';
}
