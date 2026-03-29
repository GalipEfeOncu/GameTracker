import { createContext, useContext, useState, useCallback, useEffect } from 'react';

const STORAGE_KEY = 'gt_preferences';
/** popularListMode: 'scroll' = sonsuz kaydırma; 'paged' = sayfa sayfa (Popüler) */
const DEFAULTS = { startPage: 'Home', showNsfw: false, popularListMode: 'scroll' };

function loadPreferences() {
    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (!raw) return DEFAULTS;
        const parsed = JSON.parse(raw);
        return { ...DEFAULTS, ...parsed };
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

    const value = {
        startPage: preferences.startPage,
        showNsfw: preferences.showNsfw,
        popularListMode: preferences.popularListMode === 'paged' ? 'paged' : 'scroll',
        setStartPage,
        setShowNsfw,
        setPopularListMode,
    };

    return (
        <PreferencesContext.Provider value={value}>
            {children}
        </PreferencesContext.Provider>
    );
}

export const usePreferences = () => useContext(PreferencesContext);
