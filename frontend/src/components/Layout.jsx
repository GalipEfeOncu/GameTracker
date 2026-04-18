import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { Search, Settings, LogOut, ChevronDown, Menu } from 'lucide-react';
import { useState, useRef, useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import Sidebar from './Sidebar';
import { useUser } from '../context/UserContext';
import { isDesktop, onPlaytimeUpdate } from '../desktop/bridge';
import { getSessionUserId } from '../utils/sessionUser';
import { useI18n } from '../i18n/useI18n';

export default function Layout() {
    const [searchQuery, setSearchQuery] = useState('');
    const [mobileNavOpen, setMobileNavOpen] = useState(false);
    const [profileOpen, setProfileOpen] = useState(false);
    const profileRef = useRef(null);
    const navigate = useNavigate();
    const location = useLocation();
    const queryClient = useQueryClient();
    const { user, logout } = useUser();
    const userId = getSessionUserId(user);
    const { t } = useI18n();
    const displayName = user?.username ?? user?.Username ?? t('common.user');

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

    useEffect(() => {
        const mq = window.matchMedia('(min-width: 1024px)');
        const closeOnWide = () => {
            if (mq.matches) setMobileNavOpen(false);
        };
        mq.addEventListener('change', closeOnWide);
        return () => mq.removeEventListener('change', closeOnWide);
    }, []);

    useEffect(() => {
        if (!mobileNavOpen) return;
        const prev = document.body.style.overflow;
        document.body.style.overflow = 'hidden';
        return () => {
            document.body.style.overflow = prev;
        };
    }, [mobileNavOpen]);

    useEffect(() => {
        if (!mobileNavOpen) return;
        const onKey = (e) => {
            if (e.key === 'Escape') setMobileNavOpen(false);
        };
        window.addEventListener('keydown', onKey);
        return () => window.removeEventListener('keydown', onKey);
    }, [mobileNavOpen]);

    useEffect(() => {
        if (!isDesktop || !userId) return undefined;
        const unsub = onPlaytimeUpdate(() => {
            queryClient.invalidateQueries({ queryKey: ['library', userId] });
        });
        return unsub;
    }, [queryClient, userId]);

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
        if (path.startsWith('/popular')) return { title: t('layout.header.popularTitle'), description: t('layout.header.popularDesc') };
        if (path.startsWith('/discover')) {
            const searchParams = new URLSearchParams(location.search);
            const q = searchParams.get('q');
            if (q) return { title: t('layout.header.discoverResultsTitle', { query: q }), description: t('layout.header.discoverResultsDesc') };
            return { title: t('layout.header.discoverTitle'), description: t('layout.header.discoverDesc') };
        }
        if (path.startsWith('/library')) return { title: t('layout.header.libraryTitle'), description: t('layout.header.libraryDesc') };
        if (path.startsWith('/ai')) return { title: t('layout.header.aiTitle'), description: t('layout.header.aiDesc') };
        if (path.startsWith('/settings')) return { title: t('layout.header.settingsTitle'), description: t('layout.header.settingsDesc') };
        if (path.startsWith('/installed')) return { title: t('layout.header.installedTitle'), description: t('layout.header.installedDesc') };
        return null;
    };

    const headerInfo = getPageHeaderInfo();

    return (
        <div className="flex h-screen w-full bg-[#0f111a] text-[#f3f4f6] font-sans">
            <a
                href="#main-content"
                className="sr-only left-4 top-4 z-[200] rounded-none bg-blue-600 px-4 py-2 text-sm font-medium text-white focus:not-sr-only focus:absolute focus:outline-none"
            >
                {t('layout.skipToContent')}
            </a>
            {mobileNavOpen && (
                <div
                    className="fixed inset-0 z-40 bg-black/60 backdrop-blur-sm lg:hidden"
                    aria-hidden
                    onClick={() => setMobileNavOpen(false)}
                />
            )}

            <Sidebar mobileOpen={mobileNavOpen} onCloseMobile={() => setMobileNavOpen(false)} />

            <div className="flex min-w-0 flex-1 flex-col overflow-hidden">
                {/* Header */}
                <header
                    className="flex h-14 shrink-0 items-center justify-between gap-2 border-b border-[#1f2334] bg-[#0f111a]/80 px-3 backdrop-blur-md sm:h-16 sm:gap-4 sm:px-6 lg:px-8 z-20"
                    role="banner"
                >
                    <div className="flex min-w-0 flex-1 items-center gap-2 sm:gap-3">
                        <button
                            type="button"
                            className="flex h-10 w-10 shrink-0 items-center justify-center rounded-none border border-[#1f2334] bg-[#1a1e2d] text-gray-200 hover:bg-[#252a3d] lg:hidden"
                            onClick={() => setMobileNavOpen(true)}
                            aria-expanded={mobileNavOpen}
                            aria-controls="app-sidebar"
                            aria-label={t('layout.openNav')}
                        >
                            <Menu size={20} aria-hidden />
                        </button>
                        <div className="min-w-0 flex-1">
                            {headerInfo ? (
                                <div className="flex flex-col justify-center">
                                    <h1 className="truncate text-base font-bold tracking-tight text-white sm:text-xl">{headerInfo.title}</h1>
                                </div>
                            ) : (
                                <div />
                            )}
                        </div>
                    </div>

                    <div className="flex shrink-0 items-center gap-2 sm:gap-4">
                        <div className="group relative max-[399px]:min-w-0">
                            <Search
                                size={16}
                                className="absolute left-2.5 top-1/2 -translate-y-1/2 text-gray-500 transition-colors group-focus-within:text-blue-500 min-[400px]:left-3"
                                aria-hidden
                            />
                            <input
                                id="header-game-search"
                                type="search"
                                value={searchQuery}
                                onChange={handleSearch}
                                placeholder={t('layout.searchPlaceholder')}
                                aria-label={t('layout.searchAria')}
                                className="w-[8.25rem] rounded-none border border-[#1f2334] bg-[#1a1e2d] py-2 pl-8 pr-2 text-xs text-gray-200 outline-none transition-colors placeholder:text-gray-500 focus:border-blue-500 min-[400px]:w-[10rem] min-[400px]:py-2 min-[400px]:pl-9 min-[400px]:pr-3 min-[400px]:text-sm sm:w-56"
                            />
                        </div>

                        <div className="relative z-[60]" ref={profileRef}>
                            <button
                                type="button"
                                onClick={() => setProfileOpen((v) => !v)}
                                className="flex items-center gap-2 rounded-none p-1 pr-2 hover:bg-[#1a1e2d] transition-colors outline-none"
                                aria-expanded={profileOpen}
                                aria-haspopup="menu"
                                aria-label={t('layout.accountMenu')}
                            >
                                <div className="h-8 w-8 rounded-none border border-[#1f2334] bg-[#1a1e2d] shrink-0 flex items-center justify-center overflow-hidden text-white font-bold text-sm">
                                    {user ? displayName[0]?.toUpperCase() ?? '?' : t('common.guestInitial')}
                                </div>
                                <ChevronDown size={16} className={`text-gray-400 transition-transform ${profileOpen ? 'rotate-180' : ''}`} />
                            </button>
                            {profileOpen && (
                                <div
                                    role="menu"
                                    className="absolute right-0 top-full z-[70] mt-2 w-48 rounded-none border border-[#1f2334] bg-[#141722] py-1"
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
                                                <Settings size={16} /> {t('nav.settings')}
                                            </button>
                                            <button
                                                type="button"
                                                role="menuitem"
                                                onClick={() => { setProfileOpen(false); logout(); navigate('/login'); }}
                                                className="w-full flex items-center gap-2 px-4 py-2.5 text-sm text-red-400 hover:bg-red-500/10 transition-colors"
                                            >
                                                <LogOut size={16} /> {t('nav.logout')}
                                            </button>
                                        </>
                                    ) : (
                                        <button
                                            type="button"
                                            role="menuitem"
                                            onClick={() => { setProfileOpen(false); navigate('/login'); }}
                                            className="w-full flex items-center gap-2 px-4 py-2.5 text-sm text-gray-300 hover:bg-[#1a1e2d] hover:text-white transition-colors"
                                        >
                                            {t('nav.login')}
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
