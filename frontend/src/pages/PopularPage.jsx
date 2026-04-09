import { useRef, useMemo, useEffect, useCallback, useState } from 'react';
import { useVirtualizer } from '@tanstack/react-virtual';
import { useQuery } from '@tanstack/react-query';
import { Loader2, AlertCircle, Flame, ChevronLeft, ChevronRight } from 'lucide-react';
import { usePopularGames } from '../hooks/useGames';
import { getPopularGames } from '../api/apiClient';
import { usePreferences } from '../context/PreferencesContext';
import GameCard from '../components/GameCard';
import { GameCardSkeletonGrid } from '../components/GameCardSkeleton';
import { useGameGridColumns } from '../hooks/useGameGridColumns';

function PopularEmptyState({ igdbConfigured, onRetry }) {
    return (
        <div className="flex flex-col items-center justify-center py-24 px-4 text-gray-400 max-w-lg mx-auto">
            <Flame size={56} className="mb-4 opacity-30" />
            <h2 className="text-xl font-bold text-gray-300 text-center">Henüz oyun listelenmiyor</h2>
            {igdbConfigured === false ? (
                <p className="mt-3 text-center text-sm text-gray-500 max-w-md leading-relaxed">
                    IGDB (Twitch) API yapılandırılmamış. <code className="text-gray-400">backend</code> için{' '}
                    <code className="text-gray-400">Igdb:ClientId</code> ve{' '}
                    <code className="text-gray-400">Igdb:ClientSecret</code> user-secrets veya env ile tanımlayın; ayrıntı{' '}
                    <span className="text-gray-400">docs/DEPLOY.md</span>.
                </p>
            ) : (
                <p className="mt-2 text-center max-w-sm text-sm text-gray-500">
                    Liste yüklenemedi veya geçici bir ağ sorunu oluştu. Tekrar deneyebilirsiniz.
                </p>
            )}
            <button
                type="button"
                onClick={onRetry}
                className="mt-5 px-5 py-2 text-sm font-semibold border border-[#1f2334] text-gray-300 hover:bg-[#1a1e2d] hover:text-white transition-colors"
            >
                Yenile
            </button>
        </div>
    );
}

function PopularPageInfinite() {
    const mainRef = useRef(null);
    const fetchingRef = useRef(false);
    const { showNsfw } = usePreferences();
    const columns = useGameGridColumns();

    const {
        data: popularData,
        isLoading,
        isError,
        error,
        refetch,
        isFetchingNextPage,
        fetchNextPage,
        hasNextPage,
    } = usePopularGames(showNsfw);

    useEffect(() => {
        fetchingRef.current = isFetchingNextPage;
    }, [isFetchingNextPage]);

    const games = useMemo(
        () => popularData?.pages?.flatMap(p => p.items) ?? [],
        [popularData]
    );

    const igdbConfigured = popularData?.pages?.[0]?.igdbConfigured;

    const rows = useMemo(() => {
        const result = [];
        for (let i = 0; i < games.length; i += columns) {
            result.push(games.slice(i, i + columns));
        }
        return result;
    }, [games, columns]);

    const rowVirtualizer = useVirtualizer({
        count: rows.length,
        getScrollElement: () => mainRef.current,
        // 2:3 görsel + başlık + satır altı boşluk (Keşfet ile aynı gap-y-16 ≈ pb-16)
        estimateSize: () => 500,
        overscan: 6,
    });

    const handleScroll = useCallback(() => {
        const el = mainRef.current;
        if (!el) return;
        const distanceFromBottom = el.scrollHeight - el.scrollTop - el.clientHeight;
        if (distanceFromBottom < 800 && hasNextPage && !fetchingRef.current) {
            fetchNextPage();
        }
    }, [hasNextPage, fetchNextPage]);

    useEffect(() => {
        const el = mainRef.current;
        if (!el) return;
        el.addEventListener('scroll', handleScroll, { passive: true });
        return () => el.removeEventListener('scroll', handleScroll);
    }, [handleScroll]);

    const errorMessage =
        error?.response?.data?.message ??
        error?.message ??
        (typeof error?.response?.data === 'string' ? error.response.data : null);

    return (
        <div ref={mainRef} className="h-full overflow-y-auto px-8 pt-8 pb-20">
            {isLoading ? (
                <GameCardSkeletonGrid
                    count={18}
                    className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 items-start gap-x-6 gap-y-16 pb-10"
                />
            ) : isError ? (
                <div className="flex flex-col items-center justify-center py-24 px-4 text-center max-w-md mx-auto">
                    <AlertCircle size={48} className="mb-4 text-red-400 opacity-90" />
                    <h2 className="text-lg font-bold text-gray-200">Oyunlar yüklenemedi</h2>
                    <p className="mt-2 text-sm text-gray-500">
                        {typeof errorMessage === 'string' && errorMessage.trim()
                            ? errorMessage
                            : 'Bağlantı veya sunucu hatası. İnternetinizi kontrol edip tekrar deneyin.'}
                    </p>
                    <button
                        type="button"
                        onClick={() => refetch()}
                        className="mt-6 px-5 py-2.5 text-sm font-semibold bg-blue-600 hover:bg-blue-500 text-white rounded-none border border-blue-500 transition-colors"
                    >
                        Tekrar dene
                    </button>
                </div>
            ) : games.length === 0 ? (
                <PopularEmptyState igdbConfigured={igdbConfigured} onRetry={() => refetch()} />
            ) : (
                <div style={{ height: `${rowVirtualizer.getTotalSize()}px`, position: 'relative' }}>
                    {rowVirtualizer.getVirtualItems().map((virtualRow) => {
                        const rowGames = rows[virtualRow.index];
                        return (
                            <div
                                key={virtualRow.key}
                                data-index={virtualRow.index}
                                ref={rowVirtualizer.measureElement}
                                className="pb-16"
                                style={{
                                    position: 'absolute',
                                    top: 0,
                                    left: 0,
                                    width: '100%',
                                    transform: `translateY(${virtualRow.start}px)`,
                                }}
                            >
                                <div
                                    className="grid items-start gap-x-6"
                                    style={{ gridTemplateColumns: `repeat(${columns}, minmax(0, 1fr))` }}
                                >
                                    {rowGames.map((game) => (
                                        <GameCard key={game.id} game={game} />
                                    ))}
                                </div>
                            </div>
                        );
                    })}
                </div>
            )}

            <div className="flex justify-center items-center py-6">
                {isFetchingNextPage && (
                    <div className="flex items-center gap-3 text-gray-500 text-sm">
                        <Loader2 size={20} className="animate-spin text-blue-500" />
                        Yeni oyunlar yükleniyor...
                    </div>
                )}
            </div>
        </div>
    );
}

