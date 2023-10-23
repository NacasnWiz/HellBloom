using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    private bool hasBeenSet = false;
    private HexCoord _gridCoordinates;
    public HexCoord GridCoordinates{
        get { return _gridCoordinates; }
        set{
            if (!hasBeenSet)
            {
                _gridCoordinates = value;
                hasBeenSet = true;
            }
        }
    }


}
