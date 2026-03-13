// Vercel serverless: oyun WebGL'de /api/config ile env değişkenlerini alır.
// Sadece istemcide gerekli (public) değerleri döndür; private key vb. EKLEME.
export default function handler(req, res) {
  res.setHeader('Cache-Control', 'no-store');
  res.status(200).json({
    REOWN_PROJECT_ID: process.env.REOWN_PROJECT_ID || '',
    AVALANCHE_RPC_URL: process.env.AVALANCHE_RPC_URL || '',
    AVALANCHE_TOKEN_ADDRESS: process.env.AVALANCHE_TOKEN_ADDRESS || '',
    AVALANCHE_TOKEN_DECIMALS: process.env.AVALANCHE_TOKEN_DECIMALS || '',
    AVALANCHE_DISTRIBUTOR_ADDRESS: process.env.AVALANCHE_DISTRIBUTOR_ADDRESS || ''
  });
}
