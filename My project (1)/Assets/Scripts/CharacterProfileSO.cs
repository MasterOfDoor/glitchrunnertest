using UnityEngine;

/// <summary>
/// Scene-specific character configuration: Rigidbody2D (gravity, constraints),
/// scale (height/size), and which controller script is active.
/// Create via: Right-click in Project → Create → GlitchRunner → Character Profile.
/// </summary>
[CreateAssetMenu(fileName = "CharacterProfile", menuName = "GlitchRunner/Character Profile")]
public class CharacterProfileSO : ScriptableObject
{
    [Header("Rigidbody2D")]
    [Tooltip("Gravity scale applied to Rigidbody2D (0 = no gravity, 1 = default).")]
    public float gravityScale = 1f;

    [Tooltip("Linear damping (optional).")]
    public float linearDamping = 0f;

    [Tooltip("Mass (optional; leave 0 to not override).")]
    public float mass = 0f;

    [Tooltip("Freeze rotation to prevent tipping.")]
    public bool freezeRotation = true;

    [Tooltip("Freeze position on X axis.")]
    public bool freezePositionX = false;

    [Tooltip("Freeze position on Y axis.")]
    public bool freezePositionY = false;

    [Header("Appearance / Size")]
    [Tooltip("Local scale for the character (different height/size per scene).")]
    public Vector3 characterScale = Vector3.one;

    [Header("Controller Selection")]
    [Tooltip("Index in CharacterProfileApplier's Controller Options list (0 = first, 1 = second, ...).")]
    public int activeControllerIndex = 0;
}
