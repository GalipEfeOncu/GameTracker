import { useMemo, useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import {
    Loader2,
    Sparkles,
    BrainCircuit,
    History,
    Wand2,
    Library,
    AlertCircle,
} from 'lucide-react';
import { getRecommendations, fetchUserLibrary } from '../api/apiClient';
import { useUser } from '../context/UserContext';
import GameCard from '../components/GameCard';
import { getSessionUserId } from '../utils/sessionUser';
import { useI18n } from '../i18n/useI18n';
import {
    readAiRecommendationCache,
    writeAiRecommendationCache,
    readLockedLibraryFingerprint,
} from '../utils/aiRecommendationCache';

export default function AiSuggestionPage() {
    const { user } = useUser();
    const userId = getSessionUserId(user);
    const { t } = useI18n();

    const [lockVersion, setLockVersion] = useState(0);

    const { data: library, isLoading: libraryLoading } = useQuery({
        queryKey: ['library', userId],
        queryFn: () => fetchUserLibrary(userId),
        enabled: userId != null,
    });

    const libraryFingerprint = useMemo(() => {
        if (!library?.length) return '';
        return library
            .map((g) => g.id)
            .sort((a, b) => a - b)
            .join(',');
    }, [library]);

    const likedGames = library?.map((g) => g.name) ?? [];

    const persistedSuggestions = useMemo(() => {
        void lockVersion;
        return readAiRecommendationCache(userId, libraryFingerprint);
    }, [userId, libraryFingerprint, lockVersion]);

    const lockedFingerprint = useMemo(() => {
        void lockVersion;
        return readLockedLibraryFingerprint(userId);
    }, [userId, lockVersion]);

    /** Önbellekte bu parmak izi için kayıt varsa veya storage kilidi eşleşiyorsa yeniden Gemini çağrısı yok. */
    const hasPersistedForFingerprint = persistedSuggestions !== undefined;
    const isSessionLocked =
        libraryFingerprint.length > 0 &&
        (lockedFingerprint === libraryFingerprint || hasPersistedForFingerprint);

    const recommendationMutation = useMutation({
        mutationKey: ['libraryAiRecommendations', userId, libraryFingerprint],
        mutationFn: () => getRecommendations(likedGames),
        gcTime: 1000 * 60 * 60 * 24,
        retry: false,
        onSuccess: (data) => {
            writeAiRecommendationCache(userId, libraryFingerprint, data);
            setLockVersion((v) => v + 1);
        },
    });

    const suggestions =
        recommendationMutation.data !== undefined ? recommendationMutation.data : persistedSuggestions;

    const recommendationErrMsg =
        recommendationMutation.error?.response?.data?.message ??
        recommendationMutation.error?.response?.data ??
        recommendationMutation.error?.message;

    const hasLoadedSuggestions = suggestions !== undefined;
    const showBlockingLoad =
        recommendationMutation.isPending && likedGames.length > 0 && !!libraryFingerprint;

    const showIdle =
        likedGames.length > 0 &&
        !libraryLoading &&
        !hasLoadedSuggestions &&
        !showBlockingLoad &&
        !recommendationMutation.isError;

    const showNoMatch =
        hasLoadedSuggestions &&
        Array.isArray(suggestions) &&
        suggestions.length === 0 &&
        !recommendationMutation.isError &&
        !showBlockingLoad;

    const showGrid =
        hasLoadedSuggestions &&
        Array.isArray(suggestions) &&
        suggestions.length > 0 &&
        !recommendationMutation.isError &&
        !showBlockingLoad;

    const canRun = likedGames.length > 0 && !libraryLoading && !!libraryFingerprint;

    const handleStart = () => {
        if (!canRun) return;
        recommendationMutation.mutate();
    };

    const handleRetry = () => {
        recommendationMutation.mutate();
    };

    if (!user) {
        return (
            <div className="flex h-full flex-col items-center justify-center px-6 py-20">
                <div className="mb-8 flex h-20 w-20 items-center justify-center rounded-none border border-blue-500/30 bg-blue-500/10">
                    <Sparkles className="h-10 w-10 text-blue-400" strokeWidth={1.25} />
                </div>
                <h2 className="text-center text-2xl font-bold tracking-tight text-white">{t('ai.guestTitle')}</h2>
                <p className="mt-3 max-w-sm text-center text-sm font-medium leading-relaxed text-gray-500">
                    {t('ai.guestHint')}
                </p>
            </div>
        );
    }

    return (
        <div className="h-full overflow-y-auto scroll-smooth px-4 pb-24 pt-8 sm:px-8 sm:pt-10">
            <div className="mx-auto max-w-6xl">
                <section className="rounded-none border border-[#1f2334] bg-[#141722]/40 px-6 py-10 sm:px-10 sm:py-12">
                    <div className="mb-5 inline-flex items-center gap-2 rounded-none border border-blue-500/25 bg-blue-500/10 px-3 py-1.5 text-[11px] font-bold uppercase tracking-[0.14em] text-blue-300">
                        <BrainCircuit className="h-3.5 w-3.5" aria-hidden />
                        {t('ai.badge')}
                    </div>
                    <h1 className="max-w-2xl text-3xl font-bold tracking-tight text-white sm:text-4xl">
                        {t('ai.heroTitle')}
                    </h1>
                    <p className="mt-3 max-w-xl text-sm font-medium leading-relaxed text-gray-400 sm:text-base">
                        {t('ai.heroDesc')}
                    </p>

                    <div className="mt-8 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                        <div className="flex items-center gap-3 text-sm text-gray-500">
                            <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-none border border-[#1f2334] bg-[#1a1e2d]">
                                <Library className="h-5 w-5 text-gray-400" aria-hidden />
                            </span>
                            <div>
                                {libraryLoading ? (
                                    <span className="text-gray-500">{t('ai.libLoading')}</span>
                                ) : likedGames.length === 0 ? (
                                    <span className="font-medium text-amber-500/90">{t('ai.libEmptyWarn')}</span>
                                ) : (
                                    <>
                                        <span className="font-semibold text-gray-200">{t('ai.gamesReadyCount', { n: likedGames.length })}</span>
                                        <span className="text-gray-500"> {t('ai.gamesReadySuffix')}</span>
                                    </>
                                )}
                            </div>
                        </div>

                        <div className="flex flex-wrap items-center gap-3 sm:justify-end">
                            {isSessionLocked && hasLoadedSuggestions && !recommendationMutation.isError ? (
                                <p className="max-w-md text-right text-sm leading-relaxed text-gray-500">{t('ai.lockedHint')}</p>
                            ) : recommendationMutation.isError ? null : showBlockingLoad ? (
                                <button
                                    type="button"
                                    disabled
                                    className="group inline-flex cursor-wait items-center justify-center gap-2 rounded-none border border-blue-500/50 bg-blue-600 px-6 py-3.5 text-sm font-bold text-white opacity-90"
                                >
                                    <Loader2 className="h-5 w-5 shrink-0 animate-spin" aria-hidden />
                                    {t('ai.preparing')}
                                </button>
                            ) : (
                                <button
                                    type="button"
                                    onClick={handleStart}
                                    disabled={!canRun || showBlockingLoad}
                                    className="group inline-flex items-center justify-center gap-2 rounded-none border border-blue-500/50 bg-blue-600 px-6 py-3.5 text-sm font-bold text-white transition-colors hover:bg-blue-500 disabled:cursor-not-allowed disabled:opacity-45"
                                >
                                    <Wand2 className="h-5 w-5 shrink-0 transition group-hover:rotate-6" aria-hidden />
                                    {t('ai.generate')}
                                </button>
                            )}
                        </div>
                    </div>
                </section>

                <div className="mt-10">
                    {libraryLoading ? (
                        <div className="flex flex-col items-center justify-center py-20 text-gray-500">
                            <Loader2 className="mb-4 h-10 w-10 animate-spin text-blue-500" />
                            <p className="text-sm font-medium">{t('ai.libLoadingBlock')}</p>
                        </div>
                    ) : likedGames.length === 0 ? (
                        <div className="flex flex-col items-center rounded-none border border-dashed border-[#2a3148] bg-[#141722]/40 px-8 py-16 text-center">
                            <History className="mb-5 h-12 w-12 text-gray-600" strokeWidth={1.25} aria-hidden />
                            <p className="text-lg font-bold text-gray-300">{t('ai.emptyLibTitle')}</p>
                            <p className="mt-2 max-w-md text-sm leading-relaxed text-gray-500">
                                {t('ai.emptyLibHint')}
                            </p>
                        </div>
                    ) : showIdle ? (
                        <div className="rounded-none border border-[#1f2334] bg-[#161a28] px-8 py-14 text-center">
                            <Sparkles className="mx-auto mb-4 h-11 w-11 text-blue-400/80" strokeWidth={1.25} aria-hidden />
                            <p className="text-base font-semibold text-gray-200">{t('ai.idleTitle')}</p>
                            <p className="mx-auto mt-2 max-w-lg text-sm leading-relaxed text-gray-500">
                                {t('ai.idleHint')}
                            </p>
                        </div>
                    ) : showBlockingLoad ? (
                        <div className="flex flex-col items-center justify-center rounded-none border border-[#1f2334] bg-[#141722]/30 py-24">
                            <Loader2 className="mb-5 h-11 w-11 animate-spin text-blue-500" />
                            <p className="text-base font-semibold text-gray-300">{t('ai.workingTitle')}</p>
                            <p className="mt-1 text-sm text-gray-500">{t('ai.workingSub')}</p>
                        </div>
                    ) : recommendationMutation.isError ? (
                        <div
                            role="alert"
                            className="flex flex-col items-center gap-4 rounded-none border border-red-500/25 bg-red-500/5 px-8 py-12 text-center"
                        >
                            <AlertCircle className="h-10 w-10 text-red-400" aria-hidden />
                            <div>
                                <p className="font-bold text-red-300">{t('ai.errorTitle')}</p>
                                <p className="mt-1 max-w-lg text-sm text-red-200/70">
                                    {typeof recommendationErrMsg === 'string' && recommendationErrMsg.trim()
                                        ? recommendationErrMsg
                                        : t('ai.errorHint')}
                                </p>
                            </div>
                            <button
                                type="button"
                                onClick={handleRetry}
                                className="rounded-none border border-red-500/40 bg-red-500/10 px-5 py-2.5 text-sm font-semibold text-red-200 transition hover:bg-red-500/20"
                            >
                                {t('common.retry')}
                            </button>
                        </div>
                    ) : showNoMatch ? (
                        <div className="rounded-none border border-[#1f2334] bg-[#161a28] px-8 py-14 text-center">
                            <p className="font-semibold text-gray-300">{t('ai.noMatchTitle')}</p>
                            <p className="mt-2 text-sm text-gray-500">
                                {t('ai.noMatchLockedHint')}
                            </p>
                        </div>
                    ) : showGrid ? (
                        <div>
                            <div className="mb-8 flex flex-wrap items-end justify-between gap-4 border-b border-[#1f2334] pb-6">
                                <div>
                                    <h2 className="text-lg font-bold text-white">{t('ai.resultsTitle')}</h2>
                                    <p className="mt-1 text-sm text-gray-500">{t('ai.resultsCount', { n: suggestions.length })}</p>
                                </div>
                                {recommendationMutation.isPending && (
                                    <span className="inline-flex items-center gap-2 text-xs font-medium text-blue-400/90">
                                        <Loader2 className="h-3.5 w-3.5 animate-spin" aria-hidden />
                                        {t('ai.refreshingList')}
                                    </span>
                                )}
                            </div>
                            <div
                                className={`grid grid-cols-2 items-start gap-x-6 gap-y-16 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 ${recommendationMutation.isPending ? 'pointer-events-none opacity-60' : ''} transition-opacity duration-200`}
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
