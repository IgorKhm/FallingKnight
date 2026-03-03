using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;
    public PlayerController2D controller;
    public PlayerHealth health;
    public Transform characterBody;

    [Header("Tuning")]
    public float movingThreshold = 0.05f;
    public float runAnimSpeed = 1.8f;

    private readonly int Speed = Animator.StringToHash("Speed");
    private readonly int IsMoving = Animator.StringToHash("IsMoving");
    private readonly int IsRunning = Animator.StringToHash("IsRunning");
    private readonly int IsStunned = Animator.StringToHash("IsStunned");
    private readonly int IsSliding = Animator.StringToHash("IsSliding");
    private readonly int IsDead = Animator.StringToHash("IsDead");
    private readonly int Hit = Animator.StringToHash("Hit");

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!controller) controller = GetComponentInParent<PlayerController2D>();
        if (!health) health = GetComponentInParent<PlayerHealth>();
        if (!characterBody) characterBody = GetComponentInChildren<Animator>()?.transform;
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

        animator.speed = Mathf.Lerp(1f, runAnimSpeed, speedAbs / controller.speedMax);

        animator.SetBool(IsRunning, controller.State == PlayerMoveState.MovingRun);
        animator.SetBool(IsStunned, controller.State == PlayerMoveState.Stunned);
        animator.SetBool(IsSliding, controller.State == PlayerMoveState.Slide);
        animator.SetBool(IsDead, controller.State == PlayerMoveState.Dead);

        // Flip entire rig based on input direction
        if (controller.HeldDir != 0 && characterBody != null)
        {
            Vector3 s = characterBody.localScale;
            s.x = controller.HeldDir < 0 ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);
            characterBody.localScale = s;
        }
    }

    public void ResetAnimator()
    {
        if (animator == null) return;
        animator.speed = 1f;
        animator.Rebind();
        animator.Update(0f);
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
