using System.Collections.Generic;

public class FHComparer : IComparer<Node>
{
    public int Compare(Node x, Node y)
    {
        return x.F > y.F || (x.F == y.F && x.h > y.h) ? 1 : -1;
    }
}