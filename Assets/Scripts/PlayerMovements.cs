using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovements : MonoBehaviour
{
    public Vector2Int pos = Vector2Int.zero;
    public int orientation { set { orientation = value % 6; } }

    public bool ballastRight = true;



}
