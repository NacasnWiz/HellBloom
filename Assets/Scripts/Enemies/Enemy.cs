using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[RequireComponent(typeof(Movements))]
public class Enemy : MonoBehaviour
{
    public enum PathFindingBehabiour
    {
        Idle = 0,
        Random = 1,
        DEPRECATEDpredatePlayerStraight = 2,
        predatePlayerAStar = 3,
    }

    public enum MoveBehabiour
    {
        Regular = 0,
        Free = 1,
    }

    public enum AttackMode
    {
        Regular = 0,
    }

    [SerializeField]
    private Movements m_movements;//I could implement it another way around, because there's a heavy duplicate with Player's movement handling (fuse the two in an interface? idk)
    [SerializeField] private EnemyHead m_head;

    public PathFindingBehabiour currentPathFindingBehaviour;
    public MoveBehabiour currentMoveBehabiour;
    public AttackMode currentAttackMode;

    public int ID = -1;

    public HexCoord m_startPos;
    public HexCoord.Orientation m_startOrientation;
    public HexCoord currentPos { get; private set; }
    public HexCoord lastPos { get; private set; }
    public HexCoord targetMovePos { get; private set; }
    public HexCoord isMovingToPos { get; private set; }
    public HexCoord.Orientation currentOrientation { get; private set; }
    public HexCoord.Orientation targetOrientation { get; private set; }

    [SerializeField]
    private float _modelHeight = 1f;//this could be better encapsulated

    public int _maxHealth { get; private set; } = 3;
    public int health { get; private set; }
    public bool isAlive { get; private set; } = true;

    [SerializeField, Range(0, 20)]
    public int _accurateSighRange = 10;

    private int attackDamage = 1;
    [SerializeField, Range(0.2f, 5f)]
    private float _attackCooldown = 2f;
    [SerializeField, Range(0f,3f)]
    private float _attackAfterMoveCooldown = 1f;
    [SerializeField, Range(0f, 3f)]
    private float _attackAfterRotateCooldown = 0.5f;
    private float attackTimer = 0f;

    [SerializeField, Range(0.1f, 3f)]
    private float _moveCooldown = 1f;
    [SerializeField, Range(0.1f, 1.5f)]
    private float _rotateCooldown = 0.5f;
    [SerializeField, Range(0f, 3f)]
    private float _moveAfterAttackCooldown = 0.5f;
    private float moveTimer = 0f;



    public static UnityEvent<Enemy> ev_moved = new();
    public static UnityEvent<Enemy> ev_spawned = new();
    public static UnityEvent<Enemy> ev_died = new();
    public static UnityEvent<HexCoord, int> ev_attack = new();


    private void Reset()
    {
        m_movements = gameObject.GetComponent<Movements>();
    }

    private void Start()
    {
        m_movements.doneMoving.AddListener(() => OnMoveEnd());
        m_movements.doneRotating.AddListener(() => OnRotateEnd());

        health = _maxHealth;

        InitPosition();

        ev_spawned.Invoke(this);

        StartCoroutine(LookPlayerCoroutine());
    }

    private float CooldownNoise(float cooldown, float amplitude = 0.25f)
    {
        return Random.Range(- cooldown * amplitude, cooldown * amplitude);
    }

    private void InitPosition()
    {
        currentPos = m_startPos;
        isMovingToPos = m_startPos;
        currentOrientation = m_startOrientation; AdaptRotationToOrientation();
        transform.position = GameManager.Instance.hexGrid.GetWorldPos(currentPos);
        transform.position += Vector3.up * _modelHeight;
    }

    private void OnRotateEnd()
    {
        //Debug.Log(gameObject.name + " Done rotating");

        currentOrientation = targetOrientation;

        moveTimer = _moveCooldown - _rotateCooldown;
        attackTimer = _attackCooldown - _attackAfterRotateCooldown + CooldownNoise(_attackAfterRotateCooldown);
    }

    private void OnMoveEnd()
    {
        //Debug.Log(gameObject.name + " Done moving");

        lastPos = currentPos;
        currentPos = targetMovePos;
        ev_moved.Invoke(this);

        moveTimer = CooldownNoise(_moveCooldown);
        attackTimer = _attackCooldown - _attackAfterMoveCooldown + CooldownNoise(_attackAfterMoveCooldown);
    }

    private void Update()
    {
        if (!GameManager.Instance.isGameOver)
        {
            moveTimer += Time.deltaTime;
            attackTimer += Time.deltaTime;
            Move();
        }
        else
        {

        }
    }

    private IEnumerator LookPlayerCoroutine()
    {
        while (true)
        {
            yield return null;
            m_head.transform.forward = GameManager.Instance.playerController.transform.position - transform.position;
        }
    }

