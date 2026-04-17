// Minimal VDF (Valve's key-value) parser — Steam'in .acf ve libraryfolders.vdf dosyaları için.
// Gereksiz bir npm bağımlılığı eklememek amacıyla küçük tutulmuştur.
// Sadece iç içe objeler ve string/sayı değerler; diziler veya koşullar ("[$WIN32]" gibi) atlanır.

'use strict';

function parseVdf(text) {
    if (typeof text !== 'string') return {};
    const len = text.length;
    let i = 0;

    function skipWs() {
        while (i < len) {
            const c = text[i];
            if (c === ' ' || c === '\t' || c === '\r' || c === '\n') { i++; continue; }
            if (c === '/' && text[i + 1] === '/') {
                while (i < len && text[i] !== '\n') i++;
                continue;
            }
            break;
        }
    }

    function readToken() {
        skipWs();
        if (i >= len) return null;
        const c = text[i];
        if (c === '"') {
            i++;
            let out = '';
            while (i < len) {
                const ch = text[i];
                if (ch === '\\' && i + 1 < len) { out += text[i + 1]; i += 2; continue; }
                if (ch === '"') { i++; return out; }
                out += ch; i++;
            }
            return out;
        }
        if (c === '{' || c === '}') { i++; return c; }
        let out = '';
        while (i < len) {
            const ch = text[i];
            if (ch === ' ' || ch === '\t' || ch === '\r' || ch === '\n' || ch === '"' || ch === '{' || ch === '}') break;
            out += ch; i++;
        }
        return out;
    }

    function parseObject() {
        const obj = {};
        while (true) {
            const key = readToken();
            if (key === null || key === '}') return obj;
            const next = readToken();
            if (next === '{') {
                obj[key] = parseObject();
            } else {
                obj[key] = next ?? '';
            }
        }
    }

    const first = readToken();
    if (first === null) return {};
    const second = readToken();
    if (second === '{') {
        return { [first]: parseObject() };
    }
    return { [first]: second ?? '' };
}

module.exports = { parseVdf };
