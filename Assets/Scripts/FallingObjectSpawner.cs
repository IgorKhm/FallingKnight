using System.Collections.Generic;
using UnityEngine;

public enum SpawnDistributionType
{
    Uniform,
    PlayerGaussian
}

public class FallingObjectSpawner : MonoBehaviour
{
    [Header("Spawn area")] 
    public float minX = -9f;
    public float maxX = 9f;
    public float spawnY = 6f;

    [Header("Timing")] 
    public float baseSpawnInterval = 0.9f;
    public float difficultyStepSeconds = 10f;

    [Tooltip("0 < multiplier < 1.")][Range(0f, 1f)] public float spawnMultiplier = 0.92f;

    [Header("Rules")] public SpawnDistributionType distributionType = SpawnDistributionType.Uniform;
    public int maxFallingObjects = 12;

    [Header("Prefabs")] public FallingObject fallingObjectPrefab;
    public FallingObjectConfig defaultConfig;
    
    [Header("Player Gaussian")]
    public Transform player;              // assign Player transform
    public float gaussianSigma = 2.0f;    // bigger = easier
    [Range(0f, 1f)] public float gaussianWeight = 0.8f; // 0=uniform, 1=always gaussian
    public float minSpawnDistanceFromPlayer = 0.4f;     // fairness

    

    private float _timer;
    private float _elapsed;
    private readonly HashSet<FallingObject> _alive = new();
    private bool _isActive;

    public int AliveCount => _alive.Count;
    public float DebugCurrentInterval() => CurrentSpawnInterval();
    public IEnumerable<FallingObject> AliveObjects => _alive;
    public float ElapsedSeconds => _elapsed;
    public float TimerSeconds => _timer;
    public bool IsActive => _isActive;

    public void ResetSpawner(bool isActive)
    {
        _isActive = isActive;
        _timer = 0f;
        _elapsed = 0f;

        foreach (var fo in _alive)
            if (fo != null)
                Destroy(fo.gameObject);
        _alive.Clear();
    }

    private void Update()
    {
        if (!_isActive) return;

        _elapsed += Time.deltaTime;

        _alive.RemoveWhere(x => !x);

        float interval = CurrentSpawnInterval();
        _timer += Time.deltaTime;

        if (_timer >= interval)
        {
            _timer -= interval;
            TrySpawn();
        }
    }

    private float CurrentSpawnInterval()
    {
        if (difficultyStepSeconds <= 0f) return baseSpawnInterval;

        int step = Mathf.FloorToInt(_elapsed / difficultyStepSeconds);
        float mult = Mathf.Pow(spawnMultiplier, step);
        return Mathf.Max(0.05f, baseSpawnInterval * mult);
    }

    private void TrySpawn()
    {
        if (!fallingObjectPrefab) return;
        if (_alive.Count >= maxFallingObjects) return; // skip spawn

        float x = SampleX();
        Vector3 pos = new Vector3(x, spawnY, 0f);

        FallingObject fo = Instantiate(fallingObjectPrefab, pos, Quaternion.identity);
        fo.spawnX = x;
        if (!fo.config) fo.config = defaultConfig;

        _alive.Add(fo);
    }

    private float SampleX()
    {
        switch (distributionType)
        {   
            case SpawnDistributionType.PlayerGaussian:
            {
                // Mix gaussian with uniform so it stays fair/varied
                float x = (Random.value < gaussianWeight)
                    ? SamplePlayerGaussianX()
                    : Random.Range(minX, maxX);

                // Fairness: avoid spawning exactly on the player
                if (player && Mathf.Abs(x - player.position.x) < minSpawnDistanceFromPlayer)
                {
                    // push away to nearest safe side
                    float sign = Mathf.Sign(x - player.position.x);
                    if (sign == 0f) sign = (Random.value < 0.5f) ? -1f : 1f;
                    x = Mathf.Clamp(player.position.x + sign * minSpawnDistanceFromPlayer, minX, maxX);
                }

                return x;
            }
            
            case SpawnDistributionType.Uniform:
            default:
                return Random.Range(minX, maxX);
        }
    }
    
    
    private float SampleStandardNormal()
    {
        // Box–Muller: returns ~N(0,1)
        float u1 = Mathf.Max(1e-6f, Random.value);
        float u2 = Mathf.Max(1e-6f, Random.value);
        return Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);
    }

    private float SamplePlayerGaussianX()
    {
        float mean = player ? player.position.x : (minX + maxX) * 0.5f;
        float x = mean + gaussianSigma * SampleStandardNormal();
        return Mathf.Clamp(x, minX, maxX);
    }

    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(minX, spawnY, 0), new Vector3(maxX, spawnY, 0));
    }
}