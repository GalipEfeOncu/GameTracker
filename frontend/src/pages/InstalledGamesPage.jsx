import { useEffect, useMemo, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { HardDrive, Loader2, Link2, Plus, X, Search, AlertCircle, Trash2 } from 'lucide-react';
import {
    isDesktop,
    detectInstalledGames,
    listMappings,
    upsertMapping,
    removeMapping,
    pickExe,
    onPlaytimeUpdate,
} from '../desktop/bridge';
import { searchGames, addGameToLibrary, fetchUserLibrary } from '../api/apiClient';
import { useUser } from '../context/UserContext';
import { getSessionUserId } from '../utils/sessionUser';
import { emitToast } from '../utils/toastEvents';

const PLATFORM_LABEL = { steam: 'Steam', epic: 'Epic', gog: 'GOG' };

export default function InstalledGamesPage() {
    const { user } = useUser();
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const userId = getSessionUserId(user);

    const [detected, setDetected] = useState([]);
    const [mappings, setMappings] = useState([]);
    const [loading, setLoading] = useState(true);
    const [manualOpen, setManualOpen] = useState(false);
    const [linkTarget, setLinkTarget] = useState(null); // algılanan oyun → IGDB eşleme

    const mappedGameIds = useMemo(() => new Set(mappings.map((m) => m.gameId)), [mappings]);

    const refresh = useCallback(async () => {
        setLoading(true);
        try {
            const [games, maps] = await Promise.all([detectInstalledGames(), listMappings()]);
            setDetected(games);
            setMappings(maps);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        if (!isDesktop) { setLoading(false); return; }
        refresh();
        const unsub = onPlaytimeUpdate(() => {
            queryClient.invalidateQueries({ queryKey: ['library', userId] });
        });
        return unsub;
    }, [refresh, queryClient, userId]);

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
                <h2 className="text-xl font-bold text-white">Yalnızca masaüstü sürümde</h2>
                <p className="mt-2 text-gray-500 max-w-md">
                    Yüklü oyun algılama ve oynama saati takibi, GameTracker masaüstü uygulamasında kullanılabilir.
                </p>
            </div>
        );
    }

    if (!user) {
        return (
            <div className="flex flex-col items-center justify-center h-full py-40">
                <HardDrive size={56} className="mb-6 opacity-40 text-blue-500" />
                <h2 className="text-xl font-bold text-white">Giriş gerekli</h2>
                <button onClick={() => navigate('/login')} className="mt-6 px-6 py-2.5 bg-blue-600 hover:bg-blue-500 text-white font-semibold text-sm">
                    Giriş Yap
                </button>
            </div>
        );
    }

    return (
        <div className="h-full overflow-y-auto px-8 pt-8 pb-20">
            <div className="mb-8 flex flex-wrap items-center justify-between gap-3">
                <div>
                    <h1 className="text-2xl font-bold text-white tracking-tight">Yüklü Oyunlarım</h1>
                    <p className="text-sm text-gray-500 mt-1">Steam / Epic / GOG oyunları otomatik algılanır. Diğer oyunları manuel ekleyebilirsin.</p>
                </div>
                <div className="flex gap-2">
                    <button
                        type="button"
                        onClick={refresh}
                        className="px-4 py-2 border border-[#1f2334] bg-[#141722] hover:bg-[#1a1e2d] text-gray-300 text-sm font-medium"
                    >
                        Yenile
                    </button>
                    <button
                        type="button"
                        onClick={() => setManualOpen(true)}
                        className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-500 text-white text-sm font-semibold"
                    >
                        <Plus size={16} /> Manuel Ekle
                    </button>
                </div>
            </div>

            {loading ? (
                <div className="flex justify-center py-24 text-gray-500">
                    <Loader2 size={32} className="animate-spin" />
                </div>
            ) : (
                <>
                    <Section title="Otomatik Algılanan">
                        {detected.length === 0 ? (
                            <EmptyNote>Algılanan oyun yok. Steam / Epic / GOG yüklü değilse manuel ekle.</EmptyNote>
                        ) : (
                            <div className="space-y-2">
                                {detected.map((g) => (
                                    <DetectedRow
                                        key={`${g.platform}:${g.platformId}`}
                                        game={g}
                                        mapped={[...mappedGameIds].find((id) => libraryById.get(id)?.name?.toLowerCase() === g.name.toLowerCase())}
                                        onLink={() => setLinkTarget(g)}
                                    />
                                ))}
                            </div>
                        )}
                    </Section>

                    <Section title="İzlenen Oyunlar" subtitle="Oynama saati otomatik sayılır">
                        {mappings.length === 0 ? (
                            <EmptyNote>Henüz eşleme yok. Yukarıdan bir oyunu kütüphaneye bağla.</EmptyNote>
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
                                    />
                                ))}
                            </div>
                        )}
                    </Section>
                </>
            )}

            {linkTarget && (
                <LinkDialog
                    detected={linkTarget}
                    userId={userId}
                    onClose={() => setLinkTarget(null)}
                    onLinked={async () => {
                        setLinkTarget(null);
                        setMappings(await listMappings());
                        queryClient.invalidateQueries({ queryKey: ['library', userId] });
                    }}
                />
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

function DetectedRow({ game, mapped, onLink }) {
    return (
        <div className="flex items-center justify-between gap-4 border border-[#1f2334] bg-[#141722] px-4 py-3">
            <div className="min-w-0">
                <div className="text-sm font-semibold text-gray-100 truncate">{game.name}</div>
                <div className="text-xs text-gray-500 mt-0.5 truncate">
                    <span className="inline-block px-1.5 py-0.5 bg-[#1a1e2d] border border-[#1f2334] mr-2">
                        {PLATFORM_LABEL[game.platform] ?? game.platform}
                    </span>
                    {game.installDir}
                </div>
            </div>
            <button
                type="button"
                onClick={onLink}
                className={`shrink-0 flex items-center gap-2 px-3 py-1.5 text-xs font-semibold border transition-colors ${mapped ? 'border-green-500/40 text-green-400 bg-green-500/10' : 'border-blue-500/40 text-blue-400 bg-blue-500/10 hover:bg-blue-500/20'}`}
            >
                <Link2 size={14} /> {mapped ? 'Bağlı' : 'Bağla'}
            </button>
        </div>
    );
}

function MappingRow({ mapping, library, onRemove }) {
    const title = library?.name || mapping.gameName || `Oyun #${mapping.gameId}`;
    const playtime = library?.playtimeMinutes ?? 0;
    return (
        <div className="flex items-center justify-between gap-4 border border-[#1f2334] bg-[#141722] px-4 py-3">
            <div className="min-w-0">
                <div className="text-sm font-semibold text-gray-100 truncate">{title}</div>
                <div className="text-xs text-gray-500 mt-0.5 truncate">
                    {playtime > 0 && <span className="text-blue-300 font-medium">{formatMinutes(playtime)} · </span>}
                    {(mapping.exeNames || []).slice(0, 3).join(', ')}
                    {mapping.exeNames && mapping.exeNames.length > 3 ? ` +${mapping.exeNames.length - 3}` : ''}
                </div>
            </div>
            <button
                type="button"
                onClick={onRemove}
                className="shrink-0 p-2 border border-[#1f2334] text-gray-400 hover:text-red-400 hover:border-red-500/40"
                title="İzlemeyi kaldır"
            >
                <Trash2 size={14} />
            </button>
        </div>
    );
}

function formatMinutes(m) {
    if (m < 60) return `${m} dk`;
    const h = Math.floor(m / 60);
    const rem = m % 60;
    return rem ? `${h}s ${rem}d` : `${h} saat`;
}

// Algılanan oyunu (ya da tamamen manuel bir .exe'yi) IGDB'deki bir oyunla eşleyip tracker'a yazdırır.
function LinkDialog({ detected, userId, onClose, onLinked }) {
    const [query, setQuery] = useState(detected?.name ?? '');
    const [results, setResults] = useState([]);
    const [searching, setSearching] = useState(false);
    const [selectedGame, setSelectedGame] = useState(null);
    const [exePath, setExePath] = useState('');
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        if (!query?.trim()) { setResults([]); return; }
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
        return () => { cancelled = true; clearTimeout(handle); };
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
            // 1) IGDB oyununu kütüphaneye ekle (zaten varsa backend 400 döner — yutulur).
            try {
                await addGameToLibrary(userId, {
                    id: selectedGame.id,
                    name: selectedGame.name,
                    background_image: selectedGame.background_image || '',
                }, 'Playing');
            } catch { /* muhtemelen zaten eklenmiş */ }

            // 2) Tracker için exe eşlemesi oluştur.
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
                installDir, // main process burayı tarayıp ek .exe'leri ekler
            });
            emitToast('Oyun bağlandı. Oynadıkça saat otomatik sayılacak.', 'success');
            onLinked();
        } finally {
            setSaving(false);
        }
    };

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4" onClick={onClose}>
            <div
                className="w-full max-w-2xl bg-[#141722] border border-[#1f2334] max-h-[85vh] flex flex-col"
                onClick={(e) => e.stopPropagation()}
            >
                <header className="flex items-center justify-between px-5 py-4 border-b border-[#1f2334]">
                    <div>
                        <h2 className="text-base font-bold text-white">
                            {detected ? `"${detected.name}" için eşleme` : 'Manuel oyun ekle'}
                        </h2>
                        <p className="text-xs text-gray-500 mt-0.5">Oyunu IGDB'den seç; gerekiyorsa .exe dosyasını da belirt.</p>
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
                            placeholder="Oyun adı ara..."
                            className="w-full pl-9 pr-3 py-2.5 bg-[#1a1e2d] border border-[#1f2334] text-sm text-white outline-none focus:border-blue-500"
                        />
                    </div>

                    <div className="border border-[#1f2334] bg-[#0f1117]">
                        {searching ? (
                            <div className="py-6 flex justify-center text-gray-500"><Loader2 size={18} className="animate-spin" /></div>
                        ) : results.length === 0 ? (
                            <div className="py-6 text-center text-xs text-gray-500">Sonuç yok.</div>
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
                                                    {r.background_image && <img src={r.background_image} alt="" className="w-full h-full object-cover" />}
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
                            <label className="text-xs font-bold uppercase tracking-wider text-gray-400">Çalıştırılabilir dosya (.exe)</label>
                            <div className="flex gap-2">
                                <input
                                    type="text"
                                    readOnly
                                    value={exePath}
                                    placeholder="Seçilmedi"
                                    className="flex-1 px-3 py-2 bg-[#1a1e2d] border border-[#1f2334] text-sm text-gray-300"
                                />
                                <button type="button" onClick={handlePickExe} className="px-4 py-2 border border-[#1f2334] bg-[#1a1e2d] hover:bg-[#222738] text-sm text-gray-200">
                                    Seç
                                </button>
                            </div>
                            <p className="text-[11px] text-gray-500 flex items-start gap-1.5"><AlertCircle size={12} className="mt-0.5 shrink-0" />Bu exe çalıştığı sürece oyun "açık" sayılır ve süre backend'e yazılır.</p>
                        </div>
                    )}
                </div>

                <footer className="flex justify-end gap-2 px-5 py-3 border-t border-[#1f2334]">
                    <button type="button" onClick={onClose} className="px-4 py-2 text-sm text-gray-400 hover:text-white">İptal</button>
                    <button
                        type="button"
                        onClick={handleSave}
                        disabled={!canSave || saving}
                        className="px-5 py-2 bg-blue-600 hover:bg-blue-500 text-white text-sm font-semibold disabled:opacity-40 disabled:cursor-not-allowed flex items-center gap-2"
                    >
                        {saving && <Loader2 size={14} className="animate-spin" />}
                        Kaydet
                    </button>
                </footer>
            </div>
        </div>
    );
}
