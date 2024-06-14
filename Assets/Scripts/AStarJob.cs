using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[BurstCompile]
public struct AStarJob : IJob
{
    private NativeList<Node> returnPath;
    private NativeArray<Node> nativeMap;
    private int2 gridSize;
    private Node start;
    private Node end;

    public AStarJob(NativeList<Node> returnPath,
                    NativeArray<Node> nativeMap,
                    int2 gridSize,
                    Node start,
                    Node end)
    {
        this.returnPath = returnPath;
        this.nativeMap = nativeMap;
        this.gridSize = gridSize;
        this.start = start;
        this.end = end;
    }

    public void Execute()
    {
        var openList = new NativeList<int>(Allocator.Temp);
        var closeList = new NativeList<int>(Allocator.Temp);

        openList.Add(start.index);
        start.g = 0;
        nativeMap[start.index] = start;

        var neighborOffsets = new NativeArray<int2>(4, Allocator.Temp);
        neighborOffsets[0] = new(1, 0);
        neighborOffsets[1] = new(-1, 0);
        neighborOffsets[2] = new(0, 1);
        neighborOffsets[3] = new(0, -1);

        while (openList.Length > 0)
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

        openList.Dispose();
        closeList.Dispose();
        neighborOffsets.Dispose();
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

    private readonly int GetIndex(int2 pos)
    {
        return pos.x + pos.y * gridSize.x;
    }

    private readonly int GetLowestCostIndex(NativeList<int> openList, NativeArray<Node> map)
    {
        var lowest = map[openList[0]];
        for (int i = 0; i < openList.Length; i++)
        {
            var node = map[openList[i]];
            if (node.F < lowest.F || (node.F == lowest.F && node.h < lowest.h))
            {
                lowest = node;
            }
        }
        return lowest.index;
    }

    private readonly bool IsInbound(int2 pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < gridSize.x && pos.y < gridSize.y;
    }

    private readonly bool IsWalkable(Node node)
    {
        return node.type == NodeType.Road;
    }

    private readonly float ManhattanDistance(int2 from, int2 to)
    {
        return Mathf.Abs(to.x - from.x) + Mathf.Abs(to.y - from.y);
    }
}