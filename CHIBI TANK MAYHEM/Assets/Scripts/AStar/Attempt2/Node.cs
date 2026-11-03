using UnityEngine;

public class Node 
{
    public bool isWalkable;
    public Vector3 worldPosition;

    public Node(bool isWalkable, Vector3 worldPosition)
    {
        this.isWalkable = isWalkable;
        this.worldPosition = worldPosition;
    }
}
