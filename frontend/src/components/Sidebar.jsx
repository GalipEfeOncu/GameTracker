import { NavLink, useNavigate } from 'react-router-dom';
import { Compass, Library, Flame, Sparkles, Settings, LogOut, Gamepad2, HardDrive } from 'lucide-react';
import { useUser } from '../context/UserContext';
import { isDesktop } from '../desktop/bridge';

const navItems = [
    { to: '/popular', label: 'Popüler', icon: Flame },
    { to: '/discover', label: 'Keşfet', icon: Compass },
    { to: '/library', label: 'Kütüphanem', icon: Library },
    ...(isDesktop ? [{ to: '/installed', label: 'Yüklü Oyunlarım', icon: HardDrive }] : []),
    { to: '/ai', label: 'AI Öneri', icon: Sparkles },
];

export default function Sidebar() {
    const { user, logout } = useUser();
    const navigate = useNavigate();

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    return (
        <aside className="w-64 bg-[#141722] border-r border-[#1f2334] flex flex-col pt-8 pb-4 h-full shrink-0" aria-label="Uygulama menüsü">
            {/* Logo */}
            <div className="px-6 mb-10 flex items-center gap-3">
                <div className="w-8 h-8 rounded-none border border-[#1f2334] bg-[#1a1e2d] flex items-center justify-center">
                    <Gamepad2 size={16} className="text-white" />
                </div>
                <h1 className="text-xl font-bold tracking-tight text-white">GameTracker</h1>
            </div>

            {/* Nav */}
            <nav className="flex-1 px-4 space-y-1" aria-label="Sayfalar">
                {navItems.map((item) => {
                    const Icon = item.icon;
                    return (
                    <NavLink
                        key={item.to}
                        to={item.to}
                        className={({ isActive }) =>
                            `relative w-full flex items-center gap-3 px-4 py-3 rounded-none transition-all duration-200 outline-none
               ${isActive
                                ? 'bg-blue-600/10 text-blue-500 font-semibold'
                                : 'text-gray-400 hover:text-gray-200 hover:bg-[#1a1e2d] font-medium'}`
                        }
                    >
                        {({ isActive }) => (
                            <>
                                {isActive && (
                                    <span className="absolute left-0 w-1 h-6 bg-blue-500" />
                                )}
                                <Icon size={19} />
                                {item.label}
                            </>
                        )}
                    </NavLink>
                    );
                })}
            </nav>

            {/* Bottom */}
            <div className="px-4 mt-auto space-y-1">
                {user && (
                    <div className="flex items-center gap-3 px-4 py-3 mb-2 text-gray-300">
                        <div className="w-8 h-8 rounded-none border border-[#1f2334] bg-[#1a1e2d] flex items-center justify-center text-sm font-bold text-white">
                            {user.username?.[0]?.toUpperCase() ?? 'U'}
                        </div>
                        <span className="text-sm font-medium truncate">{user.username}</span>
                    </div>
                )}
                <NavLink
                    to="/settings"
                    className={({ isActive }) =>
                        `flex items-center gap-3 px-4 py-3 rounded-none transition-all text-sm
             ${isActive ? 'text-blue-500 bg-blue-600/10' : 'text-gray-400 hover:text-gray-200 hover:bg-[#1a1e2d]'}`
                    }
                >
                    <Settings size={19} />
                    Ayarlar
                </NavLink>
                <button
                    type="button"
                    onClick={handleLogout}
                    className="w-full flex items-center gap-3 px-4 py-3 rounded-none text-gray-400 hover:text-red-400 hover:bg-red-500/10 transition-all text-sm"
                >
                    <LogOut size={19} />
                    Çıkış Yap
                </button>
            </div>
        </aside>
    );
}
