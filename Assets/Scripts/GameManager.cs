using UnityEngine;

// Tracks coins, lives, and win/lose state, and draws the HUD.
// Attach to an empty GameObject named "GameManager".
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int lives = 3;
    public float invulnerableSeconds = 1f;   // prevents one touch from costing several lives

    private int coinsCollected = 0;
    private int totalCoins = 0;
    private int maxLives;
    private bool gameEnded = false;
    private bool won = false;
    private float lastHitTime = -999f;
    private string message = "";
    private float flashAlpha = 0f;           // red screen flash when hurt

    private Texture2D whiteTex;              // 1x1 texture used to draw rectangles
    private GUIStyle hudStyle;
    private GUIStyle bigStyle;
    private GUIStyle hintStyle;
    private int styledForHeight = -1;

    public bool GameEnded { get { return gameEnded; } }

    void Awake()
    {
        Instance = this;
        maxLives = lives;

        whiteTex = new Texture2D(1, 1);
        whiteTex.SetPixel(0, 0, Color.white);
        whiteTex.Apply();
    }

    void Start()
    {
        // Count every Coin in the scene so the win condition adapts if you add/remove coins.
        totalCoins = FindObjectsByType<Coin>().Length;
        Debug.Log("Game started. Total coins: " + totalCoins + ", Lives: " + lives);
    }

    void Update()
    {
        // Fade the red hurt flash out.
        if (flashAlpha > 0f)
        {
            flashAlpha = Mathf.MoveTowards(flashAlpha, 0f, Time.deltaTime * 1.5f);
        }
    }

    public void CollectCoin()
    {
        if (gameEnded) return;

        coinsCollected++;
        Debug.Log("Coins collected: " + coinsCollected + "/" + totalCoins);

        if (coinsCollected >= totalCoins)
        {
            won = true;
            EndGame("You Win!");
        }
    }

    // Called by Obstacle when the player touches it.
    public void HitObstacle()
    {
        if (gameEnded) return;
        if (Time.time - lastHitTime < invulnerableSeconds) return;  // still invulnerable

        lastHitTime = Time.time;
        LoseLife("Hit an obstacle");
    }

    // Called by PlayerController when the player falls off the world. Returns true if the player is still alive.
    public bool PlayerFell()
    {
        if (gameEnded) return false;
        LoseLife("Fell off the world");
        return !gameEnded;
    }

    private void LoseLife(string reason)
    {
        lives--;
        flashAlpha = 0.45f;
        Debug.Log(reason + "! Lives left: " + lives);

        if (lives <= 0)
        {
            EndGame("Game Over!");
        }
    }

    private void EndGame(string endMessage)
    {
        gameEnded = true;
        message = endMessage;
        Debug.Log(endMessage);
        Invoke(nameof(QuitGame), 3f);   // wait 3 seconds so the player can read the message
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Build text styles sized relative to the screen height, so the HUD looks the same
    // on a small window, a 4K monitor, or a Retina display.
    private void BuildStyles(float s)
    {
        hudStyle = new GUIStyle(GUI.skin.label);
        hudStyle.fontSize = Mathf.RoundToInt(36 * s);
        hudStyle.fontStyle = FontStyle.Bold;
        hudStyle.normal.textColor = Color.white;

        bigStyle = new GUIStyle(hudStyle);
        bigStyle.fontSize = Mathf.RoundToInt(110 * s);
        bigStyle.alignment = TextAnchor.MiddleCenter;

        hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.fontSize = Mathf.RoundToInt(22 * s);
        hintStyle.alignment = TextAnchor.MiddleCenter;
        hintStyle.normal.textColor = new Color(1f, 1f, 1f, 0.8f);

        styledForHeight = Screen.height;
    }

    private void FillRect(Rect r, Color c)
    {
        GUI.color = c;
        GUI.DrawTexture(r, whiteTex);
        GUI.color = Color.white;
    }

    void OnGUI()
    {
        float s = Mathf.Max(0.75f, Screen.height / 1080f);   // UI scale factor
        if (hudStyle == null || styledForHeight != Screen.height) BuildStyles(s);

        // Red flash when the player loses a life.
        if (flashAlpha > 0f)
        {
            FillRect(new Rect(0, 0, Screen.width, Screen.height), new Color(1f, 0f, 0f, flashAlpha));
        }

        // HUD panel (top-left)
        float pad = 20 * s;
        Rect panel = new Rect(pad, pad, 400 * s, 130 * s);
        FillRect(panel, new Color(0f, 0f, 0f, 0.55f));
        FillRect(new Rect(panel.x, panel.y, 6 * s, panel.height), new Color(1f, 0.84f, 0f, 1f));   // gold accent bar

        GUI.Label(new Rect(panel.x + 24 * s, panel.y + 10 * s, panel.width, 55 * s),
                  "Coins: " + coinsCollected + " / " + totalCoins, hudStyle);

        GUI.Label(new Rect(panel.x + 24 * s, panel.y + 70 * s, 140 * s, 50 * s), "Lives:", hudStyle);

        // One square per life: red if still alive, gray if lost.
        float box = 32 * s;
        for (int i = 0; i < maxLives; i++)
        {
            Color c = (i < lives) ? new Color(0.95f, 0.2f, 0.2f, 1f) : new Color(0.4f, 0.4f, 0.4f, 0.8f);
            FillRect(new Rect(panel.x + 170 * s + i * (box + 10 * s), panel.y + 78 * s, box, box), c);
        }

        // Controls hint (bottom center)
        GUI.Label(new Rect(0, Screen.height - 50 * s, Screen.width, 40 * s),
                  "WASD / Arrows: Move    Shift: Run    Space: Jump    V: Switch Camera    Esc: Free Mouse", hintStyle);

        // Win / lose overlay
        if (gameEnded)
        {
            FillRect(new Rect(0, 0, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.6f));
            bigStyle.normal.textColor = won ? new Color(1f, 0.9f, 0.3f) : new Color(1f, 0.35f, 0.35f);
            GUI.Label(new Rect(0, Screen.height / 2f - 80 * s, Screen.width, 160 * s), message, bigStyle);
        }
    }
}