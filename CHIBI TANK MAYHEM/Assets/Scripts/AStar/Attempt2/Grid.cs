using UnityEngine;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    [SerializeField] private Vector2 _gridWorldSize;
    [SerializeField] private float _nodeRadius;
    [SerializeField] private LayerMask _unwalkeableMasks;

    private Node[,] _grid;
    private float _nodeDiamenter;
    private int _nodeCountInGridSizeX, _nodeCountInGridSizeY;

    public int MaxSize => _nodeCountInGridSizeX * _nodeCountInGridSizeY;

    private void Start() 
    {
        _nodeDiamenter = _nodeRadius * 2;
        // Round redondea el valor de un float al entero más cercano. Ej: 1,2f = 1, 1,5f = 2, 1,8f = 2.
        _nodeCountInGridSizeX = Mathf.RoundToInt(_gridWorldSize.x / _nodeDiamenter);
        _nodeCountInGridSizeY = Mathf.RoundToInt(_gridWorldSize.y / _nodeDiamenter);
        CreateGrid();
    }

    private void CreateGrid()
    {
        _grid = new Node[_nodeCountInGridSizeX, _nodeCountInGridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * _gridWorldSize.x / 2 - Vector3.forward * _gridWorldSize.y / 2;

        for(int x = 0; x < _nodeCountInGridSizeX; x++)
        {
            for(int y = 0; y < _nodeCountInGridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiamenter + _nodeRadius) + Vector3.forward * (y * _nodeDiamenter + _nodeRadius);
                
                // Definición de la documentación de Unity sobre Physics.CheckSphere
                // Returns true if there are any colliders overlapping the sphere defined by position and radius in world coordinates.
                bool walkable = !(Physics.CheckSphere(worldPoint, _nodeRadius, _unwalkeableMasks));
                _grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        // Normalizamos el valor de worldPosition.x y worldPosition.z para que estén entre 0 y 1.
        float percentX = (worldPosition.x - transform.position.x + _gridWorldSize.x / 2) / _gridWorldSize.x;
        float percentY = (worldPosition.z - transform.position.z + _gridWorldSize.y / 2) / _gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        int x = Mathf.RoundToInt((_nodeCountInGridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((_nodeCountInGridSizeY - 1) * percentY);
        return _grid[x, y];
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        // Recorremos los nodos vecinos en un cuadrado de 3x3 alrededor del nodo actual
        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++)
            {
                if(x == 0 && y == 0) continue;

                // Calculamos las coordenadas del nodo vecino en x e y.
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                // Si no estamos fuera de los límites, agregamos el nodo vecino a la lista.
                if(checkX >= 0 && checkX < _nodeCountInGridSizeX && checkY >= 0 && checkY < _nodeCountInGridSizeY)
                    neighbors.Add(_grid[checkX, checkY]);
            }
        }

        return neighbors;
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(_gridWorldSize.x, 1f, _gridWorldSize.y));

        if(_grid != null)
        {
            foreach(Node node in _grid)
            {
                if(node.isWalkable)
                    Gizmos.color = Color.white;
                
                else Gizmos.color = Color.red;

                Gizmos.DrawCube(node.worldPosition, Vector3.one * (_nodeDiamenter - 0.4f));
            }  
        }
    }
    #endif
}
