import { Component } from 'react';
import { AlertTriangle } from 'lucide-react';

export class ErrorBoundary extends Component {
    constructor(props) {
        super(props);
        this.state = { hasError: false, error: null };
    }

    static getDerivedStateFromError(error) {
        return { hasError: true, error };
    }

    render() {
        if (this.state.hasError) {
            return (
                <div className="flex min-h-screen flex-col items-center justify-center bg-[#0f111a] px-6 text-center text-gray-300">
                    <AlertTriangle className="mb-4 text-amber-500" size={48} aria-hidden />
                    <h1 className="text-xl font-bold text-white">Bir sorun oluştu</h1>
                    <p className="mt-2 max-w-md text-sm text-gray-500">
                        Sayfa beklenmedik şekilde durdu. Yenileyerek tekrar deneyebilirsiniz.
                    </p>
                    <button
                        type="button"
                        onClick={() => window.location.reload()}
                        className="mt-8 border border-[#1f2334] bg-blue-600 px-6 py-2.5 text-sm font-semibold text-white hover:bg-blue-500"
                    >
                        Sayfayı yenile
                    </button>
                </div>
            );
        }
        return this.props.children;
    }
}
