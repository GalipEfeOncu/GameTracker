import { createContext, useContext, useState, useCallback } from 'react';

const UserContext = createContext(null);

export function UserProvider({ children }) {
    const [user, setUser] = useState(() => {
        try {
            const stored = localStorage.getItem('gt_user');
            return stored ? JSON.parse(stored) : null;
        } catch { return null; }
    });

    const login = useCallback((userData) => {
        localStorage.setItem('gt_user', JSON.stringify(userData));
        setUser(userData);
    }, []);

    const logout = useCallback(() => {
        localStorage.removeItem('gt_user');
        setUser(null);
    }, []);

    const updateUser = useCallback((partial) => {
        setUser((prev) => {
            if (!prev) return prev;
            const next = { ...prev, ...partial };
            localStorage.setItem('gt_user', JSON.stringify(next));
            return next;
        });
    }, []);

    return (
        <UserContext.Provider value={{ user, login, logout, updateUser, isLoggedIn: !!user }}>
            {children}
        </UserContext.Provider>
    );
}

export const useUser = () => useContext(UserContext);
