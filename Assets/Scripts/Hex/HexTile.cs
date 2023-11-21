using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    private bool hasBeenSet = false;
    private HexCoord _gridCoordinates;
    [SerializeField]
    public HexCoord GridCoordinates{
        get { return _gridCoordinates; }
        set {
            if (!hasBeenSet)
            {
                _gridCoordinates = value;
                gameObject.name = "tile " + _gridCoordinates.ToString();
                hasBeenSet = true;
            }
            else
            {
                throw new Exception("GridCoordinates has already been set. It can't be set again.");
            }
        }
    }

    public Enemy containedEnemy = null;

    public bool isWalkable = true;

    public struct PathFinding
    {
        public int Total => moveCost + heurCost;

        public int moveCost;

        public int heurCost;
    }

    public PathFinding pathValues;

    public List<HexTile> neighbours { get; private set; } = new();

    private void Start()
    {
        ActualizeNeighbours();
        neighbours = GameManager.Instance.hexGrid.GetAllTileNeighbours(this);
    }

    private void OnMouseDown()
    {
        Debug.Log(_gridCoordinates);
    }

    public void Damage(int amount)
    {
        if(containedEnemy == null) { return; }

        containedEnemy.TakeDamage(amount);
    }

    private void ActualizeNeighbours()
    {
        neighbours = GameManager.Instance.hexGrid.GetAllTileNeighbours(this);
    }

}
