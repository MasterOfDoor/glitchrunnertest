# Vercel’e Deploy (Unity WebGL)

Bu dosyada: WebGL build alma, Vercel’e yükleme ve **Vercel’de ayarlaman gereken tüm Environment Variables** tek yerde.

---

## 1. Unity’de WebGL build

1. **Edit → Project Settings → Player** → **WebGL** sekmesi → **Publishing Settings**
2. **Compression Format** değerini **Disabled** yap.  
   (Vercel zaten yanıtları sıkıştırıyor; Unity Brotli/Gzip açık kalırsa çift sıkıştırma olur ve oyun açılmaz: *“double-compress” / “Unable to parse .framework.js.br”* hataları.)
3. **File → Build Settings** → **Platform: WebGL** → **Switch Platform** → **Build** veya **Build And Run** → çıktıyı **`My project (1)/Builds/WEBGL`** klasörüne al.

Çıktı: `index.html`, `Build/` (içinde **WEBGL.data**, **WEBGL.framework.js**, **WEBGL.wasm** — `.br` olmamalı), `TemplateData/`. Bu **Build/** içeriğini **repo kökündeki `webgl-deploy/Build/`** ile değiştir; `index.html` ve `TemplateData/` de üzerine yaz. Sonra commit + push. (Repoda `index.html` artık .br değil sıkıştırmasız dosyaları bekliyor; .br kullanma.)

---

## 2. Deploy klasörü (webgl-deploy)

Vercel **Serverless Function adlarında boşluk kabul etmez.** Bu yüzden deploy klasörü repo kökünde **`webgl-deploy`** (boşluksuz).

Bu klasörde olması gerekenler:

- Unity çıktısı: `index.html`, `Build/`, `TemplateData/`
- **`vercel.json`**, **`api/config.js`**, **`package.json`** (projede mevcut)

**Vercel’de:** **Settings → General → Root Directory** alanına şunu yaz:

```
webgl-deploy
```

Böylece build hatası (invalid function name / space) olmaz; her push’ta otomatik deploy çalışır.

---

## 3. Vercel Environment Variables (hepsini buradan al)

Vercel Dashboard → Proje → **Settings → Environment Variables** bölümüne gir. Aşağıdaki değişkenleri ekle (Production / Preview istersen ikisini de seç). **Private key veya gizli şifreleri buraya koyma** — sadece aşağıdaki public/ayar değerleri kullanılır; `/api/config` bunları oyuna verir.

| Environment Variable | Açıklama | Örnek |
|----------------------|----------|--------|
| `REOWN_PROJECT_ID` | Reown Cloud (WalletConnect) Project ID | `98c021d7980856feb52faa0f9c1d314c` |
| `AVALANCHE_RPC_URL` | Avalanche RPC URL (Fuji testnet veya mainnet) | `https://api.avax-test.network/ext/bc/C/rpc` |
| `AVALANCHE_TOKEN_ADDRESS` | GlitchRunnerCoin token contract adresi | `0x...` |
| `AVALANCHE_TOKEN_DECIMALS` | Token decimals | `18` |
| `AVALANCHE_DISTRIBUTOR_ADDRESS` | Coin toplayan / gelir cüzdan adresi (public address) | `0x...` |

- **Private key:** Dağıtım/backend tarafında gerekirse kendi sunucunda tut; **Vercel env’e ve `/api/config` çıktısına ekleme.**

---

## 4. Deploy

- **Git:** Repoyu Vercel’e bağla. **Root Directory:** `webgl-deploy` yap. Her push’ta otomatik deploy olur.
- **CLI:** `webgl-deploy` klasörüne gidip `vercel` çalıştır.

İlk açılışta oyun `/api/config` ile bu env değerlerini alır; cüzdan ve bakiye buna göre çalışır.

---

## Notlar

- **Sıkıştırma:** Unity’de Compression Format = **Disabled** kullan. Vercel CDN tek sefer sıkıştırır; Unity’de Brotli/Gzip açıksa çift sıkıştırma hatası alırsın.
- CORS: Aynı domain’de olduğu için ekstra ayar gerekmez.
- Custom domain: Vercel’de domain ekleyip DNS’i ayarladıktan sonra aynı env’ler kullanılır.
- Editor / standalone: WebGL dışında `.env` veya sistem env’i kullanılır; Vercel env’ler sadece WebGL deploy için.
