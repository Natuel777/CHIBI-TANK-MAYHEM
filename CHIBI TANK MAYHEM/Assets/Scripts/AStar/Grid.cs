using UnityEngine;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    [SerializeField] private Vector2 _gridWorldSize;
    [SerializeField] private float _nodeRadius;
    [SerializeField] private LayerMask _unwalkeableMasks;

    #region Weights
    [Tooltip("Each TerrainType object should be assign with one layerMask per object, otherwise will mess up the code")]
    [SerializeField] private TerrainType[] _walkableRegions;
    [SerializeField] private int _obstacleProximityPenalty = 5;
    private LayerMask _walkablesMask;
    private Dictionary<int, int> _walkableRegionsDic = new Dictionary<int, int>();

    #if UNITY_EDITOR
    private int _penaltyMin = int.MaxValue, _penaltyMax = int.MinValue;
    #endif
    #endregion

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

        foreach(TerrainType type in _walkableRegions)
        {
            _walkablesMask.value |=  type.terrainMask.value;
            _walkableRegionsDic.Add((int)Mathf.Log(type.terrainMask.value, 2), type.terrainPenalty);
        }

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
                // El nodo se crea siempre, también si no es caminable: si quedara en null,
                // OnDrawGizmos, GetNeighbors y NodeFromWorldPoint fallarían con NullReferenceException.
                int movementPenalty = FindGroundType(walkable, worldPoint);
                _grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
            }
        }

        BlurPenaltyMap(3);
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

    private int FindGroundType(bool walkable, Vector3 worldPoint)
    {
        int movementPenalty = 0;
        Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
        RaycastHit hit;

        // El orden importa: TryGetValue(..., out movementPenalty) SIEMPRE escribe en la variable (0 si no encuentra la capa).
        // El rayo ignora la capa Obstacle (no está en _walkablesMask), así que atraviesa el obstáculo y cae sobre el suelo
        // de abajo. Si la penalidad del obstáculo se sumara antes del raycast, el TryGetValue la pisaría con la del suelo.
        if(Physics.Raycast(ray, out hit, 100, _walkablesMask))
            _walkableRegionsDic.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);

        if(!walkable) movementPenalty += _obstacleProximityPenalty;

        return movementPenalty;
    }

    //Blur box algorithm
    // In this point idk wtf i'm coding, i still have much maths to learn.
    // I know what the algorithm does but how and why does it work like this, idk
    // #headache
    private void BlurPenaltyMap(int blurSize)
    {
        int kernelSize = blurSize * 2 + 1;
        int kernelExtents = (kernelSize - 1) / 2;
        int[,] penaltiesHorizontalPass = new int[_nodeCountInGridSizeX, _nodeCountInGridSizeY];
        int[,] penaltiesVerticalPass = new int[_nodeCountInGridSizeX, _nodeCountInGridSizeY];

        for(int y = 0; y < _nodeCountInGridSizeY; y++)
        {
            for(int x = -kernelExtents; x <= kernelExtents; x++)
            {
                int sampleX = Mathf.Clamp(x, 0 ,kernelExtents);
                penaltiesHorizontalPass[0, y] += _grid[sampleX, y].movementPenalty;
            }

            for(int x2 = 1; x2 < _nodeCountInGridSizeX; x2++)
            {
                int removeIndex = Mathf.Clamp(x2 - kernelExtents -1, 0 , _nodeCountInGridSizeX);
                int addIndex = Mathf.Clamp(x2 + kernelExtents, 0, _nodeCountInGridSizeX - 1);
                penaltiesHorizontalPass[x2,y] = penaltiesHorizontalPass[x2 - 1, y] - _grid[removeIndex, y].movementPenalty + _grid[addIndex,y].movementPenalty; 
            }
        }

        for(int x = 0; x < _nodeCountInGridSizeX; x++)
        {
            for(int y = -kernelExtents; y <= kernelExtents; y++)
            {
                int sampleY = Mathf.Clamp(y, 0 ,kernelExtents);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
            }

            int blurPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x,0] / (kernelSize*kernelSize));
            _grid[x,0].movementPenalty = blurPenalty;

            for(int y2 = 1; y2 < _nodeCountInGridSizeY; y2++)
            {
                int removeIndex = Mathf.Clamp(y2 - kernelExtents -1, 0 , _nodeCountInGridSizeY);
                int addIndex = Mathf.Clamp(y2 + kernelExtents, 0, _nodeCountInGridSizeY - 1);
                penaltiesVerticalPass[x,y2] = penaltiesVerticalPass[x, y2 - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x,addIndex]; 
                blurPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x,y2] / (kernelSize*kernelSize));
                _grid[x,y2].movementPenalty = blurPenalty;
            }
        }

        #if UNITY_EDITOR
        // Rango de la escala de colores del gizmo. Solo cuentan los nodos caminables: los obstáculos (rojos) tienen la
        // penalidad más alta y, si entraran acá, aplastarían la escala y la aureola se vería casi blanca.
        foreach(Node node in _grid)
        {
            if(!node.isWalkable) continue;

            if(node.movementPenalty > _penaltyMax) _penaltyMax = node.movementPenalty;

            if(node.movementPenalty < _penaltyMin) _penaltyMin = node.movementPenalty;
        }
        #endif
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
                {
                    Gizmos.color = Color.Lerp(Color.white, Color.orange, Mathf.InverseLerp(_penaltyMin, 
                                                                                    _penaltyMax, 
                                                                                    node.movementPenalty));
                }
                
                else Gizmos.color = Color.red;

                Gizmos.DrawCube(node.worldPosition, Vector3.one * (_nodeDiamenter - 0.4f));
            }  
        }
    }
    #endif

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }
}
