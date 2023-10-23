using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HexCoord
{
    public int q;
    public int s;
    public int r { get { return -q - s; } }


    public HexCoord(int q = 0, int s = 0)
    {
        this.q = q;
        this.s = s;
    }

    public static HexCoord operator +(HexCoord a, HexCoord b)
    {
        return new HexCoord(a.q + b.q, a.s + b.s);
    }
    public static HexCoord operator -(HexCoord a, HexCoord b)
    {
        return new HexCoord(a.q - b.q, a.s - b.s);
    }

    public override string ToString() => $"({q}, {s}, {r})";

    public static implicit operator HexCoord((int i, int j) v) => new HexCoord(v.i, v.j);
    public static implicit operator (int, int)(HexCoord a) => (a.q, a.s);

    public static bool operator ==(HexCoord a, HexCoord b)
    {
        return (a.q == b.q) && (a.s == b.s);
    }
    public static bool operator !=(HexCoord a, HexCoord b)
    {
        return !(a == b);
    }

    public override readonly bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            return (HexCoord)obj == this;//le cast ne pose pas problème ?
        }
    }

    public override readonly int GetHashCode()
    {
        return Tuple.Create(q, s).GetHashCode();
    }
    //Apparently this should work as well but why?
    //> public override int GetHashCode()
    //{
    //   return (x << 2) ^ y;
    //}




    //Since cube hexagonal coordinates are based on 3d cube coordinates, we can adapt the distance calculation to work on hexagonal grids.
    //Each hexagon corresponds to a cube in 3d space. Adjacent hexagons are distance 1 apart in the hex grid but distance 2 apart in the cube grid.
    //For every 2 steps in the cube grid, we need only 1 step in the hex grid.
    //In the 3d cube grid, Manhattan distances are abs(dx) + abs(dy) + abs(dz). The distance on a hex grid is half that:
    public int Distance(HexCoord a, HexCoord b)
    {
        return (Mathf.Abs(a.q - b.q) + Mathf.Abs(a.s - b.s) + Mathf.Abs(a.r - b.r)) / 2;
    }

    public List<HexCoord> CellsInRange(int searchRange)
    {
        List<HexCoord> cellsInRangeList = new List<HexCoord>();

        for (int i = -searchRange; i <= searchRange; ++i)
        {
            for (int j = Mathf.Max(-searchRange, -i); j <= Mathf.Max(searchRange, -i + searchRange); ++j)
            {
                cellsInRangeList.Add(this + new HexCoord(i, j));
            }
        }

        return cellsInRangeList;
    }

}
