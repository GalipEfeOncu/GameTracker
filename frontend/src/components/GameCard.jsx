import { memo, useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { MoreVertical, Trash2 } from 'lucide-react';
import { getStatusLabel, LIBRARY_STATUS } from '../constants/libraryStatus';

const McBadge = ({ score }) => {
    if (!score) return null;
    const color = score >= 80
        ? 'bg-green-500 text-white'
        : score >= 60
            ? 'bg-yellow-500 text-black'
            : 'bg-red-500 text-white';
    return (
        <div className={`absolute top-2 right-2 z-10 ${color} text-xs font-bold px-1.5 py-0.5 rounded-none leading-none`}>
            {score}
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

    const handleCardClick = () => {
        if (!menuOpen) navigate(`/game/${game.id}`);
    };

    return (
        <div
            onClick={handleCardClick}
            className="group cursor-pointer flex flex-col items-center hover:-translate-y-1 transition-transform duration-300"
            style={{ contentVisibility: 'auto', containIntrinsicSize: '0 220px' }}
        >
            {/* Sadece resme border */}
            <div className="w-full aspect-video relative overflow-hidden rounded-none border border-[#1f2334] group-hover:border-blue-500 transition-colors duration-300">
                {statusLabel && (
                    <div className="absolute top-2 left-2 z-10 bg-[#1a1e2d]/90 text-gray-300 text-[10px] font-semibold px-2 py-0.5 rounded-none border border-[#1f2334]">
                        {statusLabel}
                    </div>
                )}
                {hasLibraryActions && (
                    <div className="absolute top-2 right-10 z-20" ref={menuRef} onClick={(e) => e.stopPropagation()}>
                        <button
                            onClick={(e) => { e.stopPropagation(); setMenuOpen((v) => !v); }}
                            className="p-1.5 rounded-none bg-black/50 hover:bg-[#1a1e2d] text-gray-300 hover:text-white border border-[#1f2334] transition-colors"
                            aria-label="Menü"
                        >
                            <MoreVertical size={16} />
                        </button>
                        {menuOpen && (
                            <div className="absolute right-0 top-full mt-1 w-44 py-1 rounded-none bg-[#141722] border border-[#1f2334] z-30">
                                {onStatusChange && Object.entries(LIBRARY_STATUS).map(([statusId, { label }]) => (
                                    <button
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
                {game.background_image ? (
                    <img
                        src={game.background_image}
                        alt={game.name}
                        className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                        loading="lazy"
                        decoding="async"
                        onError={(e) => { e.currentTarget.style.display = 'none'; }}
                    />
                ) : (
                    <div className="w-full h-full bg-[#1a1e2d] flex items-center justify-center text-gray-600 text-xs">
                        No Image
                    </div>
                )}

                {/* Metacritic badge */}
                <McBadge score={game.metacritic} />

                {/* Hover overlay */}
                <div className="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex items-center justify-center">
                    <button className="px-4 py-2 bg-blue-600 hover:bg-blue-500 text-white text-sm font-medium rounded-none translate-y-2 group-hover:translate-y-0 opacity-0 group-hover:opacity-100 transition-all duration-300">
                        Detaylar
                    </button>
                </div>
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
        </div>
    );
});

GameCard.displayName = 'GameCard';
export default GameCard;
