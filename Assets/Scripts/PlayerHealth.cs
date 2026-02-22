using System;
using UnityEngine;

[RequireComponent(typeof(PlayerController2D))]
public class PlayerHealth : MonoBehaviour
{
    public int maxHP = 6;
    public float iFramesInterval = 0.8f;

    private int _hp;
    private float _iFrameTimer;

    private PlayerController2D controller;

    public int HP => _hp;
    public int MaxHP => maxHP;
    private bool IsInvulnerable => _iFrameTimer > 0f;

    public event Action<int, int> OnHPChanged; // (hp, max)
    public event Action OnDied;
    public float IFrameRemaining => Mathf.Max(0f, _iFrameTimer);

    public event Action OnHit;

    private void Awake()
    {
        controller = GetComponent<PlayerController2D>();
        _hp = maxHP;
    }

    private void Update()
    {
        if (_iFrameTimer > 0f) _iFrameTimer -= Time.deltaTime;
    }

    public void ResetHealth()
    {
        _hp = maxHP;
        _iFrameTimer = 0f;
        OnHPChanged?.Invoke(_hp, maxHP);
    }

    public bool TryTakeHit(int dmg)
    {
        if (_hp <= 0) return false;
        if (IsInvulnerable) return false;

        _hp -= Mathf.Max(0, dmg);
        _iFrameTimer = iFramesInterval;

        OnHPChanged?.Invoke(_hp, maxHP);

        controller.StunNow();
        
        AudioManager.I?.PlayPlayerHit();

        if (_hp <= 0)
        {
            controller.SetDead();
            OnDied?.Invoke();
        }
        else
        {
            OnHit?.Invoke();
        }

        return true;
    }
}