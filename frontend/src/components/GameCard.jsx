import { memo, useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { MoreVertical, Trash2, Clock3 } from 'lucide-react';
import { LIBRARY_STATUS } from '../constants/libraryStatus';
import { useI18n } from '../i18n/useI18n';
import { formatTrackedMinutes } from '../i18n/playtime';

const McBadge = ({ score, scoreLabel, compactCorner = false }) => {
    const n = Number(score);
    if (!Number.isFinite(n) || n <= 0) return null;
    const color = n >= 80
        ? 'bg-green-500 text-white'
        : n >= 60
            ? 'bg-yellow-500 text-black'
            : 'bg-red-500 text-white';
    const pos = compactCorner ? 'bottom-2 right-2' : 'top-2 right-2';
    const title = scoreLabel ? `${scoreLabel}: ${n}` : String(n);
    return (
        <div
            className={`absolute ${pos} z-10 ${color} text-xs font-bold px-1.5 py-0.5 rounded-none leading-none`}
            title={title}
        >
            {n}
        </div>
    );
};

const GameCard = memo(({ game, showLibraryStatus = false, onRemove, onStatusChange }) => {
    const navigate = useNavigate();
    const { t } = useI18n();
    const [menuOpen, setMenuOpen] = useState(false);
    const menuRef = useRef(null);
    const statusLabel =
        showLibraryStatus && game?.status ? t(`library.status.${game.status}`) : null;
    const hasLibraryActions = showLibraryStatus && (onRemove || onStatusChange);
    const playtimeLabel = showLibraryStatus ? formatTrackedMinutes(game?.playtimeMinutes, t) : null;

    useEffect(() => {
        if (!hasLibraryActions) return;
        const handleClickOutside = (e) => {
            if (menuRef.current && !menuRef.current.contains(e.target)) setMenuOpen(false);
        };
        if (menuOpen) document.addEventListener('click', handleClickOutside);
        return () => document.removeEventListener('click', handleClickOutside);
    }, [menuOpen, hasLibraryActions]);

    useEffect(() => {
        if (!menuOpen) return;
        const onKey = (e) => {
            if (e.key === 'Escape') setMenuOpen(false);
        };
        document.addEventListener('keydown', onKey);
        return () => document.removeEventListener('keydown', onKey);
    }, [menuOpen]);

    const handleCardClick = () => {
        if (!menuOpen) navigate(`/game/${game.id}`);
    };

    const detailPhrase = game?.name
        ? `${game.name}, ${t('gameCard.detailSuffix')}`
        : t('gameCard.cardLabel');

    return (
        <article
            tabIndex={0}
            onClick={handleCardClick}
            onKeyDown={(e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    handleCardClick();
                }
            }}
            className="group flex w-full min-w-0 cursor-pointer flex-col items-stretch rounded-none outline-none transition-transform duration-300 hover:-translate-y-1"
            aria-label={detailPhrase}
        >
            {/* IGDB kapak ~5:7 (2:3’ten biraz geniş); görsel %103 genişlikte ortalanır — yan kırpma hafifler */}
            <div className="relative w-full aspect-[5/7] rounded-none border border-[#1f2334] transition-colors duration-300 group-hover:border-blue-500">
                <div className="absolute inset-0 z-0 overflow-hidden bg-[#141722]">
                    {game.background_image ? (
                        <img
                            src={game.background_image}
                            alt={game.name}
                            className="absolute left-1/2 top-0 h-full w-[103%] max-w-none -translate-x-1/2 object-cover object-center transition-transform duration-500 group-hover:scale-[1.05]"
                            loading="lazy"
                            decoding="async"
                            onError={(e) => { e.currentTarget.style.display = 'none'; }}
                        />
                    ) : (
                        <div className="w-full h-full bg-[#1a1e2d] flex items-center justify-center text-gray-600 text-xs">
                            {t('common.noImage')}
                        </div>
                    )}
                    {/* Hover overlay — görselle aynı kırpma alanında */}
                    <div className="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex items-center justify-center pointer-events-none z-[1]">
                        <span className="px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-none translate-y-2 group-hover:translate-y-0 opacity-0 group-hover:opacity-100 transition-all duration-300 pointer-events-none">
                            {t('gameCard.detailsHover')}
                        </span>
                    </div>
                </div>

                {statusLabel && (
                    <div className="absolute top-2 left-2 z-20 bg-[#1a1e2d]/90 text-gray-300 text-[10px] font-semibold px-2 py-0.5 rounded-none border border-[#1f2334]">
                        {statusLabel}
                    </div>
                )}
                {hasLibraryActions && (
                    <div className="absolute top-2 right-2 z-30" ref={menuRef} onClick={(e) => e.stopPropagation()}>
                        <button
                            type="button"
                            onClick={(e) => { e.stopPropagation(); setMenuOpen((v) => !v); }}
                            className="p-1.5 rounded-none bg-black/50 hover:bg-[#1a1e2d] text-gray-300 hover:text-white border border-[#1f2334] transition-colors"
                            aria-label={t('gameCard.libraryMenu')}
                            aria-expanded={menuOpen}
                            aria-haspopup="menu"
                        >
                            <MoreVertical size={16} />
                        </button>
                        {menuOpen && (
                            <div
                                role="menu"
                                aria-orientation="vertical"
                                className="absolute right-0 top-full mt-1 w-44 py-1 rounded-none bg-[#141722] border border-[#1f2334] shadow-lg shadow-black/40 z-50"
                            >
                                {onStatusChange && Object.keys(LIBRARY_STATUS).map((statusId) => (
                                    <button
                                        type="button"
                                        role="menuitem"
                                        key={statusId}
                                        onClick={() => { onStatusChange(game.id, statusId); setMenuOpen(false); }}
                                        className={`w-full text-left px-3 py-2 text-xs font-medium transition-colors ${game.status === statusId ? 'text-blue-400 bg-blue-500/10' : 'text-gray-300 hover:bg-[#1a1e2d] hover:text-white'}`}
                                    >
                                        {t(`library.status.${statusId}`)}
                                    </button>
                                ))}
                                {onRemove && (
                                    <>
                                        <div className="my-1 border-t border-[#1f2334]" />
                                        <button
                                            type="button"
                                            role="menuitem"
                                            onClick={() => { onRemove(game.id); setMenuOpen(false); }}
                                            className="w-full text-left px-3 py-2 text-xs font-medium text-red-400 hover:bg-red-500/10 flex items-center gap-2"
                                        >
                                            <Trash2 size={12} /> {t('gameCard.remove')}
                                        </button>
                                    </>
                                )}
                            </div>
                        )}
                    </div>
                )}

                <McBadge
                    score={game.display_score ?? game.metacritic}
                    scoreLabel={game.display_score_label}
                    compactCorner={hasLibraryActions}
                />
            </div>

            {/* Ortalı başlık ve tür */}
            <div className="mt-3 text-center w-full px-1">
                <h3 className="text-sm font-semibold text-gray-200 truncate" title={game.name}>
                    {game.name}
                </h3>
                <p className="text-xs text-gray-500 mt-0.5 truncate">
                    {game.genres?.map(g => g.name).join(', ') || ''}
                </p>
                {playtimeLabel && (
                    <p className="mt-1 inline-flex items-center gap-1 text-[11px] font-medium text-blue-300">
                        <Clock3 size={11} /> {playtimeLabel}
                    </p>
                )}
            </div>
        </article>
    );
});

GameCard.displayName = 'GameCard';
export default GameCard;
