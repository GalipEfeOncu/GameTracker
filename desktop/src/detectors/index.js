// Tüm platform algılayıcıları tek listede toplar.
// Her algılayıcı hata üretirse yutulur; diğer platformlar çalışmaya devam eder.

'use strict';

const steam = require('./steam');
const epic = require('./epic');
const gog = require('./gog');

async function detectAll() {
    const results = await Promise.allSettled([steam.detect(), epic.detect(), gog.detect()]);
    const games = [];
    for (const r of results) {
        if (r.status === 'fulfilled' && Array.isArray(r.value)) {
            games.push(...r.value);
        } else if (r.status === 'rejected') {
            console.warn('[detectors] one platform failed:', r.reason?.message ?? r.reason);
        }
    }
    return games;
}

module.exports = { detectAll };
