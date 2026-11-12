using UnityEngine;

public class Node : IHeapItem<Node>
{
    public bool isWalkable;
    public Vector3 worldPosition;
    public int gCost, hCost, gridX, gridY, movementPenalty;
    private int _heapIndex;
    public Node parent;

    public int fCost => gCost + hCost;
    public int HeapIndex {get => _heapIndex; set => _heapIndex = value;}

    public Node(bool isWalkable, Vector3 worldPosition, int gridX, int gridY, int movementPenalty)
    {
        this.isWalkable = isWalkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
        this.movementPenalty = movementPenalty;
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);

        if(compare == 0)
            compare = hCost.CompareTo(nodeToCompare.hCost);

        // CompareTo devueve 1 si el fCost / hCost es mayor al fCost / hCost
        // del nodo a comparar. Pero en este caso, queremos devolver el nodo
        // de menor costo (fCost o hCost), por ende, hacemos
        // return compare * -1;
        return -compare;
    }
}
