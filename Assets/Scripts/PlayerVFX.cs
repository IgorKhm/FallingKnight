using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    [Header("Refs")]
    public PlayerController2D controller;

    [Header("Run Effect")]
    public ParticleSystem runParticles;

    [Header("Footstep")]
    [Tooltip("X distance from player center to the trailing foot")]
    public float footSpacing = 0.25f;

    private void Awake()
    {
        if (!controller) controller = GetComponentInParent<PlayerController2D>();
    }

    private void Update()
    {
        if (controller == null || runParticles == null) return;

        bool isRunning = controller.State == PlayerMoveState.MovingRun;

        // Trailing foot is always opposite to movement direction
        // Flipping localScale.x mirrors the emission direction (requires Simulation Space: Local)
        if (controller.HeldDir != 0)
        {
            Vector3 pos = runParticles.transform.localPosition;
            pos.x = -controller.HeldDir * footSpacing;
            runParticles.transform.localPosition = pos;

            Vector3 scale = runParticles.transform.localScale;
            scale.x = -controller.HeldDir;
            runParticles.transform.localScale = scale;
        }

        // Toggle emission — StopEmitting lets current particles fade out naturally
        if (isRunning && !runParticles.isEmitting)
            runParticles.Play();
        else if (!isRunning && runParticles.isEmitting)
            runParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
