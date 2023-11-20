using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[RequireComponent(typeof(Movements))]
public class Enemy : MonoBehaviour
{
    public enum MoveBehaviour
    {
        Idle = 0,
        Random = 1,
        predatePlayerStraight = 2,
    }

    [SerializeField]
    private Movements m_movements;
    [SerializeField]
    private GameObject m_head;

    public MoveBehaviour currentMoveBehaviour;

    public int ID = -1;

    public HexCoord m_startPos;
    public HexCoord.Orientation m_startOrientation;
    public HexCoord currentPos { get; private set; }
    public HexCoord lastPos { get; private set; }
    public HexCoord targetMovePos { get; private set; }
    public HexCoord.Orientation currentOrientation { get; private set; }
    public HexCoord.Orientation targetOrientation { get; private set; }

    [SerializeField]
    private float _modelHeight = 1f;

    public int _maxHealth { get; private set; } = 3;
    public int health { get; private set; }
    public bool isAlive { get; private set; } = true;


    [SerializeField]
    private float _moveCooldown = 1f;
    [SerializeField]
    private float _rotateCooldown = 0.5f;
    private float moveTimer = 0f;



    public static UnityEvent<Enemy> ev_moved = new();
    public static UnityEvent<Enemy> ev_spawned = new();
    public static UnityEvent<Enemy> ev_died = new();


    private void Reset()
    {
        m_movements = gameObject.GetComponent<Movements>();
    }

    private void Start()
    {
        m_movements.doneMoving.AddListener(() => OnMoveEnd());
        m_movements.doneRotating.AddListener(() => OnRotateEnd());

        health = _maxHealth;

        currentPos = m_startPos;
        currentOrientation = m_startOrientation; AdaptRotationToOrientation();
        transform.position = GameManager.Instance.hexGrid.GetWorldPos(currentPos);
        transform.position += Vector3.up * _modelHeight;

        ev_spawned.Invoke(this);
    }

    private void OnRotateEnd()
    {
        Debug.Log(gameObject.name + " Done rotating");

        currentOrientation = targetOrientation;

        moveTimer = _moveCooldown - _rotateCooldown;
    }

    private void OnMoveEnd()
    {
        Debug.Log(gameObject.name + " Done moving");

        lastPos = currentPos;
        currentPos = targetMovePos;
        ev_moved.Invoke(this);

        moveTimer = Random.Range(-0.4f, 0.4f);
    }

    private void Update()
    {
        moveTimer += Time.deltaTime;
        Move();

        m_head.transform.forward = GameManager.Instance.player.transform.position - transform.position;
    }

    private void Move()
    {
        if (!CanMove())
            return;

        targetMovePos = ChooseTargetMovePos();
        targetOrientation = HexCoord.GetCorrespondingOrientation(targetMovePos - currentPos);
        Debug.Log(targetOrientation);

        if (!CanMoveHere(targetMovePos))
        {
            targetMovePos = currentPos;
            return;
        }

        if (targetOrientation == currentOrientation)
            MoveTo(targetMovePos);
        else
            RotateTo(targetOrientation);
    }

    private bool CanMove()
    {
        return moveTimer > _moveCooldown && !m_movements.isMoving && !m_movements.isRotating;
    }

    private bool CanMoveHere(HexCoord coord)
    {
        if (!GameManager.Instance.hexGrid.IsValidMoveCoordinates(coord))
            return false;
        if (EnemiesManager.Instance.IsAlreadyTargetted(coord, this))
            return false;
        if (GameManager.Instance.player.playerPos == coord)
            return false;

        return true;
    }

    private HexCoord ChooseTargetMovePos()
    {
        switch (currentMoveBehaviour)
        {
            case MoveBehaviour.Random:
                return GetRandomNeighbourPos();
            case MoveBehaviour.predatePlayerStraight:
                return GetClosestTileToPlayer();
            default:
                return currentPos;
        }
    }

    private HexCoord GetRandomNeighbourPos()
    {
        return HexCoord.GetNeighbour(currentPos, Random.Range(0, 6));
    }

    private HexCoord GetClosestTileToPlayer()
    {
        List<HexCoord> neighbours = GameManager.Instance.hexGrid.GetAllNeighbours(currentPos);

        if (neighbours.Count == 0)
            return currentPos;

        neighbours.Sort(delegate (HexCoord o1, HexCoord o2) { return HexCoord.Distance(o1, GameManager.Instance.player.playerPos) - HexCoord.Distance(o2, GameManager.Instance.player.playerPos); });

        return neighbours[0];
    }

    private void MoveTo(HexCoord targetPos)
    {
        Vector3 targetWorldPos = GameManager.Instance.hexGrid.GetWorldPos(targetPos) + Vector3.up * _modelHeight;
        m_movements.Move(targetWorldPos);
    }

    private void RotateTo(HexCoord.Orientation targetOrientation)
    {
        Quaternion targetRotation = targetOrientation.GetUnderlyingRotation() * Quaternion.Euler(0f, -30f, 0f);
        m_movements.Rotate(targetRotation);
    }

    public int TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log(gameObject.name + " took " + amount + " pts of damage");

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
