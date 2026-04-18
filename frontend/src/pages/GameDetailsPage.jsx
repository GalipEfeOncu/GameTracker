import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Calendar, Clock, Clock3, Globe, Plus, Check, ChevronDown, ChevronUp, Trash2, Shield } from 'lucide-react';
import { useState, useRef, useEffect } from 'react';
import { getGameDetails, fetchUserLibrary, addGameToLibrary, updateGameStatus, removeGameFromLibrary, getGameScreenshots } from '../api/apiClient';
import { useUser } from '../context/UserContext';
import { useToast } from '../context/ToastContext';
import GameDetailSkeleton from '../components/GameDetailSkeleton';
import { LIBRARY_STATUS } from '../constants/libraryStatus';
import { getSessionUserId } from '../utils/sessionUser';
import { useI18n } from '../i18n/useI18n';

function formatLibraryMutationError(err, t, fallbackKey) {
    const status = err.response?.status;
    const d = err.response?.data;
    if (status === 403)
        return t('gameDetails.errForbidden');
    if (typeof d === 'string') {
        if (d.includes('already exists in library')) return t('gameDetails.errDuplicate');
        return d;
    }
    if (d && typeof d.message === 'string') return d.message;
    if (d && typeof d.title === 'string') return d.title;
    if (d && d.errors && typeof d.errors === 'object') {
        const msgs = Object.values(d.errors).flat().filter((x) => typeof x === 'string');
        if (msgs.length) return msgs.join(' ');
    }
    return t(fallbackKey);
}

function formatPlayHours(h, t) {
    if (h == null || Number.isNaN(Number(h))) return null;
    const n = Number(h);
    const s = Number.isInteger(n) ? String(n) : n.toFixed(1).replace(/\.0$/, '');
    return t('time.playHours', { n: s });
}

function formatUserPlaytimeMinutes(minutes, t) {
    const m = Number(minutes);
    if (!Number.isFinite(m) || m < 0) return '—';
    if (m === 0) return t('time.userNone');
    if (m < 60) return t('time.userMinutes', { n: m });
    const hours = Math.floor(m / 60);
    const mins = m % 60;
    return mins ? t('time.userHoursMinutes', { h: hours, m: mins }) : t('time.userHoursOnly', { h: hours });
}

/** Detay hero: tek satır, kaynak + puan aynı hizada */
function HeroScoreChip({ label, value, detail, accentClass = 'text-gray-400', className = '' }) {
    const title = detail ? `${label} — ${detail}` : label;
    return (
        <div
            className={`flex h-10 min-w-0 shrink-0 items-center gap-2.5 rounded-md border border-white/10 bg-black/40 px-3.5 backdrop-blur-md ${className}`}
            title={title}
        >
            <span className={`text-[11px] font-bold uppercase tracking-wider ${accentClass}`}>{label}</span>
            <span className="text-lg font-bold tabular-nums leading-none text-white">{value}</span>
            {detail ? (
                <span className="hidden max-w-[7.5rem] truncate text-[11px] text-gray-500 sm:inline" aria-hidden>
                    {detail}
                </span>
            ) : null}
        </div>
    );
}

