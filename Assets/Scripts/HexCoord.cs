using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct HexCoord
{
    public int q;
    public int s;

    public readonly int r { get { return -q - s; } }

    public static HexCoord zero = new HexCoord(0, 0);

    public struct Orientation
    {
        private int _direction;
        public int Direction
        {
            get => _direction;
            set => _direction = value % 6;
        }


        public Orientation (int i) { _direction = i % 6; }

        public static implicit operator Orientation(int i) => new Orientation(i);
        public static implicit operator int(Orientation o) => o.Direction;
        public static Orientation operator +(Orientation o, int i) => new Orientation(i + o.Direction);

        public override string ToString()
        {
            return Direction.ToString();
        }

    }//Some features might be missing.


    public static HexCoord[] VertexDirections = { (1, 1), (-1, 2), (-2, 1), (-1, -1), (1, -2), (2, -1) };//Peu utile
    public static HexCoord[] EdgeDirections = { (0, 1), (-1, 1), (-1, 0), (0, -1), (1, -1), (0, -1) };

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


    public static HexCoord GetNeighbour(HexCoord center, Orientation side)
    {
        return center + EdgeDirections[side];
    }

    //Since cube hexagonal coordinates are based on 3d cube coordinates, we can adapt the distance calculation to work on hexagonal grids.
    //Each hexagon corresponds to a cube in 3d space. Adjacent hexagons are distance 1 apart in the hex grid but distance 2 apart in the cube grid.
    //For every 2 steps in the cube grid, we need only 1 step in the hex grid.
    //In the 3d cube grid, Manhattan distances are abs(dx) + abs(dy) + abs(dz). The distance on a hex grid is half that:
    public static int Distance(HexCoord a, HexCoord b)
    {
        return (Mathf.Abs(a.q - b.q) + Mathf.Abs(a.s - b.s) + Mathf.Abs(a.r - b.r)) / 2;
    }

    public int Distance(HexCoord c)
    {
        return Distance(this, c);
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
