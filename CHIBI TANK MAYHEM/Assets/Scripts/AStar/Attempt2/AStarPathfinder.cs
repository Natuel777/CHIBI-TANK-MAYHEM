using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AStarPathfinder : MonoBehaviour
{
    private Grid _grid;
    private PathRequestManager _pathRequestManager;

    private void Start()
    {
        _grid = GetComponent<Grid>();
        _pathRequestManager = GetComponent<PathRequestManager>();
    }

    public void StartFindPath(Vector3 startPos, Vector3 endPos)
    {
        StartCoroutine(FindPath(startPos, endPos));
    }

    private IEnumerator FindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;
        Node startNode = _grid.NodeFromWorldPoint(startPosition);
        Node targetNode = _grid.NodeFromWorldPoint(targetPosition);

        if(startNode.isWalkable && targetNode.isWalkable)
        {
            Heap<Node> openSet = new Heap<Node>(_grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>(); 
            openSet.Add(startNode);

            while(openSet.Count > 0)
            {
                Node currentNode = openSet.Pop();
                closedSet.Add(currentNode);

                if(currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
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

        yield return null;
        
        if(pathSuccess) waypoints = RetracePath(startNode, targetNode);

        _pathRequestManager.FinishProcessingPath(waypoints, pathSuccess);
    }

    private Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Add(startNode);
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    private Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 oldDirection = Vector2.zero;

        for(int i = 0; i < path.Count; i++)
        {
            // Obtenemos la dirección entre el nodo i y el nodo anterior.
            // Si esta dirección es igual a oldDirection, significa que
            // los nodos, en este tramo, no cambiaron de dirección, y por ende
            // se pueden simplificar en el array de waypoints (Optimización).
            Vector2 newDirection = new Vector2(path[i-1].gridX - path[i].gridX,
                                                path[i-1].gridY - path[i].gridY);
            
            if(newDirection != oldDirection)
                waypoints.Add(path[i].worldPosition);
            
            oldDirection = newDirection;
        }

        return waypoints.ToArray();
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
