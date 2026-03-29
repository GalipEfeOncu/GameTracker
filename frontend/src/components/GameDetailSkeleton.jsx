/** Oyun detay sayfası ilk yükleme iskeleti */
export default function GameDetailSkeleton() {
    return (
        <div className="h-full animate-pulse overflow-y-auto bg-[#0f111a]">
            <div className="relative h-[55vh] min-h-[450px] w-full bg-[#1a1e2d]" />
            <div className="mx-auto max-w-5xl space-y-8 px-12 py-12">
                <div className="h-8 w-48 bg-[#1a1e2d]" />
                <div className="space-y-2">
                    <div className="h-3 w-full bg-[#1a1e2d]" />
                    <div className="h-3 w-full bg-[#1a1e2d]" />
                    <div className="h-3 w-[80%] bg-[#1a1e2d]" />
                </div>
            </div>
        </div>
    );
}
