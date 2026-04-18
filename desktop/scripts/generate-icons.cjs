'use strict';

/**
 * Yer tutucu tray + uygulama ikonları üretir (mavi düz renk).
 * Kurumsal ikon için çıktıları tasarımla değiştir; komutu tekrar çalıştırmana gerek yok.
 */

const fs = require('fs');
const path = require('path');
const { Jimp, JimpMime } = require('jimp');

const ACCENT = '#2563eb';

async function main() {
    const assets = path.join(__dirname, '..', 'assets');
    fs.mkdirSync(assets, { recursive: true });

    const tray = new Jimp({ width: 32, height: 32, color: ACCENT });
    await tray.write(path.join(assets, 'tray.png'));

    const sizes = [256, 48, 32];
    const buffers = [];
    for (const size of sizes) {
        const img = new Jimp({ width: size, height: size, color: ACCENT });
        buffers.push(await img.getBuffer(JimpMime.png));
    }

    const { default: pngToIco } = await import('png-to-ico');
    const icoPath = path.join(assets, 'icon.ico');
    fs.writeFileSync(icoPath, Buffer.from(await pngToIco(buffers)));

    console.log('OK:', path.join(assets, 'tray.png'), icoPath);
}

main().catch((e) => {
    console.error(e);
    process.exit(1);
});
