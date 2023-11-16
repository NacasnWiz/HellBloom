using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Movements))]
public class Enemy : MonoBehaviour
{
    [SerializeField]
    private Movements m_Movements;

    [SerializeField]
    private HexCoord m_startPos;
    public HexCoord pos { get; private set; }
    public HexCoord lastPos { get; private set; }
    private HexCoord targetMovePos;

    [SerializeField]
    private float _modelHeight = 1f;
    
    public int maxHealth { get; private set; } = 3;
    public int _health { get; private set; }


    [SerializeField]
    private float moveCooldown = 2f;
    private float moveTimer = 0f;



    public static UnityEvent<Enemy> ev_moved = new();


    private void Reset()
    {
        m_Movements = gameObject.GetComponent<Movements>();
    }

    private void Start()
    {
        m_Movements.doneMoving.AddListener(() => OnMoveEnd());
        m_Movements.doneRotating.AddListener(() => Debug.Log(gameObject.name + "Done rotating"));

        _health = maxHealth;

        pos = m_startPos;
        transform.position = GameManager.Instance.hexGrid.GetWorldPos(pos);
        transform.position += Vector3.up * _modelHeight;
    }

    private void OnMoveEnd()
    {
        Debug.Log(gameObject.name + "Done moving");

        lastPos = pos;
        pos = targetMovePos;
        ev_moved.Invoke(this);

        moveTimer = 0f;
    }

    private void Update()
    {
        moveTimer += Time.deltaTime;
        if(moveTimer > moveCooldown && !m_Movements.isMoving && !m_Movements.isRotating)
        {
            targetMovePos = GetRandomNeighbourPos();
            if(GameManager.Instance.hexGrid.isValidCoordinates(targetMovePos))
                MoveTo(targetMovePos);
        }
    }

    private HexCoord GetRandomNeighbourPos()
    {
        return HexCoord.GetNeighbour(pos, Random.Range(0, 6));
    }

    private void MoveTo(HexCoord targetPos)
    {
        Vector3 targetWorldPos = GameManager.Instance.hexGrid.GetWorldPos(targetPos) + Vector3.up * _modelHeight;
        m_Movements.Move(targetWorldPos);
    }

    public int TakeDamage(int amount)
    {
        _health -= amount;
        Debug.Log(gameObject.name + " took " + amount + " pts of damage");

        if (_health < 0)
        {
            Die();
        }

        return _health;
    }

    public void Die()
    {
        Debug.Log(gameObject.name + " has died");
    }


}
