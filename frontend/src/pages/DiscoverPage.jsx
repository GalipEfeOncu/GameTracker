import { useState, useRef, useEffect, useCallback, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useSearchGames, useDiscoverGames } from '../hooks/useGames';
import { usePreferences } from '../context/PreferencesContext';
import { Loader2, SearchX, Flame, Star, Sparkles, Gamepad2 } from 'lucide-react';
import GameCard from '../components/GameCard';
import { GameCardSkeletonGrid } from '../components/GameCardSkeleton';

// IGDB genre id'leri (backend IgdbGenreCatalog ile aynı)
const GENRES = [
    { id: null, label: 'Tümü', emoji: '🎮' },
    { id: 25, label: 'Action', emoji: '⚔️' },
    { id: 12, label: 'RPG', emoji: '🐉' },
    { id: 5, label: 'Shooter', emoji: '🔫' },
    { id: 31, label: 'Adventure', emoji: '🗺️' },
    { id: 36, label: 'Puzzle', emoji: '🧩' },
    { id: 15, label: 'Strategy', emoji: '♟️' },
    { id: 8, label: 'Platformer', emoji: '🕹️' },
    { id: 13, label: 'Simulation', emoji: '🏗️' },
    { id: 10, label: 'Racing', emoji: '🏎️' },
    { id: 14, label: 'Sports', emoji: '⚽' },
    { id: 4, label: 'Fighting', emoji: '🥊' },
    { id: 32, label: 'Indie', emoji: '🎨' },
];

const MODES = [
    { id: 'trending', label: 'Trend', icon: Flame, hint: null },
    { id: 'top_rated', label: 'En İyi', icon: Star, hint: null },
    { id: 'new', label: 'Yeni Çıkan', icon: Sparkles, hint: null },
    {
        id: 'metacritic_top',
        label: 'Eleştiri 75+',
        icon: Gamepad2,
        hint: 'IGDB eleştiri özeti (aggregated_rating) 75 ve üzeri',
    },
];

