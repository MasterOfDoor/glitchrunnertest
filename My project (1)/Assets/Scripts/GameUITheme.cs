using UnityEngine;

/// <summary>
/// Grafik tasarımcının Inspector'dan renk, font, boyut vb. düzenleyebilmesi için UI teması.
/// Market ve Envanter bu asset'i kullanır; atanmazsa varsayılan değerler kullanılır.
/// </summary>
[CreateAssetMenu(fileName = "GameUITheme", menuName = "Game/UI Theme")]
public class GameUITheme : ScriptableObject
{
    [Header("Panel")]
    public Color panelBackground = new Color(0.02f, 0.06f, 0.02f, 0.96f);
    public Color panelOutline = new Color(0f, 1f, 0.25f, 1f);
    public Vector2 panelOutlineDistance = new Vector2(2, 2);
    public Vector2 marketPanelSize = new Vector2(320, 300);
    public Vector2 inventoryPanelSize = new Vector2(300, 270);

    [Header("Başlık")]
    public Color titleColor = new Color(0f, 1f, 0.25f, 1f);
    public int titleFontSize = 18;

    [Header("Metin / İkincil")]
    public Color textColor = new Color(0f, 1f, 0.25f, 1f);
    public Color textDimColor = new Color(0f, 0.4f, 0.1f, 0.9f);
    public int bodyFontSize = 14;

    [Header("Satır / Slot")]
    public Color rowBackground = new Color(0.04f, 0.12f, 0.04f, 0.95f);
    public Color rowOutline = new Color(0f, 0.4f, 0.1f, 0.9f);
    public Vector2 rowOutlineDistance = new Vector2(1, 1);

    [Header("Buton")]
    public Color buttonColor = new Color(0f, 0.4f, 0.1f, 0.9f);
    public Color buttonTextColor = new Color(0f, 1f, 0.25f, 1f);
    public int buttonFontSize = 12;

    [Header("Referans çözünürlük (Canvas)")]
    public int referenceWidth = 640;
    public int referenceHeight = 360;

    [Header("Elle düzenlenebilir envanter")]
    [Tooltip("Atarsan envanter bu prefab'dan oluşturulur; prefab'ı sahnede veya projede istediğin gibi düzenleyebilirsin. Boş bırakırsan kodla oluşturulur.")]
    public GameObject inventoryPanelPrefab;
}
