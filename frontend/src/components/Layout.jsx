import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { Search, Settings, LogOut, ChevronDown } from 'lucide-react';
import { useState, useRef, useEffect } from 'react';
import Sidebar from './Sidebar';
import { useUser } from '../context/UserContext';

export default function Layout() {
    const [searchQuery, setSearchQuery] = useState('');
    const [profileOpen, setProfileOpen] = useState(false);
    const profileRef = useRef(null);
    const navigate = useNavigate();
    const location = useLocation();
    const { user, logout } = useUser();
    const displayName = user?.username ?? user?.Username ?? 'Kullanıcı';

    useEffect(() => {
        const handleClickOutside = (e) => {
            if (profileRef.current && !profileRef.current.contains(e.target)) setProfileOpen(false);
        };
        if (profileOpen) document.addEventListener('click', handleClickOutside);
        return () => document.removeEventListener('click', handleClickOutside);
    }, [profileOpen]);

    // Keşfet URL'sindeki ?q= ile header arama kutusunu senkron tut; başka sayfadayken kutuyu temizle
    useEffect(() => {
        if (location.pathname.startsWith('/discover')) {
            const q = new URLSearchParams(location.search).get('q') ?? '';
            setSearchQuery(q);
        } else {
            setSearchQuery('');
        }
    }, [location.pathname, location.search]);

    const handleSearch = (e) => {
        const q = e.target.value;
        setSearchQuery(q);
        if (q.trim().length > 0) {
            navigate(`/discover?q=${encodeURIComponent(q)}`);
        } else if (location.pathname.startsWith('/discover')) {
            navigate('/discover', { replace: true });
        }
    };

    const getPageHeaderInfo = () => {
        const path = location.pathname;
        if (path.startsWith('/popular')) return { title: 'Popüler Oyunlar', description: 'Şu an dünyada en çok ilgi gören oyunlar.' };
        if (path.startsWith('/discover')) {
            const searchParams = new URLSearchParams(location.search);
            const q = searchParams.get('q');
            if (q) return { title: `"${q}" Sonuçları`, description: 'Arama sonuçlarınız listeleniyor.' };
            return { title: 'Keşfet', description: 'Yeni oyunlar ve türleri keşfedin.' };
        }
        if (path.startsWith('/library')) return { title: 'Kütüphanem', description: 'Oynadığınız ve planladığınız oyunlar.' };
        if (path.startsWith('/ai')) return { title: 'AI Öneri Motoru', description: 'Size özel seçtiğimiz zeka odaklı deneyimler.' };
        if (path.startsWith('/settings')) return { title: 'Ayarlar', description: 'Uygulama ve hesap tercihleriniz.' };
        return null; // For game details page, etc.
    };

    const headerInfo = getPageHeaderInfo();

    return (
        <div className="flex h-screen w-full bg-[#0f111a] text-[#f3f4f6] font-sans">
            <a
                href="#main-content"
                className="sr-only left-4 top-4 z-[200] rounded-none bg-blue-600 px-4 py-2 text-sm font-medium text-white focus:not-sr-only focus:absolute focus:outline-none"
            >
                Ana içeriğe geç
            </a>
            <Sidebar />

            <div className="flex-1 flex flex-col overflow-hidden">
                {/* Header */}
                <header className="shrink-0 z-10 bg-[#0f111a]/80 backdrop-blur-md border-b border-[#1f2334] px-8 h-16 flex items-center justify-between gap-4" role="banner">
                    {/* Dynamic Header Info */}
                    <div className="flex-1">
                        {headerInfo ? (
                            <div className="flex flex-col justify-center">
                                <h1 className="text-xl font-bold text-white tracking-tight">{headerInfo.title}</h1>
                            </div>
                        ) : (
                            <div />
                        )}
                    </div>

                    <div className="flex items-center gap-4 shrink-0">
                        <div className="relative group">
                            <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 group-focus-within:text-blue-500 transition-colors" />
                            <input
                                id="header-game-search"
                                type="search"
                                value={searchQuery}
                                onChange={handleSearch}
                                placeholder="Oyun ara..."
                                aria-label="Oyun ara"
                                className="pl-9 pr-4 py-2 w-56 bg-[#1a1e2d] border border-[#1f2334] rounded-none text-sm outline-none focus:border-blue-500 transition-colors placeholder:text-gray-500 text-gray-200"
                            />
                        </div>

                        <div className="relative" ref={profileRef}>
                            <button
                                type="button"
                                onClick={() => setProfileOpen((v) => !v)}
                                className="flex items-center gap-2 rounded-none p-1 pr-2 hover:bg-[#1a1e2d] transition-colors outline-none"
                                aria-expanded={profileOpen}
                                aria-haspopup="menu"
                                aria-label="Hesap menüsü"
                            >
                                <div className="h-8 w-8 rounded-none border border-[#1f2334] bg-[#1a1e2d] shrink-0 flex items-center justify-center overflow-hidden text-white font-bold text-sm">
                                    {user ? displayName[0]?.toUpperCase() ?? '?' : 'G'}
                                </div>
                                <ChevronDown size={16} className={`text-gray-400 transition-transform ${profileOpen ? 'rotate-180' : ''}`} />
                            </button>
                            {profileOpen && (
                                <div
                                    role="menu"
                                    className="absolute right-0 top-full mt-2 w-48 py-1 rounded-none bg-[#141722] border border-[#1f2334] z-50"
                                >
                                    {user ? (
                                        <>
                                            <div className="px-4 py-2 border-b border-[#1f2334]">
                                                <p className="text-sm font-semibold text-white truncate">{displayName}</p>
                                                <p className="text-xs text-gray-500 truncate">{user?.email ?? user?.Email ?? ''}</p>
                                            </div>
                                            <button
                                                type="button"
                                                role="menuitem"
                                                onClick={() => { setProfileOpen(false); navigate('/settings'); }}
                                                className="w-full flex items-center gap-2 px-4 py-2.5 text-sm text-gray-300 hover:bg-[#1a1e2d] hover:text-white transition-colors"
                                            >
                                                <Settings size={16} /> Ayarlar
                                            </button>
                                            <button
                                                type="button"
                                                role="menuitem"
                                                onClick={() => { setProfileOpen(false); logout(); navigate('/login'); }}
                                                className="w-full flex items-center gap-2 px-4 py-2.5 text-sm text-red-400 hover:bg-red-500/10 transition-colors"
                                            >
                                                <LogOut size={16} /> Çıkış Yap
                                            </button>
                                        </>
                                    ) : (
                                        <button
                                            type="button"
                                            role="menuitem"
                                            onClick={() => { setProfileOpen(false); navigate('/login'); }}
                                            className="w-full flex items-center gap-2 px-4 py-2.5 text-sm text-gray-300 hover:bg-[#1a1e2d] hover:text-white transition-colors"
                                        >
                                            Giriş Yap
                                        </button>
                                    )}
                                </div>
                            )}
                        </div>
                    </div>
                </header>

                {/* Page content */}
                <main id="main-content" className="flex-1 overflow-y-auto" tabIndex={-1}>
                    <Outlet />
                </main>
            </div>
        </div>
    );
}
