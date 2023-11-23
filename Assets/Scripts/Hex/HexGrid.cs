using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class HexGrid : MonoBehaviour
{
    [field: SerializeField]
    public float hexSize { get; private set; } = 1f;//OuterRadius
    public float hexInnerRadius { get; private set; }

    private float spacingVertical;
    private float spacingHorizontal;

    private Vector2 rOffset;

    [field: SerializeField]
    public int gridSize { get; private set; } = 3;

    [SerializeField]
    private HexTile hexTilePrefab;

    public Dictionary<HexCoord, HexTile> tiles { get; private set; } = new();

    private void Reset()
    {
        hexInnerRadius = hexSize * Mathf.Sqrt(3f) / 2;
        spacingHorizontal = hexInnerRadius * 2f;
        spacingVertical = hexSize * 3f / 2f;

        rOffset = new Vector2(spacingHorizontal / 2, spacingVertical);

        DestroyAllTiles();
    }

    private void Awake()
    {
        hexInnerRadius = hexSize * Mathf.Sqrt(3f) / 2;
        spacingHorizontal = hexInnerRadius * 2f;
        spacingVertical = hexSize * 3f / 2f;

        rOffset = new Vector2(spacingHorizontal / 2, spacingVertical);

        CreateTiles();
    }

    private void CreateTiles()
    {
        for (int s = -gridSize; s <= gridSize; ++s)
        {
            for (int q = -gridSize; q <= gridSize; ++q)
            {
                int r = -s - q;
                if (Mathf.Abs(r) <= gridSize)//it is a valid coordinate if  -gridSize < r = -i -j < gridSize
                {
                    float horizontalPos = s * spacingHorizontal + r * rOffset.x;
                    float verticalPos = r * rOffset.y;

                    Vector3 newTilePos = new(horizontalPos, 0f, verticalPos);
                    HexTile newTile = Instantiate(hexTilePrefab, newTilePos, Quaternion.identity, transform);
                    newTile.GridCoordinates = new HexCoord(-q, -s);
                    tiles.Add(newTile.GridCoordinates, newTile);
                }
            }
        }
    }

    private void DestroyAllTiles()
    {
        List<HexCoord> presentTiles = tiles.Keys.ToList();
        foreach(HexCoord tileCoord in presentTiles)
        {
            Destroy(tiles[tileCoord].gameObject);
        }
        tiles.Clear();
    }

    public HexTile GetTile(HexCoord hexCoord)
    {
        return tiles[hexCoord];
    }

    public Vector3 GetWorldPos(HexCoord gridTileCoord)
    {
        return GetTile(gridTileCoord).transform.position;
    }

    public bool IsExistingCoordinates(HexCoord coord)
    {
        return tiles.ContainsKey(coord);
    }

    public bool IsValidMoveCoordinates(HexCoord coord)
    {
        if (!IsExistingCoordinates(coord)) return false;

        if (!tiles[coord].isWalkable) return false;

        if (tiles[coord].containedLiveEnemy == null)
            return true;
        else
            return !tiles[coord].containedLiveEnemy.isAlive;
    }

    public HexCoord GetRandomGridCoordinates()
    {
        return tiles.Keys.ToArray()[Random.Range(0, tiles.Count)];
    }

    /// <summary>
    /// Provides a copy of gridCoordinates, as a list
    /// </summary>
    /// <returns></returns>
    public List<HexCoord> GetAllGridCoordinates()
    {
        return new(tiles.Keys.ToList());//because HexCoord are value type
    }

    public List<HexCoord> GetAllNeighbours(HexCoord pos)
    {
        List<HexCoord> output = new();
        for(int i = 0; i < 6; ++i)
        {
            if(IsExistingCoordinates(HexCoord.GetNeighbour(pos, i)))
            {
                output.Add(HexCoord.GetNeighbour(pos, i));
            }
        }
        return output;
    }

    public List<HexTile> GetAllTileNeighbours(HexTile tile)
    {
        List<HexTile> output = new();
        List<HexCoord> coordNeighbours = GetAllNeighbours(tile.GridCoordinates);

        foreach(HexCoord coord in coordNeighbours)
        {
            output.Add(tiles[coord]);
        }

        return output;
    }

    public List<HexTile> GetAllTileNeighbours(HexCoord coords)
    {
        return GetAllTileNeighbours(tiles[coords]);
    }

}
