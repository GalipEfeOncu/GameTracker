import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Star, Calendar, Globe, Plus, Check, ChevronDown, ChevronUp, Trash2 } from 'lucide-react';
import { useState, useRef, useEffect } from 'react';
import { getGameDetails, fetchUserLibrary, addGameToLibrary, updateGameStatus, removeGameFromLibrary, getGameScreenshots } from '../api/apiClient';
import { useUser } from '../context/UserContext';
import { useToast } from '../context/ToastContext';
import GameDetailSkeleton from '../components/GameDetailSkeleton';
import { LIBRARY_STATUS, getStatusLabel } from '../constants/libraryStatus';
import { getSessionUserId } from '../utils/sessionUser';

function formatLibraryMutationError(err, fallback) {
    const status = err.response?.status;
    const d = err.response?.data;
    if (status === 403)
        return 'Bu işlem için yetkin yok. Oturumun süresi dolmuş olabilir — tekrar giriş yapın.';
    if (typeof d === 'string') {
        if (d.includes('already exists in library')) return 'Bu oyun zaten kütüphanende. Durumunu menüden güncelleyebilirsin.';
        return d;
    }
    if (d && typeof d.message === 'string') return d.message;
    if (d && typeof d.title === 'string') return d.title;
    // ASP.NET model doğrulama: { errors: { "Alan": ["mesaj"] } }
    if (d && d.errors && typeof d.errors === 'object') {
        const msgs = Object.values(d.errors).flat().filter((x) => typeof x === 'string');
        if (msgs.length) return msgs.join(' ');
    }
    return fallback;
}

