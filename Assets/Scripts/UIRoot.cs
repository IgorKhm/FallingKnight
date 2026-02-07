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
    public TMP_Text hpText;
    public TMP_Text timerText;

    [Header("GameOver")]
    public TMP_Text gameOverScoreText;
    public TMP_Text gameOverHighScoreText;

    private GameManager gm;
    private PlayerHealth ph;

    public void Init(GameManager gameManager, PlayerHealth playerHealth)
    {
        gm = gameManager;
        ph = playerHealth;

        if (ph != null)
            ph.OnHPChanged += (_, __) => RefreshHUD();

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
        if (gm == null) return;
        if (menuHighScoreText != null)
            menuHighScoreText.text = $"High Score: {gm.highScoreSeconds:0.0}s";

        RefreshHUD();

        if (gameOverScoreText != null)
            gameOverScoreText.text = $"Score: {gm.scoreSeconds:0.0}s";
        if (gameOverHighScoreText != null)
            gameOverHighScoreText.text = $"High Score: {gm.highScoreSeconds:0.0}s";
    }

    public void RefreshHUD()
    {
        if (gm == null || ph == null) return;

        if (hpText != null)
            hpText.text = $"HP: {ph.HP}/{ph.MaxHP}";
        if (timerText != null)
            timerText.text = $"{gm.scoreSeconds:0.0}s";
    }
}