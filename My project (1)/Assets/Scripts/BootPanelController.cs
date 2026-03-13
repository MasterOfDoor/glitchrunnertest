using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BootPanelController : MonoBehaviour
{
    [Header("Panel")]
    public CanvasGroup bootPanelGroup;
    public Image       panelBackground;

    [Header("Log Satırları")]
    public Text[] bootLogTexts;   // 6 adet
    public Text   cursorText;

    [Header("Prompt + Input")]
    public Text        promptText;
    public InputField  inputField;
    public Text        statusText;  // optional: "AWAITING AUTHENTICATION...", "> opening wallet modal..."
    public WalletModalUI walletModalUI;

    [Header("Zamanlama")]
    public float openDelay = 0.5f;
    public float fadeSpeed = 3.0f;
    public float typeDelay = 0.036f;
    public float lineDelay = 0.16f;

    [Header("Glitch")]
    public float glitchInterval  = 5f;
    public float glitchMaxOffset = 0.8f;
    public float glitchDuration  = 0.07f;

    private static readonly string[] LogLines =
    {
        "> BLOCKCHAIN_OS v0.1 booting...",
        "> scanning peers............[47 FOUND]",
        "> verifying chain integrity.[OK]",
        "> loading smart contracts...[OK]",
        "> wallet module..............[LOCKED]",
        "> awaiting authentication...",
    };

    private static readonly Color CDefault  = new Color(0f,   0.55f, 0.15f, 0.80f);
    private static readonly Color COK       = new Color(0f,   0.85f, 0.25f, 0.90f);
    private static readonly Color CLocked   = new Color(0.9f, 0.80f, 0f,    0.90f);
    private static readonly Color CAwaiting = new Color(0f,   1f,    0.40f, 1.00f);
    private static readonly Color CPrompt   = new Color(1f,   0f,    0f,    1.00f);
    private static readonly Color CError    = new Color(1f,   0.15f, 0.05f, 1.00f);
    private static readonly Color CSuccess  = new Color(0f,   1f,    0.35f, 1.00f);

    private float         _target;
    private float         _glitchTimer;
    private RectTransform _bootRT;
    private Vector3       _origPos;
    private bool          _awaiting;
    private Coroutine     _blinkCo;

    void Awake()
    {
        if (!bootPanelGroup) { Debug.LogError("[BootPanel] bootPanelGroup atanmamış!"); return; }

        bootPanelGroup.alpha          = 0f;
        bootPanelGroup.interactable   = true;
        bootPanelGroup.blocksRaycasts = true;

        if (panelBackground) panelBackground.color = new Color(0f, 0.02f, 0.005f, 0.88f);

        _bootRT  = bootPanelGroup.GetComponent<RectTransform>();
        _origPos = _bootRT ? _bootRT.anchoredPosition3D : Vector3.zero;

        foreach (var t in bootLogTexts) if (t) t.text = "";
        if (cursorText) cursorText.text = "";

        if (promptText)  { promptText.text = "";  promptText.gameObject.SetActive(false); }
        if (statusText)  { statusText.text = "";  statusText.gameObject.SetActive(false); }
        if (inputField)
        {
            inputField.text = "";
            inputField.gameObject.SetActive(false);
            inputField.onEndEdit.AddListener(OnEndEdit);
        }
    }

    void Start()
    {
        if (!bootPanelGroup) return;
        StartCoroutine(OpenSeq());
        StartCoroutine(BlinkCursor());
    }

    void Update()
    {
        if (!bootPanelGroup) return;
        bootPanelGroup.alpha = Mathf.MoveTowards(bootPanelGroup.alpha, _target, fadeSpeed * Time.deltaTime);
        GlitchTick();
        if (_awaiting && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            CheckInput();
    }

    IEnumerator OpenSeq()
    {
        yield return new WaitForSeconds(openDelay);
        _target = 1f;
        yield return new WaitUntil(() => bootPanelGroup.alpha >= 0.92f);
        yield return StartCoroutine(TypeLog());
        yield return new WaitForSeconds(0.4f);
        ShowPrompt();
    }

    IEnumerator TypeLog()
    {
        foreach (var t in bootLogTexts) if (t) t.text = "";
        for (int i = 0; i < LogLines.Length; i++)
        {
            if (i >= bootLogTexts.Length) break;
            var lt = bootLogTexts[i];
            if (!lt) continue;
            bool last = i == LogLines.Length - 1;
            lt.color = last ? CAwaiting : GetColor(LogLines[i]);
            lt.text  = "";
            foreach (char c in LogLines[i]) { lt.text += c; yield return new WaitForSeconds(typeDelay); }
            yield return new WaitForSeconds(lineDelay);
        }
    }

    Color GetColor(string l)
    {
        if (l.Contains("[OK]"))     return COK;
        if (l.Contains("[LOCKED]")) return CLocked;
        return CDefault;
    }

    void ShowPrompt()
    {
        if (promptText)
        {
            promptText.gameObject.SetActive(true);
            promptText.color = CPrompt;
            promptText.text  = "PLEASE ENTER: CONNECT";
            _blinkCo = StartCoroutine(BlinkPrompt());
        }
        if (statusText)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = "AWAITING AUTHENTICATION...";
        }
        if (inputField)
        {
            inputField.gameObject.SetActive(true);
            inputField.text = "";
            inputField.ActivateInputField();
        }
        _awaiting = true;
    }

    IEnumerator BlinkPrompt()
    {
        while (_awaiting)
        {
            if (promptText) promptText.enabled = true;
            yield return new WaitForSeconds(0.55f);
            if (promptText) promptText.enabled = false;
            yield return new WaitForSeconds(0.22f);
        }
        if (promptText) promptText.enabled = true;
    }

    void OnEndEdit(string val)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            CheckInput();
    }

    void CheckInput()
    {
        if (!_awaiting) return;
        string typed = inputField ? inputField.text.Trim().ToLower() : "";
        if (typed == "connect")
        {
            _awaiting = false;
            if (_blinkCo != null) StopCoroutine(_blinkCo);
            StartCoroutine(ConnectSeq());
        }
        else StartCoroutine(ShowError());
    }

    IEnumerator ShowError()
    {
        _awaiting = false;
        if (_blinkCo != null) StopCoroutine(_blinkCo);
        if (promptText) { promptText.enabled = true; promptText.color = CError; promptText.text = "ERROR: INVALID COMMAND. TRY 'connect'"; }
        if (inputField) inputField.text = "";
        yield return new WaitForSeconds(1.5f);
        if (promptText) { promptText.color = CPrompt; promptText.text = "PLEASE ENTER: CONNECT"; _blinkCo = StartCoroutine(BlinkPrompt()); }
        if (inputField) inputField.ActivateInputField();
        _awaiting = true;
    }

    IEnumerator ConnectSeq()
    {
        if (promptText) { promptText.enabled = true; promptText.color = CSuccess; promptText.text = "> AUTHENTICATING..."; }
        if (inputField) inputField.gameObject.SetActive(false);
        if (statusText) { statusText.gameObject.SetActive(true); statusText.text = "> opening wallet modal..."; }
        yield return new WaitForSeconds(0.5f);
        if (walletModalUI) walletModalUI.OpenModal();
        else Debug.LogWarning("[BootPanel] walletModalUI atanmamış!");
    }

    IEnumerator BlinkCursor()
    {
        bool s = true;
        while (true) { if (cursorText) cursorText.text = s ? "█" : ""; s = !s; yield return new WaitForSeconds(0.5f); }
    }

    void GlitchTick()
    {
        if (!_bootRT) return;
        _glitchTimer += Time.deltaTime;
        if (_glitchTimer >= glitchInterval) { _glitchTimer = 0f; StartCoroutine(Glitch()); }
    }

    IEnumerator Glitch()
    {
        float e = 0f;
        while (e < glitchDuration)
        {
            _bootRT.anchoredPosition3D = _origPos + new Vector3(
                Random.Range(-glitchMaxOffset, glitchMaxOffset),
                Random.Range(-glitchMaxOffset * 0.5f, glitchMaxOffset * 0.5f), 0f);
            e += Time.deltaTime; yield return null;
        }
        _bootRT.anchoredPosition3D = _origPos;
    }
}
