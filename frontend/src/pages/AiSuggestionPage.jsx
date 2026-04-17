import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
    Loader2,
    Sparkles,
    BrainCircuit,
    History,
    Wand2,
    RefreshCw,
    Library,
    AlertCircle,
} from 'lucide-react';
import { getRecommendations, fetchUserLibrary } from '../api/apiClient';
import { useUser } from '../context/UserContext';
import GameCard from '../components/GameCard';
import { getSessionUserId } from '../utils/sessionUser';

export default function AiSuggestionPage() {
    const { user } = useUser();
    const userId = getSessionUserId(user);
    const [shouldFetch, setShouldFetch] = useState(false);

    const { data: library, isLoading: libraryLoading } = useQuery({
        queryKey: ['library', userId],
        queryFn: () => fetchUserLibrary(userId),
        enabled: userId != null,
    });

    const likedGames = library?.map((g) => g.name) ?? [];

    const {
        data: suggestions,
        isLoading,
        isError,
        isFetching,
        isFetched,
        refetch,
    } = useQuery({
        queryKey: ['recommendations', userId, likedGames.join('\u0001')],
        queryFn: () => getRecommendations(likedGames),
        enabled: shouldFetch && likedGames.length > 0,
    });

    const handleStart = () => {
        setShouldFetch(true);
    };

    const handleRefetch = () => {
        void refetch();
    };

    const showResults = shouldFetch && isFetched && !isError;
    /** Yalnızca ilk çağrı — refetch sırasında kartları koru */
    const showBlockingLoad = shouldFetch && likedGames.length > 0 && isLoading;
    const isRefreshing = shouldFetch && isFetched && isFetching && !isLoading;
    const canRun = likedGames.length > 0 && !libraryLoading;

    if (!user) {
        return (
            <div className="flex h-full flex-col items-center justify-center px-6 py-20">
                <div className="mb-8 flex h-20 w-20 items-center justify-center rounded-none border border-blue-500/30 bg-blue-500/10">
                    <Sparkles className="h-10 w-10 text-blue-400" strokeWidth={1.25} />
                </div>
                <h2 className="text-center text-2xl font-bold tracking-tight text-white">AI önerileri</h2>
                <p className="mt-3 max-w-sm text-center text-sm font-medium leading-relaxed text-gray-500">
                    Kütüphanenize göre yeni oyunlar önermek için giriş yapın.
                </p>
            </div>
        );
    }

    return (
        <div className="h-full overflow-y-auto scroll-smooth px-4 pb-24 pt-8 sm:px-8 sm:pt-10">
            <div className="mx-auto max-w-6xl">
                {/* Hero */}
                <section className="rounded-none border border-[#1f2334] bg-[#141722]/40 px-6 py-10 sm:px-10 sm:py-12">
                    <div className="mb-5 inline-flex items-center gap-2 rounded-none border border-blue-500/25 bg-blue-500/10 px-3 py-1.5 text-[11px] font-bold uppercase tracking-[0.14em] text-blue-300">
                        <BrainCircuit className="h-3.5 w-3.5" aria-hidden />
                        AI öneri
                    </div>
                    <h1 className="max-w-2xl text-3xl font-bold tracking-tight text-white sm:text-4xl">
                        Kütüphanene göre yeni oyunlar keşfet
                    </h1>
                    <p className="mt-3 max-w-xl text-sm font-medium leading-relaxed text-gray-400 sm:text-base">
                        İstek yalnızca sen başlattığında gider; kütüphanendeki isimler Gemini ile eşleştirilip IGDB
                        üzerinden kartlar oluşturulur.
                    </p>

                    <div className="mt-8 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                        <div className="flex items-center gap-3 text-sm text-gray-500">
                            <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-none border border-[#1f2334] bg-[#1a1e2d]">
                                <Library className="h-5 w-5 text-gray-400" aria-hidden />
                            </span>
                            <div>
                                {libraryLoading ? (
                                    <span className="text-gray-500">Kütüphane yükleniyor…</span>
                                ) : likedGames.length === 0 ? (
                                    <span className="font-medium text-amber-500/90">Öneri için önce kütüphanene oyun ekle</span>
                                ) : (
                                    <>
                                        <span className="font-semibold text-gray-200">{likedGames.length} oyun</span>
                                        <span className="text-gray-500"> analiz için hazır</span>
                                    </>
                                )}
                            </div>
                        </div>

                        <div className="flex flex-wrap items-center gap-3">
                            {!shouldFetch || !isFetched ? (
                                <button
                                    type="button"
                                    onClick={handleStart}
                                    disabled={!canRun || showBlockingLoad}
                                    className="group inline-flex items-center justify-center gap-2 rounded-none border border-blue-500/50 bg-blue-600 px-6 py-3.5 text-sm font-bold text-white transition-colors hover:bg-blue-500 disabled:cursor-not-allowed disabled:opacity-45"
                                >
                                    {showBlockingLoad ? (
                                        <>
                                            <Loader2 className="h-5 w-5 shrink-0 animate-spin" aria-hidden />
                                            Hazırlanıyor…
                                        </>
                                    ) : (
                                        <>
                                            <Wand2 className="h-5 w-5 shrink-0 transition group-hover:rotate-6" aria-hidden />
                                            Önerileri oluştur
                                        </>
                                    )}
                                </button>
                            ) : (
                                <button
                                    type="button"
                                    onClick={handleRefetch}
                                    disabled={isFetching}
                                    className="inline-flex items-center justify-center gap-2 rounded-none border border-[#2a3148] bg-[#1a1e2d] px-5 py-3 text-sm font-semibold text-gray-200 transition hover:border-blue-500/35 hover:bg-[#1f2436] disabled:opacity-50"
                                >
                                    <RefreshCw className={`h-4 w-4 shrink-0 ${isFetching ? 'animate-spin' : ''}`} aria-hidden />
                                    Yeniden analiz et
                                </button>
                            )}
                        </div>
                    </div>
                </section>

                {/* İçerik */}
                <div className="mt-10">
                    {libraryLoading ? (
                        <div className="flex flex-col items-center justify-center py-20 text-gray-500">
                            <Loader2 className="mb-4 h-10 w-10 animate-spin text-blue-500" aria-hidden />
                            <p className="text-sm font-medium">Kütüphane yükleniyor…</p>
                        </div>
                    ) : likedGames.length === 0 ? (
                        <div className="flex flex-col items-center rounded-none border border-dashed border-[#2a3148] bg-[#141722]/40 px-8 py-16 text-center">
                            <History className="mb-5 h-12 w-12 text-gray-600" strokeWidth={1.25} aria-hidden />
                            <p className="text-lg font-bold text-gray-300">Kütüphanen boş</p>
                            <p className="mt-2 max-w-md text-sm leading-relaxed text-gray-500">
                                Popüler veya keşfet bölümünden oyun eklediğinde burada analiz edebilirim.
                            </p>
                        </div>
                    ) : !shouldFetch ? (
                        <div className="rounded-none border border-[#1f2334] bg-[#161a28] px-8 py-14 text-center">
                            <Sparkles className="mx-auto mb-4 h-11 w-11 text-blue-400/80" strokeWidth={1.25} aria-hidden />
                            <p className="text-base font-semibold text-gray-200">Hazırız</p>
                            <p className="mx-auto mt-2 max-w-lg text-sm leading-relaxed text-gray-500">
                                Yukarıdaki <span className="font-medium text-gray-400">Önerileri oluştur</span> ile
                                isteği başlat; sayfa açılır açılmaz API çağrılmaz.
                            </p>
                        </div>
                    ) : showBlockingLoad ? (
                        <div className="flex flex-col items-center justify-center rounded-none border border-[#1f2334] bg-[#141722]/30 py-24">
                            <Loader2 className="mb-5 h-11 w-11 animate-spin text-blue-500" aria-hidden />
                            <p className="text-base font-semibold text-gray-300">Öneriler hazırlanıyor</p>
                            <p className="mt-1 text-sm text-gray-500">Gemini + IGDB — birkaç saniye sürebilir</p>
                        </div>
                    ) : isError ? (
                        <div
                            role="alert"
                            className="flex flex-col items-center gap-4 rounded-none border border-red-500/25 bg-red-500/5 px-8 py-12 text-center"
                        >
                            <AlertCircle className="h-10 w-10 text-red-400" aria-hidden />
                            <div>
                                <p className="font-bold text-red-300">Öneriler alınamadı</p>
                                <p className="mt-1 text-sm text-red-200/70">
                                    Servis yoğun olabilir veya yapılandırma eksik. Biraz sonra yeniden dene.
                                </p>
                            </div>
                            <button
                                type="button"
                                onClick={handleRefetch}
                                className="rounded-none border border-red-500/40 bg-red-500/10 px-5 py-2.5 text-sm font-semibold text-red-200 transition hover:bg-red-500/20"
                            >
                                Tekrar dene
                            </button>
                        </div>
                    ) : showResults && (!suggestions || suggestions.length === 0) ? (
                        <div className="rounded-none border border-[#1f2334] bg-[#161a28] px-8 py-14 text-center">
                            <p className="font-semibold text-gray-300">Eşleşen oyun bulunamadı</p>
                            <p className="mt-2 text-sm text-gray-500">
                                Kütüphaneni güncelleyip yeniden analiz etmeyi dene.
                            </p>
                        </div>
                    ) : showResults ? (
                        <div>
                            <div className="mb-8 flex flex-wrap items-end justify-between gap-4 border-b border-[#1f2334] pb-6">
                                <div>
                                    <h2 className="text-lg font-bold text-white">Senin için seçilenler</h2>
                                    <p className="mt-1 text-sm text-gray-500">{suggestions.length} oyun</p>
                                </div>
                                {isRefreshing && (
                                    <span className="inline-flex items-center gap-2 text-xs font-medium text-blue-400/90">
                                        <Loader2 className="h-3.5 w-3.5 animate-spin" aria-hidden />
                                        Listeyi yeniliyor…
                                    </span>
                                )}
                            </div>
                            <div
                                className={`grid grid-cols-2 items-start gap-x-6 gap-y-16 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 ${isRefreshing ? 'pointer-events-none opacity-60' : ''} transition-opacity duration-200`}
                            >
                                {suggestions.map((game) => (
                                    <GameCard key={game.id} game={game} />
                                ))}
                            </div>
                        </div>
                    ) : null}
                </div>
            </div>
        </div>
    );
}
