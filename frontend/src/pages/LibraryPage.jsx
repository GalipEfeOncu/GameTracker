import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Loader2, Library as LibraryIcon, PlayCircle, CheckCircle, Clock, XCircle } from 'lucide-react';
import { fetchUserLibrary, removeGameFromLibrary, updateGameStatus } from '../api/apiClient';
import { useUser } from '../context/UserContext';
import GameCard from '../components/GameCard';
import { LIBRARY_TABS } from '../constants/libraryStatus';

const TAB_ICONS = {
    null: LibraryIcon,
    Playing: PlayCircle,
    Played: CheckCircle,
    PlanToPlay: Clock,
    Dropped: XCircle,
};

export default function LibraryPage() {
    const { user } = useUser();
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const userId = user?.id ?? user?.UserId;
    const [activeTab, setActiveTab] = useState(null);

    const { data: library, isLoading } = useQuery({
        queryKey: ['library', userId, activeTab],
        queryFn: () => fetchUserLibrary(userId, activeTab),
        enabled: !!userId,
    });

    const invalidateLibrary = () => {
        queryClient.invalidateQueries({ queryKey: ['library', userId] });
    };

    const removeMutation = useMutation({
        mutationFn: (gameId) => removeGameFromLibrary(userId, gameId),
        onSuccess: invalidateLibrary,
    });
    const updateStatusMutation = useMutation({
        mutationFn: ({ gameId, newStatus }) => updateGameStatus(userId, gameId, newStatus),
        onSuccess: invalidateLibrary,
    });

    const handleRemove = (gameId) => removeMutation.mutate(gameId);
    const handleStatusChange = (gameId, newStatus) => updateStatusMutation.mutate({ gameId, newStatus });

    if (!user) {
        return (
            <div className="flex flex-col items-center justify-center h-full py-40">
                <LibraryIcon size={56} className="mb-6 opacity-40 text-blue-500" />
                <h2 className="text-xl font-bold text-white tracking-tight">Kütüphane Erişimi</h2>
                <p className="mt-2 text-gray-500 font-medium text-sm">Lütfen oyunlarınıza ulaşmak için giriş yapın.</p>
                <button
                    onClick={() => navigate('/login')}
                    className="mt-6 px-6 py-2.5 bg-blue-600 hover:bg-blue-500 text-white rounded-none transition-all font-semibold shadow-sm text-sm"
                >
                    Giriş Yap
                </button>
            </div>
        );
    }

    return (
        <div className="h-full overflow-y-auto px-8 pt-8 pb-20 scroll-smooth">
            <div className="mb-8">
                <div className="flex flex-wrap items-center gap-1 bg-[#141722]/80 p-1.5 rounded-none border border-[#1f2334] w-fit">
                    {LIBRARY_TABS.map(({ id, label }) => {
                        const Icon = TAB_ICONS[id ?? 'null'];
                        return (
                            <button
                                key={id ?? 'all'}
                                onClick={() => setActiveTab(id)}
                                className={`flex items-center gap-2 px-4 py-2 rounded-none text-[13px] font-semibold transition-all ${activeTab === id
                                    ? 'bg-blue-600 text-white shadow-sm'
                                    : 'text-gray-400 hover:text-gray-200 hover:bg-[#1a1e2d]'
                                    }`}
                            >
                                <Icon size={16} />
                                {label}
                            </button>
                        );
                    })}
                </div>
            </div>

            {isLoading ? (
                <div className="flex flex-col items-center justify-center py-32 text-gray-500">
                    <Loader2 size={40} className="animate-spin mb-6 text-blue-500" />
                    <p className="font-semibold text-lg animate-pulse">Oyunlarınız yükleniyor...</p>
                </div>
            ) : library && library.length > 0 ? (
                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-x-6 gap-y-12">
                    {library.map((game) => (
                        <GameCard
                            key={game.id}
                            game={game}
                            showLibraryStatus
                            onRemove={handleRemove}
                            onStatusChange={handleStatusChange}
                        />
                    ))}
                </div>
            ) : (
                <div className="flex flex-col items-center justify-center py-32 text-gray-400 bg-[#141722]/20 rounded-none border-2 border-dashed border-[#1f2334]">
                    <LibraryIcon size={64} className="mb-6 opacity-20" />
                    <h2 className="text-xl font-bold text-gray-300">Bu bölümde henüz bir oyun yok</h2>
                    <p className="mt-2 text-center max-w-sm font-medium">Popüler oyunlara göz atıp koleksiyonunuzu oluşturmaya başlayın!</p>
                </div>
            )}
        </div>
    );
}
