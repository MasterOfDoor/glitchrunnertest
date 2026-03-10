# Vercel’e Deploy (Unity WebGL)

Bu dosyada: WebGL build alma, Vercel’e yükleme ve **Vercel’de ayarlaman gereken tüm Environment Variables** tek yerde.

---

## 1. Unity’de WebGL build

1. **File → Build Settings**
2. **Platform:** WebGL → **Switch Platform**
3. **Build** veya **Build And Run** → çıktıyı **`My project (1)/Builds/WEBGL`** klasörüne al.

Çıktı: `index.html`, `Build/`, `TemplateData/`. Sonra bu dosyaları **repo kökündeki `webgl-deploy`** klasörüne kopyala (içindeki `index.html`, `Build/`, `TemplateData/` üzerine yaz). `webgl-deploy` içinde `vercel.json` ve `api/` zaten var; commit + push yap.

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

- CORS: Aynı domain’de olduğu için ekstra ayar gerekmez.
- Custom domain: Vercel’de domain ekleyip DNS’i ayarladıktan sonra aynı env’ler kullanılır.
- Editor / standalone: WebGL dışında `.env` veya sistem env’i kullanılır; Vercel env’ler sadece WebGL deploy için.
