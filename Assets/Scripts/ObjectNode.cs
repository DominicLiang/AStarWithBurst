using UnityEngine;

public class ObjectNode
{
    public int index;
    public Vector2Int pos;
    public NodeType type;
    public int parentIndex;
    public float g;
    public float h;
    public float F => g + h;
}