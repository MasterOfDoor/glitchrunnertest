using System;
using UnityEngine;

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

    /// <summary>Reown AppKit paketi eklendiğinde burada AppKit.InitializeAsync + OpenModal + AccountConnected ile onAddress(addr) çağırın; true dönün.</summary>
    static bool TryConnectReown(Action<string> onAddress, Action<string> onError)
    {
        return false;
    }
}
