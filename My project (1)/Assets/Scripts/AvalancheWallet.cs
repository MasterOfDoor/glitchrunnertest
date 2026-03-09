using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Cüzdan adresi GameState'ten; bakiye okuma (ERC-20) ve dağıtıcıya transfer.
/// İmza için WalletConnect/MetaMask entegrasyonu ileride bağlanır.
/// </summary>
public class AvalancheWallet : MonoBehaviour
{
    public static AvalancheWallet Instance { get; private set; }

    AvalancheConfig config;

    /// <summary>Transfer tetiklenince (to, amountWei). İmzalı işlem WalletConnect vb. ile yapılacak.</summary>
    public event Action<string, string, string, Action<bool>> OnTransferRequested;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        config = Resources.Load<AvalancheConfig>("AvalancheConfig");
        if (config == null)
            config = ScriptableObject.CreateInstance<AvalancheConfig>();
    }

    void Start()
    {
        if (GameState.Instance != null)
            GameState.Instance.OnWalletChanged += SyncCoinFromWallet;
        SyncCoinFromWallet();
    }

    void OnDestroy()
    {
        if (GameState.Instance != null)
            GameState.Instance.OnWalletChanged -= SyncCoinFromWallet;
        if (Instance == this) Instance = null;
    }

    /// <summary>Cüzdan bakiyesini okuyup GameState coin olarak yazar. Cüzdan yoksa dokunmaz.</summary>
    public void SyncCoinFromWallet()
    {
        if (GameState.Instance == null) return;
        if (string.IsNullOrEmpty(WalletAddress))
            return;
        GetBalanceAsync(
            balance => GameState.Instance.SetCoinBalance(balance),
            _ => { }
        );
    }

    public string WalletAddress => GameState.Instance != null ? GameState.Instance.WalletAddress : "";

    public void SetConfig(AvalancheConfig c) => config = c ?? config;

    /// <summary>ERC-20 bakiye okur (Fuji RPC eth_call balanceOf).</summary>
    public void GetBalanceAsync(Action<decimal> onBalance, Action<string> onError)
    {
        string addr = WalletAddress;
        if (string.IsNullOrEmpty(addr))
            onError?.Invoke("Cüzdan bağlı değil");
        else
            StartCoroutine(GetBalanceRoutine(addr, onBalance, onError));
    }

    IEnumerator GetBalanceRoutine(string ownerAddress, Action<decimal> onBalance, Action<string> onError)
    {
        if (config == null || string.IsNullOrEmpty(config.tokenContractAddress))
        {
            onBalance?.Invoke(0m);
            yield break;
        }
        string data = "0x70a08231" + ownerAddress.Replace("0x", "").PadLeft(64, '0').ToLowerInvariant();
        string fullJson = "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"eth_call\",\"params\":[{\"to\":\"" + config.tokenContractAddress + "\",\"data\":\"" + data + "\"},\"latest\"]}";
        using (var req = new UnityWebRequest(config.rpcUrl, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(fullJson));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(req.error);
                yield break;
            }
            string resultJson = req.downloadHandler.text;
            try
            {
                var resp = JsonUtility.FromJson<EthCallResponse>(resultJson);
                if (resp.result != null && resp.result.Length >= 2)
                {
                    string hex = resp.result.StartsWith("0x") ? resp.result.Substring(2) : resp.result;
                    var bi = BigInteger.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                    int d = config.tokenDecimals;
                    decimal balance = (decimal)bi;
                    for (int i = 0; i < d; i++) balance /= 10m;
                    onBalance?.Invoke(balance);
                }
                else
                    onBalance?.Invoke(0m);
            }
            catch (Exception e)
            {
                onError?.Invoke(e.Message);
            }
        }
    }

    [Serializable]
    class EthCallResponse { public string result; }

    /// <summary>Dağıtıcı cüzdana token transferi isteği. OnTransferRequested aboneleri (WalletConnect vb.) işlemi imzalar ve gönderir.</summary>
    public void TransferToDistributorAsync(decimal amount, Action<bool> onResult)
    {
        if (config == null || string.IsNullOrEmpty(config.distributorAddress))
        {
            onResult?.Invoke(true);
            return;
        }
        if (string.IsNullOrEmpty(WalletAddress))
        {
            onResult?.Invoke(false);
            return;
        }
        decimal scale = 1m;
        for (int i = 0; i < config.tokenDecimals; i++) scale *= 10m;
        var wei = (BigInteger)(amount * scale);
        string amountWei = "0x" + wei.ToString("x");
        if (OnTransferRequested != null)
            OnTransferRequested.Invoke(config.distributorAddress, amountWei, config.tokenContractAddress, onResult ?? (_ => { }));
        else
            onResult?.Invoke(true);
    }
}
