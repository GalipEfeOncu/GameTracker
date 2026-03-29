import { useQuery } from '@tanstack/react-query';
import { Loader2, Sparkles, BrainCircuit, History } from 'lucide-react';
import { getRecommendations, fetchUserLibrary } from '../api/apiClient';
import { useUser } from '../context/UserContext';
import GameCard from '../components/GameCard';
import { getSessionUserId } from '../utils/sessionUser';

export default function AiSuggestionPage() {
    const { user } = useUser();
    const userId = getSessionUserId(user);

    const { data: library } = useQuery({
        queryKey: ['library', userId],
        queryFn: () => fetchUserLibrary(userId),
        enabled: userId != null,
    });

    const likedGames = library?.map(g => g.name) || [];

    const { data: suggestions, isLoading, isError } = useQuery({
        queryKey: ['recommendations', likedGames.join(',')],
        queryFn: () => getRecommendations(likedGames),
        enabled: likedGames.length > 0,
    });

    if (!user) {
        return (
            <div className="flex flex-col items-center justify-center h-full py-20 px-4">
                <div className="w-16 h-16 rounded-none bg-blue-500/10 flex items-center justify-center mb-6 border border-blue-500/30">
                    <Sparkles size={32} className="text-blue-500" />
                </div>
                <h2 className="text-xl font-bold text-white tracking-tight">Zeka Katmanına Erişin</h2>
                <p className="mt-2 text-gray-500 font-medium text-center text-sm max-w-sm">Size özel oyun önerileri almak için lütfen önce giriş yapın.</p>
            </div>
        );
    }

    return (
        <div className="h-full overflow-y-auto px-8 pt-10 pb-20 scroll-smooth">
            <div className="mb-10 p-8 rounded-none bg-gradient-to-br from-[#1a1e2d] to-[#141722] border border-[#1f2334] shadow-md relative overflow-hidden">
                <div className="relative z-10">
                    <div className="inline-flex items-center gap-2 px-3 py-1.5 rounded-none bg-blue-600/10 border border-blue-500/30 text-blue-500 text-xs font-bold uppercase tracking-wider mb-4">
                        <BrainCircuit size={14} /> AI Engine Active
                    </div>
                    <h1 className="text-3xl font-bold text-white tracking-tight max-w-2xl">Sizin için seçtiğimiz yeni deneyimler.</h1>
                    <p className="text-gray-400 mt-2 font-medium">Kütüphanenizden topladığımız verilerle zevkinize en uygun oyunları analiz ettik.</p>
                </div>
            </div>

            {likedGames.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-20 text-gray-400">
                    <History size={48} className="mb-6 opacity-20" />
                    <p className="font-bold text-gray-300">Henüz kütüphaneniz boş.</p>
                    <p className="mt-2 text-center max-w-xs opacity-60">Öneri yapabilmemiz için sevdiğiniz oyunları kütüphanenize ekleyin.</p>
                </div>
            ) : isLoading ? (
                <div className="flex flex-col items-center justify-center py-24 text-gray-500">
                    <Loader2 size={40} className="animate-spin mb-6 text-blue-500" />
                    <p className="font-bold text-lg animate-pulse tracking-wide italic">Gemini zevkinizi analiz ediyor...</p>
                </div>
            ) : isError ? (
                <div className="p-8 rounded-none bg-red-500/5 border border-red-500/20 text-red-500 text-center font-bold">
                    Öneriler şu an alınamıyor, AI motorunda yoğunluk olabilir.
                </div>
            ) : (
                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-x-6 gap-y-12">
                    {suggestions?.map((game) => (
                        <GameCard key={game.id} game={game} />
                    ))}
                </div>
            )}
        </div>
    );
}
