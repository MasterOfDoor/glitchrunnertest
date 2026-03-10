using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Escape point'e (bu objenin trigger'ına) oyuncu gelince ekran kararır, SpawnPoint'te canlanır, ekran açılır.
/// Bu scripti EscapePoint objesine ekle; objeye BoxCollider2D ekleyip Is Trigger işaretle.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EscapePointRespawn : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Boş bırakırsan 'SpawnPoint' isimli objeyi arar.")]
    public Transform spawnPoint;
    [Tooltip("Boş bırakırsan 'Player' tag'li veya AsılScript/PlayerControllerFemale taşıyan objeyi arar.")]
    public Transform player;
    [Tooltip("Spawn + fade-in bittikten sonra aktif edilecek ikinci robot (SecondTutorialRobot). Başta inactive olmalı.")]
    public GameObject secondRobotToEnable;

    [Header("Fade")]
    public float fadeOutDuration = 0.6f;
    public float fadeInDuration = 0.6f;
    public Color fadeColor = Color.black;

    private bool _isFading;
    private GameObject _fadeRoot;
    private Image _fadeImage;

    void Start()
    {
        if (spawnPoint == null)
        {
            var sp = GameObject.Find("SpawnPoint");
            if (sp != null) spawnPoint = sp.transform;
        }
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
            if (player == null)
            {
                var asil = FindObjectOfType<AsılScript>();
                if (asil != null) player = asil.transform;
                if (player == null)
                {
                    var female = FindObjectOfType<PlayerControllerFemale>();
                    if (female != null) player = female.transform;
                }
            }
        }

        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_isFading) return;
        if (spawnPoint == null) return;

        Transform root = other.transform.root;
        if (player != null && root != player) return;
        if (player == null && root.GetComponent<AsılScript>() == null && root.GetComponent<PlayerControllerFemale>() == null) return;

        if (root.GetComponent<AsılScript>() != null || root.GetComponent<PlayerControllerFemale>() != null)
            StartCoroutine(FadeAndRespawn(root));
    }

    IEnumerator FadeAndRespawn(Transform playerTransform)
    {
        _isFading = true;

        EnsureFadeCanvas();
        if (_fadeImage == null) { _isFading = false; yield break; }

        MonoBehaviour moveScript = playerTransform.GetComponent<AsılScript>();
        if (moveScript == null) moveScript = playerTransform.GetComponent<PlayerControllerFemale>();
        if (moveScript != null) moveScript.enabled = false;

        Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        yield return StartCoroutine(FadeTo(1f, fadeOutDuration));

        playerTransform.position = new Vector3(spawnPoint.position.x, spawnPoint.position.y, playerTransform.position.z);

        yield return StartCoroutine(FadeTo(0f, fadeInDuration));

        if (moveScript != null) moveScript.enabled = true;
        if (secondRobotToEnable != null) secondRobotToEnable.SetActive(true);
        _isFading = false;
    }

    IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (_fadeImage == null || duration <= 0f) yield break;
        float start = _fadeImage.color.a;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float a = Mathf.Lerp(start, targetAlpha, t);
            Color c = fadeColor;
            c.a = a;
            _fadeImage.color = c;
            yield return null;
        }
        Color final = fadeColor;
        final.a = targetAlpha;
        _fadeImage.color = final;
    }

    void EnsureFadeCanvas()
    {
        if (_fadeRoot != null) return;

        _fadeRoot = new GameObject("EscapeFadeCanvas");
        var canvas = _fadeRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;
        _fadeRoot.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _fadeRoot.AddComponent<GraphicRaycaster>();

        GameObject imgGo = new GameObject("FadeImage");
        imgGo.transform.SetParent(_fadeRoot.transform, false);
        _fadeImage = imgGo.AddComponent<Image>();
        _fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        var rect = imgGo.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
