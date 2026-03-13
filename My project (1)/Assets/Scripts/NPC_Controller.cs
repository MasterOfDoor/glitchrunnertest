using UnityEngine;
using TMPro;

public class NPC_Controller : MonoBehaviour
{
    [Header("Diyalog Ayarları")]
    public DialogueData diyalogVerisi; 
    public GameObject dialogCanvas;    
    public TextMeshProUGUI dialogText; 

    private int index = 0;
    private bool oyuncuYakin = false;

    void Start() 
    { 
        // Oyun başında konuşma balonunu gizle
        if(dialogCanvas != null) dialogCanvas.SetActive(false); 
    }

    void Update()
    {
        // "E" tuşuna basıldığında ve oyuncu menzildeyse
        if (oyuncuYakin && Input.GetKeyDown(KeyCode.E))
        {
            // --- TEST İÇİN LOG ---
            Debug.Log("E'ye basıldı ve oyuncu menzilde!"); 
            
            KonusmayiYonet();
        }
    }

    void KonusmayiYonet()
    {
        if (diyalogVerisi == null) 
        {
            Debug.LogError("Diyalog Verisi (Scriptable Object) atanmamış!");
            return;
        }

        if (dialogCanvas == null)
        {
            Debug.LogError("Dialog Canvas atanmamış! Inspector panelinden NPC scriptine Canvas'ı sürükle.");
            return;
        }

        // Eğer balon kapalıysa aç ve ilk cümleyi yazdır
        if (!dialogCanvas.activeSelf)
        {
            dialogCanvas.SetActive(true);
            index = 0;
            Yazdir();
        }
        else // Balon zaten açıksa bir sonraki cümleye geç
        {
            index++;
            if (index < diyalogVerisi.cumleler.Length) 
            {
                Yazdir();
            }
            else // Cümleler bittiyse kapat
            {
                dialogCanvas.SetActive(false);
            }
        }
    }

    void Yazdir() 
    { 
        if (dialogText != null)
            dialogText.text = diyalogVerisi.cumleler[index];
        else
            Debug.LogWarning("Dialog Text atanmamış!");
    }

    private void OnTriggerEnter2D(Collider2D other) 
    { 
        if(other.CompareTag("Player")) 
        {
            oyuncuYakin = true; 
            Debug.Log("Oyuncu menzile girdi.");
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    { 
        if(other.CompareTag("Player")) 
        { 
            oyuncuYakin = false; 
            dialogCanvas.SetActive(false); 
            Debug.Log("Oyuncu menzilden çıktı.");
        } 
    }
}