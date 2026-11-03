using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private Vector2 _gridWorldSize;
    [SerializeField] private float _nodeRadius;
    [SerializeField] private LayerMask _unwalkeableMasks;

    private Node[,] _grid;
    private float _nodeDiamenter;
    private int _nodeCountInGridSizeX, _nodeCountInGridSizeY;

    private void Start() 
    {
        _nodeDiamenter = _nodeRadius * 2;
        // FloorToInt redondea el valor de un float al entero más cercano tirando hacia abajo. Ej: 1,2f = 1, 1,5f = 1, 1,8f = 1.
        _nodeCountInGridSizeX = Mathf.FloorToInt(_gridWorldSize.x / _nodeDiamenter);
        _nodeCountInGridSizeY = Mathf.FloorToInt(_gridWorldSize.y / _nodeDiamenter);
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
                _grid[x, y] = new Node(walkable, worldPoint);
            }
        }
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
