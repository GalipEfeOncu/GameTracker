// Kalıcı ayarlar (tray, autostart, oyun eşlemeleri) için basit JSON dosyası.
// app.getPath('userData') altında saklanır; kullanıcı başına izole.

'use strict';

const fs = require('node:fs');
const path = require('node:path');

function createStore(fileName, app, defaults = {}) {
    const filePath = path.join(app.getPath('userData'), fileName);
    let cache = null;

    function load() {
        if (cache) return cache;
        try {
            if (fs.existsSync(filePath)) {
                const raw = fs.readFileSync(filePath, 'utf8');
                cache = { ...defaults, ...JSON.parse(raw) };
                return cache;
            }
        } catch { /* bozuk dosya → default'a dön */ }
        cache = { ...defaults };
        return cache;
    }

    function save() {
        try {
            fs.mkdirSync(path.dirname(filePath), { recursive: true });
            fs.writeFileSync(filePath, JSON.stringify(cache ?? defaults, null, 2), 'utf8');
        } catch (err) {
            console.error('[store] save failed:', err.message);
        }
    }

    return {
        get(key) { return load()[key]; },
        getAll() { return { ...load() }; },
        set(key, value) { load()[key] = value; save(); },
        patch(obj) { Object.assign(load(), obj); save(); },
        path: filePath,
    };
}

module.exports = { createStore };