export default function GameDetailsPage() {
    const { id } = useParams();
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const { user } = useUser();
    const { showToast } = useToast();
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
            showToast('Oyun kütüphanene eklendi.', 'success');
        },
        onError: (error) => {
            showToast(formatLibraryMutationError(error, 'Oyun kütüphaneye eklenirken bir hata oluştu.'), 'error');
        },
    });
    const updateMutation = useMutation({
        mutationFn: ({ newStatus }) => updateGameStatus(userId, id, newStatus),
        onSuccess: () => {
            invalidateLibrary();
            showToast('Oyun durumu güncellendi.', 'success');
        },
        onError: (error) => {
            showToast(formatLibraryMutationError(error, 'Oyun durumu güncellenirken bir hata oluştu.'), 'error');
        },
    });
    const removeMutation = useMutation({
        mutationFn: () => removeGameFromLibrary(userId, id),
        onSuccess: () => {
            invalidateLibrary();
            showToast('Oyun kütüphanenden kaldırıldı.', 'success');
        },
        onError: (error) => {
            showToast(formatLibraryMutationError(error, 'Oyun kütüphaneden kaldırılırken bir hata oluştu.'), 'error');
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

    if (!game) return <div className="p-8">Oyun bulunamadı.</div>;

    const pcRequirements = game.platforms?.find(p => p.platform?.slug === 'pc')?.requirements || game.platforms?.find(p => p.platform?.slug === 'pc')?.requirements_en;
    const requirements = pcRequirements || game.platforms?.find(p => p.requirements || p.requirements_en)?.requirements || game.platforms?.find(p => p.requirements || p.requirements_en)?.requirements_en;

    const maxDescriptionLength = 500;
    const descriptionHtml = game.description || game.description_raw || 'Açıklama bulunamadı.';

    // Basit bir tag temizleme işlemiyle uzunluk hesaplama (tam doğru olmasa da görsel uzunluk için yeterli)
    const textLength = descriptionHtml.replace(/<[^>]+>/g, '').length;
    const shouldTruncate = textLength > maxDescriptionLength;

    return (
        <div className="h-full overflow-y-auto bg-[#0f111a] scroll-smooth">
            {/* Hero Header */}
            <div className="relative h-[55vh] min-h-[450px] w-full">
                <div className="absolute inset-0 z-0">
                    <img
                        src={game.background_image}
                        alt={game.name}
                        className="w-full h-full object-cover"
                    />
                    <div className="absolute inset-0 bg-gradient-to-t from-[#0f111a] via-[#0f111a]/50 to-black/30" />
                    <div className="absolute inset-0 bg-gradient-to-r from-[#0f111a] via-[#0f111a]/50 to-transparent" />
                </div>

                <button
                    onClick={() => navigate(-1)}
                    className="absolute top-8 left-12 z-20 w-12 h-12 rounded-none bg-black/50 backdrop-blur-md flex items-center justify-center text-gray-200 hover:bg-black/70 hover:text-white transition-all border border-white/10 shadow-sm"
                >
                    <ArrowLeft size={24} />
                </button>

                {/* Oyun ismi ve butonlar solda */}
                <div className="absolute bottom-0 left-0 px-12 pb-12 z-10 w-full flex justify-between items-end">
                    <div className="max-w-4xl">
                        <div className="flex items-center gap-3 mb-6">
                            {game.genres?.map(g => (
                                <span key={g.id} className="px-3 py-1 rounded-none bg-blue-600/20 border border-blue-500/30 text-blue-400 text-xs font-bold uppercase tracking-wider">
                                    {g.name}
                                </span>
                            ))}
                        </div>
                        <h1 className="text-6xl lg:text-7xl font-bold text-white tracking-tight mb-4 drop-shadow-lg">{game.name}</h1>

                        <div className="flex flex-wrap items-center gap-8 mt-4 text-gray-300 font-semibold">
                            <div className="flex items-center gap-2">
                                <Star className="text-yellow-500" fill="currentColor" size={20} />
                                <span className="text-xl text-white font-bold">{game.metacritic || 'N/A'}</span>
                            </div>
                            <div className="flex items-center gap-2">
                                <Calendar className="text-blue-500" size={20} />
                                <span>{game.released}</span>
                            </div>
                            <div className="flex items-center gap-2">
                                <Globe className="text-indigo-400" size={20} />
                                <a href={game.website} target="_blank" rel="noreferrer" className="hover:text-white transition-colors">Resmi Site</a>
                            </div>
                        </div>

                        <div className="mt-10 flex gap-4 relative" ref={libraryMenuRef}>
                            {userId ? (
                                <>
                                    <button
                                        onClick={() => setLibraryMenuOpen((v) => !v)}
                                        className={`px-6 py-3.5 rounded-none font-bold text-base flex items-center gap-2 transition-colors shadow-md border ${
                                            isInLibrary
                                                ? 'bg-[#1a1e2d] text-green-400 border-green-500/40 hover:bg-[#22262f]'
                                                : 'bg-blue-600 hover:bg-blue-500 text-white border-transparent'
                                        }`}
                                    >
                                        {isInLibrary ? (
                                            <><Check size={22} /> Kütüphanede {libraryStatus ? `(${getStatusLabel(libraryStatus)})` : ''}</>
                                        ) : (
                                            <><Plus size={22} /> Kütüphaneye Ekle</>
                                        )}
                                        <ChevronDown size={18} className={libraryMenuOpen ? 'rotate-180' : ''} />
                                    </button>
                                    {libraryMenuOpen && (
                                        <div className="absolute left-0 top-full mt-2 w-56 py-1 rounded-none bg-[#141722] border border-[#1f2334] shadow-xl z-30">
                                            {Object.entries(LIBRARY_STATUS).map(([statusId, { label }]) => (
                                                <button
                                                    key={statusId}
                                                    onClick={() => handleAddOrUpdate(statusId)}
                                                    disabled={(addMutation.isPending || updateMutation.isPending) && (isInLibrary ? libraryStatus === statusId : false)}
                                                    className={`w-full text-left px-4 py-2.5 text-sm font-medium transition-colors ${libraryStatus === statusId ? 'text-blue-400 bg-blue-500/10' : 'text-gray-300 hover:bg-[#1a1e2d] hover:text-white'}`}
                                                >
                                                    {label}
                                                </button>
                                            ))}
                                            {isInLibrary && (
                                                <>
                                                    <div className="my-1 border-t border-[#1f2334]" />
                                                    <button
                                                        onClick={handleRemove}
                                                        disabled={removeMutation.isPending}
                                                        className="w-full text-left px-4 py-2.5 text-sm font-medium text-red-400 hover:bg-red-500/10 flex items-center gap-2"
                                                    >
                                                        <Trash2 size={14} /> Kütüphaneden çıkar
                                                    </button>
                                                </>
                                            )}
                                        </div>
                                    )}
                                </>
                            ) : (
                                <span className="px-6 py-3.5 text-gray-500 text-sm font-medium">Kütüphaneye eklemek için giriş yapın.</span>
                            )}
                        </div>
                    </div>
                </div>
            </div>

            {/* İçerik Sola ve Sağa Yapışık */}
            <div className="px-12 py-12 flex flex-col xl:flex-row justify-between gap-16 w-full">

                {/* Sol Taraf: Açıklama */}
                <div className="flex-1 max-w-5xl space-y-12 shrink-0">
                    <section>
                        <h2 className="text-xl font-bold text-white uppercase tracking-widest mb-6 opacity-50 border-b border-[#1f2334] pb-2">Hakkında</h2>
                        <div className="relative">
                            <div
                                className={`text-gray-400 text-lg leading-[1.8] font-medium transition-all duration-500 ${!isExpanded && shouldTruncate ? 'max-h-[300px] overflow-hidden' : ''}`}
                                dangerouslySetInnerHTML={{ __html: descriptionHtml }}
                            />
                            {!isExpanded && shouldTruncate && (
                                <div className="absolute bottom-0 left-0 w-full h-32 bg-gradient-to-t from-[#0f111a] to-transparent pointer-events-none" />
                            )}
                        </div>
                        {shouldTruncate && (
                            <button
                                onClick={() => setIsExpanded(!isExpanded)}
                                className="mt-4 flex items-center gap-2 text-blue-500 hover:text-blue-400 font-bold transition-colors"
                            >
                                {isExpanded ? (
                                    <>Küçült <ChevronUp size={18} /></>
                                ) : (
                                    <>Genişlet <ChevronDown size={18} /></>
                                )}
                            </button>
                        )}
                    </section>
                </div>

                {/* Sağ Taraf: Detaylar, Geliştiriciler, Sistem Gereksinimleri */}
                <div className="w-full xl:w-[450px] shrink-0 space-y-8 flex flex-col">

                    {/* Geliştirici vb UI Kartı */}
                    <div className="p-6 rounded-none bg-[#141722] border border-[#1f2334] shadow-md space-y-6">
                        <div>
                            <h3 className="text-sm font-bold text-gray-500 uppercase tracking-widest mb-2">Geliştirici</h3>
                            <div className="flex flex-wrap gap-2 text-gray-200 font-semibold">
                                {game.developers?.map(d => d.name).join(', ') || '-'}
                            </div>
                        </div>
                        <div>
                            <h3 className="text-sm font-bold text-gray-500 uppercase tracking-widest mb-2">Yayıncı</h3>
                            <div className="flex flex-wrap gap-2 text-gray-200 font-semibold">
                                {game.publishers?.map(p => p.name).join(', ') || '-'}
                            </div>
                        </div>
                        <div>
                            <h3 className="text-sm font-bold text-gray-500 uppercase tracking-widest mb-2">Platformlar</h3>
                            <div className="flex flex-wrap gap-2">
                                {game.platforms?.map(p => (
                                    <span key={p.platform.id} className="px-2.5 py-1 rounded-none bg-[#1a1e2d] text-gray-300 text-xs font-semibold uppercase border border-[#1f2334]">
                                        {p.platform.name}
                                    </span>
                                ))}
                            </div>
                        </div>
                        {game.stores && game.stores.length > 0 && (
                            <div className="pt-2">
                                <h3 className="text-sm font-bold text-gray-500 uppercase tracking-widest mb-2">Satın Al</h3>
                                <div className="flex flex-wrap gap-2">
                                    {game.stores.map((s, idx) => (
                                        <a
                                            key={idx}
                                            href={s.url}
                                            target="_blank"
                                            rel="noreferrer"
                                            className="px-3 py-1.5 rounded-none bg-blue-500/10 hover:bg-blue-500/20 border border-blue-500/30 text-blue-400 hover:text-white text-xs font-semibold transition-all flex items-center shadow-sm"
                                        >
                                            {s.store?.name || 'Mağaza'}
                                        </a>
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>

                    {/* Sistem Gereksinimleri Kartı */}
                    <div className="p-6 rounded-none bg-[#141722] border border-[#1f2334] shadow-md relative overflow-hidden">
                        <h3 className="text-lg font-bold text-white mb-6 tracking-tight flex items-center gap-2 border-b border-[#1f2334] pb-3">
                            <Check className="text-blue-500" size={20} /> Sistem Gereksinimleri
                        </h3>

                        {requirements && (requirements.minimum || requirements.recommended) ? (
                            <div className="space-y-6">
                                {requirements.minimum && (
                                    <div>
                                        <h4 className="font-semibold text-gray-300 mb-2">Minimum</h4>
                                        <p className="text-gray-400 text-sm leading-relaxed whitespace-pre-line bg-[#0f111a] p-4 rounded-none border border-[#1f2334]">
                                            {requirements.minimum.replace(/^Minimum:\s*/i, '')}
                                        </p>
                                    </div>
                                )}
                                {requirements.recommended && (
                                    <div>
                                        <h4 className="font-semibold text-gray-300 mb-2">Önerilen</h4>
                                        <p className="text-gray-400 text-sm leading-relaxed whitespace-pre-line bg-[#0f111a] p-4 rounded-none border border-[#1f2334]">
                                            {requirements.recommended.replace(/^Recommended:\s*/i, '')}
                                        </p>
                                    </div>
                                )}
                            </div>
                        ) : (
                            <p className="text-sm text-gray-500 leading-relaxed italic">Minimum gereksinimler RAWG üzerinde belirtilmediyse burada görünmez.</p>
                        )}
                    </div>

                </div>
            </div>

            {/* Ekran görüntüleri */}
            {screenshots.length > 0 && (
                <section className="px-12 py-8 border-t border-[#1f2334]">
                    <h2 className="text-xl font-bold text-white uppercase tracking-widest mb-6 opacity-50 border-b border-[#1f2334] pb-2">Ekran Görüntüleri</h2>
                    <div className="flex gap-4 overflow-x-auto pb-4 scroll-smooth snap-x snap-mandatory">
                        {screenshots.map((shot) => (
                            <a
                                key={shot.id}
                                href={shot.imageUrl ?? shot.image}
                                target="_blank"
                                rel="noreferrer"
                                className="flex-shrink-0 w-72 sm:w-80 rounded-none overflow-hidden border border-[#1f2334] snap-center hover:border-blue-500/50 transition-colors"
                            >
                                <img
                                    src={shot.imageUrl ?? shot.image}
                                    alt=""
                                    className="w-full h-40 object-cover"
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
