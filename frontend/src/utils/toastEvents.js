/** Axios vb. React dışı kodun toast tetiklemesi için (CustomEvent). */
export const GT_TOAST_EVENT = 'gt-toast';

export function emitToast(message, type = 'info') {
    if (typeof window !== 'undefined') {
        window.dispatchEvent(new CustomEvent(GT_TOAST_EVENT, { detail: { message, type } }));
    }
}
