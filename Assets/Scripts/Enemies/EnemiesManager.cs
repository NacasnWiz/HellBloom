using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    public enum EnemyType
    {
        Regular = 0,
        FreeMove = 1,
    }

    private int nextEnemyID = 1;

    [SerializeField]
    private Enemy regularEnemyPrefab;
    [SerializeField]
    private Enemy freeMoveEnemyPrefab;

    private Dictionary<EnemyType, Enemy> enemyPrefabs = new ();


    public List<Enemy> aliveEnemies { get; private set; } = new();
    public List<HexCoord> validSpawnCoordinates { get; private set; } = new();

    public List<int> nb_enemiesStart;
    [field: SerializeField] public bool registerPosOnMoveEnd { get; private set; } = false;

    [SerializeField]
    private Enemy.PathFindingBehabiour basePathFindingBehaviour;
    //[SerializeField]
    //private Enemy.MoveBehabiour baseMoveBehabiour;

    //[field: SerializeField]
    //public int baseAccurateSighRange { get; private set; } = 10;



    public UnityEvent ev_allEnemiesDefeated = new();

    private void Awake()
    {
        _instance = this;
        AddListenersToEnemyEvents();

        SetEnemyPrefabsDictionary();
    }

    private void AddListenersToEnemyEvents()
    {
        Enemy.ev_spawned.AddListener((enemy) => RegisterEnemySpawn(enemy));
        if (registerPosOnMoveEnd)
        {
            Enemy.ev_finishedMove.AddListener((enemy) => RegisterEnemyMove(enemy));
        }
        else
        {
            Enemy.ev_startedMove.AddListener((enemy) => RegisterEnemyMove(enemy));
        }
        Enemy.ev_died.AddListener((enemy) => RegisterEnemyDied(enemy));
    }

    private void SetEnemyPrefabsDictionary()
    {
        enemyPrefabs.Add(EnemyType.Regular, regularEnemyPrefab);
        enemyPrefabs.Add(EnemyType.FreeMove, freeMoveEnemyPrefab);
    }

    void Start()
    {
        SpawnStartEnemies();
    }

    private void SpawnStartEnemies()
    {
        validSpawnCoordinates = GameManager.Instance.hexGrid.GetAllGridCoordinates(true);
        validSpawnCoordinates.Remove(GameManager.Instance.playerStartPos);

        for(int k = 0; k < nb_enemiesStart.Count; ++k)
        {
            for (int i = 0; i < nb_enemiesStart[k]; i++)
            {
                if (validSpawnCoordinates.Count > 0)
                {
                    HexCoord targetSpawnPos = validSpawnCoordinates[Random.Range(0, validSpawnCoordinates.Count)];
                    EnemyType type = (EnemyType)k;
                    SpawnEnemy(type, targetSpawnPos);
                    validSpawnCoordinates.Remove(targetSpawnPos);
                }
                else
                {
                    Debug.Log("Couldn't spawn enemies, there was no valid spawn coordinates");
                }
            }
        }
    }

    private void RegisterEnemySpawn(Enemy spawnedEnemy)
    {
        aliveEnemies.Add(spawnedEnemy);

        GameManager.Instance.hexGrid.tiles[spawnedEnemy.currentPos].containedLiveEnemy = spawnedEnemy;
    }

    private void RegisterEnemyMove(Enemy enemy)
    {
        if (GameManager.Instance.hexGrid.tiles[enemy.lastPos].containedLiveEnemy == enemy)
        {
            GameManager.Instance.hexGrid.tiles[enemy.lastPos].containedLiveEnemy = null;
        }
        GameManager.Instance.hexGrid.tiles[enemy.currentPos].containedLiveEnemy = enemy;
    }

    private void RegisterEnemyDied(Enemy dyingEnemy)
    {
        aliveEnemies.Remove(dyingEnemy);

        if(aliveEnemies.Count == 0)
        {
            ev_allEnemiesDefeated.Invoke();
        }
    }

    public bool AnEnemyIsMovingToThisTile(HexCoord coord, Enemy enemyAsking = null)
    {
        foreach (Enemy enemy in aliveEnemies)
        {
            if (enemy.isMovingToPos == coord && enemy != enemyAsking && enemy.isAlive)
            {
                return true;
            }
        }

        if (enemyAsking == null)
            return false;
        else
            return GameManager.Instance.playerController.targetGridPos == coord && GameManager.Instance.playerController.targetGridPos != GameManager.Instance.playerController.playerPos;//targetting its own pos doesn't count.
    }

    private void SpawnEnemy(EnemyType type, HexCoord pos)
    {
        Enemy spawnedEnemy = Instantiate(enemyPrefabs[type], transform);
        spawnedEnemy.ID = nextEnemyID;
        ++nextEnemyID;
        spawnedEnemy.m_startPos = pos;
        spawnedEnemy.m_startOrientation = 0;
        spawnedEnemy.currentPathFindingBehaviour = basePathFindingBehaviour;
        //spawnedEnemy._accurateSighRange = baseAccurateSighRange;
        //spawnedEnemy.currentMoveBehabiour = baseMoveBehabiour;

        spawnedEnemy.gameObject.name = "Enemy" + spawnedEnemy.ID;
    }
}
