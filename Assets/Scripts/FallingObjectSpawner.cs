using System.Collections.Generic;
using UnityEngine;

public enum SpawnDistributionType
{
    Uniform
}

public class FallingObjectSpawner : MonoBehaviour
{
    [Header("Spawn area")] public float minX = -9f;
    public float maxX = 9f;
    public float spawnY = 6f;

    [Header("Timing")] 
    public float baseSpawnInterval = 0.9f;
    public float difficultyStepSeconds = 10f;

    [Tooltip("0 < multiplier < 1.")] public float spawnMultiplier = 0.92f;

    [Header("Rules")] public SpawnDistributionType distributionType = SpawnDistributionType.Uniform;
    public int maxFallingObjects = 12;

    [Header("Prefabs")] public FallingObject fallingObjectPrefab;
    public FallingObjectConfig defaultConfig;

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
            default:
            case SpawnDistributionType.Uniform:
                return Random.Range(minX, maxX);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(minX, spawnY, 0), new Vector3(maxX, spawnY, 0));
    }
}