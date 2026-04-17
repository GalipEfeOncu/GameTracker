// Playtime tracker: tanınan oyun process'leri için arka planda oturum sayar
// ve her HEARTBEAT_MS'de backend'e artışı gönderir.
//
// Mimari:
// - `mappings` yerel bir listedir: [{ gameId, exeNames: ['elden ring.exe'], installDirs: [...] }]
// - Her SCAN_MS'de çalışan .exe listesi alınır (tasklist).
// - Her mapping için şu an çalışan var mı kontrol edilir; var ise "aktif oturum" süresi ilerler.
// - Biriken süre HEARTBEAT_MS'de dakikaya dönüştürülüp backend'e delta olarak POST edilir; tam dakikalar tüketilir, kalan saniyeler sonraki heartbeat'e devreder.
// - App çöker / bilgisayar kapanırsa en kötü son HEARTBEAT_MS kadar süre kaybolur.

'use strict';

const { spawn } = require('node:child_process');
const path = require('node:path');

const SCAN_MS = 5000;        // 5 sn: çalışan process taraması
const HEARTBEAT_MS = 30000;  // 30 sn: backend'e sync

function normalizeExe(name) {
    if (!name) return '';
    return String(name).trim().toLowerCase();
}

// tasklist CSV: "Image Name","PID","Session Name","Session#","Mem Usage"
function parseTasklist(text) {
    const running = new Set();
    if (!text) return running;
    const lines = text.split(/\r?\n/);
    for (const line of lines) {
        const match = line.match(/^"([^"]+)"/);
        if (match) running.add(match[1].toLowerCase());
    }
    return running;
}

function listRunningExes() {
    return new Promise((resolve) => {
        if (process.platform !== 'win32') { resolve(new Set()); return; }
        const proc = spawn('tasklist', ['/FO', 'CSV', '/NH'], { windowsHide: true });
        let stdout = '';
        proc.stdout.on('data', (chunk) => { stdout += chunk.toString(); });
        proc.on('error', () => resolve(new Set()));
        proc.on('close', () => resolve(parseTasklist(stdout)));
    });
}

function createTracker({ api, store, getUser, onUpdate }) {
    // mappings: Array<{ gameId, gameName, exeNames: string[] }>
    let mappings = (store.get('mappings') || []).map((m) => ({
        gameId: Number(m.gameId),
        gameName: String(m.gameName || ''),
        exeNames: (m.exeNames || []).map(normalizeExe).filter(Boolean),
    }));

    // aktif oturumlar: gameId -> { accumulatedMs, lastSeenAt, lastHeartbeatMs }
    const active = new Map();
    let scanTimer = null;
    let heartbeatTimer = null;
    let running = false;

    function persistMappings() {
        store.set('mappings', mappings.map((m) => ({
            gameId: m.gameId, gameName: m.gameName, exeNames: m.exeNames,
        })));
    }

    function setMappings(next) {
        mappings = (next || []).map((m) => ({
            gameId: Number(m.gameId),
            gameName: String(m.gameName || ''),
            exeNames: (m.exeNames || []).map(normalizeExe).filter(Boolean),
        })).filter((m) => m.gameId > 0 && m.exeNames.length > 0);
        persistMappings();
    }

    function upsertMapping(entry) {
        const next = mappings.filter((m) => m.gameId !== Number(entry.gameId));
        next.push({
            gameId: Number(entry.gameId),
            gameName: String(entry.gameName || ''),
            exeNames: (entry.exeNames || []).map(normalizeExe).filter(Boolean),
        });
        mappings = next;
        persistMappings();
    }

    function removeMapping(gameId) {
        mappings = mappings.filter((m) => m.gameId !== Number(gameId));
        persistMappings();
    }

    async function scanOnce() {
        if (!mappings.length) return;
        const running = await listRunningExes();
        const now = Date.now();

        for (const m of mappings) {
            const isRunning = m.exeNames.some((e) => running.has(e));
            const prev = active.get(m.gameId);
            if (isRunning) {
                if (prev) {
                    prev.accumulatedMs += now - prev.lastSeenAt;
                    prev.lastSeenAt = now;
                } else {
                    active.set(m.gameId, {
                        accumulatedMs: 0,
                        lastSeenAt: now,
                        carrySeconds: 0,
                        gameName: m.gameName,
                    });
                }
            } else if (prev) {
                // Oturum bitti; birikeni korudur (heartbeat son kalanı göndersin, sonra temizlensin).
                // Kısaysa (< 30 sn) dakika yuvarlamasında 0 olur — pratikte sorun değil.
                prev.ended = true;
            }
        }
    }

    async function flushHeartbeat() {
        const user = getUser();
        if (!user?.userId || !user?.accessToken) return;

        for (const [gameId, session] of [...active.entries()]) {
            const totalSec = Math.floor(session.accumulatedMs / 1000) + session.carrySeconds;
            const minutes = Math.floor(totalSec / 60);
            if (minutes > 0) {
                try {
                    const result = await api.sendPlaytime({ userId: user.userId, gameId, minutesDelta: minutes });
                    if (onUpdate) onUpdate({ gameId, minutesDelta: minutes, total: result?.playtimeMinutes });
                    session.accumulatedMs = 0;
                    session.carrySeconds = totalSec - minutes * 60;
                } catch (err) {
                    // Ağ hatası → bir sonraki heartbeat'te tekrar denenir; birikim kaybolmaz.
                    console.warn('[tracker] heartbeat failed:', err.message);
                }
            }
            if (session.ended && session.accumulatedMs === 0 && session.carrySeconds < 60) {
                active.delete(gameId);
            }
        }
    }

    function start() {
        if (running) return;
        running = true;
        scanTimer = setInterval(() => { scanOnce().catch(() => {}); }, SCAN_MS);
        heartbeatTimer = setInterval(() => { flushHeartbeat().catch(() => {}); }, HEARTBEAT_MS);
    }

    function stop() {
        if (!running) return;
        running = false;
        clearInterval(scanTimer); scanTimer = null;
        clearInterval(heartbeatTimer); heartbeatTimer = null;
    }

    async function shutdown() {
        // Kapanmadan önce son heartbeat'i gönder (kısa da olsa kalanları kurtar).
        try { await flushHeartbeat(); } catch { /* ignore */ }
        stop();
    }

    return {
        start, stop, shutdown,
        getMappings: () => mappings.slice(),
        setMappings, upsertMapping, removeMapping,
        // Debug / UI için: şu an aktif session var mı?
        getActive: () => [...active.entries()].map(([gameId, s]) => ({ gameId, gameName: s.gameName, accumulatedMs: s.accumulatedMs })),
    };
}

module.exports = { createTracker };

// Basit utility: klasördeki .exe'lerin dosya adlarını verir (launch exe bilinmeyen Steam oyunları için).
module.exports.listExesInDir = function listExesInDir(fs, dir, maxDepth = 2) {
    const out = new Set();
    function walk(current, depth) {
        if (depth > maxDepth) return;
        let entries;
        try { entries = fs.readdirSync(current, { withFileTypes: true }); } catch { return; }
        for (const entry of entries) {
            const full = path.join(current, entry.name);
            if (entry.isDirectory()) { walk(full, depth + 1); continue; }
            if (entry.isFile() && entry.name.toLowerCase().endsWith('.exe')) {
                out.add(entry.name.toLowerCase());
            }
        }
    }
    walk(dir, 0);
    return [...out];
};
