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
        set {
            if (!hasBeenSet)
            {
                _gridCoordinates = value;
                hasBeenSet = true;
            }
            else
            {
                throw new Exception("GridCoordinates has already been set. It can't be set again.");
            }
        }
    }



}
