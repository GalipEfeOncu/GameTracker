import { useEffect, useMemo, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { HardDrive, Loader2, Plus, X, Search, AlertCircle, Trash2, ScanSearch } from 'lucide-react';
import {
    isDesktop,
    detectInstalledGames,
    listMappings,
    upsertMapping,
    removeMapping,
    pickExe,
} from '../desktop/bridge';
import { searchGames, addGameToLibrary, fetchUserLibrary } from '../api/apiClient';
import { useUser } from '../context/UserContext';
import { getSessionUserId } from '../utils/sessionUser';
import { emitToast } from '../utils/toastEvents';
import { useI18n } from '../i18n/useI18n';
import { formatTrackedMinutes } from '../i18n/playtime';

export default function InstalledGamesPage() {
    const { user } = useUser();
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const userId = getSessionUserId(user);
    const { t } = useI18n();

    const [mappings, setMappings] = useState([]);
    const [loading, setLoading] = useState(true);
    const [manualOpen, setManualOpen] = useState(false);
    const [autoScanning, setAutoScanning] = useState(false);
    const [scanProgress, setScanProgress] = useState(null);

    const loadMappings = useCallback(async () => {
        setLoading(true);
        try {
            const maps = await listMappings();
            setMappings(maps);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        if (!isDesktop) {
            setLoading(false);
            return;
        }
        loadMappings();
    }, [loadMappings]);

    const runAutoScanAndLink = useCallback(async () => {
        if (!userId) return;
        setAutoScanning(true);
        setScanProgress(null);
        try {
            const detectedList = await detectInstalledGames();
            let mappedIds = new Set((await listMappings()).map((m) => m.gameId));

            if (detectedList.length === 0) {
                emitToast(t('installed.toastNoGames'), 'error');
                return;
            }

            let linked = 0;
            let skipped = 0;
            let failed = 0;

            for (let i = 0; i < detectedList.length; i++) {
                const d = detectedList[i];
                setScanProgress({ current: i + 1, total: detectedList.length });

                let results = [];
                try {
                    results = await fetchCatalogMatches(d.name, searchGames);
                } catch {
                    failed++;
                    continue;
                }

                const match = pickBestCatalogMatch(d.name, results);
                if (!match) {
                    failed++;
                    continue;
                }

                if (mappedIds.has(match.id)) {
                    skipped++;
                    continue;
                }

                try {
                    await addGameToLibrary(
                        userId,
                        {
                            id: match.id,
                            name: match.name,
                            background_image: match.background_image || '',
                        },
                        'Playing',
                    );
                } catch {
                    /* çoğu zaman zaten kütüphanede */
                }

                const ok = await upsertMapping({
                    gameId: match.id,
                    gameName: match.name,
                    exeNames: [],
                    installDir: d.installDir,
                });
                if (!ok) {
                    failed++;
                    continue;
                }

                mappedIds.add(match.id);
                linked++;
                await new Promise((r) => setTimeout(r, 150));
            }

            setMappings(await listMappings());
            queryClient.invalidateQueries({ queryKey: ['library', userId] });

            const msg = t('installed.toastScanDone', { linked, skipped, failed });
            emitToast(msg, linked > 0 ? 'success' : 'info');
        } finally {
            setScanProgress(null);
            setAutoScanning(false);
        }
    }, [userId, queryClient, t]);

    const { data: library } = useQuery({
        queryKey: ['library', userId, null],
        queryFn: () => fetchUserLibrary(userId, null),
        enabled: !!userId && isDesktop,
    });

    const libraryById = useMemo(() => {
        const m = new Map();
        (library || []).forEach((g) => m.set(g.id, g));
        return m;
    }, [library]);

    if (!isDesktop) {
        return (
            <div className="flex flex-col items-center justify-center h-full py-40 text-center">
                <HardDrive size={56} className="mb-6 opacity-40 text-blue-500" />
                <h2 className="text-xl font-bold text-white">{t('installed.desktopOnlyTitle')}</h2>
                    <p className="mt-2 text-gray-500 max-w-md">
                    {t('installed.desktopOnlyHint')}
                </p>
            </div>
        );
    }

    if (!user) {
        return (
            <div className="flex flex-col items-center justify-center h-full py-40">
                <HardDrive size={56} className="mb-6 opacity-40 text-blue-500" />
                <h2 className="text-xl font-bold text-white">{t('installed.loginTitle')}</h2>
                <button
                    type="button"
                    onClick={() => navigate('/login')}
                    className="mt-6 px-6 py-2.5 bg-blue-600 hover:bg-blue-500 text-white font-semibold text-sm"
                >
                    {t('nav.login')}
                </button>
            </div>
        );
    }

    return (
        <div className="h-full overflow-y-auto px-8 pt-8 pb-20">
            <div className="mb-8 flex flex-wrap items-center justify-between gap-3">
                <div>
                    <h1 className="text-2xl font-bold text-white tracking-tight">{t('installed.pageTitle')}</h1>
                    <p className="text-sm text-gray-500 mt-1">
                        {t('installed.intro')}
                    </p>
                </div>
                <div className="flex flex-wrap gap-2">
                    <button
                        type="button"
                        onClick={() => loadMappings()}
                        disabled={loading || autoScanning}
                        className="px-4 py-2 border border-[#1f2334] bg-[#141722] hover:bg-[#1a1e2d] text-gray-300 text-sm font-medium disabled:opacity-40"
                    >
                        {t('common.refresh')}
                    </button>
                    <button
                        type="button"
                        onClick={() => setManualOpen(true)}
                        disabled={autoScanning}
                        className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-500 text-white text-sm font-semibold disabled:opacity-40"
                    >
                        <Plus size={16} /> {t('installed.manualAdd')}
                    </button>
                </div>
            </div>

            {loading ? (
                <div className="flex justify-center py-24 text-gray-500">
                    <Loader2 size={32} className="animate-spin" />
                </div>
            ) : (
                <>
                    <Section title={t('installed.sectionAutoTitle')} subtitle={t('installed.sectionAutoSubtitle')}>
                        <div className="border border-[#1f2334] bg-[#141722] px-4 py-5 space-y-4">
                            <p className="text-sm text-gray-400 leading-relaxed">
                                {t('installed.autoBody')}
                            </p>
                            <button
                                type="button"
                                disabled={autoScanning}
                                onClick={runAutoScanAndLink}
                                className="inline-flex w-full items-center justify-center gap-2 px-4 py-3 border border-blue-500/35 bg-blue-600/15 hover:bg-blue-600/25 text-blue-300 text-sm font-semibold disabled:opacity-50 disabled:cursor-not-allowed sm:w-auto"
                            >
                                {autoScanning ? (
                                    <>
                                        <Loader2 size={18} className="animate-spin shrink-0" />
                                        <span>
                                            {scanProgress
                                                ? t('installed.scanConnecting', {
                                                      current: scanProgress.current,
                                                      total: scanProgress.total,
                                                  })
                                                : t('installed.scanning')}
                                        </span>
                                    </>
                                ) : (
                                    <>
                                        <ScanSearch size={18} className="shrink-0" /> {t('installed.scanButton')}
                                    </>
                                )}
                            </button>
                        </div>
                    </Section>

                    <Section title={t('installed.sectionTrackedTitle')} subtitle={t('installed.sectionTrackedSubtitle')}>
                        {mappings.length === 0 ? (
                            <EmptyNote>
                                {t('installed.emptyTracked')}
                            </EmptyNote>
                        ) : (
                            <div className="space-y-2">
                                {mappings.map((m) => (
                                    <MappingRow
                                        key={m.gameId}
                                        mapping={m}
                                        library={libraryById.get(m.gameId)}
                                        onRemove={async () => {
                                            await removeMapping(m.gameId);
                                            setMappings(await listMappings());
                                        }}
                                        t={t}
                                    />
                                ))}
                            </div>
                        )}
                    </Section>
                </>
            )}

            {manualOpen && (
                <LinkDialog
                    detected={null}
                    userId={userId}
                    onClose={() => setManualOpen(false)}
                    onLinked={async () => {
                        setManualOpen(false);
                        setMappings(await listMappings());
                        queryClient.invalidateQueries({ queryKey: ['library', userId] });
                    }}
                />
            )}
        </div>
    );
}

function Section({ title, subtitle, children }) {
    return (
        <section className="mb-10">
            <div className="mb-3">
                <h2 className="text-sm font-bold text-gray-300 uppercase tracking-wider">{title}</h2>
                {subtitle && <p className="text-xs text-gray-500 mt-0.5">{subtitle}</p>}
            </div>
            {children}
        </section>
    );
}

function EmptyNote({ children }) {
    return (
        <div className="border border-dashed border-[#1f2334] bg-[#141722]/40 px-4 py-6 text-center text-sm text-gray-500">
            {children}
        </div>
    );
}

function MappingRow({ mapping, library, onRemove, t }) {
    const title = library?.name || mapping.gameName || t('installed.gameFallback', { id: mapping.gameId });
    const playtime = library?.playtimeMinutes ?? 0;
    const playLabel = formatTrackedMinutes(playtime, t);
    return (
        <div className="flex items-center justify-between gap-4 border border-[#1f2334] bg-[#141722] px-4 py-3">
            <div className="min-w-0">
                <div className="text-sm font-semibold text-gray-100 truncate">{title}</div>
                <div className="text-xs text-gray-500 mt-0.5 truncate">
                    {playLabel && <span className="text-blue-300 font-medium">{playLabel} · </span>}
                    {(mapping.exeNames || []).slice(0, 3).join(', ')}
                    {mapping.exeNames && mapping.exeNames.length > 3 ? ` +${mapping.exeNames.length - 3}` : ''}
                </div>
            </div>
            <button
                type="button"
                onClick={onRemove}
                className="shrink-0 p-2 border border-[#1f2334] text-gray-400 hover:text-red-400 hover:border-red-500/40"
                title={t('installed.unlinkTitle')}
            >
                <Trash2 size={14} />
            </button>
        </div>
    );
}

function normalizeGameTitle(s) {
    return String(s || '')
        .toLowerCase()
        .replace(/[™®©']/g, '')
        .replace(/\s+/g, ' ')
        .trim();
}

function catalogSearchQueries(original) {
    const base = String(original || '').trim();
    const out = [];
    const push = (s) => {
        const x = String(s || '').trim();
        if (x.length >= 2 && !out.includes(x)) out.push(x);
    };
    push(base);
    push(base.replace(/[™®©]/gu, '').trim());
    const noSub = base.split(/[:\-–—]/)[0].trim();
    push(noSub);
    push(base.replace(/\s*\([^)]*\)\s*$/, '').trim());
    push(base.replace(/\s+(edition|deluxe|goty|complete|bundle)\s*$/i, '').trim());
    return out;
}

async function fetchCatalogMatches(name, searchGamesApi) {
    const queries = catalogSearchQueries(name);
    for (const q of queries) {
        try {
            const data = await searchGamesApi(q, 24, false);
            if (Array.isArray(data) && data.length > 0) return data;
        } catch {
            /* bir sonraki sorgu */
        }
    }
    return [];
}

function pickBestCatalogMatch(detectedName, results) {
    if (!Array.isArray(results) || results.length === 0) return null;
    const q = normalizeGameTitle(detectedName);
    if (!q) return results[0];

    const exact = results.find((r) => normalizeGameTitle(r.name) === q);
    if (exact) return exact;

    const partial = results.find((r) => {
        const n = normalizeGameTitle(r.name);
        return n.includes(q) || q.includes(n);
    });
    if (partial) return partial;

    return results[0];
}

function LinkDialog({ detected, userId, onClose, onLinked }) {
    const { t } = useI18n();
    const [query, setQuery] = useState(detected?.name ?? '');
    const [results, setResults] = useState([]);
    const [searching, setSearching] = useState(false);
    const [selectedGame, setSelectedGame] = useState(null);
    const [exePath, setExePath] = useState('');
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        if (!query?.trim()) {
            setResults([]);
            return;
        }
        let cancelled = false;
        setSearching(true);
        const handle = setTimeout(async () => {
            try {
                const data = await searchGames(query.trim(), 12, false);
                if (!cancelled) setResults(Array.isArray(data) ? data : []);
            } catch {
                if (!cancelled) setResults([]);
            } finally {
                if (!cancelled) setSearching(false);
            }
        }, 300);
        return () => {
            cancelled = true;
            clearTimeout(handle);
        };
    }, [query]);

    const handlePickExe = async () => {
        const p = await pickExe();
        if (p) setExePath(p);
    };

    const canSave = selectedGame && (detected || exePath);

    const handleSave = async () => {
        if (!canSave) return;
        setSaving(true);
        try {
            try {
                await addGameToLibrary(
                    userId,
                    {
                        id: selectedGame.id,
                        name: selectedGame.name,
                        background_image: selectedGame.background_image || '',
                    },
                    'Playing',
                );
            } catch {
                /* muhtemelen zaten eklenmiş */
            }

            const exeNames = [];
            let installDir = detected?.installDir;
            if (exePath) {
                const fileName = exePath.split(/[\\/]/).pop();
                if (fileName) exeNames.push(fileName);
            }
            await upsertMapping({
                gameId: selectedGame.id,
                gameName: selectedGame.name,
                exeNames,
                installDir,
            });
            emitToast(t('installed.toastLinked'), 'success');
            onLinked();
        } finally {
            setSaving(false);
        }
    };

    const dialogTitle = detected
        ? t('installed.dialogTitleDetected', { name: detected.name })
        : t('installed.dialogTitleManual');

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4" onClick={onClose}>
            <div
                className="w-full max-w-2xl bg-[#141722] border border-[#1f2334] max-h-[85vh] flex flex-col"
                onClick={(e) => e.stopPropagation()}
            >
                <header className="flex items-center justify-between px-5 py-4 border-b border-[#1f2334]">
                    <div>
                        <h2 className="text-base font-bold text-white">
                            {dialogTitle}
                        </h2>
                        <p className="text-xs text-gray-500 mt-0.5">
                            {t('installed.dialogHint')}
                        </p>
                    </div>
                    <button type="button" onClick={onClose} className="p-2 text-gray-400 hover:text-white">
                        <X size={18} />
                    </button>
                </header>

                <div className="flex-1 overflow-y-auto p-5 space-y-4">
                    <div className="relative">
                        <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" />
                        <input
                            type="text"
                            value={query}
                            onChange={(e) => setQuery(e.target.value)}
                            placeholder={t('installed.searchPh')}
                            className="w-full pl-9 pr-3 py-2.5 bg-[#1a1e2d] border border-[#1f2334] text-sm text-white outline-none focus:border-blue-500"
                        />
                    </div>

                    <div className="border border-[#1f2334] bg-[#0f1117]">
                        {searching ? (
                            <div className="py-6 flex justify-center text-gray-500">
                                <Loader2 size={18} className="animate-spin" />
                            </div>
                        ) : results.length === 0 ? (
                            <div className="py-6 text-center text-xs text-gray-500">{t('common.noResults')}</div>
                        ) : (
                            <ul className="max-h-72 overflow-y-auto">
                                {results.map((r) => {
                                    const active = selectedGame?.id === r.id;
                                    return (
                                        <li key={r.id}>
                                            <button
                                                type="button"
                                                onClick={() => setSelectedGame(r)}
                                                className={`w-full flex items-center gap-3 px-3 py-2 text-left transition-colors ${active ? 'bg-blue-600/20' : 'hover:bg-[#1a1e2d]'}`}
                                            >
                                                <div className="w-8 h-10 bg-[#1a1e2d] border border-[#1f2334] overflow-hidden shrink-0">
                                                    {r.background_image && (
                                                        <img src={r.background_image} alt="" className="w-full h-full object-cover" />
                                                    )}
                                                </div>
                                                <div className="min-w-0">
                                                    <div className="text-sm font-semibold text-gray-100 truncate">{r.name}</div>
                                                    <div className="text-[11px] text-gray-500">{r.released?.slice(0, 4) ?? ''}</div>
                                                </div>
                                            </button>
                                        </li>
                                    );
                                })}
                            </ul>
                        )}
                    </div>

                    {!detected && (
                        <div className="space-y-2">
                            <label className="text-xs font-bold uppercase tracking-wider text-gray-400">
                                {t('installed.exeLabel')}
                            </label>
                            <div className="flex gap-2">
                                <input
                                    type="text"
                                    readOnly
                                    value={exePath}
                                    placeholder={t('installed.exeNotSelected')}
                                    className="flex-1 px-3 py-2 bg-[#1a1e2d] border border-[#1f2334] text-sm text-gray-300"
                                />
                                <button
                                    type="button"
                                    onClick={handlePickExe}
                                    className="px-4 py-2 border border-[#1f2334] bg-[#1a1e2d] hover:bg-[#222738] text-sm text-gray-200"
                                >
                                    {t('common.select')}
                                </button>
                            </div>
                            <p className="text-[11px] text-gray-500 flex items-start gap-1.5">
                                <AlertCircle size={12} className="mt-0.5 shrink-0" />
                                {t('installed.exeHelp')}
                            </p>
                        </div>
                    )}
                </div>

                <footer className="flex justify-end gap-2 px-5 py-3 border-t border-[#1f2334]">
                    <button type="button" onClick={onClose} className="px-4 py-2 text-sm text-gray-400 hover:text-white">
                        {t('common.cancel')}
                    </button>
                    <button
                        type="button"
                        onClick={handleSave}
                        disabled={!canSave || saving}
                        className="px-5 py-2 bg-blue-600 hover:bg-blue-500 text-white text-sm font-semibold disabled:opacity-40 disabled:cursor-not-allowed flex items-center gap-2"
                    >
                        {saving && <Loader2 size={14} className="animate-spin" />}
                        {t('common.save')}
                    </button>
                </footer>
            </div>
        </div>
    );
}
