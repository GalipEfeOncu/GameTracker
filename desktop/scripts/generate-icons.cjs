'use strict';

/**
 * Kök dizindeki GameTracker_Icon.png → desktop/assets/tray.png + icon.ico
 * Web favicon için: aynı kaynak frontend/public/favicon.png olarak tutulur (manuel veya deploy öncesi kopyala).
 */

const fs = require('fs');
const path = require('path');
const { Jimp, JimpMime } = require('jimp');

async function squarePngFromBase(base, size) {
    const img = base.clone();
    await img.resize({ w: size, h: size });
    return img.getBuffer(JimpMime.png);
}

async function main() {
    const repoRoot = path.join(__dirname, '..', '..');
    const sourcePath = path.join(repoRoot, 'GameTracker_Icon.png');
    if (!fs.existsSync(sourcePath)) {
        console.error('Kök dizinde GameTracker_Icon.png bulunamadı:', sourcePath);
        process.exit(1);
    }

    const base = await Jimp.read(sourcePath);
    const assets = path.join(__dirname, '..', 'assets');
    fs.mkdirSync(assets, { recursive: true });

    const trayBuf = await squarePngFromBase(base, 32);
    fs.writeFileSync(path.join(assets, 'tray.png'), trayBuf);

    const buffers = [];
    for (const size of [256, 48, 32]) {
        buffers.push(await squarePngFromBase(base, size));
    }

    const { default: pngToIco } = await import('png-to-ico');
    const icoPath = path.join(assets, 'icon.ico');
    fs.writeFileSync(icoPath, Buffer.from(await pngToIco(buffers)));

    const publicDir = path.join(repoRoot, 'frontend', 'public');
    fs.mkdirSync(publicDir, { recursive: true });
    const favBuf = await squarePngFromBase(base, 512);
    fs.writeFileSync(path.join(publicDir, 'favicon.png'), favBuf);

    console.log('OK:', path.join(assets, 'tray.png'), icoPath, path.join(publicDir, 'favicon.png'));
}

main().catch((e) => {
    console.error(e);
    process.exit(1);
});
