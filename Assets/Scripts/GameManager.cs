using UnityEngine;
using TMPro; // Importing the TextMeshPro namespace for text rendering
using System.Collections; // Importing the System.Collections namespace for using collections like IEnumerator

public class GameManager : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField] private BallController ball; // Assign the Ball GameObject
    [SerializeField] private PaddleController playerPaddle; // Assign Player Paddle
    [SerializeField] private AIPaddleController aiPaddle;   // Assign AI Paddle

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI playerScoreText; // Assign Player Score Text UI
    [SerializeField] private TextMeshProUGUI aiScoreText;     // Assign AI Score Text UI
    [SerializeField] private TextMeshProUGUI statusText;      // Assign Status Text UI (for win/start messages)

    [Header("Game Settings")]
    [SerializeField] private int scoreToWin = 11; // Points needed to win
    [SerializeField] private float delayBeforeServe = 2f; // Delay after scoring before ball serves

    // --- Private Variables ---
    private int playerScore;
    private int aiScore;
    private bool isGameOver = false;

    // --- Singleton Pattern (Optional but common) ---
    // public static GameManager Instance { get; private set; }

    // void Awake()
    // {
    //     if (Instance == null)
    //     {
    //         Instance = this;
    //         // DontDestroyOnLoad(gameObject); // Optional: if you have multiple scenes
    //     }
    //     else
    //     {
    //         Destroy(gameObject);
    //     }
    // }
    // --- End Singleton ---

    void Start()
    {
        // Ensure status text is hidden at start
        if (statusText != null) statusText.gameObject.SetActive(false);
        StartGame();
    }

    void Update()
    {
        // Optional: Check for restart input if game is over
        if (isGameOver && Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
    }

    // Initializes or resets the game state
    public void StartGame()
    {
        isGameOver = false;
        playerScore = 0;
        aiScore = 0;
        UpdateScoreUI();

        if (statusText != null) statusText.gameObject.SetActive(false); // Hide status text

        // Reset paddles and ball positions (optional, good practice)
        playerPaddle?.ResetPosition();
        aiPaddle?.ResetPosition();

        // Start the first round
        StartCoroutine(ServeBall(true)); // Player serves first initially
    }

    // Called by the BallController when a goal is detected
    public void ScorePoint(bool playerScored)
    {
        if (isGameOver) return; // Don't score if game is already over

        if (playerScored)
        {
            playerScore++;
        }
        else
        {
            aiScore++;
        }

        // --- Play Score Sound ---
        SoundManager.Instance?.PlayScoreSound(); // Use ?. for null safety

        UpdateScoreUI();
        CheckWinCondition();

        // If game isn't over, serve the next ball
        if (!isGameOver)
        {
            // Serve to the player who did NOT score
            StartCoroutine(ServeBall(!playerScored));
        }
    }

    // Updates the score display UI elements
    private void UpdateScoreUI()
    {
        if (playerScoreText != null) playerScoreText.text = playerScore.ToString();
        if (aiScoreText != null) aiScoreText.text = aiScore.ToString();
    }

    // Checks if either player has reached the score needed to win
    private void CheckWinCondition()
    {
        if (playerScore >= scoreToWin || aiScore >= scoreToWin)
        {
            EndGame();
        }
    }

    // Handles the end of the game
    private void EndGame()
    {
        isGameOver = true;
        ball.StopBall(); // Stop the ball immediately

        // Display winner message
        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = (playerScore >= scoreToWin ? "Player Wins!" : "AI Wins!") + "\nPress Space to Restart";
        }

        Debug.Log("Game Over! Winner: " + (playerScore >= scoreToWin ? "Player" : "AI"));
    }

    // Coroutine to serve the ball after a delay
    private IEnumerator ServeBall(bool toPlayer)
    {
        // Reset ball position and stop it before serving
        ball.ResetBall();
        ball.StopBall();

        // Wait for a specified delay
        yield return new WaitForSeconds(delayBeforeServe);

        // Only serve if the game is not over
        if (!isGameOver)
        {
            ball.LaunchBall(toPlayer); // Launch towards the specified player
        }
    }
}
