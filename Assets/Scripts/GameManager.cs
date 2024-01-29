using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("GameManager is null !!!");
            }
            return _instance;
        }

    }

    public bool isGameOver { get; private set; } = false;

    [field: SerializeField]
    public HexGrid hexGrid { get; private set; }
    [field: SerializeField]
    public Player player1 { get; private set; }

    public PlayerController playerController => player1.controller;

    [field: SerializeField]
    public HexCoord playerStartPos { get; private set; }


    public bool isGamePaused { get; private set; } = false;

    [field: SerializeField]
    public int currentLevel { get; private set; } = 1;

    public UnityEvent ev_playerPickedUpDemon = new();
    public UnityEvent<bool> ev_gamePaused = new();
    public UnityEvent ev_gameOver = new();


    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }

        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        player1.character.ev_playerDied.AddListener(() => GameOver());

        player1.transform.position = hexGrid.GetWorldPos(playerStartPos);
        ev_playerPickedUpDemon.Invoke();

        UIManager.Instance.nextLevelCountdownOver.AddListener(() => LaunchNextLevel());
    }

    private void LaunchNextLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level " + ++currentLevel);
    }

    public void DamageTiles(List<HexCoord> targettedTiles, int attackDamage)
    {
        foreach (HexCoord coord in targettedTiles)
        {
            DamageTile(coord, attackDamage);
        }
    }

    public void DamageTile(HexCoord coord, int attackDamage)
    {
        HexTile targetTile = hexGrid.GetTile(coord);
        targetTile.Damage(attackDamage);
        targetTile.Glow();
    }

    public void Pause()
    {
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f;
        ev_gamePaused.Invoke(isGamePaused);
    }

    private void GameOver()
    {
        ev_gameOver.Invoke();
        isGameOver = true;
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }
}
