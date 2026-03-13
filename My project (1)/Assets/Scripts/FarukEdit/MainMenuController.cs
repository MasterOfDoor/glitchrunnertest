using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// ÖNEMLI: Bu script MenuCanvas objesine eklenmeli — BootPanel'e değil!
/// MenuCanvas'ta olursa bootPanel.SetActive(false) güvenle çalışır.

public class MainMenuController : MonoBehaviour
{
    [Header("Boot Sekansı")]
    [SerializeField] TMP_Text   bootText;
    [SerializeField] float      charDelay = 0.025f;
    [SerializeField] float      lineDelay = 0.35f;

    [Header("Paneller")]
    [SerializeField] GameObject bootPanel;
    [SerializeField] GameObject menuPanel;

    [Header("Başlık")]
    [SerializeField] TMP_Text   titleText;
    [SerializeField] TMP_Text   subText;
    [SerializeField] TMP_Text   cursorBlink;

    [Header("Buton Etiketleri")]
    [SerializeField] TMP_Text   lblStart;
    [SerializeField] TMP_Text   lblContinue;
    [SerializeField] TMP_Text   lblSettings;
    [SerializeField] TMP_Text   lblQuit;

    [Header("Matrix Arka Plan (opsiyonel)")]
    [SerializeField] TMP_Text   matrixText;
    [SerializeField] int        matrixCols = 60;
    [SerializeField] int        matrixRows = 24;

    [Header("TV / Fade Efekti")]
    [SerializeField] Image      fadeImage;
    [SerializeField] float      tvDuration = 0.45f;

    [Header("Sahne")]
    [SerializeField] string     gameSceneName = "OgreticiSahne";

    // ── Renkler ──────────────────────────────────────────────
    static readonly Color ColCyan    = new Color(0f, 0.898f, 1f,    1f);
    static readonly Color ColGreen   = new Color(0f, 1f,    0.533f, 1f);
    static readonly Color ColOrange  = new Color(1f, 0.333f, 0f,    1f);
    static readonly Color ColWarning = new Color(1f, 0.8f,   0f,    1f);

    // ── Boot satırları ───────────────────────────────────────
    readonly (string text, float pause)[] _bootLines =
    {
        ("> GLITCHRUNNER_OS v0.1",                 0.25f),
        ("> BIOS INIT..................... OK",     0.08f),
        ("> CPU CHECK.................... OK",     0.08f),
        ("> RAM CHECK.................... FAILED", 0.45f),
        ("> ATTEMPTING RECOVERY......... ...",     0.70f),
        ("> MEMORY SECTOR 7: CORRUPTED",           0.20f),
        ("> UNKNOWN ENTITY DETECTED IN SECTOR 7",  0.55f),
        ("> LOADING ENVIRONMENT......... OK",      0.15f),
        ("> WARNING: SECURITY BREACH ACTIVE",      0.25f),
        ("> LAUNCHING INTERFACE...",               0.55f),
    };

    readonly string[] _menuLabels = { "START", "CONTINUE", "SETTINGS", "QUIT" };

    TMP_Text[] _labels;
    Button[]   _buttons;

    int   _selectedIndex;
    bool  _menuActive;
    // Menü açıldıktan sonra kısa süre input kilidi — EventSystem spurious Submit önler
    float _inputUnlockTime;

    float _blinkTimer;
    float _glitchTimer;
    bool  _blinkOn = true;

    const string Title       = "GLITCHRUNNER";
    const string GlitchChars = "01#@$%!?";

    // ════════════════════════════════════════════════════════
    void Awake()
    {
        _labels  = new TMP_Text[] { lblStart, lblContinue, lblSettings, lblQuit };
        _buttons = new Button[4];

        for (int i = 0; i < 4; i++)
        {
            if (_labels[i] == null) continue;
            _buttons[i] = _labels[i].GetComponent<Button>();
            if (_buttons[i] == null)
                _buttons[i] = _labels[i].GetComponentInParent<Button>(true);
        }

        // Capture index için local var kullan
        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            _buttons[idx]?.onClick.AddListener(() => OnSelect(idx));
        }

        // Başlangıç durumu
        if (bootPanel  != null) bootPanel.SetActive(true);
        if (menuPanel  != null) menuPanel.SetActive(false);
        if (bootText   != null) bootText.text = "";
        if (cursorBlink!= null) cursorBlink.text = "";

