import { createContext, useContext, useState, useCallback, useEffect } from 'react';

const STORAGE_KEY = 'gt_preferences';
const DEFAULTS = { startPage: 'Home', showNsfw: false };

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
    } catch (_) {}
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

    const value = {
        startPage: preferences.startPage,
        showNsfw: preferences.showNsfw,
        setStartPage,
        setShowNsfw,
    };

    return (
        <PreferencesContext.Provider value={value}>
            {children}
        </PreferencesContext.Provider>
    );
}

export const usePreferences = () => useContext(PreferencesContext);