    private void Move()
    {
        if (!CanMove())
            return;

        targetMovePos = ChooseTargetMovePos();
        if (HexCoord.Distance(targetMovePos, currentPos) == 1)
            targetOrientation = HexCoord.GetCorrespondingOrientation(targetMovePos - currentPos);
        else
        {
            Debug.Log("The next tile was not at distance 1 from me, " + gameObject.name);
            targetMovePos = currentPos;
            return;
        }

        if (!CanMoveHere(targetMovePos))
        {
            targetMovePos = currentPos;
            return;
        }

        if (targetOrientation != currentOrientation)
        {
            RotateTo(targetOrientation);
            if(!(currentMoveBehabiour == MoveBehabiour.Free))
            {
                targetMovePos = currentPos;
                return;
            }
        }

        if (targetMovePos != GameManager.Instance.playerController.playerPos)
        {
            MoveTo(targetMovePos);
        }
        else //attackPlayer here maybe
        {
            Attack(targetMovePos);

            //Debug.Log(gameObject.name + " reset targetmovePos from " + targetMovePos + " to current " + currentPos);
            targetMovePos = currentPos;
            return;
        }
    }

    private void Attack(HexCoord tilePos)
    {
        if (attackTimer < _attackCooldown) { Debug.Log(gameObject.name + " tried to attack but it's on cooldown"); return; }

        ev_attack.Invoke(tilePos, attackDamage);
        StartCoroutine(m_head.AttackCoroutine());

        attackTimer = CooldownNoise(_attackCooldown);
        moveTimer = _moveCooldown - _moveAfterAttackCooldown + CooldownNoise(_moveAfterAttackCooldown);
    }



    private bool CanMove()
    {
        return moveTimer > _moveCooldown && !m_movements.isMoving && !m_movements.isRotating;
    }

    private bool CanMoveHere(HexCoord coord)
    {
        if (!GameManager.Instance.hexGrid.IsValidMoveCoordinates(coord))
            return false;
        if (EnemiesManager.Instance.AnEnemyIsMovingToThisTile(coord, this))
            return false;

        return true;
    }

    private HexCoord ChooseTargetMovePos()
    {
        switch (currentPathFindingBehaviour)
        {
            case PathFindingBehabiour.Random:
                return GetRandomNeighbourPos();
            case PathFindingBehabiour.DEPRECATEDpredatePlayerStraight:
                return GetClosestTileToPlayer();
            case PathFindingBehabiour.predatePlayerAStar:
                return GetNextTileToPlayer();
            default:
                return currentPos;
        }
    }

    private HexCoord GetNextTileToPlayer() //name could be better
    {
        if (HexCoord.Distance(currentPos, GameManager.Instance.playerController.playerPos) > _accurateSighRange)
        {
            //Debug.Log("Player is too far for " + gameObject.name + " to accurately determine path");
            return GetClosestTileToPlayer();
        }

        List<HexTile> path = PathFinding.FindPathAStarBi(GameManager.Instance.hexGrid.tiles[currentPos], GameManager.Instance.hexGrid.tiles[GameManager.Instance.playerController.playerPos], GameManager.Instance.hexGrid, _accurateSighRange);

        if (path.Count > 1)
        {
            return path[1].GridCoordinates; //path[0] is the tile at currentPos
        }
        Debug.Log(gameObject.name + "Couldn't find a precise path to Player");
        return GetClosestTileToPlayer();
    }

    private HexCoord GetRandomNeighbourPos()
    {
        return HexCoord.GetNeighbour(currentPos, Random.Range(0, 6));
    }

    private HexCoord GetClosestTileToPlayer()
    {
        List<HexTile> neighbourTiles = GameManager.Instance.hexGrid.tiles[currentPos].neighbours;

        List<HexCoord> neighbours = new();
        foreach(HexTile tile in neighbourTiles)
        {
            neighbours.Add(tile.GridCoordinates);
        }

        neighbours = neighbours.Where(o => CanMoveHere(o)).ToList();

        if (neighbours.Count == 0)
            return currentPos;

        neighbours.Sort(delegate (HexCoord o1, HexCoord o2) { return HexCoord.Distance(o1, GameManager.Instance.playerController.playerPos) - HexCoord.Distance(o2, GameManager.Instance.playerController.playerPos); });

        return neighbours.FirstOrDefault();
    }

    private void MoveTo(HexCoord targetPos)
    {
        if (targetMovePos == currentPos)
            return;

        Vector3 targetWorldPos = GameManager.Instance.hexGrid.GetWorldPos(targetPos) + Vector3.up * _modelHeight;
        m_movements.Move(targetWorldPos);
        isMovingToPos = targetMovePos;
    }

    private void RotateTo(HexCoord.Orientation targetOrientation)
    {
        if (targetOrientation == currentOrientation) return;

        Quaternion targetRotation = targetOrientation.GetUnderlyingRotation() * Quaternion.Euler(0f, -30f, 0f);
        m_movements.Rotate(targetRotation);
    }

    public int TakeDamage(int amount)
    {
        health -= amount;
        //Debug.Log(gameObject.name + " took " + amount + " pts of damage");
        
        StartCoroutine(m_head.FlickerEyesCoroutine());

        if (health <= 0)
        {
            Die();
        }

        return health;
    }


    public void Die()
    {
        isAlive = false;
        Debug.Log(gameObject.name + " has died");
        ev_died.Invoke(this);
        gameObject.SetActive(false);
    }


    private void AdaptRotationToOrientation()
    {
        transform.rotation = currentOrientation.GetUnderlyingRotation() * Quaternion.Euler(0f, -30f, 0f);
    }
}
