// Steam yüklü oyun algılama.
// 1) Kurulum kökünü registry'den bul (HKCU\Software\Valve\Steam).
// 2) libraryfolders.vdf'den tüm kütüphane klasörlerini topla.
// 3) Her kütüphanede steamapps/appmanifest_*.acf dosyalarındaki oyunları oku.

'use strict';

const fs = require('node:fs');
const path = require('node:path');
const { spawnSync } = require('node:child_process');
const { parseVdf } = require('../lib/vdf');

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
        const p = regQueryValue(key, value);
        if (p && fs.existsSync(p)) return p;
    }
    // Fallback: varsayılan yol
    const def = 'C:\\Program Files (x86)\\Steam';
    return fs.existsSync(def) ? def : null;
}

function findLibraryFolders(steamRoot) {
    const libs = new Set([path.join(steamRoot, 'steamapps')]);
    const vdfPath = path.join(steamRoot, 'steamapps', 'libraryfolders.vdf');
    try {
        if (!fs.existsSync(vdfPath)) return [...libs];
        const parsed = parseVdf(fs.readFileSync(vdfPath, 'utf8'));
        const container = parsed.libraryfolders || parsed.LibraryFolders;
        if (container && typeof container === 'object') {
            for (const value of Object.values(container)) {
                if (value && typeof value === 'object' && value.path) {
                    libs.add(path.join(value.path, 'steamapps'));
                } else if (typeof value === 'string' && fs.existsSync(value)) {
                    libs.add(path.join(value, 'steamapps'));
                }
            }
        }
    } catch { /* bozuk vdf → varsayılan kullan */ }
    return [...libs];
}

function readAppManifest(filePath) {
    try {
        const parsed = parseVdf(fs.readFileSync(filePath, 'utf8'));
        const state = parsed.AppState || parsed.appstate;
        if (!state || typeof state !== 'object') return null;
        const appId = state.appid;
        const name = state.name;
        const installDir = state.installdir;
        if (!appId || !name || !installDir) return null;
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
