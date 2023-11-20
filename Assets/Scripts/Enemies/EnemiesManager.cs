using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesManager : MonoBehaviour
{
    private static EnemiesManager _instance;
    public static EnemiesManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("EnemiesManager is null !!!");
            return _instance;
        }
    }

    private int nextEnemyID = 1;

    public Enemy enemyPrefab;
    public List<Enemy> aliveEnemies { get; private set; } = new();
    public List<HexCoord> validSpawnCoordinates { get; private set; } = new();

    public int nb_enemiesStart;


    private void Awake()
    {
        _instance = this;
        Enemy.ev_spawned.AddListener((enemy) => RegisterEnemySpawn(enemy));
        Enemy.ev_moved.AddListener((enemy) => RegisterEnemyMove(enemy));
        Enemy.ev_died.AddListener((enemy) => RegisterEnemyDied(enemy));
    }

    // Start is called before the first frame update
    void Start()
    {
        validSpawnCoordinates = GameManager.Instance.hexGrid.GetAllGridCoordinates();
        validSpawnCoordinates.Remove(GameManager.Instance.playerStartPos);

        for (int i = 0; i < nb_enemiesStart; i++)
        {
            if (validSpawnCoordinates.Count > 0)
            {
                HexCoord targetSpawnPos = validSpawnCoordinates[Random.Range(0, validSpawnCoordinates.Count)];
                SpawnEnemy(targetSpawnPos);
                validSpawnCoordinates.Remove(targetSpawnPos);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void RegisterEnemySpawn(Enemy spawnedEnemy)
    {
        aliveEnemies.Add(spawnedEnemy);

        GameManager.Instance.hexGrid.tiles[spawnedEnemy.currentPos].containedEnemy = spawnedEnemy;
    }

    private void RegisterEnemyMove(Enemy enemy)
    {
        if (GameManager.Instance.hexGrid.tiles[enemy.lastPos].containedEnemy == enemy)
        {
            GameManager.Instance.hexGrid.tiles[enemy.lastPos].containedEnemy = null;
        }
        GameManager.Instance.hexGrid.tiles[enemy.currentPos].containedEnemy = enemy;
    }

    private void RegisterEnemyDied(Enemy dyingEnemy)
    {
        aliveEnemies.Remove(dyingEnemy);
    }

    public bool IsAlreadyTargetted(HexCoord coord, Enemy enemyAsking = null)
    {
        foreach (Enemy enemy in aliveEnemies)
        {
            if (enemy != enemyAsking)
            {
                if (enemy.targetMovePos == coord)
                {
                    //Debug.Log("Tile " + coord + " is already targetted by enemy " + enemy.gameObject.name);
                    return true;
                }
            }
        }

        if (enemyAsking == null)
        {
            return false;
        }
        else
            return GameManager.Instance.player.targetGridPos == coord;
    }

    private void SpawnEnemy(HexCoord pos)
    {
        Enemy spawnedEnemy = Instantiate(enemyPrefab, transform);
        spawnedEnemy.ID = nextEnemyID;
        ++nextEnemyID;
        spawnedEnemy.m_startPos = pos;
        spawnedEnemy.m_startOrientation = 0;
        spawnedEnemy.currentMoveBehaviour = Enemy.MoveBehaviour.predatePlayerStraight;

        spawnedEnemy.gameObject.name = "Enemy" + spawnedEnemy.ID;
    }
}
