import { memo, useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { MoreVertical, Trash2 } from 'lucide-react';
import { getStatusLabel, LIBRARY_STATUS } from '../constants/libraryStatus';

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
    const [menuOpen, setMenuOpen] = useState(false);
    const menuRef = useRef(null);
    const statusLabel = showLibraryStatus && game?.status ? getStatusLabel(game.status) : null;
    const hasLibraryActions = showLibraryStatus && (onRemove || onStatusChange);

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
            className="group flex cursor-pointer flex-col items-center rounded-none outline-none hover:-translate-y-1 transition-transform duration-300"
            style={{ contentVisibility: 'auto', containIntrinsicSize: '0 220px' }}
            aria-label={game?.name ? `${game.name}, detay` : 'Oyun kartı'}
        >
            {/* Dış çerçeve overflow-visible: menü taşabilsin. Kırpma sadece içteki görsel katmanda. */}
            <div className="w-full aspect-video relative rounded-none border border-[#1f2334] group-hover:border-blue-500 transition-colors duration-300">
                <div className="absolute inset-0 overflow-hidden z-0">
                    {game.background_image ? (
                        <img
                            src={game.background_image}
                            alt={game.name}
                            className="h-full w-full object-cover object-center transition-transform duration-500 group-hover:scale-105"
                            loading="lazy"
                            decoding="async"
                            onError={(e) => { e.currentTarget.style.display = 'none'; }}
                        />
                    ) : (
                        <div className="w-full h-full bg-[#1a1e2d] flex items-center justify-center text-gray-600 text-xs">
                            No Image
                        </div>
                    )}
                    {/* Hover overlay — görselle aynı kırpma alanında */}
                    <div className="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex items-center justify-center pointer-events-none z-[1]">
                        <span className="px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-none translate-y-2 group-hover:translate-y-0 opacity-0 group-hover:opacity-100 transition-all duration-300 pointer-events-none">
                            Detaylar
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
                            aria-label="Kütüphane menüsü"
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
                                {onStatusChange && Object.entries(LIBRARY_STATUS).map(([statusId, { label }]) => (
                                    <button
                                        type="button"
                                        role="menuitem"
                                        key={statusId}
                                        onClick={() => { onStatusChange(game.id, statusId); setMenuOpen(false); }}
                                        className={`w-full text-left px-3 py-2 text-xs font-medium transition-colors ${game.status === statusId ? 'text-blue-400 bg-blue-500/10' : 'text-gray-300 hover:bg-[#1a1e2d] hover:text-white'}`}
                                    >
                                        {label}
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
                                            <Trash2 size={12} /> Kaldır
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
            </div>
        </article>
    );
});

GameCard.displayName = 'GameCard';
export default GameCard;
