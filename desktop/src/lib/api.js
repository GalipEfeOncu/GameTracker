// Backend'e playtime heartbeat göndermek için hafif HTTP wrapper.
// Electron 34 Node 20 tabanlıdır; global fetch doğrudan kullanılır.

'use strict';

function normalizeBase(base) {
    if (!base) return '';
    return String(base).replace(/\/+$/, '');
}

function createApi({ baseUrlProvider, tokenProvider }) {
    async function sendPlaytime({ userId, gameId, minutesDelta }) {
        const base = normalizeBase(baseUrlProvider());
        if (!base) throw new Error('API_BASE_URL not configured');
        const token = tokenProvider();
        if (!token) throw new Error('no_access_token');

        const url = `${base}/Library/user/${userId}/playtime/${gameId}`;
        const res = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                Authorization: `Bearer ${token}`,
            },
            body: JSON.stringify({ MinutesDelta: minutesDelta }),
        });
        if (!res.ok) {
            const text = await res.text().catch(() => '');
            throw new Error(`playtime POST ${res.status}: ${text.slice(0, 200)}`);
        }
        return res.json().catch(() => ({}));
    }

    return { sendPlaytime };
}

module.exports = { createApi };
