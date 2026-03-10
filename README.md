# GlitchRunner

Unity 2D platformer with wallet login (Reown AppKit) and Avalanche token (GlitchRunnerCoin). Scenes: Login → Öğretici → CPU Giriş → CPU Bazaar → RAM.

## Gereksinimler

- Unity (2021.3+ önerilir)
- Reown AppKit Unity paketi (WalletConnect)

## Yerel çalıştırma

1. Projeyi Unity ile aç: **My project (1)** klasörünü aç veya `.unity` ile biten ana sahneyi aç.
2. Geliştirme için `.env` (proje kökünde veya `Assets` yanında) kullanabilirsin; yoksa Reown + Avalanche ayarları Editor’da varsayılan/boş çalışır.
3. **Play** ile Login sahnesinden başlat.

## Build

- **WebGL:** File → Build Settings → WebGL → Build (veya Build And Run).
- **PC/Desktop:** Aynı menüden Windows/Mac/Linux seçip build al.

## Deploy

Vercel’e WebGL deploy ve **tüm environment variable’lar** için: **[DEPLOY.md](DEPLOY.md)**.

## Proje yapısı (kısa)

- `Assets/Scripts/` — Oyun ve UI scriptleri
- `Assets/Scripts/FarukEdit/` — Robot diyalog vb.
- Kök: `vercel.json`, `api/config.js`, `DEPLOY.md` — Vercel deploy için
