using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    public PlayerController player { get; private set; }




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

    }


    public bool CanMoveTo(HexCoord coord)
    {
        return hexGrid.isValidCoordinates(coord);
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
        hexGrid.GetTile(coord).Damage(attackDamage);
    }
}
