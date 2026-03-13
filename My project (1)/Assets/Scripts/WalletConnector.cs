using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Reown.AppKit.Unity;

public class WalletConnector : MonoBehaviour
{
    [Header("UI")]
    public Button button;
    public Text   buttonLabel;
    public Text   statusText;
    public Image  buttonBg;

    private static readonly Color CIdle      = new Color(0f,   1f,   0.31f, 1f);
    private static readonly Color CConnecting = new Color(1f,   0.9f, 0f,    1f);
    private static readonly Color CConnected  = new Color(0f,   1f,   0.5f,  1f);

    private bool _busy;

    private static readonly string[] Frames =
        { "[ CONNECT_WALLET ]", "[ CONNECT.WALLET ]", "[ C0NNECT_WALL3T ]", "[ CONNECT_WALLET ]" };

    void Start()
    {
        if (!button) button = GetComponent<Button>();
        if (button)  button.onClick.AddListener(OnClick);
        AppKit.AccountConnected    += OnConnected;
        AppKit.AccountDisconnected += OnDisconnected;
        ResetUI();
    }

    void OnDestroy()
    {
        AppKit.AccountConnected    -= OnConnected;
        AppKit.AccountDisconnected -= OnDisconnected;
    }

    public void OnClick()
    {
        if (_busy) return;
        _busy = true;
        if (button) button.interactable = false;
        StartCoroutine(ConnectCo());
    }

    IEnumerator ConnectCo()
    {
        string[] lines = { "> initializing secure channel...", "> scanning wallet providers...", "> opening modal..." };
        for (int i = 0; i < 4; i++)
        {
            SetLabel(Frames[i % Frames.Length]);
            SetColors(CConnecting, new Color(0f, 0.8f, 0f, 0.07f));
            if (i < lines.Length) SetStatus(lines[i]);
            yield return new WaitForSeconds(0.4f);
        }
        try { AppKit.OpenModal(); }
        catch (System.Exception e)
        {
            Debug.LogError($"[WalletConnector] {e.Message}");
            _busy = false;
            if (button) button.interactable = true;
            ResetUI();
        }
    }

    void OnConnected(object sender, Connector.AccountConnectedEventArgs e)
    {
        _busy = false;
        if (button) button.interactable = true;
        string addr = AppKit.AccountController?.Address ?? "";
        string s = addr.Length > 10 ? $"{addr[..6]}...{addr[^4..]}" : addr;
        SetLabel($"[ {s} ]");
        SetStatus("> access granted.");
        SetColors(CConnected, new Color(0f, 1f, 0.3f, 0.10f));
    }

    void OnDisconnected(object sender, Connector.AccountDisconnectedEventArgs e)
    {
        _busy = false;
        if (button) button.interactable = true;
        ResetUI();
    }

    void ResetUI()
    {
        SetLabel("[ CONNECT_WALLET ]");
        SetStatus("CLICK TO AUTHENTICATE");
        SetColors(CIdle, new Color(0, 0, 0, 0));
    }

    void SetLabel(string t)  { if (buttonLabel) buttonLabel.text  = t; }
    void SetStatus(string t) { if (statusText)  statusText.text   = t; }
    void SetColors(Color l, Color b) { if (buttonLabel) buttonLabel.color = l; if (buttonBg) buttonBg.color = b; }
}
