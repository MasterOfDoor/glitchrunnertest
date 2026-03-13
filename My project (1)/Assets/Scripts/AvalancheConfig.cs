using UnityEngine;

/// <summary>
/// Avalanche Fuji (Testnet) ve ERC-20 token ayarları.
/// Dağıtıcı cüzdan adresi tüm market ödemelerinin gideceği adres.
/// </summary>
[CreateAssetMenu(fileName = "AvalancheConfig", menuName = "Game/Avalanche Config")]
public class AvalancheConfig : ScriptableObject
{
    [Header("Avalanche Fuji Testnet")]
    public string rpcUrl = "https://api.avax-test.network/ext/bc/C/rpc";
    public int chainId = 43113;

    [Header("ERC-20 Token")]
    [Tooltip("Oyun coin token sözleşme adresi (Fuji üzerinde).")]
    public string tokenContractAddress = "";

    [Tooltip("Token decimals (örn. 18).")]
    public int tokenDecimals = 18;

    [Header("Dağıtıcı Cüzdan")]
    [Tooltip("Market ödemelerinin gideceği adres.")]
    public string distributorAddress = "";
}
