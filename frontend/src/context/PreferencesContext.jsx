import { createContext, useContext, useState, useCallback, useEffect } from 'react';

const STORAGE_KEY = 'gt_preferences';
/** popularListMode: 'scroll' = sonsuz kaydırma; 'paged' = sayfa sayfa (Popüler) */
const DEFAULTS = { startPage: 'Home', showNsfw: false, popularListMode: 'scroll', locale: 'tr' };

function loadPreferences() {
    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (!raw) return DEFAULTS;
        const parsed = JSON.parse(raw);
        const merged = { ...DEFAULTS, ...parsed };
        merged.locale = merged.locale === 'en' ? 'en' : 'tr';
        return merged;
    } catch {
        return DEFAULTS;
    }
}

function savePreferences(prefs) {
    try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(prefs));
    } catch {
        /* localStorage dolu veya devre dışı */
    }
}

const PreferencesContext = createContext(null);

export function PreferencesProvider({ children }) {
    const [preferences, setPreferencesState] = useState(loadPreferences);

    useEffect(() => {
        savePreferences(preferences);
    }, [preferences]);

    const setStartPage = useCallback((startPage) => {
        setPreferencesState((p) => ({ ...p, startPage: startPage === 'Library' ? 'Library' : 'Home' }));
    }, []);

    const setShowNsfw = useCallback((showNsfw) => {
        setPreferencesState((p) => ({ ...p, showNsfw: !!showNsfw }));
    }, []);

    const setPopularListMode = useCallback((mode) => {
        setPreferencesState((p) => ({
            ...p,
            popularListMode: mode === 'paged' ? 'paged' : 'scroll',
        }));
    }, []);

    const setLocale = useCallback((locale) => {
        setPreferencesState((p) => ({
            ...p,
            locale: locale === 'en' ? 'en' : 'tr',
        }));
    }, []);

    useEffect(() => {
        if (typeof document !== 'undefined') {
            document.documentElement.lang = preferences.locale === 'en' ? 'en' : 'tr';
        }
    }, [preferences.locale]);

    const value = {
        startPage: preferences.startPage,
        showNsfw: preferences.showNsfw,
        popularListMode: preferences.popularListMode === 'paged' ? 'paged' : 'scroll',
        locale: preferences.locale === 'en' ? 'en' : 'tr',
        setStartPage,
        setShowNsfw,
        setPopularListMode,
        setLocale,
    };

    return (
        <PreferencesContext.Provider value={value}>
            {children}
        </PreferencesContext.Provider>
    );
}

export const usePreferences = () => useContext(PreferencesContext);