        // FadeImage başta opak siyah — BootSequence fade-in yapacak
        if (fadeImage != null)
        {
            fadeImage.color = Color.black;
            SetAlpha(fadeImage, 1f);
        }
    }

    void Start()
    {
        StartCoroutine(BootSequence());
        if (matrixText != null)
            StartCoroutine(MatrixLoop());
    }

    // ════════════════════════════════════════════════════════
    void Update()
    {
        if (!_menuActive) return;
        if (Time.unscaledTime < _inputUnlockTime) return; // input kilidi

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            Navigate(1);
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            Navigate(-1);
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            OnSelect(_selectedIndex);

        // Cursor blink
        _blinkTimer += Time.unscaledDeltaTime;
        if (_blinkTimer >= 0.5f)
        {
            _blinkTimer = 0f;
            _blinkOn = !_blinkOn;
            if (cursorBlink != null)
                cursorBlink.text = _blinkOn ? "█" : " ";
        }

        // Başlık glitch
        _glitchTimer += Time.unscaledDeltaTime;
        if (_glitchTimer >= 4f)
        {
            _glitchTimer = 0f;
            StartCoroutine(GlitchTitle());
        }
    }

    // ── Boot sekansı ─────────────────────────────────────────
    IEnumerator BootSequence()
    {
        // Siyah ekrandan fade-in
        if (fadeImage != null)
            yield return StartCoroutine(FadeAlpha(fadeImage, 1f, 0f, 0.5f));
        else
            yield return new WaitForSecondsRealtime(0.4f);

        string accumulated = "";

        foreach (var (text, pause) in _bootLines)
        {
            string open, close;
            GetLineColor(text, out open, out close);

            string building = open;
            for (int c = 0; c < text.Length; c++)
            {
                building += text[c];
                if (bootText != null)
                    bootText.text = accumulated + building + close;
                yield return new WaitForSecondsRealtime(charDelay);
            }

            accumulated += open + text + close + "\n";
            if (bootText != null) bootText.text = accumulated;
            yield return new WaitForSecondsRealtime(pause);
        }

        yield return new WaitForSecondsRealtime(0.2f);
        yield return StartCoroutine(TVEffect());
    }

    void GetLineColor(string text, out string open, out string close)
    {
        close = "</color>";
        if (text.Contains("FAILED") || text.Contains("BREACH") || text.Contains("CORRUPTED"))
            open = "<color=#FF1040>";
        else if (text.Contains("WARNING") || text.Contains("UNKNOWN") || text.Contains("RECOVERY"))
            open = "<color=#FF5500>";
        else if (text.Contains(" OK"))
            open = "<color=#00FF88>";
        else
            open = "<color=#00E5FF>";
    }

    // ── TV Efekti ─────────────────────────────────────────────
    // Controller artık MenuCanvas'ta → bootPanel.SetActive(false) güvenli!
    IEnumerator TVEffect()
    {
        // 1) BootPanel'i kapat (artık güvenli — controller burada değil)
        if (bootPanel != null) bootPanel.SetActive(false);

        // 2) Beyaz flaş
        if (fadeImage != null)
        {
            fadeImage.color = Color.white;
            SetAlpha(fadeImage, 0.9f);
            yield return new WaitForSecondsRealtime(0.06f);
            yield return StartCoroutine(FadeAlpha(fadeImage, 0.9f, 0f, 0.12f));
            // Color.black.a = 1 olur — sadece rengi değiştir, alpha 0 kalsın
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
        }

        // 3) MenuPanel'i scale=0.01 (yatay çizgi) ile aç
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
            menuPanel.transform.localScale = new Vector3(1f, 0.01f, 1f);
        }

        // 4) Scale Y'yi 0→1 aç (TV açılma animasyonu)
        float elapsed    = 0f;
        float expandTime = tvDuration * 0.6f; // ~0.27s
        while (elapsed < expandTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float ratio = Mathf.Clamp01(elapsed / expandTime);
            float y     = Mathf.Sin(ratio * Mathf.PI * 0.5f);
            // Hafif overshoot
            if (ratio > 0.85f)
                y = 1f + Mathf.Sin((ratio - 0.85f) / 0.15f * Mathf.PI) * 0.04f;
            if (menuPanel != null)
                menuPanel.transform.localScale = new Vector3(1f, Mathf.Max(0.01f, y), 1f);
            yield return null;
        }
        if (menuPanel != null)
            menuPanel.transform.localScale = Vector3.one;

        // 5) Kısa cyan flaş (tarama çizgisi)
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0f, 0.9f, 1f, 1f);
            SetAlpha(fadeImage, 0.12f);
            yield return StartCoroutine(FadeAlpha(fadeImage, 0.12f, 0f, 0.2f));
            // Color.black.a = 1 olur — alpha 0'da bırak
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
        }

        OpenMenu();
    }

    // ── Menü ─────────────────────────────────────────────────
    void OpenMenu()
    {
        // EventSystem seçimini temizle — InputSystem'in spurious Submit önlenir
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // Input'u 0.3 saniye kilitle
        _inputUnlockTime = Time.unscaledTime + 0.3f;

        _menuActive    = true;
        _selectedIndex = 0;

        if (titleText != null) { titleText.text = Title;                        titleText.color = ColCyan;   }
        if (subText   != null) { subText.text   = "v0.1 // SYSTEM COMPROMISED"; subText.color   = ColOrange; }

        RefreshButtons();
    }

    void RefreshButtons()
    {
        for (int i = 0; i < _labels.Length; i++)
        {
            if (_labels[i] == null) continue;
            bool sel = i == _selectedIndex;
            _labels[i].text  = (sel ? ">> " : ">  ") + _menuLabels[i];
            _labels[i].color = sel ? ColGreen : ColCyan;
        }
    }

    void Navigate(int dir)
    {
        _selectedIndex = (_selectedIndex + dir + _labels.Length) % _labels.Length;
        RefreshButtons();
    }

    void OnSelect(int index)
    {
        if (!_menuActive) return;
        if (Time.unscaledTime < _inputUnlockTime) return; // kilitteyse yoksay
        _menuActive = false;

        switch (index)
        {
            case 0: StartCoroutine(LoadScene(gameSceneName)); break;
            case 1: StartCoroutine(FlashMsg(lblContinue, "NO SAVE DATA"));      break;
            case 2: StartCoroutine(FlashMsg(lblSettings, "COMING SOON")); break;
            case 3: StartCoroutine(QuitRoutine()); break;
        }
    }

    // ── Sahne geçişi ─────────────────────────────────────────
    IEnumerator LoadScene(string scene)
    {
        // TV kapanma (scale Y → 0)
        if (menuPanel != null)
        {
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.unscaledDeltaTime;
                float y = 1f - Mathf.Clamp01(t / 0.3f);
                menuPanel.transform.localScale = new Vector3(1f, Mathf.Max(0.01f, y), 1f);
                yield return null;
            }
            menuPanel.transform.localScale = new Vector3(1f, 0.01f, 1f);
        }

        // Fade to black
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            yield return StartCoroutine(FadeAlpha(fadeImage, 0f, 1f, 0.4f));
        }
        else yield return new WaitForSecondsRealtime(0.4f);

        SceneManager.LoadScene(scene);
    }

    IEnumerator QuitRoutine()
    {
        if (titleText != null) titleText.text = "SHUTTING DOWN...";
        yield return new WaitForSecondsRealtime(0.7f);
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            yield return StartCoroutine(FadeAlpha(fadeImage, 0f, 1f, 0.4f));
        }
        Application.Quit();
    }

    IEnumerator FlashMsg(TMP_Text label, string msg)
    {
        _menuActive = true;
        if (label == null) yield break;
        string orig = label.text; Color origCol = label.color;
        label.text = ">  " + msg; label.color = ColWarning;
        yield return new WaitForSecondsRealtime(1.2f);
        label.text = orig; label.color = origCol;
    }

    // ── Başlık glitch ────────────────────────────────────────
    IEnumerator GlitchTitle()
    {
        if (titleText == null) yield break;
        for (int i = 0; i < 6; i++)
        {
            char[] arr = Title.ToCharArray();
            arr[Random.Range(0, arr.Length)] = GlitchChars[Random.Range(0, GlitchChars.Length)];
            titleText.text = new string(arr);
            yield return new WaitForSecondsRealtime(0.05f);
        }
        titleText.text = Title;
    }

    // ── Matrix arka plan ─────────────────────────────────────
    IEnumerator MatrixLoop()
    {
        var sb = new System.Text.StringBuilder();
        string[] cols = { "<color=#00E5FF18>", "<color=#00FF8818>", "<color=#00CC6618>" };
        string   end  = "</color>";
        char[]   pool = { '0', '1', ' ', ' ', ' ' };

        while (true)
        {
            sb.Clear();
            for (int r = 0; r < matrixRows; r++)
            {
                for (int c = 0; c < matrixCols; c++)
                {
                    char ch = pool[Random.Range(0, pool.Length)];
                    sb.Append(ch != ' '
                        ? cols[Random.Range(0, cols.Length)] + ch + end
                        : " ");
                }
                sb.Append('\n');
            }
            if (matrixText != null) matrixText.text = sb.ToString();
            yield return new WaitForSecondsRealtime(0.15f);
        }
    }

    // ── Yardımcılar ──────────────────────────────────────────
    void SetAlpha(Graphic g, float a)
    {
        if (g == null) return;
        Color c = g.color; c.a = a; g.color = c;
    }

    IEnumerator FadeAlpha(Graphic g, float from, float to, float duration)
    {
        if (g == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(g, Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetAlpha(g, to);
    }
}