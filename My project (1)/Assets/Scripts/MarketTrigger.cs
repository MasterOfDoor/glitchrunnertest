using UnityEngine;

/// <summary>
/// M ile marketi açar/kapatır. İsteğe bağlı: sadece robota yakındayken veya her zaman.
/// </summary>
public class MarketTrigger : MonoBehaviour
{
    public KeyCode openKey = KeyCode.M;
    [Tooltip("Açıksa M her yerde çalışır. Kapalıysa sadece oyuncu yakındayken.")]
    public bool openWithoutRange = true;
    [Tooltip("Oyuncu (Tag=Player). Mesafe kontrolü için.")]
    public Transform player;
    public float interactDistance = 4f;

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (MarketUI.Instance == null || openKey == KeyCode.None || !Input.GetKeyDown(openKey)) return;

        bool inRange = openWithoutRange || (player != null && Vector2.Distance(transform.position, player.position) <= interactDistance);

        // Sadece kapalıyken ve menzildeyken aç (kapatma MarketUI'da M/Escape ile)
        if (!MarketUI.Instance.IsVisible && inRange)
            MarketUI.Instance.Show();
    }

    public void OpenMarket()
    {
        if (MarketUI.Instance != null) MarketUI.Instance.Show();
    }
}
