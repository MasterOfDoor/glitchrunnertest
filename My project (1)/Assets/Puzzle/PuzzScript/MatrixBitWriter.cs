using UnityEngine;
using TMPro;
using System.Text;

/// <summary>
/// Arka planda matrix animasyonu çizer.
/// Animasyon bitince OnFinished callback'i tetiklenir.
/// Time.unscaledDeltaTime kullanır — timeScale=0 ile çalışır.
/// </summary>
public class MatrixBitWriter : MonoBehaviour
{
    [Header("Referanslar")]
    public TMP_Text textArea;

    [Header("Grid Boyutu")]
    public int columns        = 70;
    public int rows           = 14;

    [Header("Animasyon Hızı")]
    public float rowFillInterval = 0.05f;

    // Callback — PuzzleManager tarafından atanır
    public System.Action OnFinished;

    // ── Private ───────────────────────────────────────────────────────────
    private char[,]  _buffer;
    private bool[,]  _mask;
    private int      _currentRow;
    private float    _timer;
    private bool     _finished;
    private StringBuilder _sb;

    // Renk havuzu — daha canlı cyberpunk görünüm
    private static readonly string[] Colors =
    {
        "#00FF88", "#00CC66", "#FFDD00", "#FF5500", "#00E5FF"
    };

    // ─────────────────────────────────────────────────────────────────────
    void OnEnable()
    {
        // Her açılışta sıfırla (puzzle tekrar oynanabilir)
        Init();
    }

    void Init()
    {
        _buffer     = new char[rows, columns];
        _mask       = new bool[rows, columns];
        _sb         = new StringBuilder(rows * (columns + 30));
        _finished   = false;
        _currentRow = rows - 1;
        _timer      = 0f;

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                _buffer[r, c] = ' ';

        GenerateMask();
        Render();
    }

    void Update()
    {
        if (_finished) return;

        _timer += Time.unscaledDeltaTime;
        if (_timer < rowFillInterval) return;

        _timer = 0f;
        FillRow(_currentRow);
        Render();

        _currentRow--;

        if (_currentRow < 0)
        {
            _finished = true;
            OnFinished?.Invoke();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    void FillRow(int r)
    {
        for (int c = 0; c < columns; c++)
        {
            _buffer[r, c] = _mask[r, c]
                ? (Random.value > 0.75f ? '1' : '0')
                : ' ';
        }
    }

    void Render()
    {
        _sb.Clear();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (_mask[r, c] && _buffer[r, c] != ' ')
                {
                    // Rengi satır ve sütuna göre varyasyon ekle
                    string col = Colors[(r + c) % Colors.Length];
                    _sb.Append("<color=").Append(col).Append(">")
                       .Append(_buffer[r, c])
                       .Append("</color>");
                }
                else
                {
                    _sb.Append(' ');
                }
            }
            _sb.Append('\n');
        }

        if (textArea != null)
            textArea.text = _sb.ToString();
    }

    void GenerateMask()
    {
        // "FIX THIS INDEX" yazısı — ASCII art
        string[] lines =
        {
            "FFF  I  X X    TTT  H  H  EEEE     I  N  N  DDD  EEEE  X X",
            "F    I  X X     T   H  H  E        I  NN N  D  D E     X X",
            "FFF  I   X      T   HHHH  EEE      I  N NN  D  D EEE    X ",
            "F    I  X X     T   H  H  E        I  N  N  D  D E     X X",
            "F    I  X X     T   H  H  EEEE     I  N  N  DDD  EEEE  X X"
        };

        int startRow = (rows - lines.Length) / 2;
        int startCol = 2;

        for (int r = 0; r < lines.Length; r++)
        {
            for (int c = 0; c < lines[r].Length && startCol + c < columns; c++)
            {
                if (startRow + r < rows && lines[r][c] != ' ')
                    _mask[startRow + r, startCol + c] = true;
            }
        }
    }
}