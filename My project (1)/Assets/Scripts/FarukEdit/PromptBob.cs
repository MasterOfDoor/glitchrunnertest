using UnityEngine;

/// <summary>
/// PromptAnchor'ı Animator'ın kendi zamanını okuyarak
/// Robot4_Idle ile tam senkron hareket ettirir.
/// </summary>
public class PromptBob : MonoBehaviour
{
    [Header("Frame Y Offset'leri (local metre)")]
    [SerializeField] private float frame0Y =  0.000f;
    [SerializeField] private float frame1Y =  0.025f;
    [SerializeField] private float frame2Y =  0.040f;
    [SerializeField] private float frame3Y =  0.015f;

    [Header("Bağlantı")]
    [Tooltip("DataMinerNPC root'undaki Animator")]
    [SerializeField] private Animator animator;

    [Tooltip("Animator'daki Idle state adı (Base Layer.Robot4_Idle gibi)")]
    [SerializeField] private string idleStateName = "Robot4_Idle";

    [Header("Fallback — Animator yoksa kullan")]
    [SerializeField] private float fps        = 12f;
    [SerializeField] private int   frameCount = 4;

    private Vector3 _baseLocalPos;
    private int     _stateHash;

    void Start()
    {
        _baseLocalPos = transform.localPosition;
        _stateHash    = Animator.StringToHash(idleStateName);

        // Animator verilmediyse parent'ta ara
        if (animator == null)
            animator = GetComponentInParent<Animator>();
    }

    void Update()
    {
        int frame = GetCurrentFrame();

        float yOffset = frame switch
        {
            0 => frame0Y,
            1 => frame1Y,
            2 => frame2Y,
            3 => frame3Y,
            _ => 0f
        };

        transform.localPosition = new Vector3(
            _baseLocalPos.x,
            _baseLocalPos.y + yOffset,
            _baseLocalPos.z
        );
    }

    int GetCurrentFrame()
    {
        // Animator varsa onun normalizedTime'ını kullan — tam senkron
        if (animator != null)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            // normalizedTime: 0.0 → 1.0 arası döngü
            float normalized = info.normalizedTime % 1f;
            return Mathf.Clamp(Mathf.FloorToInt(normalized * frameCount), 0, frameCount - 1);
        }

        // Fallback: Time.time ile hesapla
        float frameDuration = 1f / fps;
        float t = Time.time % (frameDuration * frameCount);
        return Mathf.Clamp(Mathf.FloorToInt(t / frameDuration), 0, frameCount - 1);
    }
}