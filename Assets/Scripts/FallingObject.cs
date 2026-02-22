using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class FallingObject : MonoBehaviour
{
    public FallingObjectConfig config;

    [Header("Runtime debug")]
    public float spawnX;

    private Rigidbody2D _rb;
    private bool _despawning;
    
    public float CurrentSpeedY => (_rb != null) ? _rb.linearVelocity.y : 0f;
    public int Damage => (config != null) ? config.dmg : 0;

    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && config != null && config.sprite != null) sr.sprite = config.sprite;

        var anim = GetComponentInChildren<Animator>();
        if (anim != null && config != null && config.animatorController != null)
            anim.runtimeAnimatorController = config.animatorController;
    }

    private void FixedUpdate()
    {
        if (_despawning || !config) return;

        float dt = Time.fixedDeltaTime;

        float vy = _rb.linearVelocity.y; 
        float targetVy = -config.speedMax;

        float dv = targetVy - vy;
        float maxStep = config.accel * dt;
        float step = Mathf.Clamp(dv, -maxStep, maxStep);
        vy += step;

        _rb.linearVelocity = new Vector2(0f, vy);
    }

    private void Despawn()
    {
        if (_despawning) return;
        _despawning = true;
        Destroy(gameObject);
    }

    private void OnBecameInvisible()
    {
        Despawn();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (_despawning) return;

        if (col.collider.CompareTag("Ground"))
        {
            AudioManager.I?.PlayObjectImpact();
            Despawn();
            return;
        }

        var health = col.collider.GetComponentInParent<PlayerHealth>();
        if (health != null && config != null)
        {
            // Damage only if not invulnerable; object despawns either way (simple rule)
            health.TryTakeHit(config.dmg);
            Despawn();
        }
    }
}
