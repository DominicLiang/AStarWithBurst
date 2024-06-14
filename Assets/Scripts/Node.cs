using Unity.Mathematics;

public struct Node
{
    public int index;
    public int2 pos;
    public NodeType type;
    public int parentIndex;
    public float g;
    public float h;
    public readonly float F => g + h;
}




