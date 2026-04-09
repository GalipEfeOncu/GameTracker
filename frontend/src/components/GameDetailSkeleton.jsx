/** Oyun detay sayfası ilk yükleme iskeleti (başlık + kapak | video | meta, alt: about | yan kartlar) */
export default function GameDetailSkeleton() {
    return (
        <div className="h-full animate-pulse overflow-y-auto bg-[#0f111a]">
            <div className="relative min-h-[min(78vh,880px)] w-full pb-10">
                <div className="absolute inset-0 bg-[#1a1e2d]" />
                <div className="relative z-10 mx-auto max-w-[1600px] px-6 pt-24 lg:px-12 lg:pt-28">
                    <div className="mb-8 h-12 w-3/4 max-w-xl bg-[#1a1e2d] lg:mb-10" />
                    <div className="flex flex-col gap-10 xl:flex-row xl:items-start xl:gap-8">
                        <div className="mx-auto w-full max-w-[240px] shrink-0 space-y-3 xl:mx-0">
                            <div className="aspect-[2/3] w-full border border-[#1f2334] bg-[#141722]" />
                            <div className="h-12 w-full border border-[#1f2334] bg-[#141722]" />
                        </div>
                        <div className="min-w-0 flex-1">
                            <div className="aspect-video w-full border border-[#1f2334] bg-[#141722]" />
                        </div>
                        <div className="w-full space-y-4 border border-[#1f2334] bg-[#141722]/80 p-4 xl:w-[280px]">
                            <div className="h-4 w-16 bg-[#1a1e2d]" />
                            <div className="flex flex-wrap gap-2">
                                <div className="h-6 w-14 bg-[#1a1e2d]" />
                                <div className="h-6 w-20 bg-[#1a1e2d]" />
                            </div>
                            <div className="h-10 w-full bg-[#1a1e2d]" />
                            <div className="h-10 w-full bg-[#1a1e2d]" />
                        </div>
                    </div>
                </div>
            </div>
            <div className="border-t border-[#1f2334] px-6 py-12 lg:px-12">
                <div className="mx-auto flex max-w-[1600px] flex-col gap-12 xl:flex-row xl:gap-12">
                    <div className="min-w-0 flex-1 space-y-4 xl:max-w-5xl">
                        <div className="h-6 w-32 bg-[#1a1e2d]" />
                        <div className="space-y-2">
                            <div className="h-3 w-full bg-[#1a1e2d]" />
                            <div className="h-3 w-full bg-[#1a1e2d]" />
                            <div className="h-3 w-[80%] bg-[#1a1e2d]" />
                        </div>
                    </div>
                    <div className="w-full shrink-0 space-y-8 xl:w-[450px]">
                        <div className="space-y-4 border border-[#1f2334] bg-[#141722] p-6">
                            <div className="h-4 w-24 bg-[#1a1e2d]" />
                            <div className="h-3 w-full bg-[#1a1e2d]" />
                            <div className="h-4 w-20 bg-[#1a1e2d]" />
                            <div className="h-3 w-4/5 bg-[#1a1e2d]" />
                        </div>
                        <div className="border border-[#1f2334] bg-[#141722] p-6">
                            <div className="mb-4 h-5 w-48 bg-[#1a1e2d]" />
                            <div className="h-20 w-full bg-[#0f111a]" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
