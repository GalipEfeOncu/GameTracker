// GameTracker Electron ana süreci.
// - Geliştirmede: GAMETRACKER_DEV_SERVER_URL (örn. http://localhost:5173) → Vite dev server.
// - Üretimde: packaged renderer (resources/renderer/index.html).
// Arka plan sorumlulukları: yüklü oyun algılama, playtime tracker (process izleme),
// sistem tray, Windows autostart toggle'ı. Ayarlar userData altında JSON olarak saklanır.

'use strict';

const { app, BrowserWindow, shell, Menu, Tray, ipcMain, dialog } = require('electron');
const path = require('node:path');
const fs = require('node:fs');

const { createStore } = require('./lib/store');
const { createApi } = require('./lib/api');
const { createTracker, listExesInDir } = require('./tracker');
const detectors = require('./detectors');

// --- Tek instance kilidi: ikinci açılış var olan pencereye odaklanır ---
const gotLock = app.requestSingleInstanceLock();
if (!gotLock) {
    app.quit();
    return;
}

const isDev = !app.isPackaged;
const devServerUrl = process.env.GAMETRACKER_DEV_SERVER_URL;

let mainWindow = null;
let tray = null;
let quitting = false;
let settingsStore = null;
let mappingsStore = null;
let tracker = null;
let session = { userId: null, accessToken: null };
let apiBaseUrl = '';

// --- Pencere oluşturma ---
function resolveRendererIndex() {
    const packaged = path.join(process.resourcesPath || '', 'renderer', 'index.html');
    if (fs.existsSync(packaged)) return packaged;
    const local = path.join(__dirname, '..', '..', 'frontend', 'dist', 'index.html');
    if (fs.existsSync(local)) return local;
    return null;
}

function createWindow() {
    mainWindow = new BrowserWindow({
        width: 1280,
        height: 820,
        minWidth: 960,
        minHeight: 640,
        backgroundColor: '#0f1117',
        autoHideMenuBar: true,
        show: !settingsStore?.get('startMinimized'),
        webPreferences: {
            preload: path.join(__dirname, 'preload.js'),
            contextIsolation: true,
            nodeIntegration: false,
            sandbox: false, // preload'tan Node modülleri (fs/spawn) kullanabilmek için
        },
    });

    mainWindow.webContents.setWindowOpenHandler(({ url }) => {
        shell.openExternal(url);
        return { action: 'deny' };
    });

    // Kullanıcı X'e tıklasa da uygulama tray'de yaşar; "quit" menüsüyle gerçekten kapanır.
    mainWindow.on('close', (event) => {
        if (quitting) return;
        if (settingsStore?.get('minimizeToTray')) {
            event.preventDefault();
            mainWindow.hide();
        }
    });

    if (devServerUrl) {
        mainWindow.loadURL(devServerUrl);
        if (isDev) mainWindow.webContents.openDevTools({ mode: 'detach' });
        return;
    }

    const indexPath = resolveRendererIndex();
    if (!indexPath) {
        mainWindow.loadURL(
            'data:text/html;charset=utf-8,' +
                encodeURIComponent('<h1 style="font-family:sans-serif;padding:24px">GameTracker</h1><p style="font-family:sans-serif;padding:0 24px">Renderer build bulunamadi. \'npm run build:renderer\' gerekiyor.</p>'),
        );
        return;
    }
    mainWindow.loadFile(indexPath);
}

// --- Sistem tray ---
function showWindow() {
    if (!mainWindow) { createWindow(); return; }
    if (mainWindow.isMinimized()) mainWindow.restore();
    mainWindow.show();
    mainWindow.focus();
}

function createTray() {
    // İkon yoksa ikon yolunu yine de verip Electron'un varsayılan 1x1 göstermesine bırakırız.
    // Görünür kalite için ileride assets/tray.png eklenir.
    const iconPath = path.join(__dirname, '..', 'assets', 'tray.png');
    const tryIcon = fs.existsSync(iconPath) ? iconPath : undefined;
    tray = tryIcon ? new Tray(tryIcon) : new Tray(require('electron').nativeImage.createEmpty());
    tray.setToolTip('GameTracker');
    const contextMenu = Menu.buildFromTemplate([
        { label: 'Göster', click: showWindow },
        { type: 'separator' },
        { label: 'Çıkış', click: () => { quitting = true; app.quit(); } },
    ]);
    tray.setContextMenu(contextMenu);
    tray.on('click', showWindow);
}

// --- Autostart (Windows login item) ---
function applyAutoStart(enabled) {
    if (process.platform !== 'win32') return;
    app.setLoginItemSettings({
        openAtLogin: !!enabled,
        path: process.execPath,
        args: ['--hidden'], // otomatik başlangıçta tray'e gelsin
    });
}

