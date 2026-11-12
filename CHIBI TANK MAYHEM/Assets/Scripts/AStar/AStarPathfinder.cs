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
            // Openset son los nodos ya verificados que no se recorrieron y son candidatos a recorrer.
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

                    int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor) + currentNode.movementPenalty;

                    if(newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, targetNode);
                        neighbor.parent = currentNode;

                        if(!openSet.Contains(neighbor))
                            openSet.Add(neighbor);

                        else openSet.UpdateItem(neighbor);
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
        // La lista se armó desde el destino hasta el inicio: se la da vuelta para recorrerla en orden de marcha.
        path.Reverse();
        return SimplifyPath(path);
    }

    private Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 oldDirection = Vector2.zero;

        // path[0] es el nodo de inicio y no se agrega: el agente ya está ahí.
        for(int i = 1; i < path.Count; i++)
        {
            // Obtenemos la dirección entre el nodo i y el nodo anterior.
            // Si esta dirección es igual a oldDirection, significa que
            // los nodos, en este tramo, no cambiaron de dirección, y por ende
            // se pueden simplificar en el array de waypoints (Optimización).
            Vector2 newDirection = new Vector2(path[i].gridX - path[i-1].gridX,
                                                path[i].gridY - path[i-1].gridY);
            
            // Si cambió la dirección respecto del paso anterior, el nodo anterior (i - 1) es una esquina y se guarda.
            if(i > 1 && newDirection != oldDirection)
                waypoints.Add(path[i-1].worldPosition);
            
            oldDirection = newDirection;
        }

        // El último waypoint siempre es el destino.
        waypoints.Add(path[path.Count - 1].worldPosition);

        return waypoints.ToArray();
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        // Calculamos la distancia entre dos nodos con un resultado devuelto en números naturales.
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        // ¿Por qué 14 y 10?
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
