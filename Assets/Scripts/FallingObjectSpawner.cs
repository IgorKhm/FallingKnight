using System.Collections.Generic;
using UnityEngine;

public enum SpawnDistributionType
{
    Uniform
}

public class FallingObjectSpawner : MonoBehaviour
{
    [Header("Spawn area")]
    public float minX = -7f;
    public float maxX = 7f;
    public float spawnY = 6f;

    [Header("Timing")]
    public float baseSpawnInterval = 0.9f;
    public float difficultyStepSeconds = 10f;
    [Tooltip("0 < multiplier < 1.")]
    public float spawnMultiplier = 0.92f;

    [Header("Rules")]
    public SpawnDistributionType distributionType = SpawnDistributionType.Uniform;
    public int maxFallingObjects = 12;

    [Header("Prefabs")]
    public FallingObject fallingObjectPrefab;
    public FallingObjectConfig defaultConfig;

    private float timer;
    private float elapsed;
    private readonly HashSet<FallingObject> alive = new();
    private bool isActive;
    
    public int AliveCount => alive.Count;

    public float DebugCurrentInterval()
    {
        return CurrentSpawnInterval();
    }

    public System.Collections.Generic.IEnumerable<FallingObject> AliveObjects => alive;
    public float ElapsedSeconds => elapsed;
    public float TimerSeconds => timer;
    public bool IsActive => isActive;

    public void ResetSpawner()
    {
        isActive = false;
        timer = 0f;
        elapsed = 0f;

        // Clean up any leftovers
        foreach (var fo in alive)
            if (fo != null) Destroy(fo.gameObject);
        alive.Clear();
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
        if (!isActive)
        {
            foreach (var fo in alive)
                if (fo != null) Destroy(fo.gameObject);
        }
    }

    private void Update()
    {
        if (!isActive) return;
        
        elapsed += Time.deltaTime;

        // prune dead
        alive.RemoveWhere(x => x == null);

        float interval = CurrentSpawnInterval();
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer -= interval;
            TrySpawn();
        }
    }

    private float CurrentSpawnInterval()
    {
        if (difficultyStepSeconds <= 0f) return baseSpawnInterval;

        int step = Mathf.FloorToInt(elapsed / difficultyStepSeconds);
        float mult = Mathf.Pow(spawnMultiplier, step);
        return Mathf.Max(0.05f, baseSpawnInterval * mult);
    }

    private void TrySpawn()
    {
        if (fallingObjectPrefab == null) return;
        if (alive.Count >= maxFallingObjects) return; // skip spawn

        float x = SampleX();
        Vector3 pos = new Vector3(x, spawnY, 0f);

        FallingObject fo = Instantiate(fallingObjectPrefab, pos, Quaternion.identity);
        fo.spawnX = x;
        if (fo.config == null) fo.config = defaultConfig;

        alive.Add(fo);
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
}
