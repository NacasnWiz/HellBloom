using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Net;

/////// <summary>
/////// Single grid element.
/////// </summary>
////public class PathFindingTile : MonoBehaviour
////{
////    /// <summary>
////    /// Sum of G and H.
////    /// </summary>
////    public int F => g + h;

////    /// <summary>
////    /// Cost from start tile to this tile.
////    /// </summary>
////    public int g;

////    /// <summary>
////    /// Estimated cost from this tile to destination tile.
////    /// </summary>
////    public int h;

////    /// <summary>
////    /// Tile's coordinates.
////    /// </summary>
////    public HexCoord position;

////    /// <summary>
////    /// References to all adjacent tiles.
////    /// </summary>
////    public List<PathFindingTile> adjacentTiles = new List<PathFindingTile>();

////    public HexTile associatedTile;

////    /// <summary>
////    /// If true - Tile is an obstacle impossible to pass.
////    /// </summary>
////    public bool isObstacle;

////    public PathFindingTile(HexTile hexTile)
////    {
////        g = 0;
////        h = 0;
////        foreach(HexTile tile in hexTile.neighbours)
////        {
////            adjacentTiles.Add(tile);
////        }

////        isObstacle = hexTile.isWalkable;
////    }
////}

///// <summary>
///// Implementation of A* pathfinding algorithm.
///// </summary>
//public class Pathfinding
//{
//    /// <summary>
//    /// Finds path from given start point to end point. Returns an empty list if the path couldn't be found.
//    /// </summary>
//    /// <param name="startPoint">Start tile.</param>
//    /// <param name="endPoint">Destination tile.</param>
//    public static List<HexTile> FindPathAStar(HexTile startPoint, HexTile endPoint, HexGrid hexGrid)
//    {
//        List<HexTile> openPathTiles = new();
//        List<HexTile> closedPathTiles = new();

//        // Prepare the start tile.
//        HexTile currentTile = startPoint;

//        currentTile.pathValues.g = 0;
//        currentTile.pathValues.h = GetEstimatedPathCost(startPoint, endPoint);

//        // Add the start tile to the open list.
//        openPathTiles.Add(currentTile);

//        while (openPathTiles.Count != 0)
//        {
//            // Sorting the open list to get the tile with the lowest F.
//            openPathTiles = openPathTiles.OrderBy(o => o.pathValues.F).ThenByDescending(o => o.pathValues.g).ToList();
//            currentTile = openPathTiles[0];

//            // Removing the current tile from the open list and adding it to the closed list.
//            openPathTiles.Remove(currentTile);
//            closedPathTiles.Add(currentTile);

//            int g = currentTile.pathValues.g + 1;

//            // If there is a target tile in the closed list, we have found a path.
//            if (closedPathTiles.Contains(endPoint))
//            {
//                break;
//            }

//            // Investigating each adjacent tile of the current tile.
//            foreach (HexTile adjacentTile in currentTile.neighbours)
//            {
//                // Ignore not walkable adjacent tiles.
//                if (hexGrid.IsValidMoveCoordinates(adjacentTile.GridCoordinates) && !closedPathTiles.Contains(adjacentTile))
//                {
//                    if (!openPathTiles.Contains(adjacentTile))
//                    {
//                        adjacentTile.pathValues.g = g;
//                        adjacentTile.pathValues.h = GetEstimatedPathCost(adjacentTile, endPoint);
//                        openPathTiles.Add(adjacentTile);
//                    }
//                    // Otherwise check if using current G we can get a lower value of F, if so update it's value.
//                    else if (adjacentTile.pathValues.F > g + adjacentTile.pathValues.h)
//                    {
//                        adjacentTile.pathValues.g = g;
//                    }
//                }
//            }
//        }

//        List<HexTile> finalPathTiles = new();

//        // Backtracking - setting the final path.
//        if (closedPathTiles.Contains(endPoint))
//        {
//            currentTile = endPoint;
//            finalPathTiles.Add(currentTile);

//            for (int i = endPoint.pathValues.g - 1; i >= 0; i--)
//            {
//                currentTile = closedPathTiles.Find(o => o.pathValues.g == i && currentTile.neighbours.Contains(o));
//                finalPathTiles.Add(currentTile);
//            }

//            finalPathTiles.Reverse();
//        }

//        return finalPathTiles;
//    }

//    /// <summary>
//    /// Returns estimated path cost from given start position to target position of hex tile using Manhattan distance.
//    /// </summary>
//    /// <param name="startPosition">Start position.</param>
//    /// <param name="targetPosition">Destination position.</param>
//    protected static int GetEstimatedPathCost(HexTile startPosition, HexTile targetPosition)
//    {
//        return HexCoord.Distance(startPosition.GridCoordinates, targetPosition.GridCoordinates);
//    }
//}

public class PathFinding
{
    public static List<HexTile> FindPathAStarBi(HexTile start, HexTile goal, HexGrid hexGrid)
    {
        List<HexTile> outputPath = new();

        List<HexTile> toSearchFromStart = new();
        List<HexTile> searched = new();

        if (!goal.neighbours.Any(o=> hexGrid.IsValidMoveCoordinates(o.GridCoordinates)))
        {
            return outputPath;
        }

        HexTile currentTile = start;
        currentTile.pathValues.moveCost = 0;
        currentTile.pathValues.heurCost = Heuristic(currentTile, goal);

        toSearchFromStart.Add(currentTile);

        while (toSearchFromStart.Count > 0)
        {
            HexTile lowestTotalCostTile = toSearchFromStart[0];
            for (int i = 0; i < toSearchFromStart.Count; ++i)
            {
                if (toSearchFromStart[i].pathValues.Total < lowestTotalCostTile.pathValues.Total)
                {
                    lowestTotalCostTile = toSearchFromStart[i];
                }
            }
            currentTile = lowestTotalCostTile;

            toSearchFromStart.Remove(currentTile);
            searched.Add(currentTile);

            if (currentTile == goal)
            {
                break;
            }

            foreach (HexTile neighbour in currentTile.neighbours)
            {
                if (hexGrid.IsValidMoveCoordinates(neighbour.GridCoordinates) && !searched.Contains(neighbour))
                {
                    if (!toSearchFromStart.Contains(neighbour))
                    {
                        neighbour.pathValues.moveCost = currentTile.pathValues.moveCost + 1;//Heuristic(currentTile, neighbour);
                        neighbour.pathValues.heurCost = Heuristic(neighbour, goal);
                        toSearchFromStart.Add(neighbour);
                    }
                    else if (neighbour.pathValues.moveCost > currentTile.pathValues.moveCost + 1)//Heuristic(currentTile, neighbour))
                    {
                        neighbour.pathValues.moveCost = currentTile.pathValues.moveCost + 1;//Heuristic(currentTile, neighbour);
                    }
                }
            }
        }

        if (searched.Contains(goal))
        {
            currentTile = goal;
            List<HexTile> nextTileCandidates = new();

            outputPath.Add(currentTile);

            for (int i = goal.pathValues.moveCost - 1; i >= 0; --i)
            {
                currentTile = searched.Find(o => o.pathValues.moveCost == i && currentTile.neighbours.Contains(o));
                outputPath.Add(currentTile);
            }

            outputPath.Reverse();
        }

        return outputPath;
    }

    protected static int Heuristic(HexTile from, HexTile to)
    {
        return HexCoord.Distance(from.GridCoordinates, to.GridCoordinates);
    }

}