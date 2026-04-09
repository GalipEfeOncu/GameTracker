import { useState, useEffect } from 'react';

/** Tailwind sm/md/lg/xl ile uyumlu: büyük ekranda 6 sütun */
function columnsFromWidth(width) {
    if (width < 640) return 2;
    if (width < 768) return 3;
    if (width < 1024) return 4;
    if (width < 1280) return 5;
    return 6;
}

export function useGameGridColumns() {
    const [columns, setColumns] = useState(() =>
        typeof window !== 'undefined' ? columnsFromWidth(window.innerWidth) : 6
    );

    useEffect(() => {
        const onResize = () => setColumns(columnsFromWidth(window.innerWidth));
        onResize();
        window.addEventListener('resize', onResize);
        return () => window.removeEventListener('resize', onResize);
    }, []);

    return columns;
}
