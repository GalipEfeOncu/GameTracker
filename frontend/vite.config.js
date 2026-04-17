import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const proxyTarget = (env.VITE_DEV_PROXY_TARGET || 'http://localhost:5118').replace(/\/+$/, '')
  // Desktop (Electron) build'inde SPA `file://` üzerinden yüklenir; varlık yolları göreli olmalıdır.
  // `VITE_DESKTOP=1` ile işaretle (desktop/ paketleme scripti bu env'i geçebilir); aksi halde web varsayılanı `/`.
  const isDesktopBuild = env.VITE_DESKTOP === '1' || env.VITE_DESKTOP === 'true'

  return {
    base: isDesktopBuild ? './' : '/',
    plugins: [react(), tailwindcss()],
    server: {
      proxy: {
        '/api': {
          target: proxyTarget,
          changeOrigin: true,
        },
      },
    },
  }
})
