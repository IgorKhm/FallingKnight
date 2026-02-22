using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameState _state = GameState.BootMenu;

    [Header("Refs")] public PlayerHealth playerHealth;
    public PlayerController2D playerController;
    public FallingObjectSpawner spawner;
    public UIRoot ui;

    private float _scoreSeconds;
    private float _highScoreSeconds;
    private const string HighScoreKey = "Evasion_HighScoreSeconds";

    public float ScoreSeconds => _scoreSeconds;
    public float HighScoreSeconds => _highScoreSeconds;
    public GameState GameState => _state;

    private void Awake()
    {
        _highScoreSeconds = PlayerPrefs.GetFloat(HighScoreKey, 0f);
        
        if (playerHealth != null)
            playerHealth.OnDied += HandlePlayerDied;

        ui?.Init(this, playerHealth);
        
        GoToMenu();
    }

    private void Update()
    {
        if (_state == GameState.Playing)
        {
            _scoreSeconds += Time.deltaTime;
            ui?.RefreshHUD();
        }
    }

    private void GoToMenu()
    {
        _state = GameState.BootMenu;
        _scoreSeconds = 0f;

        playerController?.SetInputEnabled(false);
        playerHealth?.ResetHealth();
        spawner?.ResetSpawner(false);

        ui?.ShowMenu();
    }

    public void StartGame()
    {
        _state = GameState.Playing;
        _scoreSeconds = 0f;

        playerHealth?.ResetHealth();
        playerController?.ReviveAndReset();
        spawner?.ResetSpawner(true);
        AudioManager.I?.PlayMusic();

        ui?.ShowHUD();
    }

    private void HandlePlayerDied()
    {
        if (_state != GameState.Playing) return;

        _state = GameState.GameOver;

        if (_scoreSeconds > _highScoreSeconds)
        {
            _highScoreSeconds = _scoreSeconds;
            PlayerPrefs.SetFloat(HighScoreKey, _highScoreSeconds);
            PlayerPrefs.Save();
        }

        playerController?.SetInputEnabled(false);
        spawner?.ResetSpawner(false);
        ui?.ShowGameOver();
        AudioManager.I?.StopMusic(); 
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}