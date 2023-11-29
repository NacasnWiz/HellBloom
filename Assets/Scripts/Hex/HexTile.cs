using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class HexTile : MonoBehaviour
{
    private bool hasBeenSet = false;
    private HexCoord _gridCoordinates;
    public HexCoord GridCoordinates
    {
        get { return _gridCoordinates; }
        set
        {
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

    [field: SerializeField]
    public float modelBaseSize { get; private set; } = 0.55f;
    [SerializeField] private GameObject _model;
    [SerializeField] private VisualEffect glowEffectPrefab;

    public Enemy containedLiveEnemy = null;

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
        if (!isWalkable)
        {
            _model.transform.localScale += new Vector3(0f, 0f, 150f);
        }

        ActualizeNeighbours();
    }

    private void OnMouseDown()
    {
        Debug.Log(_gridCoordinates);
    }

    public void Damage(int amount)
    {
        if (containedLiveEnemy == null) { return; }

        containedLiveEnemy.TakeDamage(amount);
    }

    private void ActualizeNeighbours()
    {
        neighbours = GameManager.Instance.hexGrid.GetAllTileNeighbours(this);
        MyLib.Shuffle(neighbours);
    }

    public void Glow()
    {
        VisualEffect glowEffect = Instantiate(glowEffectPrefab, transform);
        Destroy(glowEffect, 1f);

    }

}
