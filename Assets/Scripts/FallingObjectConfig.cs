using UnityEngine;

[CreateAssetMenu(menuName = "Evasion/Falling Object Config")]
public class FallingObjectConfig : ScriptableObject
{
    public int dmg = 1;
    public float speedMax = 12f;
    public float accel = 40f;

    [Header("Animation")]
    public Sprite sprite;
    public RuntimeAnimatorController animatorController;

    [Header("Audio")]
    public AudioClip hitSfx;
    public AudioClip impactSfx;
}