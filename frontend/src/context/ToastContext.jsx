import { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { X, CheckCircle2, AlertCircle, Info } from 'lucide-react';
import { GT_TOAST_EVENT } from '../utils/toastEvents';

const ToastContext = createContext(null);

function ToastItem({ id, message, type, onDismiss }) {
    useEffect(() => {
        const t = setTimeout(() => onDismiss(id), 4800);
        return () => clearTimeout(t);
    }, [id, onDismiss]);

    const styles =
        type === 'success'
            ? 'border-emerald-500/40 bg-emerald-500/10 text-emerald-200'
            : type === 'error'
              ? 'border-red-500/40 bg-red-500/10 text-red-200'
              : 'border-blue-500/40 bg-blue-500/10 text-blue-100';

    const Icon = type === 'success' ? CheckCircle2 : type === 'error' ? AlertCircle : Info;

    return (
        <div
            role="status"
            className={`pointer-events-auto flex max-w-md items-start gap-3 border px-4 py-3 text-sm shadow-lg ${styles}`}
        >
            <Icon size={18} className="mt-0.5 shrink-0 opacity-90" aria-hidden />
            <p className="flex-1 leading-snug">{message}</p>
            <button
                type="button"
                onClick={() => onDismiss(id)}
                className="shrink-0 text-gray-400 hover:text-white"
                aria-label="Kapat"
            >
                <X size={16} />
            </button>
        </div>
    );
}

export function ToastProvider({ children }) {
    const [toasts, setToasts] = useState([]);
    const showToast = useCallback((message, type = 'info') => {
        if (!message) return;
        const id = globalThis.crypto?.randomUUID?.() ?? `t-${Date.now()}-${Math.random()}`;
        setToasts((prev) => [...prev, { id, message, type }]);
    }, []);

    const dismiss = useCallback((id) => {
        setToasts((prev) => prev.filter((t) => t.id !== id));
    }, []);

    useEffect(() => {
        const onGlobal = (e) => {
            const { message, type } = e.detail ?? {};
            if (message) showToast(message, type ?? 'info');
        };
        window.addEventListener(GT_TOAST_EVENT, onGlobal);
        return () => window.removeEventListener(GT_TOAST_EVENT, onGlobal);
    }, [showToast]);

    return (
        <ToastContext.Provider value={{ showToast }}>
            {children}
            <div
                className="pointer-events-none fixed bottom-6 right-6 z-[300] flex w-[min(100vw-2rem,24rem)] flex-col gap-2"
                aria-live="polite"
                aria-relevant="additions"
            >
                {toasts.map((t) => (
                    <ToastItem key={t.id} {...t} onDismiss={dismiss} />
                ))}
            </div>
        </ToastContext.Provider>
    );
}

export function useToast() {
    const ctx = useContext(ToastContext);
    if (!ctx) throw new Error('useToast ToastProvider içinde kullanılmalı');
    return ctx;
}
