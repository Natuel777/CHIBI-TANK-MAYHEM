using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder : MonoBehaviour
{
    private Grid _grid;

    #if UNITY_EDITOR
    [SerializeField] private Transform seeker, target;
    #endif

    private void Start()
    {
        _grid = GetComponent<Grid>();
    }

    #if UNITY_EDITOR
    private void Update()
    {
        FindPath(seeker.position, target.position);
    }
    #endif

    private void FindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        Node startNode = _grid.NodeFromWorldPoint(startPosition);
        Node targetNode = _grid.NodeFromWorldPoint(targetPosition);
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>(); 
        openSet.Add(startNode);

        while(openSet.Count > 0)
        {
            Node currentNode = openSet[0];

            for(int i = 1; i < openSet.Count; i++)
            {
                if(openSet[i].fCost < currentNode.fCost || 
                    openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                    currentNode = openSet[i];
            }

            closedSet.Add(currentNode);
            openSet.Remove(currentNode);

            if(currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            foreach(Node neighbor in _grid.GetNeighbors(currentNode))
            {
                if(!neighbor.isWalkable || closedSet.Contains(neighbor)) continue;

                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

                if(newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if(!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
    }

    private void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Add(startNode);
        path.Reverse();
        
        #if UNITY_EDITOR
        _grid.path = path;
        #endif
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        // Calculamos la distancia entre dos nodos con un resultado devuelto en números naturales.
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        // ¿Por qué 13 y 10?
        // Se da por sentado que la distancia entre nodo consecutivo al otro (no diagonal) es 1.
        // La distancia entre nodos diagonales es la hipotenusa de un triángulo rectángulo con catetos de longitud 1.
        // La hipotenussa de este caso es √2 = 1.41421356237. Trabajar con este tipo de números es dificíl,
        // por lo que se multiplica por 10 para trabajar con números enteros. 1.41421356237 * 10 = 14.1421356237.
        // Por lo tanto, la distancia entre nodos consecutivos es 10 y la distancia entre nodos diagonales es 14.
        if(distX > distY)
        {
            // 14 * la cantidad de nodos verticales, aptos para moverse en diagonal.
            // + el resto de nodos horizontales, calculados con: 10 * (distX - distY)
            return 14 * distY + 10 * (distX - distY);
        }
            
        // Caso contrario: Hay más nodos verticales que horizontales, por lo que se calcula las diagonales 
        // con la cantidad de nodos horizontales y el resto de nodos verticales con: 10 * (distY - distX)
        return 14 * distX + 10 * (distY - distX);
    }
}
