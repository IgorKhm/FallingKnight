using System;
using UnityEngine;

[RequireComponent(typeof(PlayerController2D))]
public class PlayerHealth : MonoBehaviour
{
    public int maxHP = 3;
    public float iFramesInterval = 0.8f;

    [Header("Debug (read-only)")]
    [SerializeField] private int hp;
    [SerializeField] private float iFrameTimer;

    private PlayerController2D controller;

    public int HP => hp;
    public int MaxHP => maxHP;
    public bool IsInvulnerable => iFrameTimer > 0f;

    public event Action<int, int> OnHPChanged; // (hp, max)
    public event Action OnDied;

    private void Awake()
    {
        controller = GetComponent<PlayerController2D>();
        hp = maxHP;
    }

    private void Update()
    {
        if (iFrameTimer > 0f) iFrameTimer -= Time.deltaTime;
    }

    public void ResetHealth()
    {
        hp = maxHP;
        iFrameTimer = 0f;
        OnHPChanged?.Invoke(hp, maxHP);
    }

    public bool TryTakeHit(int dmg)
    {
        if (hp <= 0) return false;
        if (IsInvulnerable) return false;

        hp -= Mathf.Max(0, dmg);
        iFrameTimer = iFramesInterval;

        OnHPChanged?.Invoke(hp, maxHP);

        controller.StunNow();

        if (hp <= 0)
        {
            controller.SetDead();
            OnDied?.Invoke();
        }

        return true;
    }
}