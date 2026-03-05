using TMPro;
using UnityEngine;

public class UIRoot : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject hudPanel;
    public GameObject gameOverPanel;

    [Header("Menu")]
    public TMP_Text menuHighScoreText;

    [Header("HUD")]
    // public TMP_Text hpText;
    public TMP_Text timerText;
    public HeartsUI heartsUI;

    [Header("GameOver")]
    public TMP_Text gameOverScoreText;
    public TMP_Text gameOverHighScoreText;

    private GameManager _gm;
    private PlayerHealth _ph;

    public void Init(GameManager gameManager, PlayerHealth playerHealth)
    {
        _gm = gameManager;
        _ph = playerHealth;

        if (heartsUI != null && _ph != null)
        {
            heartsUI.Init();
            heartsUI.SetHP(_ph.HP);

            _ph.OnHPChanged += (hp, _) => heartsUI.SetHP(hp);
        }

        RefreshAll();
    }

    public void ShowMenu()
    {
        menuPanel.SetActive(true);
        hudPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        RefreshAll();
    }

    public void ShowHUD()
    {
        menuPanel.SetActive(false);
        hudPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        RefreshHUD();
    }

    public void ShowGameOver()
    {
        menuPanel.SetActive(false);
        hudPanel.SetActive(false);
        gameOverPanel.SetActive(true);
        RefreshAll();
    }

    public void RefreshAll()
    {
        if (_gm == null) return;
        if (menuHighScoreText != null)
            menuHighScoreText.text = $"High Score: {_gm.HighScoreSeconds:0.0}s";

        RefreshHUD();

        if (gameOverScoreText != null)
            gameOverScoreText.text = $"Score: {_gm.ScoreSeconds:0.0}s";
        if (gameOverHighScoreText != null)
            gameOverHighScoreText.text = $"High Score: {_gm.HighScoreSeconds:0.0}s";
    }

    public void RefreshHUD()
    {
        if (_gm == null || _ph == null) return;

        if (timerText != null)
            timerText.text = $"{_gm.ScoreSeconds:0.0}s";
    }
}