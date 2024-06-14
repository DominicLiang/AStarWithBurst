using System.Collections.Generic;
using UnityEngine;

public class AStar
{
    private List<ObjectNode> returnPath;
    private List<ObjectNode> nativeMap;
    private Vector2Int gridSize;
    private ObjectNode start;
    private ObjectNode end;

    public AStar(List<ObjectNode> returnPath,
                 List<ObjectNode> nativeMap,
                 Vector2Int gridSize,
                 ObjectNode start,
                 ObjectNode end)
    {
        this.returnPath = returnPath;
        this.nativeMap = nativeMap;
        this.gridSize = gridSize;
        this.start = start;
        this.end = end;
    }

    public void Execute()
    {
        var openList = new List<int>();
        var closeList = new List<int>();

        openList.Add(start.index);
        start.g = 0;
        nativeMap[start.index] = start;

        var neighborOffsets = new Vector2Int[4];
        neighborOffsets[0] = new(1, 0);
        neighborOffsets[1] = new(-1, 0);
        neighborOffsets[2] = new(0, 1);
        neighborOffsets[3] = new(0, -1);

        while (openList.Count > 0)
        {
            var currentIndex = GetLowestCostIndex(openList, nativeMap);
            var current = nativeMap[currentIndex];

            if (currentIndex == end.index)
            {
                GetPath();
                break;
            }

            openList.RemoveAt(0);
            closeList.Add(currentIndex);

            for (int i = 0; i < neighborOffsets.Length; i++)
            {
                var neighborPos = current.pos + neighborOffsets[i];

                if (!IsInbound(neighborPos)) continue;

                var neighborIndex = GetIndex(neighborPos);
                var neighbor = nativeMap[neighborIndex];

                if (!IsWalkable(neighbor)) continue;

                if (closeList.Contains(neighborIndex)) continue;
                if (openList.Contains(neighborIndex)) continue;

                var neighborG = current.g + ManhattanDistance(current.pos, neighborPos);
                if (neighborG >= neighbor.g) continue;

                neighbor.g = neighborG;
                neighbor.h = ManhattanDistance(neighborPos, end.pos);
                neighbor.parentIndex = currentIndex;
                nativeMap[neighborIndex] = neighbor;

                openList.Add(neighborIndex);
            }
        }
    }

    private void GetPath()
    {
        var endNode = nativeMap[end.index];
        returnPath.Add(endNode);
        var returnCurrent = endNode;

        while (returnCurrent.parentIndex != -1)
        {
            var parent = nativeMap[returnCurrent.parentIndex];
            returnPath.Add(parent);
            returnCurrent = parent;
        }
    }

    private int GetIndex(Vector2Int pos)
    {
        return pos.x + pos.y * gridSize.x;
    }

    private int GetLowestCostIndex(List<int> openList, List<ObjectNode> map)
    {
        var lowest = map[openList[0]];
        for (int i = 0; i < openList.Count; i++)
        {
            var node = map[openList[i]];
            if (node.F < lowest.F || (node.F == lowest.F && node.h < lowest.h))
            {
                lowest = node;
            }
        }
        return lowest.index;
    }

    private bool IsInbound(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < gridSize.x && pos.y < gridSize.y;
    }

    private bool IsWalkable(ObjectNode node)
    {
        return node.type == NodeType.Road;
    }

    private float ManhattanDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(to.x - from.x) + Mathf.Abs(to.y - from.y);
    }
}