export default function DiscoverPage() {
    const [searchParams] = useSearchParams();
    const query = searchParams.get('q') || '';
    const { showNsfw } = usePreferences();

    const [activeGenre, setActiveGenre] = useState(null);
    const [activeMode, setActiveModeState] = useState('trending');
    const scrollRef = useRef(null);
    const containerRef = useRef(null);
    const fetchingRef = useRef(false);

    const { data: searchResults, isLoading: isSearching } = useSearchGames(query, showNsfw);

    const {
        data: discoverData,
        isLoading: isDiscovering,
        isFetchingNextPage,
        fetchNextPage,
        hasNextPage,
    } = useDiscoverGames(activeGenre, activeMode, showNsfw);

    useEffect(() => {
        fetchingRef.current = isFetchingNextPage;
    }, [isFetchingNextPage]);

    const games = useMemo(
        () => discoverData?.pages?.flatMap(p => p.items) ?? [],
        [discoverData]
    );

    const handleScroll = useCallback(() => {
        const el = containerRef.current;
        if (!el) return;
        const dist = el.scrollHeight - el.scrollTop - el.clientHeight;
        if (dist < 800 && hasNextPage && !fetchingRef.current) {
            fetchNextPage();
        }
    }, [hasNextPage, fetchNextPage]);

    useEffect(() => {
        const el = containerRef.current;
        if (!el) return;
        el.addEventListener('scroll', handleScroll, { passive: true });
        return () => el.removeEventListener('scroll', handleScroll);
    }, [handleScroll]);

    // Arama modu aktifse arama sonuçlarını göster
    if (query) {
        return (
            <div className="h-full overflow-y-auto px-8 pt-8 pb-20">
                {isSearching ? (
                    <GameCardSkeletonGrid
                        count={15}
                        className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 items-start gap-x-6 gap-y-16"
                    />
                ) : searchResults && searchResults.length > 0 ? (
                    <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 items-start gap-x-6 gap-y-16">
                        {searchResults.map((game) => (
                            <GameCard key={game.id} game={game} />
                        ))}
                    </div>
                ) : (
                    <div className="flex flex-col items-center justify-center py-32 text-gray-400">
                        <SearchX size={64} className="mb-6 opacity-30" />
                        <h2 className="text-xl font-bold text-gray-300">Açıkçası bir şey bulamadık</h2>
                        <p className="mt-2 text-center max-w-sm">Arama teriminizi kontrol edebilir veya farklı anahtar kelimelerle deneyebilirsiniz.</p>
                    </div>
                )}
            </div>
        );
    }

    // Keşfet modu
    return (
        <div ref={containerRef} className="h-full overflow-y-auto pb-20">

            {/* Filtre Barı */}
            <div className="sticky top-0 z-10 bg-[#0f111a] border-b border-[#1f2334] px-8 py-3 flex flex-col gap-3">

                {/* Mode butonları */}
                <div className="flex items-center gap-2">
                    {MODES.map(m => {
                        const Icon = m.icon;
                        const active = activeMode === m.id;
                        return (
                            <button
                                key={m.id}
                                type="button"
                                title={m.hint ?? undefined}
                                onClick={() => setActiveModeState(m.id)}
                                className={`flex items-center gap-1.5 px-4 py-1.5 rounded-none text-sm font-bold transition-all border
                                    ${active
                                        ? 'bg-blue-600 border-blue-600 text-white'
                                        : 'border-[#1f2334] text-gray-400 hover:text-white hover:border-gray-500 bg-transparent'
                                    }`}
                            >
                                <Icon size={14} />
                                {m.label}
                            </button>
                        );
                    })}
                </div>

                {/* Genre pill'leri — horizontal scroll */}
                <div
                    ref={scrollRef}
                    className="flex items-center gap-2 overflow-x-auto pb-1 scrollbar-none"
                    style={{ scrollbarWidth: 'none', msOverflowStyle: 'none' }}
                >
                    {GENRES.map(g => {
                        const active = activeGenre === g.id;
                        return (
                            <button
                                key={String(g.id)}
                                onClick={() => setActiveGenre(g.id)}
                                className={`flex-shrink-0 flex items-center gap-1.5 px-3 py-1 rounded-none text-xs font-semibold transition-all border whitespace-nowrap
                                    ${active
                                        ? 'bg-[#1f2334] border-blue-500 text-white'
                                        : 'border-[#1f2334] text-gray-500 hover:text-gray-200 hover:border-gray-500 bg-transparent'
                                    }`}
                            >
                                <span>{g.emoji}</span>
                                {g.label}
                            </button>
                        );
                    })}
                </div>
            </div>

            {/* Oyun Grid'i */}
            <div className="px-8 pt-6">
                {isDiscovering ? (
                    <GameCardSkeletonGrid
                        count={15}
                        className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 items-start gap-x-6 gap-y-16"
                    />
                ) : games.length > 0 ? (
                    <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 items-start gap-x-6 gap-y-16">
                        {games.map((game) => (
                            <GameCard key={game.id} game={game} />
                        ))}
                    </div>
                ) : (
                    <div className="flex flex-col items-center justify-center py-32 text-gray-400">
                        <Gamepad2 size={64} className="mb-6 opacity-30" />
                        <h2 className="text-xl font-bold text-gray-300">Oyun bulunamadı</h2>
                        <p className="mt-2 text-center max-w-sm">Bu kombinasyon için henüz veri yok, farklı bir mod veya tür deneyin.</p>
                    </div>
                )}

                {/* Yükleniyor göstergesi */}
                <div className="flex justify-center items-center py-10">
                    {isFetchingNextPage && (
                        <div className="flex items-center gap-3 text-gray-500 text-sm">
                            <Loader2 size={20} className="animate-spin text-blue-500" />
                            Daha fazla yükleniyor...
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}
