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

    [field: SerializeField]
    public HexGrid hexGrid { get; private set; }
    [field: SerializeField]
    public Player player { get; private set; }

    public PlayerController playerController => player.controller;

    [field: SerializeField]
    public HexCoord playerStartPos { get; private set; }


    public bool isGamePaused { get; private set; } = false;

    public UnityEvent ev_playerPickedUpDemon = new();
    public UnityEvent<bool> ev_gamePaused = new();


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
        player.transform.position = hexGrid.GetWorldPos(playerStartPos);
        ev_playerPickedUpDemon.Invoke();
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
}
