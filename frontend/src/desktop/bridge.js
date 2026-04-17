// Web + desktop ortak kod; Electron preload tarafından window.gameTracker açılır.
// Web modunda undefined'dır → tüm helper'lar güvenli no-op döner.

const bridge = typeof window !== 'undefined' ? window.gameTracker : undefined;

export const isDesktop = !!bridge?.isDesktop;

export async function detectInstalledGames() {
    if (!bridge) return [];
    return (await bridge.detectInstalledGames()) || [];
}

export async function listMappings() {
    if (!bridge) return [];
    return (await bridge.listMappings()) || [];
}

export async function upsertMapping(mapping) {
    if (!bridge) return false;
    return bridge.upsertMapping(mapping);
}

export async function removeMapping(gameId) {
    if (!bridge) return false;
    return bridge.removeMapping(gameId);
}

export async function pickExe() {
    if (!bridge) return null;
    return bridge.pickExe();
}

export async function scanExesInDir(dir) {
    if (!bridge) return [];
    return (await bridge.scanExesInDir(dir)) || [];
}

export async function setDesktopSession({ userId, accessToken, apiBaseUrl }) {
    if (!bridge) return;
    await bridge.setSession({ userId, accessToken, apiBaseUrl });
}

export async function getDesktopSettings() {
    if (!bridge) return null;
    return bridge.getSettings();
}

export async function setDesktopSettings(partial) {
    if (!bridge) return null;
    return bridge.setSettings(partial);
}

export function onPlaytimeUpdate(cb) {
    if (!bridge) return () => {};
    return bridge.onPlaytimeUpdate(cb);
}