export default function GameDetailsPage() {
    const { id } = useParams();
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const { user } = useUser();
    const { showToast } = useToast();
    const { t } = useI18n();
    const userId = getSessionUserId(user);
    const [isExpanded, setIsExpanded] = useState(false);
    const [libraryMenuOpen, setLibraryMenuOpen] = useState(false);
    const libraryMenuRef = useRef(null);

    const { data: game, isLoading } = useQuery({
        queryKey: ['game', id],
        queryFn: () => getGameDetails(id),
    });

    const { data: userLibrary } = useQuery({
        queryKey: ['library', userId],
        queryFn: () => fetchUserLibrary(userId, null),
        enabled: !!userId,
    });

    const libraryEntry = userLibrary?.find((entry) => Number(entry.id) === Number(id));
    const isInLibrary = !!libraryEntry;
    const libraryStatus = libraryEntry?.status ?? null;

    const invalidateLibrary = () => {
        queryClient.invalidateQueries({ queryKey: ['library', userId] });
    };

    const addMutation = useMutation({
        mutationFn: ({ status }) => addGameToLibrary(userId, game, status),
        onSuccess: () => {
            invalidateLibrary();
            showToast(t('gameDetails.toastAdded'), 'success');
        },
        onError: (error) => {
            showToast(formatLibraryMutationError(error, t, 'gameDetails.errAdd'), 'error');
        },
    });
    const updateMutation = useMutation({
        mutationFn: ({ newStatus }) => updateGameStatus(userId, id, newStatus),
        onSuccess: () => {
            invalidateLibrary();
            showToast(t('gameDetails.toastUpdated'), 'success');
        },
        onError: (error) => {
            showToast(formatLibraryMutationError(error, t, 'gameDetails.errUpdate'), 'error');
        },
    });
    const removeMutation = useMutation({
        mutationFn: () => removeGameFromLibrary(userId, id),
        onSuccess: () => {
            invalidateLibrary();
            showToast(t('gameDetails.toastRemoved'), 'success');
        },
        onError: (error) => {
            showToast(formatLibraryMutationError(error, t, 'gameDetails.errRemove'), 'error');
        },
    });

    const handleAddOrUpdate = (status) => {
        setLibraryMenuOpen(false);
        if (isInLibrary) updateMutation.mutate({ newStatus: status });
        else addMutation.mutate({ status });
    };
    const handleRemove = () => {
        setLibraryMenuOpen(false);
        removeMutation.mutate();
    };

    useEffect(() => {
        const handleClickOutside = (e) => {
            if (libraryMenuRef.current && !libraryMenuRef.current.contains(e.target)) setLibraryMenuOpen(false);
        };
        if (libraryMenuOpen) document.addEventListener('click', handleClickOutside);
        return () => document.removeEventListener('click', handleClickOutside);
    }, [libraryMenuOpen]);

    const { data: screenshots = [] } = useQuery({
        queryKey: ['screenshots', id],
        queryFn: () => getGameScreenshots(id),
        enabled: !!id,
    });

    if (isLoading) {
        return <GameDetailSkeleton />;
    }

    if (!game) return <div className="p-8">{t('gameDetails.notFound')}</div>;

    const pcRequirements = game.platforms?.find(p => p.platform?.slug === 'pc')?.requirements || game.platforms?.find(p => p.platform?.slug === 'pc')?.requirements_en;
    const requirements = pcRequirements || game.platforms?.find(p => p.requirements || p.requirements_en)?.requirements || game.platforms?.find(p => p.requirements || p.requirements_en)?.requirements_en;

    const maxDescriptionLength = 500;
    const descriptionHtml = game.description || game.description_raw || t('gameDetails.noDescription');

    // Basit bir tag temizleme işlemiyle uzunluk hesaplama (tam doğru olmasa da görsel uzunluk için yeterli)
    const textLength = descriptionHtml.replace(/<[^>]+>/g, '').length;
    const shouldTruncate = textLength > maxDescriptionLength;

    const heroImageSrc = game.background_image_additional || game.background_image;
    const hasMetacritic = game.metacritic != null && Number(game.metacritic) > 0;
    const hasIgdbCritic =
        game.igdb_aggregated_rating != null &&
        game.igdb_aggregated_rating_count != null &&
        Number(game.igdb_aggregated_rating_count) > 0;
    const hasIgdbTotal = game.igdb_total_rating != null && Number(game.igdb_total_rating) > 0;
    const showIgdbTotalOnly = hasIgdbTotal && !hasIgdbCritic;
    const hasAnyScore = hasMetacritic || hasIgdbCritic || showIgdbTotalOnly;
    const ageRatings = Array.isArray(game.age_ratings_display) ? game.age_ratings_display : [];
    const primaryAgeRating = ageRatings[0] ?? null;
    const trailerId = typeof game.trailer_youtube_id === 'string' && game.trailer_youtube_id.trim().length >= 6
        ? game.trailer_youtube_id.trim()
        : null;
    const coverSrc = game.background_image || heroImageSrc;
    const ttb = game.time_to_beat;
    const hasTtb =
        ttb &&
        (ttb.main_story_hours != null || ttb.main_extra_hours != null || ttb.completionist_hours != null);

    return (
        <div className="h-full overflow-y-auto bg-[#0f111a] scroll-smooth">
            {/* Arka plan + hero: başlık üstte; kapak | video | sağ meta */}
            <section className="relative min-h-[min(78vh,880px)] w-full pb-10 lg:pb-14">
                <div className="absolute inset-0 z-0 overflow-hidden bg-[#0f111a]">
                    {heroImageSrc ? (
                        <>
                            <img
                                src={heroImageSrc}
                                alt=""
                                className="absolute inset-0 h-full w-full scale-110 object-cover opacity-45 blur-3xl"
                                aria-hidden
                            />
                            <img
                                src={heroImageSrc}
                                alt=""
                                className="absolute inset-0 z-[1] h-full w-full object-cover object-center opacity-35"
                                aria-hidden
                            />
                        </>
                    ) : null}
                    <div className="absolute inset-0 z-[2] bg-gradient-to-t from-[#0f111a] via-[#0f111a]/80 to-[#0f111a]/90" />
                    <div className="absolute inset-0 z-[2] bg-gradient-to-r from-[#0f111a]/95 via-transparent to-[#0f111a]/40" />
                </div>

                <button
                    type="button"
                    onClick={() => navigate(-1)}
                    className="absolute left-6 top-8 z-30 flex h-12 w-12 items-center justify-center rounded-none border border-white/10 bg-black/50 text-gray-200 shadow-sm backdrop-blur-md transition-all hover:bg-black/70 hover:text-white lg:left-12"
                >
                    <ArrowLeft size={24} />
                </button>

                <div className="relative z-10 mx-auto max-w-[1600px] px-6 pt-24 lg:px-12 lg:pt-28">
                    <h1 className="mb-8 text-4xl font-bold tracking-tight text-white drop-shadow-lg sm:text-5xl lg:mb-10 lg:text-6xl">
                        {game.name}
                    </h1>

                    {/* Kapak | video | meta (geniş ekranda yan yana) */}
                    <div className="flex flex-col gap-10 xl:flex-row xl:items-start xl:gap-8">
                        <aside className="mx-auto w-full max-w-[240px] shrink-0 xl:mx-0">
                            <div className="aspect-[2/3] w-full overflow-hidden border border-[#1f2334] bg-[#141722] shadow-lg shadow-black/40">
                                {coverSrc ? (
                                    <img
                                        src={coverSrc}
                                        alt={game.name}
                                        className="h-full w-full object-cover object-center"
                                    />
                                ) : (
                                    <div className="flex h-full items-center justify-center text-xs text-gray-600">{t('gameDetails.coverMissing')}</div>
                                )}
                            </div>
                            <div className="relative mt-3 w-full" ref={libraryMenuRef}>
                                {userId ? (
                                    <>
                                        <button
                                            type="button"
                                            onClick={() => setLibraryMenuOpen((v) => !v)}
                                            className={`flex w-full items-center justify-center gap-2 rounded-none border px-4 py-3.5 text-sm font-bold shadow-md transition-colors ${
                                                isInLibrary
                                                    ? 'border-green-500/40 bg-[#1a1e2d] text-green-400 hover:bg-[#22262f]'
                                                    : 'border-transparent bg-blue-600 text-white hover:bg-blue-500'
                                            }`}
                                        >
                                            {isInLibrary ? (
                                                <>
                                                    <Check size={20} />
                                                    <span className="truncate">
                                                        {t('gameDetails.inLibrary')}
                                                        {libraryStatus ? ` (${t(`library.status.${libraryStatus}`)})` : ''}
                                                    </span>
                                                </>
                                            ) : (
                                                <>
                                                    <Plus size={20} /> {t('gameDetails.addToLibrary')}
                                                </>
                                            )}
                                            <ChevronDown size={18} className={`shrink-0 ${libraryMenuOpen ? 'rotate-180' : ''}`} />
                                        </button>
                                        {libraryMenuOpen && (
                                            <div className="absolute left-0 right-0 top-full z-30 mt-2 border border-[#1f2334] bg-[#141722] py-1 shadow-xl">
                                                {Object.keys(LIBRARY_STATUS).map((statusId) => (
                                                    <button
                                                        key={statusId}
                                                        type="button"
                                                        onClick={() => handleAddOrUpdate(statusId)}
                                                        disabled={(addMutation.isPending || updateMutation.isPending) && (isInLibrary ? libraryStatus === statusId : false)}
                                                        className={`w-full px-4 py-2.5 text-left text-sm font-medium transition-colors ${libraryStatus === statusId ? 'bg-blue-500/10 text-blue-400' : 'text-gray-300 hover:bg-[#1a1e2d] hover:text-white'}`}
                                                    >
                                                        {t(`library.status.${statusId}`)}
                                                    </button>
                                                ))}
                                                {isInLibrary && (
                                                    <>
                                                        <div className="my-1 border-t border-[#1f2334]" />
                                                        <button
                                                            type="button"
                                                            onClick={handleRemove}
                                                            disabled={removeMutation.isPending}
                                                            className="flex w-full items-center gap-2 px-4 py-2.5 text-left text-sm font-medium text-red-400 hover:bg-red-500/10"
                                                        >
                                                            <Trash2 size={14} /> {t('gameDetails.removeFromLibrary')}
                                                        </button>
                                                    </>
                                                )}
                                            </div>
                                        )}
                                    </>
                                ) : (
                                    <p className="border border-[#1f2334] bg-black/30 px-3 py-3 text-center text-xs text-gray-500">
                                        {t('gameDetails.loginToAdd')}
                                    </p>
                                )}
                            </div>
                            {userId && isInLibrary ? (
                                <div className="mt-4 border border-[#1f2334] bg-black/35 px-3 py-3">
                                    <div className="mb-1 flex items-center gap-2 text-[10px] font-bold uppercase tracking-widest text-gray-500">
                                        <Clock3 size={14} className="shrink-0 text-blue-400" aria-hidden />
                                        {t('gameDetails.yourPlaytime')}
                                    </div>
                                    <p className={`text-sm font-semibold tabular-nums ${(libraryEntry?.playtimeMinutes ?? 0) > 0 ? 'text-blue-300' : 'text-gray-500'}`}>
                                        {formatUserPlaytimeMinutes(libraryEntry?.playtimeMinutes ?? 0, t)}
                                    </p>
                                    {(libraryEntry?.playtimeMinutes ?? 0) === 0 && (
                                        <p className="mt-1.5 text-[11px] leading-relaxed text-gray-600">
                                            {t('gameDetails.playtimeDesktopHint')}
                                        </p>
                                    )}
                                </div>
                            ) : null}
                        </aside>

                        <div className="min-w-0 w-full flex-1 overflow-hidden border border-[#1f2334] bg-black/50 shadow-lg">
                            {trailerId ? (
                                <div className="aspect-video w-full">
                                    <iframe
                                        title={t('gameDetails.trailerTitle', { name: game.name })}
                                        src={`https://www.youtube-nocookie.com/embed/${encodeURIComponent(trailerId)}?rel=0`}
                                        className="h-full w-full border-0"
                                        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
                                        allowFullScreen
                                    />
                                </div>
                            ) : (
                                <div className="flex aspect-video w-full items-center justify-center bg-[#0a0c12] px-4 text-center text-sm text-gray-500">
                                    {t('gameDetails.noTrailer')}
                                </div>
                            )}
                        </div>

                        <aside className="w-full shrink-0 space-y-5 border border-[#1f2334] bg-black/35 p-4 backdrop-blur-sm xl:w-[min(100%,280px)]">
                            {game.genres?.length ? (
                                <div>
                                    <h2 className="mb-2 text-[10px] font-bold uppercase tracking-widest text-gray-500">{t('gameDetails.genres')}</h2>
                                    <div className="flex flex-wrap gap-1.5">
                                        {game.genres.map((g) => (
                                            <span
                                                key={g.id}
                                                className="rounded-none border border-blue-500/30 bg-blue-600/20 px-2 py-0.5 text-[11px] font-bold uppercase tracking-wide text-blue-400"
                                            >
                                                {g.name}
                                            </span>
                                        ))}
                                    </div>
                                </div>
                            ) : null}

                            {hasAnyScore ? (
                                <div>
                                    <h2 className="mb-2 text-[10px] font-bold uppercase tracking-widest text-gray-500">{t('gameDetails.scores')}</h2>
                                    <div className="flex flex-col gap-2">
                                        {hasMetacritic ? (
                                            <HeroScoreChip
                                                className="w-full"
                                                label={t('gameDetails.labelMetacritic')}
                                                value={game.metacritic}
                                                accentClass="text-yellow-500/90"
                                            />
                                        ) : null}
                                        {hasIgdbCritic ? (
                                            <HeroScoreChip
                                                className="w-full"
                                                label={t('gameDetails.labelCritic')}
                                                value={game.igdb_aggregated_rating}
                                                detail={t('gameDetails.criticSources', { n: game.igdb_aggregated_rating_count })}
                                                accentClass="text-amber-400/90"
                                            />
                                        ) : null}
                                        {showIgdbTotalOnly ? (
                                            <HeroScoreChip
                                                className="w-full"
                                                label={t('gameDetails.labelOverall')}
                                                value={game.igdb_total_rating}
                                                detail={t('gameDetails.overallDetail')}
                                                accentClass="text-amber-400/90"
                                            />
                                        ) : null}
                                    </div>
                                </div>
                            ) : null}

                            <div className="space-y-3 text-sm text-gray-300">
                                {primaryAgeRating ? (
                                    <div className="flex items-start gap-2">
                                        <Shield className="mt-0.5 shrink-0 text-orange-400" size={18} strokeWidth={2} aria-hidden />
                                        <div>
                                            <div className="text-[10px] font-bold uppercase tracking-widest text-gray-500">{t('gameDetails.ageRating')}</div>
                                            <span
                                                className="mt-0.5 inline-flex max-w-full items-center border border-orange-500/35 bg-orange-500/10 px-2 py-1 text-xs font-bold uppercase tracking-wide text-orange-100"
                                                title={
                                                    primaryAgeRating.organization
                                                        ? `${primaryAgeRating.organization}: ${primaryAgeRating.label}`
                                                        : primaryAgeRating.label
                                                }
                                            >
                                                {primaryAgeRating.organization
                                                    ? `${primaryAgeRating.organization}: ${primaryAgeRating.label}`
                                                    : primaryAgeRating.label}
                                            </span>
                                        </div>
                                    </div>
                                ) : null}
                                {game.released ? (
                                    <div className="flex items-center gap-2">
                                        <Calendar className="shrink-0 text-blue-400" size={18} aria-hidden />
                                        <div>
                                            <div className="text-[10px] font-bold uppercase tracking-widest text-gray-500">{t('gameDetails.released')}</div>
                                            <span className="tabular-nums text-gray-200">{game.released}</span>
                                        </div>
                                    </div>
                                ) : null}
                                {game.website ? (
                                    <div className="flex items-center gap-2">
                                        <Globe className="shrink-0 text-indigo-400" size={18} aria-hidden />
                                        <div>
                                            <div className="text-[10px] font-bold uppercase tracking-widest text-gray-500">{t('gameDetails.site')}</div>
                                            <a
                                                href={game.website}
                                                target="_blank"
                                                rel="noreferrer"
                                                className="text-gray-200 underline-offset-2 transition-colors hover:text-white hover:underline"
                                            >
                                                {t('gameDetails.officialSite')}
                                            </a>
                                        </div>
                                    </div>
                                ) : null}
                            </div>

                            {hasTtb ? (
                                <div className="border-t border-white/10 pt-4">
                                    <div className="mb-2 flex items-center gap-2 text-[10px] font-bold uppercase tracking-widest text-gray-500">
                                        <Clock className="text-emerald-400/90" size={14} aria-hidden />
                                        {t('gameDetails.ttbTitle')}
                                    </div>
                                    <ul className="space-y-1.5 text-sm text-gray-200">
                                        {formatPlayHours(ttb.main_story_hours, t) ? (
                                            <li className="flex justify-between gap-2">
                                                <span className="text-gray-500">{t('gameDetails.ttbMain')}</span>
                                                <span className="tabular-nums font-semibold">{formatPlayHours(ttb.main_story_hours, t)}</span>
                                            </li>
                                        ) : null}
                                        {formatPlayHours(ttb.main_extra_hours, t) ? (
                                            <li className="flex justify-between gap-2">
                                                <span className="text-gray-500">{t('gameDetails.ttbMainExtra')}</span>
                                                <span className="tabular-nums font-semibold">{formatPlayHours(ttb.main_extra_hours, t)}</span>
                                            </li>
                                        ) : null}
                                        {formatPlayHours(ttb.completionist_hours, t) ? (
                                            <li className="flex justify-between gap-2">
                                                <span className="text-gray-500">{t('gameDetails.ttbCompletionist')}</span>
                                                <span className="tabular-nums font-semibold">{formatPlayHours(ttb.completionist_hours, t)}</span>
                                            </li>
                                        ) : null}
                                    </ul>
                                    {ttb.submission_count != null && ttb.submission_count > 0 ? (
                                        <p className="mt-2 text-[11px] text-gray-600">{t('gameDetails.ttbBasedOn', { n: ttb.submission_count })}</p>
                                    ) : null}
                                </div>
                            ) : null}
                        </aside>
                    </div>
                </div>
            </section>

            <div className="w-full border-t border-[#1f2334] px-6 py-12 lg:px-12">
                <div className="mx-auto flex max-w-[1600px] flex-col gap-12 xl:flex-row xl:gap-12">
                    <div className="min-w-0 flex-1 xl:max-w-5xl">
                        <section>
                            <h2 className="mb-6 border-b border-[#1f2334] pb-2 text-xl font-bold uppercase tracking-widest text-white opacity-50">
                                {t('gameDetails.about')}
                            </h2>
                            <div className="relative">
                                <div
                                    className={`text-lg font-medium leading-[1.8] text-gray-400 transition-all duration-500 ${!isExpanded && shouldTruncate ? 'max-h-[300px] overflow-hidden' : ''}`}
                                    dangerouslySetInnerHTML={{ __html: descriptionHtml }}
                                />
                                {!isExpanded && shouldTruncate && (
                                    <div className="pointer-events-none absolute bottom-0 left-0 h-32 w-full bg-gradient-to-t from-[#0f111a] to-transparent" />
                                )}
                            </div>
                            {shouldTruncate && (
                                <button
                                    type="button"
                                    onClick={() => setIsExpanded(!isExpanded)}
                                    className="mt-4 flex items-center gap-2 font-bold text-blue-500 transition-colors hover:text-blue-400"
                                >
                                    {isExpanded ? (
                                        <>
                                            {t('gameDetails.collapse')} <ChevronUp size={18} />
                                        </>
                                    ) : (
                                        <>
                                            {t('gameDetails.expand')} <ChevronDown size={18} />
                                        </>
                                    )}
                                </button>
                            )}
                        </section>
                    </div>

                    <div className="w-full shrink-0 space-y-8 xl:w-[450px]">
                        <div className="space-y-6 rounded-none border border-[#1f2334] bg-[#141722] p-6 shadow-md">
                            <div>
                                <h3 className="mb-2 text-sm font-bold uppercase tracking-widest text-gray-500">{t('gameDetails.developer')}</h3>
                                <div className="flex flex-wrap gap-2 font-semibold text-gray-200">
                                    {game.developers?.map((d) => d.name).join(', ') || '-'}
                                </div>
                            </div>
                            <div>
                                <h3 className="mb-2 text-sm font-bold uppercase tracking-widest text-gray-500">{t('gameDetails.publisher')}</h3>
                                <div className="flex flex-wrap gap-2 font-semibold text-gray-200">
                                    {game.publishers?.map((p) => p.name).join(', ') || '-'}
                                </div>
                            </div>
                            <div>
                                <h3 className="mb-2 text-sm font-bold uppercase tracking-widest text-gray-500">{t('gameDetails.platforms')}</h3>
                                <div className="flex flex-wrap gap-2">
                                    {game.platforms?.map((p) => (
                                        <span
                                            key={p.platform.id}
                                            className="rounded-none border border-[#1f2334] bg-[#1a1e2d] px-2.5 py-1 text-xs font-semibold uppercase text-gray-300"
                                        >
                                            {p.platform.name}
                                        </span>
                                    ))}
                                </div>
                            </div>
                            {game.stores && game.stores.length > 0 && (
                                <div className="pt-2">
                                    <h3 className="mb-2 text-sm font-bold uppercase tracking-widest text-gray-500">{t('gameDetails.buy')}</h3>
                                    <div className="flex flex-wrap gap-2">
                                        {game.stores.map((s, idx) => (
                                            <a
                                                key={idx}
                                                href={s.url}
                                                target="_blank"
                                                rel="noreferrer"
                                                className="flex items-center rounded-none border border-blue-500/30 bg-blue-500/10 px-3 py-1.5 text-xs font-semibold text-blue-400 shadow-sm transition-all hover:bg-blue-500/20 hover:text-white"
                                            >
                                                {s.store?.name || t('common.store')}
                                            </a>
                                        ))}
                                    </div>
                                </div>
                            )}
                        </div>

                        <div className="relative overflow-hidden rounded-none border border-[#1f2334] bg-[#141722] p-6 shadow-md">
                            <h3 className="mb-6 flex items-center gap-2 border-b border-[#1f2334] pb-3 text-lg font-bold tracking-tight text-white">
                                <Check className="text-blue-500" size={20} /> {t('gameDetails.sysReqTitle')}
                            </h3>

                            {requirements && (requirements.minimum || requirements.recommended) ? (
                                <div className="space-y-6">
                                    {requirements.minimum && (
                                        <div>
                                            <h4 className="mb-2 font-semibold text-gray-300">{t('gameDetails.minimum')}</h4>
                                            <p className="whitespace-pre-line rounded-none border border-[#1f2334] bg-[#0f111a] p-4 text-sm leading-relaxed text-gray-400">
                                                {requirements.minimum.replace(/^Minimum:\s*/i, '')}
                                            </p>
                                        </div>
                                    )}
                                    {requirements.recommended && (
                                        <div>
                                            <h4 className="mb-2 font-semibold text-gray-300">{t('gameDetails.recommended')}</h4>
                                            <p className="whitespace-pre-line rounded-none border border-[#1f2334] bg-[#0f111a] p-4 text-sm leading-relaxed text-gray-400">
                                                {requirements.recommended.replace(/^Recommended:\s*/i, '')}
                                            </p>
                                        </div>
                                    )}
                                </div>
                            ) : (
                                <p className="text-sm italic leading-relaxed text-gray-500">
                                    {t('gameDetails.sysReqEmpty')}
                                </p>
                            )}
                        </div>
                    </div>
                </div>
            </div>

            {/* Ekran görüntüleri */}
            {screenshots.length > 0 && (
                <section className="px-12 py-8 border-t border-[#1f2334]">
                    <h2 className="text-xl font-bold text-white uppercase tracking-widest mb-6 opacity-50 border-b border-[#1f2334] pb-2">{t('gameDetails.screenshots')}</h2>
                    <div className="flex gap-4 overflow-x-auto pb-4 scroll-smooth snap-x snap-mandatory">
                        {screenshots.map((shot) => (
                            <a
                                key={shot.id}
                                href={shot.imageUrl ?? shot.image}
                                target="_blank"
                                rel="noreferrer"
                                className="flex h-48 w-72 shrink-0 snap-center items-center justify-center rounded-none border border-[#1f2334] bg-[#0a0c12] transition-colors hover:border-blue-500/50 sm:w-80"
                            >
                                <img
                                    src={shot.imageUrl ?? shot.image}
                                    alt=""
                                    className="max-h-full max-w-full object-contain"
                                    loading="lazy"
                                />
                            </a>
                        ))}
                    </div>
                </section>
            )}
        </div>
    );
}
