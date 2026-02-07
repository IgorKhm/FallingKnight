using System.Text;
using TMPro;
using UnityEngine;

public class DebugOverlay : MonoBehaviour
{
    [Header("UI")]
    public GameObject debugPanel;
    public TMP_Text debugText;

    [Header("Toggle")]
    public KeyCode toggleKey = KeyCode.F3;
    public bool startVisible = false;

    [Header("Refs")]
    public GameManager gm;
    public PlayerController2D player;
    public PlayerHealth health;
    public FallingObjectSpawner spawner;

    [Header("Falling objects display")]
    public int maxObjectsToShow = 6;

    private readonly StringBuilder sb = new StringBuilder(2048);
    private bool visible;

    private void Awake()
    {
        visible = startVisible;
        if (debugPanel != null) debugPanel.SetActive(visible);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            SetVisible(!visible);
        }

        if (!visible) return;

        Render();
    }

    public void SetVisible(bool v)
    {
        visible = v;
        if (debugPanel != null) debugPanel.SetActive(visible);
    }

    private void Render()
    {
        if (debugText == null)
            return;

        sb.Clear();

        // --- GameManager ---
        if (gm != null)
        {
            sb.AppendLine("== GAME ==");
            sb.Append("State: ").Append(gm.GameState).AppendLine();
            sb.Append("Score: ").Append(gm.ScoreSeconds.ToString("0.00")).Append("s  ");
            sb.Append("High: ").Append(gm.HighScoreSeconds.ToString("0.00")).AppendLine("s");
            sb.AppendLine();
        }

        // --- Player ---
        if (player != null)
        {
            sb.AppendLine("== PLAYER ==");
            sb.Append("PosX: ").Append(player.transform.position.x.ToString("0.00")).Append("  ");
            sb.Append("VelX: ").Append(player.Speed.ToString("0.00")).Append("  ");
            sb.Append("AccX: ").Append(player.Acceleration.ToString("0.00")).AppendLine();
            sb.Append("State: ").Append(player.State).Append("  ");
            sb.Append("Input: ").Append(player.InputEnabled ? "ON" : "OFF").Append("  ");
            sb.Append("Dir: ").Append(player.HeldDir).Append("  ");
            sb.Append("RunHeld: ").Append(player.RunHeldTime.ToString("0.00")).AppendLine("s");
            sb.Append("Stun: ").Append(player.StunRemaining.ToString("0.00")).AppendLine("s");
            sb.AppendLine();
        }

        // --- Health ---
        if (health != null)
        {
            sb.AppendLine("== HEALTH ==");
            sb.Append("HP: ").Append(health.HP).Append("/").Append(health.MaxHP).Append("  ");
            sb.Append("IFrames: ").Append(health.IFrameRemaining.ToString("0.00")).AppendLine("s");
            sb.AppendLine();
        }

        // --- Spawner ---
        if (spawner != null)
        {
            sb.AppendLine("== SPAWNER ==");
            sb.Append("Active: ").Append(spawner.IsActive ? "YES" : "NO").Append("  ");
            sb.Append("Alive: ").Append(spawner.AliveCount).Append("/").Append(spawner.maxFallingObjects).AppendLine();
            sb.Append("Interval: ").Append(spawner.DebugCurrentInterval().ToString("0.000")).Append("s  ");
            sb.Append("Elapsed: ").Append(spawner.ElapsedSeconds.ToString("0.0")).Append("s  ");
            sb.Append("Timer: ").Append(spawner.TimerSeconds.ToString("0.000")).AppendLine("s");
            sb.Append("Dist: ").Append(spawner.distributionType).Append("  ");
            sb.Append("RangeX: [").Append(spawner.minX.ToString("0.0")).Append(", ").Append(spawner.maxX.ToString("0.0")).AppendLine("]");
            sb.AppendLine();
        }

        // --- Falling objects (sample) ---
        if (spawner != null)
        {
            sb.AppendLine("== FALLING OBJECTS (sample) ==");
            int shown = 0;

            foreach (var fo in spawner.AliveObjects)
            {
                if (fo == null) continue;

                sb.Append("#").Append(shown).Append("  ");
                sb.Append("x=").Append(fo.transform.position.x.ToString("0.00")).Append("  ");
                sb.Append("y=").Append(fo.transform.position.y.ToString("0.00")).Append("  ");
                sb.Append("vy=").Append(fo.CurrentSpeedY.ToString("0.00")).Append("  ");
                sb.Append("dmg=").Append(fo.Damage).Append("  ");
                sb.Append("spawnX=").Append(fo.spawnX.ToString("0.00"));
                sb.AppendLine();

                shown++;
                if (shown >= maxObjectsToShow) break;
            }

            if (shown == 0) sb.AppendLine("(none)");
        }

        debugText.text = sb.ToString();
    }
}
