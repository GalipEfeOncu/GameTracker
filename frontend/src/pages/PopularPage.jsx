import { useRef, useMemo, useEffect, useCallback } from 'react';
import { useVirtualizer } from '@tanstack/react-virtual';
import { Loader2 } from 'lucide-react';
import { usePopularGames } from '../hooks/useGames';
import { usePreferences } from '../context/PreferencesContext';
import GameCard from '../components/GameCard';

const COLUMNS = 5;

export default function PopularPage() {
    const mainRef = useRef(null);
    const fetchingRef = useRef(false);
    const { showNsfw } = usePreferences();

    const {
        data: popularData,
        isLoading,
        isFetchingNextPage,
        fetchNextPage,
        hasNextPage,
    } = usePopularGames(showNsfw);

    // fetchingRef'i güncel tut — stale closure sorununu engeller
    useEffect(() => {
        fetchingRef.current = isFetchingNextPage;
    }, [isFetchingNextPage]);

    const games = useMemo(
        () => popularData?.pages?.flatMap(p => p.items) ?? [],
        [popularData]
    );

    const rows = useMemo(() => {
        const result = [];
        for (let i = 0; i < games.length; i += COLUMNS) {
            result.push(games.slice(i, i + COLUMNS));
        }
        return result;
    }, [games]);

    const rowVirtualizer = useVirtualizer({
        count: rows.length,
        getScrollElement: () => mainRef.current,
        estimateSize: () => 280,
        overscan: 10,
    });

    const handleScroll = useCallback(() => {
        const el = mainRef.current;
        if (!el) return;
        const distanceFromBottom = el.scrollHeight - el.scrollTop - el.clientHeight;
        if (distanceFromBottom < 800 && hasNextPage && !fetchingRef.current) {
            fetchNextPage();
        }
    }, [hasNextPage, fetchNextPage]);

    // Scroll event listener'ı direkt DOM'a bağla (synthetic event yerine)
    useEffect(() => {
        const el = mainRef.current;
        if (!el) return;
        el.addEventListener('scroll', handleScroll, { passive: true });
        return () => el.removeEventListener('scroll', handleScroll);
    }, [handleScroll]);

    return (
        <div ref={mainRef} className="h-full overflow-y-auto px-8 pt-8 pb-20">

            {isLoading ? (
                <div className="flex flex-col items-center justify-center py-20 text-gray-500">
                    <Loader2 size={32} className="animate-spin mb-4 text-blue-500" />
                    <p>Oyunlar yükleniyor...</p>
                </div>
            ) : (
                <div style={{ height: `${rowVirtualizer.getTotalSize()}px`, position: 'relative' }}>
                    {rowVirtualizer.getVirtualItems().map((virtualRow) => {
                        const rowGames = rows[virtualRow.index];
                        return (
                            <div
                                key={virtualRow.key}
                                style={{
                                    position: 'absolute',
                                    top: 0,
                                    left: 0,
                                    width: '100%',
                                    transform: `translateY(${virtualRow.start}px)`,
                                }}
                            >
                                <div className="grid grid-cols-5 gap-x-5 gap-y-10 pb-10">
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
