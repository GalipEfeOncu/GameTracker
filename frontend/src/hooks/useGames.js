import { useInfiniteQuery, useQuery } from '@tanstack/react-query';
import { getPopularGames, searchGames, getDiscoverGames } from '../api/apiClient';
import { useDebounce } from 'use-debounce';

export const usePopularGames = (nsfw = false) => {
    return useInfiniteQuery({
        queryKey: ['popularGames', nsfw],
        queryFn: ({ pageParam = 0 }) => getPopularGames(pageParam, nsfw),
        initialPageParam: 0,
        getNextPageParam: (lastPage) => {
            if (lastPage?.hasMore) return lastPage.nextOffset;
            return undefined;
        },
        staleTime: 1000 * 60 * 10,
    });
};

export const useDiscoverGames = (genre, mode, nsfw = false) => {
    return useInfiniteQuery({
        queryKey: ['discoverGames', genre, mode, nsfw],
        queryFn: ({ pageParam = 1 }) => getDiscoverGames({ genre, mode, page: pageParam }, nsfw),
        initialPageParam: 1,
        getNextPageParam: (lastPage) => {
            if (lastPage?.hasMore) return lastPage.nextPage;
            return undefined;
        },
        staleTime: 1000 * 60 * 5,
    });
};

export const useSearchGames = (searchQuery, nsfw = false) => {
    const [debouncedQuery] = useDebounce(searchQuery, 400);
    return useQuery({
        queryKey: ['searchGames', debouncedQuery, nsfw],
        queryFn: () => searchGames(debouncedQuery, 40, nsfw),
        enabled: !!debouncedQuery,
        staleTime: 1000 * 60 * 5,
    });
};
