using UnityEngine;

public class Node 
{
    public bool isWalkable;
    public Vector3 worldPosition;
    public int gCost, hCost, gridX, gridY;
    public Node parent;

    public int fCost => gCost + hCost;

    public Node(bool isWalkable, Vector3 worldPosition, int gridX, int gridY)
    {
        this.isWalkable = isWalkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}
