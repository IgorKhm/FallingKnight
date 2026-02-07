using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("PlayerMovment")]
    public float speedMax = 6f;
    public float accelWalk = 30f;
    public float accelRun = 60f;
    public float brakingForce = 40f;
    public float runFromTime = 0.6f;
    public float stunTime = 0.35f;

    [Header("Bounds")]
    public float minX = -7f;
    public float maxX = 7f;

    [Header("Debug (read-only)")]
    [SerializeField] private float speed;
    [SerializeField] private float acceleration;
    [SerializeField] private PlayerMoveState state;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private int heldDir;               // -1, 0, +1
    private int lastNonZeroDir;        // -1, +1
    private float sameDirTimer;
    private float stunTimer;
    private bool inputEnabled = true;

    public PlayerMoveState State => state;
    public float Speed => speed;
    public float Acceleration => acceleration;
    
    private Vector2 spawnPos;
    
    public bool InputEnabled => inputEnabled;
    public float StunRemaining => Mathf.Max(0f, stunTimer);
    public float RunHeldTime => sameDirTimer;
    public int HeldDir => heldDir;
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        state = PlayerMoveState.Idle;
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (!enabled)
        {
            moveInput = Vector2.zero;
            heldDir = 0;
            sameDirTimer = 0f;
        }
    }

    public void StunNow()
    {
        stunTimer = Mathf.Max(stunTimer, stunTime);
        sameDirTimer = 0f;
        heldDir = 0;
        moveInput = Vector2.zero;
        state = PlayerMoveState.Stunned;
    }

    public void SetDead()
    {
        state = PlayerMoveState.Dead;
        SetInputEnabled(false);
        rb.linearVelocity = Vector2.zero;
    }

    public void OnMove(InputValue value)
    {
        if (!inputEnabled || state == PlayerMoveState.Dead) return;

        moveInput = value.Get<Vector2>();
        Debug.Log($"OnMove fired: {moveInput}");
    }
    
    public void ReviveAndReset()
    {
        ReviveAndReset(spawnPos);
    }
    
    public void ReviveAndReset(Vector2 startPos)
    {
        // clear gameplay flags
        stunTimer = 0f;
        sameDirTimer = 0f;
        heldDir = 0;
        moveInput = Vector2.zero;

        // reset motion + position
        rb.linearVelocity = Vector2.zero;
        rb.position = startPos;

        // reset state + enable input
        state = PlayerMoveState.Idle;
        SetInputEnabled(true);
    }
    

    private void Start()
    {
        spawnPos = rb.position;
    }



    private void Update()
    {
        if (state == PlayerMoveState.Dead) return;

        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                state = PlayerMoveState.Idle;
            }
        }

        int dir = 0;
        if (inputEnabled && stunTimer <= 0f)
        {
            float x = moveInput.x;
            if (x > 0.1f) dir = 1;
            else if (x < -0.1f) dir = -1;
        }

        if (dir == 0)
        {
            sameDirTimer = 0f;
            heldDir = 0;
        }
        else
        {
            if (heldDir == 0)
            {
                heldDir = dir;
                lastNonZeroDir = dir;
                sameDirTimer = 0f;
            }
            else if (dir != heldDir)
            {
                // direction flip resets run timer
                heldDir = dir;
                lastNonZeroDir = dir;
                sameDirTimer = 0f;
            }
            else
            {
                sameDirTimer += Time.deltaTime;
            }
        }

        if (stunTimer > 0f)
        {
            state = PlayerMoveState.Stunned;
        }
        else if (dir == 0 && Mathf.Abs(rb.linearVelocity.x) < 0.05f)
        {
            state = PlayerMoveState.Idle;
        }
        else if (dir != 0 && sameDirTimer >= runFromTime)
        {
            state = PlayerMoveState.MovingRun;
        }
        else if (dir != 0)
        {
            state = PlayerMoveState.MovingWalk;
        }
        else
        {
            // no input but still sliding
            state = PlayerMoveState.MovingWalk;
        }
    }

    private void FixedUpdate()
    {
        if (state == PlayerMoveState.Dead) return;

        float dt = Time.fixedDeltaTime;
        float vx = rb.linearVelocity.x;

        bool stunned = (stunTimer > 0f);
        int dir = (stunned || !inputEnabled) ? 0 : heldDir;

        float targetVx = dir * speedMax;

        float accel = 0f;

        if (dir != 0)
        {
            float a = (sameDirTimer >= runFromTime) ? accelRun : accelWalk;
            float dv = targetVx - vx;
            float maxStep = a * dt;
            float step = Mathf.Clamp(dv, -maxStep, maxStep);
            vx += step;
            accel = (step / dt);
        }
        else
        {
            // brake toward 0
            float dv = -vx;
            float maxStep = brakingForce * dt;
            float step = Mathf.Clamp(dv, -maxStep, maxStep);
            vx += step;
            accel = (step / dt);
        }

        rb.linearVelocity = new Vector2(vx, 0f);

        // clamp position
        Vector2 p = rb.position;
        p.x = Mathf.Clamp(p.x, minX, maxX);
        rb.position = p;

        speed = rb.linearVelocity.x;
        acceleration = accel;
    }
}
