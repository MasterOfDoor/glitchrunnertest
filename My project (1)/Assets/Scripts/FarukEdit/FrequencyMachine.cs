using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Robot_Variant prefabına ekle. AudioSource zaten mevcut.
///
/// HUD KURULUM (Screen Space - Overlay Canvas):
///   FrequencyHUD
///   ├── HUDRoot          ← başta inactive, bu scriptte açılır
///   │   ├── FreqText     ← TMP  "~ 440.0 Hz"
///   │   └── StatusText   ← TMP  "░ SİNYAL ZAYIF"
///
/// PREFAB KURULUM:
///   Robot_Variant
///   ├── AudioSource (zaten var)
///   └── FrequencyMachine.cs (ekle)
///       RobotDialogue referansını Inspector'dan bağla
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class FrequencyMachine : MonoBehaviour
{
    // ── Ses ─────────────────────────────────────────────────
    [Header("Ses")]
    [SerializeField] AudioClip robotClip;
    [SerializeField] float targetFrequency   = 440f;
    [SerializeField] float freqMin           = 200f;
    [SerializeField] float freqMax           = 800f;
    [SerializeField] float scrollSensitivity = 25f;

    // ── Mesafe ───────────────────────────────────────────────
    [Header("Mesafe")]
    [SerializeField] string playerTag    = "Player";
    [SerializeField] float  hearDist     = 8f;   // ses başlar
    [SerializeField] float  interactDist = 2.5f; // HUD açılır

    // ── Hassasiyet ───────────────────────────────────────────
    [Header("Hassasiyet")]
    [SerializeField] float lockRange   = 15f;
    [SerializeField] float unlockRange = 5f;

    // ── HUD ─────────────────────────────────────────────────
    [Header("HUD (Screen Space Canvas)")]
    [SerializeField] GameObject hudRoot;
    [SerializeField] TMP_Text   freqText;
    [SerializeField] TMP_Text   statusText;

    // ── Referanslar ──────────────────────────────────────────
    [Header("Bağlantılar")]
    [SerializeField] RobotDialogue robotDialogue; // aynı objedeki RobotDialogue

    // ── İç durum ─────────────────────────────────────────────
    AudioSource _audio;
    Transform   _player;
    float       _currentFreq;
    bool        _hudVisible;
    bool        _unlocked;

    // Renkler
    static readonly Color ColWeak   = new Color(1f,  0.35f, 0.1f);
    static readonly Color ColClose  = new Color(1f,  0.9f,  0.1f);
    static readonly Color ColLocked = new Color(0f,  1f,    0.4f);

    // ════════════════════════════════════════════════════════
    void Awake()
    {
        _audio             = GetComponent<AudioSource>();
        _audio.loop        = true;
        _audio.spatialBlend = 0f;
        _audio.volume      = 0f;
        _audio.pitch       = 1f;

        if (robotClip != null)
        {
            _audio.clip = robotClip;
            _audio.Play();
        }

        _currentFreq = (freqMin + freqMax) * 0.5f;

        SetHUD(false);
    }

    void Start()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (go != null) _player = go.transform;
    }

    // ════════════════════════════════════════════════════════
    void Update()
    {
        if (_unlocked) return;

        // Oyuncuyu bul
        if (_player == null)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go == null) return;
            _player = go.transform;
        }

        float dist = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(_player.position.x,   _player.position.y));

        HandleVolume(dist);
        HandleHUD(dist);

        if (_hudVisible)
        {
            HandleScroll();
            UpdateHUD();
            CheckUnlock();
        }
    }

    // ── Yaklaştıkça ses artar ────────────────────────────────
    void HandleVolume(float dist)
    {
        if (robotClip == null) return;

        float targetVol = 0f;

        if (dist <= hearDist)
        {
            // hearDist → 0 arası lineer artış
            targetVol = 1f - Mathf.Clamp01(dist / hearDist);
            targetVol = Mathf.Max(targetVol, 0.05f); // minimum gürültü

            // Interactdist dışındayken pitch bozuk (parazit hissi)
            if (dist > interactDist)
                _audio.pitch = 1f + Mathf.Sin(Time.time * 8f) * 0.04f;
        }

        _audio.volume = Mathf.Lerp(_audio.volume, targetVol, Time.deltaTime * 4f);
    }

    // ── HUD aç/kapat ─────────────────────────────────────────
    void HandleHUD(float dist)
    {
        bool shouldOpen = dist <= interactDist;

        if (shouldOpen && !_hudVisible)  SetHUD(true);
        if (!shouldOpen && _hudVisible)  SetHUD(false);
    }

    void SetHUD(bool state)
    {
        _hudVisible = state;
        if (hudRoot != null) hudRoot.SetActive(state);
    }

    // ── Scroll → frekans ─────────────────────────────────────
    void HandleScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        _currentFreq += scroll * scrollSensitivity * 100f;
        _currentFreq  = Mathf.Clamp(_currentFreq, freqMin, freqMax);

        // Pitch: scroll'a göre gerçek zamanlı
        _audio.pitch = _currentFreq / targetFrequency;
    }

    // ── HUD güncelle ─────────────────────────────────────────
    // cache — gereksiz TMP rebuild önler
    float  _lastFreq   = -1f;
    string _lastStatus = "";

    void UpdateHUD()
    {
        float  diff   = Mathf.Abs(_currentFreq - targetFrequency);
        // Türkçe locale virgül üretmesin → InvariantCulture
        string freqStr = "~ " + _currentFreq.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + " Hz";

        Color  col;
        string status;

        if (diff <= unlockRange)      { status = ">> KİLİT AÇILDI"; col = ColLocked; }
        else if (diff <= lockRange)   { status = "~~ SİNYAL YAKIN"; col = ColClose;  }
        else                          { status = ".. SİNYAL ZAYIF"; col = ColWeak;   }

        // Sadece değer değişince yaz — sürekli rebuild önlenir
        if (freqText != null && _currentFreq != _lastFreq)
        {
            freqText.text  = freqStr;
            freqText.color = col;
            _lastFreq      = _currentFreq;
        }

        if (statusText != null && status != _lastStatus)
        {
            statusText.text  = status;
            statusText.color = col;
            _lastStatus      = status;
        }
    }

    // ── Kilit kontrolü ───────────────────────────────────────
    void CheckUnlock()
    {
        float diff = Mathf.Abs(_currentFreq - targetFrequency);
        if (diff <= unlockRange)
            StartCoroutine(UnlockSequence());
    }

    // ── Kilit açılma sekansı ─────────────────────────────────
    IEnumerator UnlockSequence()
    {
        if (_unlocked) yield break;
        _unlocked = true;

        // RobotDialogue'u durdur, "RAMON!" dedirt
        if (robotDialogue != null)
        {
            robotDialogue.StopAllCoroutines();
            // Balonda "RAMON!" göster
            if (robotDialogue.bubbleUI   != null) robotDialogue.bubbleUI.SetActive(true);
            if (robotDialogue.bubbleText != null) robotDialogue.bubbleText.text = "RAMON!";
        }

        // HUD'da kilit mesajı
        if (statusText != null) { statusText.text = ">> KİLİT AÇILDI"; statusText.color = ColLocked; }

        // Pitch ve volume yavaşça düş
        float t = 0f;
        float startPitch  = _audio.pitch;
        float startVolume = _audio.volume;

        while (t < 1f)
        {
            t += Time.deltaTime / 2.5f;   // 2.5 saniyede söner
            _audio.pitch  = Mathf.Lerp(startPitch,  0.1f, t);
            _audio.volume = Mathf.Lerp(startVolume, 0f,   t);
            yield return null;
        }

        _audio.Stop();
        SetHUD(false);

        // Balonu kapat
        if (robotDialogue != null && robotDialogue.bubbleUI != null)
            robotDialogue.bubbleUI.SetActive(false);
    }

    // ── Gizmo ────────────────────────────────────────────────
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, hearDist);
        Gizmos.color = new Color(0f, 1f, 0.4f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, interactDist);
    }
#endif
}