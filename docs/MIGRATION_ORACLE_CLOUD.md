# Migration: Render.com → Oracle Cloud (API Hosting)

> **Hedef:** .NET 9 API'yi Render.com'dan Oracle Cloud Always Free ARM VM'e taşımak.
> Cold start sorunu tamamen ortadan kalkar — VM 7/24 çalışır.

---

## Ön koşullar

- [ ] Oracle Cloud hesabı oluşturuldu ([cloud.oracle.com](https://cloud.oracle.com))
- [ ] Kredi kartı girildi (ücret alınmaz, Always Free kalır)
- [ ] Hesap onaylandı (bazen 1-2 gün sürebilir)

---

## Adım 1 — VM Oluşturma

Oracle Cloud Console → Compute → Instances → **Create Instance**

| Alan | Değer | Not |
|------|-------|-----|
| **Name** | `gametracker-api` | |
| **Image** | Ubuntu 22.04 (veya 24.04) | Canonical, Minimal |
| **Shape** | `VM.Standard.A1.Flex` | **ARM** — Always Free |
| **OCPU** | 1 | (toplam 4 hakkınız var) |
| **RAM** | 6 GB | (toplam 24 GB hakkınız var) |
| **Boot volume** | 47 GB (varsayılan) | Always Free |
| **Network** | Varsayılan VCN + public subnet | Otomatik public IP atanır |
| **SSH Key** | Kendi public key'inizi yükleyin | `ssh-keygen -t ed25519` ile üretin |

> [!IMPORTANT]
> Shape olarak **VM.Standard.A1.Flex** seçin. E2.1.Micro (AMD) sadece 1 GB RAM verir ve .NET için sıkışık olur.

---

## Adım 2 — Firewall / Security List

Oracle Cloud Console → Networking → Virtual Cloud Networks → VCN → Subnet → Security List

### Ingress kuralları ekle:

| Source CIDR | Protocol | Dest Port | Açıklama |
|-------------|----------|-----------|----------|
| `0.0.0.0/0` | TCP | 80 | HTTP |
| `0.0.0.0/0` | TCP | 443 | HTTPS |

### VM içi firewall (iptables):

```bash
# SSH bağlantısı sonrası VM'de çalıştırın
sudo iptables -I INPUT 6 -m state --state NEW -p tcp --dport 80 -j ACCEPT
sudo iptables -I INPUT 6 -m state --state NEW -p tcp --dport 443 -j ACCEPT
sudo netfilter-persistent save
```

---

## Adım 3 — Docker Kurulumu

```bash
# VM'e SSH ile bağlan
ssh -i ~/.ssh/oracle_key ubuntu@<VM_PUBLIC_IP>

# Docker kur (ARM64 uyumlu)
sudo apt update && sudo apt upgrade -y
sudo apt install -y ca-certificates curl gnupg
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg

echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

# ubuntu kullanıcısını docker grubuna ekle (logout/login gerekir)
sudo usermod -aG docker ubuntu
```

> [!NOTE]
> ARM64 VM'de Docker ve .NET imajları sorunsuz çalışır. `mcr.microsoft.com/dotnet/aspnet:9.0` ve `sdk:9.0` multi-arch imajlardır.

---

## Adım 4 — Proje Deploy

### 4a. Repo'yu klonla

```bash
cd ~
git clone https://github.com/<KULLANICI>/GameTracker.git
cd GameTracker/backend
```

### 4b. Ortam değişkenleri dosyası oluştur

```bash
cat > ~/gametracker.env << 'EOF'
# DB bağlantısı (Azure SQL veya PostgreSQL — bkz. MIGRATION_DATABASE.md)
ConnectionStrings__GameTrackerDB=<CONNECTION_STRING>

# API anahtarları
Igdb__ClientId=<TWITCH_CLIENT_ID>
Igdb__ClientSecret=<TWITCH_CLIENT_SECRET>
ApiKeys__RawgApiKey=<RAWG_KEY>
ApiKeys__GeminiApiKey=<GEMINI_KEY>

# JWT
Jwt__SigningKey=<EN_AZ_32_KARAKTER_GUVENLI_ANAHTAR>

# CORS — frontend URL'leri (noktalı virgülle ayır veya __0, __1 kullan)
Cors__AllowedOrigins=https://your-frontend.vercel.app

# E-posta
EmailSettings__MailAddress=<GMAIL_ADRESI>
EmailSettings__MailPassword=<UYGULAMA_SIFRESI>

# Port (Caddy reverse proxy 8080'e yönlendirecek)
PORT=8080
ASPNETCORE_ENVIRONMENT=Production
EOF

# Dosyayı koru
chmod 600 ~/gametracker.env
```

### 4c. Docker ile başlat

```bash
cd ~/GameTracker/backend

docker build -t gametracker-api .

docker run -d \
  --name gametracker-api \
  --restart unless-stopped \
  --env-file ~/gametracker.env \
  -p 8080:8080 \
  gametracker-api
```

### 4d. Test

```bash
curl http://localhost:8080/swagger/index.html
# veya health check endpoint'i varsa onu çağır
```

---

## Adım 5 — Reverse Proxy (Caddy) + HTTPS

Caddy otomatik Let's Encrypt sertifikası alır — yapılandırma çok basit.

### 5a. Caddy kur

```bash
sudo apt install -y debian-keyring debian-archive-keyring apt-transport-https
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/gpg.key' | sudo gpg --dearmor -o /usr/share/keyrings/caddy-stable-archive-keyring.gpg
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/debian.deb.txt' | sudo tee /etc/apt/sources.list.d/caddy-stable.list
sudo apt update
sudo apt install caddy
```

### 5b. Caddyfile

```bash
sudo tee /etc/caddy/Caddyfile << 'EOF'
api.gametracker.example.com {
    reverse_proxy localhost:8080
}
EOF

sudo systemctl restart caddy
```

> [!TIP]
> `api.gametracker.example.com` yerine kendi domain'inizi koyun. DNS'te A kaydı VM'in public IP'sine yönlendirilmeli.

### 5c. Domain yoksa (geçici erişim)

Domain olmadan doğrudan IP ile HTTP erişimi:

```bash
sudo tee /etc/caddy/Caddyfile << 'EOF'
:80 {
    reverse_proxy localhost:8080
}
EOF
sudo systemctl restart caddy
```

Bu durumda frontend `VITE_API_BASE_URL=http://<VM_PUBLIC_IP>/api` kullanır.

---

## Adım 6 — Otomatik Yeniden Başlatma (systemd)

Docker `--restart unless-stopped` ile container otomatik başlar. Ek olarak Docker daemon'u systemd ile yönetilir:

```bash
sudo systemctl enable docker
```

VM reboot'larında API otomatik ayağa kalkar.

---

## Adım 7 — Güncelleme prosedürü

Her deploy'da:

```bash
cd ~/GameTracker
git pull origin main
cd backend
docker build -t gametracker-api .
docker stop gametracker-api && docker rm gametracker-api
docker run -d \
  --name gametracker-api \
  --restart unless-stopped \
  --env-file ~/gametracker.env \
  -p 8080:8080 \
  gametracker-api

# Eski imajları temizle
docker image prune -f
```

---

## Adım 8 — Frontend yapılandırma güncellemesi

`frontend/.env.production` (veya Vercel env):

```bash
VITE_API_BASE_URL=https://api.gametracker.example.com/api
```

`Cors__AllowedOrigins` env'inde frontend URL'sinin doğru olduğundan emin ol.

---

## Adım 9 — Render.com temizliği

Her şey Oracle'da çalıştığını doğruladıktan sonra:

1. Render Dashboard → Service → **Delete**
2. `.env` / env dosyalarındaki eski Render URL'lerini temizle

---

## Doğrulama kontrol listesi

- [ ] `curl https://api.gametracker.example.com/api/Game/popular` → JSON yanıt
- [ ] Frontend'den login/register çalışıyor
- [ ] Kütüphaneye oyun ekleme çalışıyor
- [ ] Desktop heartbeat (playtime) çalışıyor
- [ ] Cold start yok — 15 dk boşta kaldıktan sonra ilk istek anında yanıt veriyor

---

## Maliyet

**$0/ay** — Oracle Cloud Always Free. VM, boot volume, public IP, bandwidth (10 TB/ay) dahil.

---

## Dockerfile değişikliği

Mevcut `backend/Dockerfile` **değişiklik gerektirmez**. Multi-arch imaj (`mcr.microsoft.com/dotnet/aspnet:9.0`) ARM64'te çalışır.

Yalnızca yorum satırı güncellenebilir:

```dockerfile
# Oracle Cloud VM ve diğer container ortamları (.NET 9 API)
```

---

*Referans: [AGENT_HANDOFF.md](./AGENT_HANDOFF.md) · [DEPLOY.md](./DEPLOY.md)*