function PopularPagePaged() {
    const { showNsfw } = usePreferences();
    const [offset, setOffset] = useState(0);
    const [backStack, setBackStack] = useState([]);

    useEffect(() => {
        setOffset(0);
        setBackStack([]);
    }, [showNsfw]);

    const { data, isLoading, isError, error, refetch, isFetching } = useQuery({
        queryKey: ['popularGamesPaged', offset, showNsfw],
        queryFn: () => getPopularGames(offset, showNsfw),
        staleTime: 1000 * 60 * 10,
    });

    const goNext = useCallback(() => {
        if (!data?.hasMore) return;
        setBackStack((s) => [...s, offset]);
        setOffset(data.nextOffset);
    }, [data, offset]);

    const goPrev = useCallback(() => {
        if (backStack.length === 0) return;
        const prevOffset = backStack[backStack.length - 1];
        setBackStack((s) => s.slice(0, -1));
        setOffset(prevOffset);
    }, [backStack]);

    const games = data?.items ?? [];
    const pageNum = backStack.length + 1;
    const igdbConfigured = data?.igdbConfigured;

    const errorMessage =
        error?.response?.data?.message ??
        error?.message ??
        (typeof error?.response?.data === 'string' ? error.response.data : null);

    return (
        <div className="h-full overflow-y-auto px-8 pt-8 pb-20">
            {isLoading && !data ? (
                <GameCardSkeletonGrid count={18} />
            ) : isError ? (
                <div className="flex flex-col items-center justify-center py-24 px-4 text-center max-w-md mx-auto">
                    <AlertCircle size={48} className="mb-4 text-red-400 opacity-90" />
                    <h2 className="text-lg font-bold text-gray-200">Oyunlar yüklenemedi</h2>
                    <p className="mt-2 text-sm text-gray-500">
                        {typeof errorMessage === 'string' && errorMessage.trim()
                            ? errorMessage
                            : 'Bağlantı veya sunucu hatası. İnternetinizi kontrol edip tekrar deneyin.'}
                    </p>
                    <button
                        type="button"
                        onClick={() => refetch()}
                        className="mt-6 px-5 py-2.5 text-sm font-semibold bg-blue-600 hover:bg-blue-500 text-white rounded-none border border-blue-500 transition-colors"
                    >
                        Tekrar dene
                    </button>
                </div>
            ) : games.length === 0 ? (
                <PopularEmptyState igdbConfigured={igdbConfigured} onRetry={() => refetch()} />
            ) : (
                <>
                    <div className={`grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 items-start gap-x-6 gap-y-16 pb-6 ${isFetching ? 'opacity-70' : ''}`}>
                        {games.map((game) => (
                            <GameCard key={`${offset}-${game.id}`} game={game} />
                        ))}
                    </div>
                    <div className="flex flex-wrap items-center justify-center gap-4 py-8 border-t border-[#1f2334]">
                        <button
                            type="button"
                            onClick={goPrev}
                            disabled={backStack.length === 0 || isFetching}
                            className="inline-flex items-center gap-2 px-4 py-2.5 text-sm font-medium border border-[#1f2334] text-gray-300 hover:bg-[#1a1e2d] disabled:opacity-40 disabled:pointer-events-none rounded-none"
                        >
                            <ChevronLeft size={18} /> Önceki
                        </button>
                        <span className="text-sm text-gray-500 tabular-nums min-w-[5rem] text-center">
                            Sayfa {pageNum}
                        </span>
                        <button
                            type="button"
                            onClick={goNext}
                            disabled={!data?.hasMore || isFetching}
                            className="inline-flex items-center gap-2 px-4 py-2.5 text-sm font-medium border border-[#1f2334] text-gray-300 hover:bg-[#1a1e2d] disabled:opacity-40 disabled:pointer-events-none rounded-none"
                        >
                            Sonraki <ChevronRight size={18} />
                        </button>
                    </div>
                    {isFetching && (
                        <div className="flex justify-center pb-4 text-gray-500 text-sm gap-2 items-center">
                            <Loader2 size={18} className="animate-spin text-blue-500" />
                            Yükleniyor...
                        </div>
                    )}
                </>
            )}
        </div>
    );
}

export default function PopularPage() {
    const { popularListMode } = usePreferences();
    if (popularListMode === 'paged') {
        return <PopularPagePaged />;
    }
    return <PopularPageInfinite />;
}
