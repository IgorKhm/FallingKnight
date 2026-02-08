using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;
    public PlayerController2D controller;
    public PlayerHealth health;

    [Header("Tuning")]
    public float movingThreshold = 0.05f;

    private readonly int SpeedX = Animator.StringToHash("SpeedX");
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
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnHit += HandleHit; // see note below
        if (health != null)
            health.OnDied += HandleDied; // already exists in your health
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnHit -= HandleHit;
        if (health != null)
            health.OnDied -= HandleDied;
    }

    private void Update()
    {
        if (animator == null || controller == null) return;

        float speedAbs = Mathf.Abs(controller.Speed);

        animator.SetFloat(SpeedX, speedAbs);
        animator.SetBool(IsMoving, speedAbs > movingThreshold);

        animator.SetBool(IsRunning, controller.State == PlayerMoveState.MovingRun);
        animator.SetBool(IsStunned, controller.State == PlayerMoveState.Stunned);
        animator.SetBool(IsDead, controller.State == PlayerMoveState.Dead);
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
