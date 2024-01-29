using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance
    { get { if (instance == null) Debug.LogError("UIManager is null !!!"); return instance; } }

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject congratulationsPanel;

    [SerializeField] private TMP_Text nextGameTimerCountdown;
    [SerializeField] private TMP_Text playerHealthText;


    public UnityEvent nextLevelCountdownOver = new();


    private void Awake()
    {
        if (instance == null) instance = this; else { Debug.LogError("Tried to instantiate another UIManager !!"); Destroy(this); }
    }

    private void Start()
    {
        GameManager.Instance.player1.character.ev_playerHealthChanged.AddListener((health) => ActualizePlayerHealth(health));
        GameManager.Instance.ev_gamePaused.AddListener((isGamePaused) => ActualizePausePanel(isGamePaused));
        GameManager.Instance.ev_gameOver.AddListener(() => ShowGameOverPanel());
        EnemiesManager.Instance.ev_allEnemiesDefeated.AddListener(() => ShowCongratulationsPanel());
        ActualizePlayerHealth(GameManager.Instance.player1.GetHealth());
    }

    private void ActualizePausePanel(bool isPaused)
    {
        pausePanel.SetActive(isPaused);
    }

    private void ActualizePlayerHealth(int health)
    {
        playerHealthText.text = health + " HP";
    }

    private void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
    }

    private void ShowCongratulationsPanel()
    {
        congratulationsPanel.SetActive(true);

        StartCoroutine(NextGameCoroutine());
    }

    private IEnumerator NextGameCoroutine()
    {
        int timeLeft = 10;
        nextGameTimerCountdown.text = "Congratulations! All enemies are defeated!\r\nTo next level in " + timeLeft.ToString() + "...";

        while (timeLeft > 0)
        {
            yield return new WaitForSeconds(1f);
            --timeLeft;
            nextGameTimerCountdown.text = "Congratulations! All enemies are defeated!\r\nTo next level in " + timeLeft.ToString() + "...";
        }

        nextGameTimerCountdown.text = "Restarting...";
        yield return new WaitForSeconds(1.5f);

        nextLevelCountdownOver.Invoke();
    }
}