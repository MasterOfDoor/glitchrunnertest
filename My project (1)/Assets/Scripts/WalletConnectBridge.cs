using System;
using UnityEngine;
using Reown.AppKit.Unity;

/// <summary>
/// Cüzdan bağlantısı: .env'deki DEV_WALLET_ADDRESS (test) veya ileride Reown AppKit.
/// Reown eklemek: Package Manager → OpenUPM → com.reown.appkit.unity, sonra TryConnectReown içini implement edin.
/// </summary>
public static class WalletConnectBridge
{
    const string EnvDevWallet = "DEV_WALLET_ADDRESS";

    /// <summary>Cüzdan bağlanır; adres onAddress ile döner, hata onError ile.</summary>
    public static void ConnectAsync(Action<string> onAddress, Action<string> onError)
    {
        if (onAddress == null) return;

        if (TryConnectReown(onAddress, onError))
            return;

        string devAddr = EnvLoader.Get(EnvDevWallet);
        if (!string.IsNullOrWhiteSpace(devAddr))
        {
            onAddress(devAddr.Trim());
            return;
        }

        onError?.Invoke("Cüzdan bağlanamadı. .env dosyasına DEV_WALLET_ADDRESS=0xAdresiniz ekleyin veya Reown AppKit kurun.");
    }

    /// <summary>
    /// Real wallet connect via Reown AppKit.
    /// If AppKit prefab is present, we initialize it (once), open the modal,
    /// and wait for AccountConnected; on success we call onAddress(address) and return true.
    /// If AppKit is not available we return false so .env fallback is used.
    /// </summary>
    static bool TryConnectReown(Action<string> onAddress, Action<string> onError)
    {
        // AppKit prefab'ı sahnede yoksa Reown kullanma
        if (AppKit.Instance == null)
            return false;

        ConnectWithReownAsync(onAddress, onError);
        return true;
    }

    /// <summary>
    /// Async connection flow using Reown AppKit.
    /// </summary>
    static async void ConnectWithReownAsync(Action<string> onAddress, Action<string> onError)
    {
        try
        {
            if (!AppKit.IsInitialized)
            {
                var metadata = new Metadata(
                    name: "GlitchRunner",
                    description: "On-chain puzzle platformer",
                    url: "https://glitchrunner.example.com",
                    iconUrl: "https://glitchrunner.example.com/logo.png"
                );

                // Avalanche Fuji (eip155:43113) — oyun token'ı bu ağda; cüzdan ETH yerine Fuji'ye geçsin
                var avalancheFuji = new Chain(
                    ChainConstants.Namespaces.Evm,
                    chainReference: "43113",
                    name: "Avalanche Fuji",
                    nativeCurrency: new Currency("Avalanche", "AVAX", 18),
                    blockExplorer: new BlockExplorer("Snowtrace", "https://testnet.snowtrace.io"),
                    rpcUrl: "https://api.avax-test.network/ext/bc/C/rpc",
                    isTestnet: true,
                    imageUrl: "https://avatars.githubusercontent.com/u/42355201?s=200&v=4"
                );
                var config = new AppKitConfig(
                    projectId: EnvLoader.Get("REOWN_PROJECT_ID", "98c021d7980856feb52faa0f9c1d314c"),
                    metadata: metadata
                )
                {
                    supportedChains = new[] { avalancheFuji }
                };

                await AppKit.InitializeAsync(config);
            }

            // Eğer zaten bağlı bir hesap varsa, doğrudan onu kullan
            if (AppKit.IsAccountConnected)
            {
                var account = AppKit.Account;
                if (account != null && !string.IsNullOrEmpty(account.Address))
                {
                    onAddress?.Invoke(account.Address);
                    return;
                }

                onError?.Invoke("Wallet connected but no account address found.");
                return;
            }

            // Yeni bağlantı: AccountConnected event'ini bir kez dinle, ardından modal aç.
            void Handler(object _, Connector.AccountConnectedEventArgs e)
            {
                AppKit.AccountConnected -= Handler;

                var account = AppKit.Account;
                if (account != null && !string.IsNullOrEmpty(account.Address))
                    onAddress?.Invoke(account.Address);
                else
                    onError?.Invoke("Wallet connected but address is empty.");
            }

            AppKit.AccountConnected += Handler;
            AppKit.OpenModal();
        }
        catch (Exception ex)
        {
            Debug.LogError("[WalletConnectBridge] Reown connect failed: " + ex.Message);
            onError?.Invoke(ex.Message);
        }
    }
}
