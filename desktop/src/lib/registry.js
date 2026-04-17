// Windows registry'den anahtar/değer okuyan hafif wrapper.
// `reg query` çağrısını spawn eder; ek npm bağımlılığı yok.

'use strict';

const { spawnSync } = require('node:child_process');

function queryKey(keyPath) {
    if (process.platform !== 'win32') return null;
    const result = spawnSync('reg', ['query', keyPath, '/s'], { encoding: 'utf8', windowsHide: true });
    if (result.status !== 0) return null;
    return result.stdout ?? '';
}

function querySubKeys(keyPath) {
    if (process.platform !== 'win32') return [];
    const result = spawnSync('reg', ['query', keyPath], { encoding: 'utf8', windowsHide: true });
    if (result.status !== 0) return [];
    const text = result.stdout ?? '';
    const lines = text.split(/\r?\n/);
    const subKeys = [];
    for (const line of lines) {
        const trimmed = line.trim();
        if (!trimmed) continue;
        if (trimmed.toUpperCase().startsWith('HKEY') && trimmed !== keyPath) {
            subKeys.push(trimmed);
        }
    }
    return subKeys;
}

// Bir kaydın içindeki tüm (name, type, value) üçlülerini döner.
function parseValues(text) {
    const out = {};
    if (!text) return out;
    const lines = text.split(/\r?\n/);
    for (const line of lines) {
        // Örnek: "    EXE    REG_SZ    C:\Games\Foo\Foo.exe"
        const match = line.match(/^\s{4}(\S.*?)\s{2,}(REG_\w+)\s{2,}(.*)$/);
        if (match) out[match[1]] = match[3];
    }
    return out;
}

module.exports = { queryKey, querySubKeys, parseValues };