// --- IPC handlers ---
function registerIpc() {
    ipcMain.handle('gt:detect-installed', async () => {
        try {
            return await detectors.detectAll();
        } catch (err) {
            console.warn('[detect] failed:', err.message);
            return [];
        }
    });

    ipcMain.handle('gt:scan-exes', async (_e, dir) => {
        if (!dir || typeof dir !== 'string') return [];
        try { return listExesInDir(fs, dir, 2); } catch { return []; }
    });

    ipcMain.handle('gt:pick-exe', async () => {
        const result = await dialog.showOpenDialog(mainWindow, {
            title: 'Oyun çalıştırılabiliri seçin',
            properties: ['openFile'],
            filters: [{ name: 'Uygulamalar', extensions: ['exe'] }],
        });
        if (result.canceled || !result.filePaths?.length) return null;
        return result.filePaths[0];
    });

    ipcMain.handle('gt:list-mappings', () => tracker?.getMappings() ?? []);

    ipcMain.handle('gt:upsert-mapping', (_e, mapping) => {
        if (!tracker || !mapping?.gameId) return false;
        const exeNames = Array.isArray(mapping.exeNames) ? mapping.exeNames.slice() : [];
        // İsteğe bağlı olarak installDir verilirse, içindeki .exe'leri de otomatik ekle.
        if (mapping.installDir && fs.existsSync(mapping.installDir)) {
            try {
                const discovered = listExesInDir(fs, mapping.installDir, 2);
                for (const e of discovered) if (!exeNames.includes(e)) exeNames.push(e);
            } catch { /* ignore */ }
        }
        tracker.upsertMapping({
            gameId: mapping.gameId,
            gameName: mapping.gameName,
            exeNames,
        });
        return true;
    });

    ipcMain.handle('gt:remove-mapping', (_e, gameId) => {
        if (!tracker) return false;
        tracker.removeMapping(gameId);
        return true;
    });

    ipcMain.handle('gt:set-session', (_e, payload) => {
        session = {
            userId: payload?.userId ? Number(payload.userId) : null,
            accessToken: payload?.accessToken ?? null,
        };
        if (payload?.apiBaseUrl) apiBaseUrl = String(payload.apiBaseUrl).replace(/\/+$/, '');
        return true;
    });

    ipcMain.handle('gt:get-settings', () => settingsStore.getAll());

    ipcMain.handle('gt:set-settings', (_e, partial) => {
        if (!partial || typeof partial !== 'object') return settingsStore.getAll();
        const allowed = ['minimizeToTray', 'startMinimized', 'autoStart'];
        const patch = {};
        for (const key of allowed) {
            if (key in partial) patch[key] = !!partial[key];
        }
        settingsStore.patch(patch);
        if ('autoStart' in patch) applyAutoStart(patch.autoStart);
        return settingsStore.getAll();
    });

    ipcMain.handle('gt:get-active', () => tracker?.getActive() ?? []);
}

// --- App lifecycle ---
if (!isDev) Menu.setApplicationMenu(null);

app.on('second-instance', () => { showWindow(); });

app.whenReady().then(() => {
    settingsStore = createStore('settings.json', app, {
        minimizeToTray: true,
        startMinimized: false,
        autoStart: false,
    });
    mappingsStore = createStore('mappings.json', app, { mappings: [] });

    const api = createApi({
        baseUrlProvider: () => apiBaseUrl,
        tokenProvider: () => session.accessToken,
    });
    tracker = createTracker({
        api,
        store: mappingsStore,
        getUser: () => session,
        onUpdate: (payload) => {
            if (mainWindow && !mainWindow.isDestroyed()) {
                mainWindow.webContents.send('gt:playtime-update', payload);
            }
        },
    });

    registerIpc();
    applyAutoStart(settingsStore.get('autoStart'));
    createTray();
    createWindow();
    tracker.start();

    app.on('activate', () => {
        if (BrowserWindow.getAllWindows().length === 0) createWindow();
        else showWindow();
    });
});

app.on('before-quit', async (event) => {
    if (!tracker) return;
    if (quitting) return; // ikinci çağrı; flush zaten başladı
    quitting = true;
    event.preventDefault();
    try { await tracker.shutdown(); } catch { /* ignore */ }
    app.exit(0);
});

// Tray'de yaşadığımız için window-all-closed'ta quit etmeyiz.
app.on('window-all-closed', () => {
    if (process.platform === 'darwin') return;
    if (!settingsStore?.get('minimizeToTray')) app.quit();
});
