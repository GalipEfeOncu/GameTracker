import tr from './tr.json';
import en from './en.json';

const STORAGE_KEY = 'gt_preferences';

const dicts = { tr, en };

function lookup(locale, key) {
    const d = dicts[locale] || dicts.tr;
    const parts = key.split('.');
    let node = d;
    for (const p of parts) {
        node = node?.[p];
        if (node === undefined) return undefined;
    }
    return typeof node === 'string' ? node : undefined;
}

/**
 * @param {'tr'|'en'} locale
 * @param {string} key dot.path
 * @param {Record<string, string | number>} [vars]
 */
export function translate(locale, key, vars = {}) {
    let str = lookup(locale, key);
    if (str === undefined && locale !== 'tr') str = lookup('tr', key);
    if (str === undefined) return key;
    return str.replace(/\{\{(\w+)\}\}/g, (_, k) =>
        vars[k] != null ? String(vars[k]) : '',
    );
}

export function getStoredLocale() {
    if (typeof window === 'undefined') return 'tr';
    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (!raw) return 'tr';
        const p = JSON.parse(raw);
        return p.locale === 'en' ? 'en' : 'tr';
    } catch {
        return 'tr';
    }
}
