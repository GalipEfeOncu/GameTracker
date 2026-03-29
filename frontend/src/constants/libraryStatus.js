export const LIBRARY_STATUS = {
    Playing:    { id: 'Playing',    label: 'Oynanıyor',      labelEn: 'Playing' },
    Completed:  { id: 'Completed',  label: 'Bitirildi',      labelEn: 'Completed' },
    Dropped:    { id: 'Dropped',    label: 'Bırakıldı',      labelEn: 'Dropped' },
    Wishlist:   { id: 'Wishlist',   label: 'İstek Listesi',  labelEn: 'Wishlist' },
    PlanToPlay: { id: 'PlanToPlay', label: 'Oynanacak',      labelEn: 'Plan To Play' },
};

export const LIBRARY_TABS = [
    { id: null, label: 'Hepsi', statusId: null },
    { id: 'Playing', label: LIBRARY_STATUS.Playing.label, statusId: 'Playing' },
    { id: 'Completed', label: LIBRARY_STATUS.Completed.label, statusId: 'Completed' },
    { id: 'Dropped', label: LIBRARY_STATUS.Dropped.label, statusId: 'Dropped' },
    { id: 'Wishlist', label: LIBRARY_STATUS.Wishlist.label, statusId: 'Wishlist' },
    { id: 'PlanToPlay', label: LIBRARY_STATUS.PlanToPlay.label, statusId: 'PlanToPlay' },
];

export const getStatusLabel = (statusId) => LIBRARY_STATUS[statusId]?.label ?? statusId ?? '';
