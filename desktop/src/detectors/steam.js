// Steam yüklü oyun algılama.
// 1) Kurulum kökünü registry'den bul (HKCU\Software\Valve\Steam).
// 2) libraryfolders.vdf'den tüm kütüphane klasörlerini topla.
// 3) Her kütüphanede steamapps/appmanifest_*.acf dosyalarındaki oyunları oku.

'use strict';

const fs = require('node:fs');
const path = require('node:path');
const os = require('node:os');
const { spawnSync } = require('node:child_process');
const { parseVdf } = require('../lib/vdf');

function stripUtf8Bom(text) {
    if (typeof text !== 'string' || text.length === 0) return text;
    return text.charCodeAt(0) === 0xfeff ? text.slice(1) : text;
}

/** Registry / VDF’den gelen yolu Windows’ta güvenilir biçime getirir ve varlığını doğrular. */
function normalizeExistingDir(p) {
    if (!p || typeof p !== 'string') return null;
    let x = p.trim().replace(/^["']|["']$/g, '');
    x = x.replace(/\//g, path.sep);
    try {
        x = path.normalize(x);
        return fs.existsSync(x) ? x : null;
    } catch {
        return null;
    }
}

function regQueryValue(keyPath, valueName) {
    const res = spawnSync('reg', ['query', keyPath, '/v', valueName], { encoding: 'utf8', windowsHide: true });
    if (res.status !== 0) return null;
    const line = (res.stdout || '').split(/\r?\n/).find((l) => l.includes(valueName));
    if (!line) return null;
    const match = line.match(/REG_\w+\s+(.+)$/);
    return match ? match[1].trim() : null;
}

function findSteamRoot() {
    if (process.platform !== 'win32') return null;
    const candidates = [
        ['HKCU\\Software\\Valve\\Steam', 'SteamPath'],
        ['HKLM\\SOFTWARE\\WOW6432Node\\Valve\\Steam', 'InstallPath'],
        ['HKLM\\SOFTWARE\\Valve\\Steam', 'InstallPath'],
    ];
    for (const [key, value] of candidates) {
        const raw = regQueryValue(key, value);
        const p = normalizeExistingDir(raw);
        if (p && fs.existsSync(path.join(p, 'steam.exe'))) return p;
    }
    const pf86 = process.env['ProgramFiles(x86)'] || 'C:\\Program Files (x86)';
    const pf = process.env.ProgramFiles || 'C:\\Program Files';
    const fallbacks = [
        path.join(pf86, 'Steam'),
        path.join(pf, 'Steam'),
        'D:\\Steam',
        'C:\\Steam',
        path.join(os.homedir(), 'Steam'),
    ];
    for (const f of fallbacks) {
        const p = normalizeExistingDir(f);
        if (p && fs.existsSync(path.join(p, 'steam.exe'))) return p;
    }
    return null;
}

function mergeLibraryFoldersVdf(rawText, libs) {
    let parsed;
    try {
        parsed = parseVdf(stripUtf8Bom(rawText));
    } catch {
        return;
    }
    const roots = [
        parsed.libraryfolders,
        parsed.LibraryFolders,
        parsed.libraryfolder,
    ].filter(Boolean);
    for (const container of roots) {
        if (!container || typeof container !== 'object') continue;
        for (const value of Object.values(container)) {
            if (!value) continue;
            if (typeof value === 'string') {
                const base = normalizeExistingDir(value);
                if (base) libs.add(path.join(base, 'steamapps'));
                continue;
            }
            if (typeof value === 'object') {
                const rawPath = value.path || value.Path;
                if (rawPath) {
                    const base = normalizeExistingDir(String(rawPath));
                    if (base) libs.add(path.join(base, 'steamapps'));
                }
            }
        }
    }
}

function findLibraryFolders(steamRoot) {
    const libs = new Set();
    const primary = path.join(steamRoot, 'steamapps');
    if (fs.existsSync(primary)) libs.add(primary);

    const vdfPaths = [
        path.join(steamRoot, 'steamapps', 'libraryfolders.vdf'),
        path.join(steamRoot, 'config', 'libraryfolders.vdf'),
    ];
    for (const vdfPath of vdfPaths) {
        try {
            if (!fs.existsSync(vdfPath)) continue;
            mergeLibraryFoldersVdf(fs.readFileSync(vdfPath, 'utf8'), libs);
        } catch { /* bozuk dosya → atla */ }
    }

    return [...libs];
}

function readAppManifest(filePath) {
    try {
        const raw = stripUtf8Bom(fs.readFileSync(filePath, 'utf8'));
        const parsed = parseVdf(raw);
        const state = parsed.AppState || parsed.appstate;
        if (!state || typeof state !== 'object') return null;
        const appId = state.appid ?? state.AppID;
        const name = state.name ?? state.Name;
        const installDir = state.installdir ?? state.InstallDir;
        if (appId == null || name == null || installDir == null) return null;
        return { appId: String(appId), name: String(name), installDir: String(installDir) };
    } catch { return null; }
}

async function detect() {
    if (process.platform !== 'win32') return [];
    const steamRoot = findSteamRoot();
    if (!steamRoot) return [];

    const games = [];
    const libs = findLibraryFolders(steamRoot);
    for (const lib of libs) {
        let entries;
        try { entries = fs.readdirSync(lib); } catch { continue; }
        for (const entry of entries) {
            if (!entry.startsWith('appmanifest_') || !entry.endsWith('.acf')) continue;
            const manifest = readAppManifest(path.join(lib, entry));
            if (!manifest) continue;

            // Launcher / tools / redistributable'ları dışla.
            const nameLower = manifest.name.toLowerCase();
            if (/^(steamworks|proton|steam linux runtime|steam runtime)/.test(nameLower)) continue;

            const installPath = path.join(lib, 'common', manifest.installDir);
            games.push({
                platform: 'steam',
                platformId: manifest.appId,
                name: manifest.name,
                installDir: installPath,
                // Oyunun birincil .exe'si launcher'dan başlatıldığı için manifest'ten bilinmez;
                // tracker installDir altındaki .exe'leri process adına göre eşleştirir.
                launchExe: null,
            });
        }
    }
    return games;
}

module.exports = { detect };
