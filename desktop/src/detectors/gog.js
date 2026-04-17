// GOG Galaxy yüklü oyun algılama.
// GOG her oyunu HKLM\SOFTWARE\WOW6432Node\GOG.com\Games\<gameId> altında kayıt eder.
// Tek `reg query /s` çağrısı ile tüm alt anahtarları + değerleri tek seferde alır.

'use strict';

const fs = require('node:fs');
const path = require('node:path');
const { queryKey } = require('../lib/registry');

const ROOTS = [
    'HKLM\\SOFTWARE\\WOW6432Node\\GOG.com\\Games',
    'HKLM\\SOFTWARE\\GOG.com\\Games',
];

// `reg query ... /s` çıktısını ayrıştırır: her oyun anahtarı başlık satırıdır,
// altındaki "    name    REG_SZ    value" satırları o anahtarın değerleridir.
function parseRegDump(text) {
    const sections = {};
    if (!text) return sections;
    let current = null;
    for (const rawLine of text.split(/\r?\n/)) {
        const line = rawLine.trimEnd();
        if (!line) { current = null; continue; }
        if (line.toUpperCase().startsWith('HKEY')) {
            const parts = line.split('\\');
            const leaf = parts[parts.length - 1];
            if (/^\d+$/.test(leaf) || /[A-Z0-9_\-]/i.test(leaf)) {
                current = leaf;
                sections[current] = sections[current] || {};
            } else {
                current = null;
            }
            continue;
        }
        if (!current) continue;
        const match = line.match(/^\s{4}(\S.*?)\s{2,}REG_\w+\s{2,}(.*)$/);
        if (match) sections[current][match[1]] = match[2];
    }
    return sections;
}

async function detect() {
    if (process.platform !== 'win32') return [];
    const games = [];
    const seen = new Set();

    for (const root of ROOTS) {
        const dump = queryKey(root);
        if (!dump) continue;
        const entries = parseRegDump(dump);
        for (const [gameId, values] of Object.entries(entries)) {
            if (seen.has(gameId)) continue;
            const name = values.gameName || values.GameName;
            const installDir = values.path || values.PATH || values.installDir;
            const exe = values.exe || values.EXE || values.launchExe;
            if (!name || !installDir) continue;
            if (!fs.existsSync(installDir)) continue;

            seen.add(gameId);
            games.push({
                platform: 'gog',
                platformId: gameId,
                name,
                installDir,
                launchExe: exe ? (path.isAbsolute(exe) ? exe : path.join(installDir, exe)) : null,
            });
        }
    }
    return games;
}

module.exports = { detect };
