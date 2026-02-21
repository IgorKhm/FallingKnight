using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;
    public PlayerController2D controller;
    public PlayerHealth health;
    public SpriteRenderer spriteRenderer;

    [Header("Tuning")]
    public float movingThreshold = 0.05f;

    private readonly int Speed = Animator.StringToHash("Speed");
    private readonly int IsMoving = Animator.StringToHash("IsMoving");
    private readonly int IsRunning = Animator.StringToHash("IsRunning");
    private readonly int IsStunned = Animator.StringToHash("IsStunned");
    private readonly int IsDead = Animator.StringToHash("IsDead");
    private readonly int Hit = Animator.StringToHash("Hit");

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!controller) controller = GetComponentInParent<PlayerController2D>();
        if (!health) health = GetComponentInParent<PlayerHealth>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnHit += HandleHit;
            health.OnDied += HandleDied;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnHit -= HandleHit;
            health.OnDied -= HandleDied;
        }
    }

    private void Update()
    {
        if (animator == null || controller == null) return;

        float speedAbs = Mathf.Abs(controller.Speed);

        animator.SetFloat(Speed, speedAbs);
        animator.SetBool(IsMoving, speedAbs > movingThreshold);

        animator.SetBool(IsRunning, controller.State == PlayerMoveState.MovingRun);
        animator.SetBool(IsStunned, controller.State == PlayerMoveState.Stunned);
        animator.SetBool(IsDead, controller.State == PlayerMoveState.Dead);

        // Flip sprite based on input direction
        if (controller.HeldDir != 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = controller.HeldDir < 0;
        }
    }

    private void HandleHit()
    {
        animator?.SetTrigger(Hit);
    }

    private void HandleDied()
    {
        animator?.SetBool(IsDead, true);
    }
}
