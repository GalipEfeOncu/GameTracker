import { NavLink, useNavigate } from 'react-router-dom';
import { Compass, Library, Flame, Sparkles, Settings, LogOut, Gamepad2, HardDrive, X } from 'lucide-react';
import { useUser } from '../context/UserContext';
import { isDesktop } from '../desktop/bridge';
import { useI18n } from '../i18n/useI18n';

/**
 * @param {boolean} mobileOpen Mobil çekmece açık mı (lg altı)
 * @param {() => void} [onCloseMobile] Route değişince veya backdrop’ta çağrılır
 */
export default function Sidebar({ mobileOpen = false, onCloseMobile }) {
    const { user, logout } = useUser();
    const navigate = useNavigate();
    const { t } = useI18n();

    const closeIfMobile = () => {
        onCloseMobile?.();
    };

    const navItems = [
        { to: '/popular', label: t('nav.popular'), icon: Flame },
        { to: '/discover', label: t('nav.discover'), icon: Compass },
        { to: '/library', label: t('nav.library'), icon: Library },
        ...(isDesktop ? [{ to: '/installed', label: t('nav.installed'), icon: HardDrive }] : []),
        { to: '/ai', label: t('nav.ai'), icon: Sparkles },
    ];

    const handleLogout = () => {
        closeIfMobile();
        logout();
        navigate('/login');
    };

    return (
        <aside
            id="app-sidebar"
            className={`fixed inset-y-0 left-0 z-50 flex h-full w-[min(18rem,88vw)] shrink-0 flex-col border-r border-[#1f2334] bg-[#141722] pt-6 pb-4 shadow-2xl transition-transform duration-300 ease-out lg:relative lg:inset-auto lg:z-0 lg:h-full lg:w-64 lg:max-w-none lg:translate-x-0 lg:shadow-none ${
                mobileOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
            }`}
            aria-label={t('nav.appMenu')}
        >
            {/* Logo + mobil kapat */}
            <div className="mb-8 flex items-center justify-between gap-3 px-4 sm:px-6">
                <div className="flex min-w-0 items-center gap-3">
                    <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-none border border-[#1f2334] bg-[#1a1e2d]">
                        <Gamepad2 size={16} className="text-white" />
                    </div>
                    <h1 className="truncate text-lg font-bold tracking-tight text-white sm:text-xl">GameTracker</h1>
                </div>
                <button
                    type="button"
                    className="flex h-10 w-10 shrink-0 items-center justify-center rounded-none border border-[#1f2334] bg-[#1a1e2d] text-gray-300 hover:bg-[#252a3d] hover:text-white lg:hidden"
                    onClick={closeIfMobile}
                    aria-label={t('layout.closeNav')}
                >
                    <X size={20} aria-hidden />
                </button>
            </div>

            <nav className="flex-1 space-y-1 overflow-y-auto px-3 sm:px-4" aria-label={t('nav.pages')}>
                {navItems.map((item) => {
                    const Icon = item.icon;
                    return (
                        <NavLink
                            key={item.to}
                            to={item.to}
                            onClick={closeIfMobile}
                            className={({ isActive }) =>
                                `relative flex w-full items-center gap-3 rounded-none px-4 py-3 outline-none transition-all duration-200
               ${isActive
                                    ? 'bg-blue-600/10 font-semibold text-blue-500'
                                    : 'font-medium text-gray-400 hover:bg-[#1a1e2d] hover:text-gray-200'}`
                            }
                        >
                            {({ isActive }) => (
                                <>
                                    {isActive && <span className="absolute left-0 h-6 w-1 bg-blue-500" />}
                                    <Icon size={19} />
                                    <span className="truncate">{item.label}</span>
                                </>
                            )}
                        </NavLink>
                    );
                })}
            </nav>

            <div className="mt-auto space-y-1 px-3 sm:px-4">
                {user && (
                    <div className="mb-2 flex items-center gap-3 px-4 py-3 text-gray-300">
                        <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-none border border-[#1f2334] bg-[#1a1e2d] text-sm font-bold text-white">
                            {user.username?.[0]?.toUpperCase() ?? 'U'}
                        </div>
                        <span className="truncate text-sm font-medium">{user.username}</span>
                    </div>
                )}
                <NavLink
                    to="/settings"
                    onClick={closeIfMobile}
                    className={({ isActive }) =>
                        `flex items-center gap-3 rounded-none px-4 py-3 text-sm transition-all
             ${isActive ? 'bg-blue-600/10 text-blue-500' : 'text-gray-400 hover:bg-[#1a1e2d] hover:text-gray-200'}`
                    }
                >
                    <Settings size={19} />
                    {t('nav.settings')}
                </NavLink>
                <button
                    type="button"
                    onClick={handleLogout}
                    className="flex w-full items-center gap-3 rounded-none px-4 py-3 text-sm text-gray-400 transition-all hover:bg-red-500/10 hover:text-red-400"
                >
                    <LogOut size={19} />
                    {t('nav.logout')}
                </button>
            </div>
        </aside>
    );
}
