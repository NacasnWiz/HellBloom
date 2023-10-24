using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    private static HexGrid _instance;

    public static HexGrid Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("Hex Grid is Null !!!");
            }

            return _instance;
        }
    }

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

    private Dictionary<HexCoord, HexTile> tiles = new Dictionary<HexCoord, HexTile>(); //Soon to be HexCube instead of Vector3Int



    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }

        hexInnerRadius = hexSize * Mathf.Sqrt(3f) / 2;
        spacingHorizontal = hexInnerRadius * 2f;
        spacingVertical = hexSize * 3f / 2f;

        rOffset = new Vector2(spacingHorizontal / 2, spacingVertical);
    }


    private void Start()
    {
        CreateTiles();

        //foreach(HexCoord coord in ((HexCoord)(0, 0)).CellsInRange(1))
        //{
        //    Debug.Log(coord);
        //}

        //Debug.Log(tiles[(0, 0)].GridCoordinates);

        //HexCoord hexTest = new HexCoord(0,0);
        //Debug.Log( (0,0) == hexTest);//true
        //Debug.Log( hexTest == (HexCoord)((int, int))hexTest);//true grâce à la surcharge d'opérateur==

        //tiles[(0, 1)].GridCoordinates += (0, 1);  //Throws an exception

    }

    private void CreateTiles()
    {
        for (int i = -gridSize; i <= gridSize; ++i)
        {
            for(int j = -gridSize; j <= gridSize; ++j)
            {
                int r = -i - j;
                if(Mathf.Abs(r) <= gridSize)//it is a valid coordinate if  -gridSize < r = -i -j < gridSize
                {
                    float horizontalPos = i * spacingHorizontal + r * rOffset.x;
                    float verticalPos = r * rOffset.y;

                    Vector3 newTilePos = new (horizontalPos, 0f, verticalPos);
                    HexTile newTile = Instantiate(hexTilePrefab, newTilePos, Quaternion.identity, transform);
                    newTile.GridCoordinates = new HexCoord(-j, -i);
                    tiles.Add(newTile.GridCoordinates, newTile);
                }
            }
        }
    }

    public HexTile GetTile(HexCoord hexCoord)
    {
        return tiles[hexCoord];
    }

    public Vector3 GetWorldPos(HexCoord gridTileCoord)
    {
        return GetTile(gridTileCoord).transform.position;
    }

}
