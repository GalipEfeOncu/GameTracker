/** Popüler / kütüphane / keşfet grid yükleme yer tutucusu */
export default function GameCardSkeleton() {
    return (
        <div className="flex animate-pulse flex-col items-center">
            <div className="aspect-video w-full border border-[#1f2334] bg-[#1a1e2d]" />
            <div className="mt-3 h-4 w-[80%] max-w-[200px] bg-[#1a1e2d]" />
            <div className="mt-2 h-3 w-[60%] max-w-[120px] bg-[#1a1e2d]" />
        </div>
    );
}

export function GameCardSkeletonGrid({ count = 10, className }) {
    const gridClass =
        className ??
        'grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-x-5 gap-y-10 pb-6';
    return (
        <div className={gridClass}>
            {Array.from({ length: count }, (_, i) => (
                <GameCardSkeleton key={i} />
            ))}
        </div>
    );
}
