import { useCallback } from 'react';
import { usePreferences } from '../context/PreferencesContext';
import { translate } from './t';

export function useI18n() {
    const { locale, setLocale } = usePreferences();
    const t = useCallback((key, vars) => translate(locale, key, vars), [locale]);
    return { t, locale, setLocale };
}
