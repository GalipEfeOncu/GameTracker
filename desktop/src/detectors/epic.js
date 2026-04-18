// Epic Games Launcher yüklü oyun algılama.
// Launcher, kurulu her oyun için C:\ProgramData\Epic\EpicGamesLauncher\Data\Manifests\*.item
// JSON dosyaları üretir; LaunchExecutable + InstallLocation gibi alanlar oyun başına vardır.

'use strict';

const fs = require('node:fs');
const path = require('node:path');

function manifestsDir() {
    const programData = process.env.ProgramData || 'C:\\ProgramData';
    return path.join(programData, 'Epic', 'EpicGamesLauncher', 'Data', 'Manifests');
}

async function detect() {
    if (process.platform !== 'win32') return [];
    const dir = manifestsDir();
    if (!fs.existsSync(dir)) return [];

    let entries;
    try { entries = fs.readdirSync(dir); } catch { return []; }

    const games = [];
    for (const entry of entries) {
        if (!entry.toLowerCase().endsWith('.item')) continue;
        try {
            let raw = fs.readFileSync(path.join(dir, entry), 'utf8');
            if (raw.charCodeAt(0) === 0xfeff) raw = raw.slice(1);
            const json = JSON.parse(raw);
            const name = json.DisplayName || json.AppName;
            const installLocation = json.InstallLocation;
            const launchExec = json.LaunchExecutable;
            if (!name || !installLocation) continue;

            // Launcher'ın kendisi / plugin'ler genelde AppCategories'te 'plugin' vb. içerir; atla.
            const categories = Array.isArray(json.AppCategories) ? json.AppCategories.map((c) => String(c).toLowerCase()) : [];
            if (categories.includes('plugins') || categories.includes('plugin') || categories.includes('engine')) continue;

            games.push({
                platform: 'epic',
                platformId: json.CatalogItemId || json.AppName,
                name,
                installDir: installLocation,
                launchExe: launchExec ? path.join(installLocation, launchExec) : null,
            });
        } catch { /* bozuk manifest → atla */ }
    }
    return games;
}

module.exports = { detect };
