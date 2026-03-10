# Vercel’e Deploy (Unity WebGL)

Bu dosyada: WebGL build alma, Vercel’e yükleme ve **Vercel’de ayarlaman gereken tüm Environment Variables** tek yerde.

---

## 1. Unity’de WebGL build

1. **File → Build Settings**
2. **Platform:** WebGL → **Switch Platform**
3. **Build** veya **Build And Run** → çıktıyı **`My project (1)/Builds/WEBGL`** klasörüne al (mevcut deploy klasörünü kullan veya üzerine yaz).

Çıktı: `index.html`, `Build/`, `TemplateData/`. Sonra bu klasörde `vercel.json` ve `api/` zaten var; commit + push yap, Vercel Root Directory = `My project (1)/Builds/WEBGL` olsun.

---

## 2. Deploy klasörü (Builds/WEBGL)

Build çıktın **`My project (1)/Builds/WEBGL/`** klasöründe. Bu klasörde şunlar olmalı:

- Unity çıktısı: `index.html`, `Build/`, `TemplateData/`
- **`vercel.json`** ve **`api/config.js`** — projede bu klasöre eklendi.

**Vercel’de:** Repoyu bağladıktan sonra **Settings → General → Root Directory** alanına şunu yaz:

```
My project (1)/Builds/WEBGL
```

Böylece Vercel deploy’u doğrudan bu klasörden alır; her push’ta (bu klasör güncellenmişse) otomatik deploy olur.

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

- **Git:** Repoyu Vercel’e bağla. **Root Directory:** `My project (1)/Builds/WEBGL` yap. Her push’ta bu klasör güncellenmişse otomatik deploy olur.
- **CLI:** `My project (1)/Builds/WEBGL` klasörüne gidip `vercel` çalıştır.

İlk açılışta oyun `/api/config` ile bu env değerlerini alır; cüzdan ve bakiye buna göre çalışır.

---

## Notlar

- CORS: Aynı domain’de olduğu için ekstra ayar gerekmez.
- Custom domain: Vercel’de domain ekleyip DNS’i ayarladıktan sonra aynı env’ler kullanılır.
- Editor / standalone: WebGL dışında `.env` veya sistem env’i kullanılır; Vercel env’ler sadece WebGL deploy için.
