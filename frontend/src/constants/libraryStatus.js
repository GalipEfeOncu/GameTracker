/**
 * Kütüphane oyun durumları — backend ile aynı değerler (PlanToPlay, Playing, Played, Dropped).
 * Tek kaynak; tab'lar ve etiketler buradan türetilir.
 */
export const LIBRARY_STATUS = {
    PlanToPlay: { id: 'PlanToPlay', label: 'Planlanan', labelEn: 'Plan to Play' },
    Playing: { id: 'Playing', label: 'Oynanıyor', labelEn: 'Playing' },
    Played: { id: 'Played', label: 'Tamamlandı', labelEn: 'Played' },
    Dropped: { id: 'Dropped', label: 'Bırakılan', labelEn: 'Dropped' },
};

/** Tab listesi: Hepsi + her status (backend ile uyumlu). */
export const LIBRARY_TABS = [
    { id: null, label: 'Hepsi', statusId: null },
    { id: 'Playing', label: LIBRARY_STATUS.Playing.label, statusId: 'Playing' },
    { id: 'Played', label: LIBRARY_STATUS.Played.label, statusId: 'Played' },
    { id: 'PlanToPlay', label: LIBRARY_STATUS.PlanToPlay.label, statusId: 'PlanToPlay' },
    { id: 'Dropped', label: LIBRARY_STATUS.Dropped.label, statusId: 'Dropped' },
];

export const getStatusLabel = (statusId) => LIBRARY_STATUS[statusId]?.label ?? statusId ?? '';
