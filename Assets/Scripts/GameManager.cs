using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameState state = GameState.BootMenu;

    [Header("Refs")] public PlayerHealth playerHealth;
    public PlayerController2D playerController;
    public FallingObjectSpawner spawner;

    [Header("Score")] public float scoreSeconds;
    public float highScoreSeconds;

    private const string HighScoreKey = "Evasion_HighScoreSeconds";

    public UIRoot ui;


    private void Awake()
    {
        if (ui != null && playerHealth != null)
            ui.Init(this, playerHealth);
        highScoreSeconds = PlayerPrefs.GetFloat(HighScoreKey, 0f);
        if (playerHealth != null) playerHealth.OnDied += HandlePlayerDied;
        GoToMenu();
    }

    private void Update()
    {
        if (state == GameState.Playing)
        {
            scoreSeconds += Time.deltaTime;
        }

        // // quick keys (optional)
        // if (state == GameState.GameOver && Input.GetKeyDown(KeyCode.R))
        //     StartGame();
        // if (state == GameState.BootMenu && Input.GetKeyDown(KeyCode.Space))
        //     StartGame();
        //
        ui?.RefreshHUD();
    }

    public void GoToMenu()
    {
        state = GameState.BootMenu;
        scoreSeconds = 0f;

        if (playerController != null) playerController.SetInputEnabled(false);
        if (spawner != null) spawner.ResetSpawner();
        if (playerHealth != null) playerHealth.ResetHealth();

        spawner?.ResetSpawner();
        spawner?.SetActive(false);

        ui?.ShowMenu();
    }

    public void StartGame()
    {
        Debug.Log("Starting game");
        state = GameState.Playing;
        scoreSeconds = 0f;

        if (playerHealth != null) playerHealth.ResetHealth();
        if (spawner != null) spawner.ResetSpawner();
        if (playerController != null) playerController.SetInputEnabled(true);
        
        if (playerController != null)
            playerController.ReviveAndReset();

        spawner?.ResetSpawner();
        spawner?.SetActive(true);

        ui?.ShowHUD();
    }

    private void HandlePlayerDied()
    {
        if (state != GameState.Playing) return;

        state = GameState.GameOver;

        if (scoreSeconds > highScoreSeconds)
        {
            highScoreSeconds = scoreSeconds;
            PlayerPrefs.SetFloat(HighScoreKey, highScoreSeconds);
            PlayerPrefs.Save();
        }

        if (playerController != null) playerController.SetInputEnabled(false);
        spawner?.SetActive(false);

        ui?.ShowGameOver();
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game");
        spawner?.ResetSpawner();
        spawner?.SetActive(true);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}