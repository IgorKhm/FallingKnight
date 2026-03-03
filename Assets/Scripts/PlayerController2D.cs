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

    [Header("Refs")]
    public PlayerHealth health;
    public PlayerAnimator playerAnimator;

    [Header("Bounds")]
    public float minX = -9.5f;
    public float maxX = 9.5f;

    private float _speed;
    private float _acceleration;
    private PlayerMoveState _state;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;
    private int _heldDir; // -1, 0, +1
    private float _sameDirTimer;
    private float _stunTimer;
    private bool _inputEnabled = true;
    private bool _slideEligible;

    public PlayerMoveState State => _state;
    public float Speed => _speed;
    public float Acceleration => _acceleration;

    private Vector2 _spawnPos;

    public bool InputEnabled => _inputEnabled;
    public float StunRemaining => Mathf.Max(0f, _stunTimer);
    public float RunHeldTime => _sameDirTimer;
    public int HeldDir => _heldDir;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _state = PlayerMoveState.Idle;
        _spawnPos = _rb.position;
    }

    public void SetInputEnabled(bool inputEnable)
    {
        _inputEnabled = inputEnable;
        if (!inputEnable)
        {
            _moveInput = Vector2.zero;
            _heldDir = 0;
            _sameDirTimer = 0f;
        }
    }

    public void StunNow()
    {
        _stunTimer = Mathf.Max(_stunTimer, stunTime);
        _sameDirTimer = 0f;
        _heldDir = 0;
        _moveInput = Vector2.zero;
        _slideEligible = false;
        _state = PlayerMoveState.Stunned;
    }

    public void SetDead()
    {
        _state = PlayerMoveState.Dead;
        SetInputEnabled(false);
        _rb.linearVelocity = Vector2.zero;
    }

    public void OnMove(InputValue value)
    {
        if (!_inputEnabled || _state == PlayerMoveState.Dead || _state == PlayerMoveState.Stunned) return;

        _moveInput = value.Get<Vector2>();
    }

    public void ReviveAndReset()
    {
        _stunTimer = 0f;
        _sameDirTimer = 0f;
        _heldDir = 0;
        _moveInput = Vector2.zero;

        _rb.linearVelocity = Vector2.zero;
        _rb.position = _spawnPos;

        _slideEligible = false;
        _state = PlayerMoveState.Idle;
        SetInputEnabled(true);

        health?.ResetHealth();
        playerAnimator?.ResetAnimator();
    }


    private void Update()
    {
        if (_state == PlayerMoveState.Dead) return;

        if (_stunTimer > 0f)
        {
            _stunTimer -= Time.deltaTime;
            if (_stunTimer <= 0f)
            {
                _state = PlayerMoveState.Idle;
            }
        }

        int dir = 0;
        if (_inputEnabled && _stunTimer <= 0f)
        {
            float x = _moveInput.x;
            if (x > 0.1f) dir = 1;
            else if (x < -0.1f) dir = -1;
        }

        if (dir == 0)
        {
            _sameDirTimer = 0f;
            _heldDir = 0;
        }
        else
        {
            if (_heldDir == 0 || dir != _heldDir)
            {
                _heldDir = dir;
                _sameDirTimer = 0f;
            }
            else
            {
                _sameDirTimer += Time.deltaTime;
            }
        }

        if (dir == 0 && _state == PlayerMoveState.MovingRun) _slideEligible = true;
        if (dir != 0) _slideEligible = false;

        if (_stunTimer > 0f)
        {
            _state = PlayerMoveState.Stunned;
        }
        else if (dir == 0 && Mathf.Abs(_rb.linearVelocity.x) < 0.05f)
        {
            _slideEligible = false;
            _state = PlayerMoveState.Idle;
        }
        else if (dir != 0 && _sameDirTimer >= runFromTime)
        {
            _state = PlayerMoveState.MovingRun;
        }
        else if (dir != 0)
        {
            _state = PlayerMoveState.MovingWalk;
        }
        else
        {
            _state = _slideEligible ? PlayerMoveState.Slide : PlayerMoveState.Idle;
        }
    }

    private void FixedUpdate()
    {
        if (_state == PlayerMoveState.Dead) return;

        float dt = Time.fixedDeltaTime;
        float vx = _rb.linearVelocity.x;

        int dir = (_stunTimer > 0f || !_inputEnabled) ? 0 : _heldDir;

        float targetVx = dir * speedMax;

        float accel;

        if (dir != 0)
        {
            float a = (_sameDirTimer >= runFromTime) ? accelRun : accelWalk;
            float dv = targetVx - vx;
            float maxStep = a * dt;
            float step = Mathf.Clamp(dv, -maxStep, maxStep);
            vx += step;
            accel = (step / dt);
        }
        else
        {
            float dv = -vx;
            float maxStep = brakingForce * dt;
            float step = Mathf.Clamp(dv, -maxStep, maxStep);
            vx += step;
            accel = (step / dt);
        }

        _rb.linearVelocity = new Vector2(vx, 0f);

        Vector2 p = _rb.position;
        p.x = Mathf.Clamp(p.x, minX, maxX);
        _rb.position = p;

        _speed = _rb.linearVelocity.x;
        _acceleration = accel;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(minX, transform.position.y, 0), new Vector3(minX, transform.position.y + 1, 0));
        Gizmos.DrawLine(new Vector3(maxX, transform.position.y, 0), new Vector3(maxX, transform.position.y + 1, 0));
    }

}