using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class FallingObject : MonoBehaviour
{
    public FallingObjectConfig config;

    [Header("Runtime debug")]
    public float spawnX;
    [SerializeField] private float speed;
    [SerializeField] private float accel;

    private Rigidbody2D rb;
    private bool despawning;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Apply optional visuals
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && config != null && config.sprite != null) sr.sprite = config.sprite;

        var anim = GetComponentInChildren<Animator>();
        if (anim != null && config != null && config.animatorController != null)
            anim.runtimeAnimatorController = config.animatorController;
    }

    private void FixedUpdate()
    {
        if (despawning || config == null) return;

        float dt = Time.fixedDeltaTime;

        float vy = rb.linearVelocity.y; 
        float targetVy = -config.speedMax;

        float dv = targetVy - vy;
        float maxStep = config.accel * dt;
        float step = Mathf.Clamp(dv, -maxStep, maxStep);
        vy += step;

        rb.linearVelocity = new Vector2(0f, vy);

        speed = vy;
        accel = (step / dt);
    }

    private void Despawn()
    {
        if (despawning) return;
        despawning = true;
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (despawning) return;

        if (col.collider.CompareTag("Ground"))
        {
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
