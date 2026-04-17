// Context-isolated IPC köprüsü. Renderer (Vite SPA) `window.gameTracker` üzerinden
// yalnızca beyaz listedeki kanallara erişir. Hiçbir Node API'si doğrudan açılmaz.

'use strict';

const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('gameTracker', {
    isDesktop: true,

    // --- Keşif ---
    detectInstalledGames: () => ipcRenderer.invoke('gt:detect-installed'),

    // --- Oyun → process eşlemesi ---
    // mapping: { gameId, gameName, exeNames: ['foo.exe'], installDir? }
    // installDir verilirse main process o klasörden .exe listesini otomatik genişletir.
    upsertMapping: (mapping) => ipcRenderer.invoke('gt:upsert-mapping', mapping),
    removeMapping: (gameId) => ipcRenderer.invoke('gt:remove-mapping', gameId),
    listMappings: () => ipcRenderer.invoke('gt:list-mappings'),
    scanExesInDir: (dir) => ipcRenderer.invoke('gt:scan-exes', dir),
    pickExe: () => ipcRenderer.invoke('gt:pick-exe'),

    // --- Oturum / kimlik (tracker'ın heartbeat için kullandığı token) ---
    setSession: (session) => ipcRenderer.invoke('gt:set-session', session),

    // --- Ayarlar ---
    getSettings: () => ipcRenderer.invoke('gt:get-settings'),
    setSettings: (partial) => ipcRenderer.invoke('gt:set-settings', partial),

    // --- Tracker durumu (aktif oturumlar) ---
    getActiveSessions: () => ipcRenderer.invoke('gt:get-active'),

    // --- Event'ler (heartbeat sonrası UI yenilemesi için) ---
    onPlaytimeUpdate: (cb) => {
        const handler = (_event, payload) => cb(payload);
        ipcRenderer.on('gt:playtime-update', handler);
        return () => ipcRenderer.removeListener('gt:playtime-update', handler);
    },
});
