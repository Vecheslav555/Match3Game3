using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public Text scoreText;
    public Button newGameButton;

    [Header("Звуки")]
    public AudioSource audioSource;
    public AudioClip successSound;
    public AudioClip failSound;

    private int currentScore = 0;
    private Board board;

    void Start()
    {
        board = FindObjectOfType<Board>();

        if (newGameButton != null)
            newGameButton.onClick.AddListener(NewGame);

        UpdateScoreUI();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore;
    }

    void NewGame()
    {
        currentScore = 0;
        UpdateScoreUI();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PlaySuccessSound()
    {
        if (audioSource != null && successSound != null)
            audioSource.PlayOneShot(successSound);
    }

    public void PlayFailSound()
    {
        if (audioSource != null && failSound != null)
            audioSource.PlayOneShot(failSound);
    }
}