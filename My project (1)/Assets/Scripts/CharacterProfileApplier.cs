using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Applies a CharacterProfileSO to this object: sets Rigidbody2D (gravity, constraints),
/// transform scale, and enables exactly one controller from the options list.
/// Add via Add Component; assign a profile and drag controller components into Controller Options.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterProfileApplier : MonoBehaviour
{
    [Tooltip("Scene-specific profile. Leave empty to skip applying.")]
    [SerializeField] private CharacterProfileSO profile;

    [Tooltip("Controller components to choose from. Enable the one at profile.ActiveControllerIndex.")]
    [SerializeField] private List<MonoBehaviour> controllerOptions = new List<MonoBehaviour>();

    private void Awake()
    {
        Apply();
    }

    /// <summary>
    /// Applies the assigned profile to Rigidbody2D, Transform, and controller selection.
    /// Call this again after changing profile at runtime (e.g. scene load).
    /// </summary>
    public void Apply()
    {
        if (profile == null)
            return;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            ApplyToRigidbody(rb);

        transform.localScale = profile.characterScale;

        ApplyControllerSelection();
    }

    private void ApplyToRigidbody(Rigidbody2D rb)
    {
        rb.gravityScale = profile.gravityScale;
        rb.linearDamping = profile.linearDamping;
        if (profile.mass > 0f)
            rb.mass = profile.mass;

        RigidbodyConstraints2D constraints = RigidbodyConstraints2D.None;
        if (profile.freezeRotation)
            constraints |= RigidbodyConstraints2D.FreezeRotation;
        if (profile.freezePositionX)
            constraints |= RigidbodyConstraints2D.FreezePositionX;
        if (profile.freezePositionY)
            constraints |= RigidbodyConstraints2D.FreezePositionY;
        rb.constraints = constraints;
    }

    private void ApplyControllerSelection()
    {
        if (controllerOptions == null || controllerOptions.Count == 0)
            return;

        int index = Mathf.Clamp(profile.activeControllerIndex, 0, controllerOptions.Count - 1);

        for (int i = 0; i < controllerOptions.Count; i++)
        {
            MonoBehaviour c = controllerOptions[i];
            if (c != null)
                c.enabled = (i == index);
        }
    }

    /// <summary>
    /// Assign a profile at runtime and re-apply (e.g. when loading a new scene).
    /// </summary>
    public void SetProfile(CharacterProfileSO newProfile)
    {
        profile = newProfile;
        Apply();
    }

    public CharacterProfileSO Profile => profile;
}
