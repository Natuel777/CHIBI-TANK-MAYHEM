using System.Collections.Generic;
using UnityEngine;

public class PathfindingDebug : MonoBehaviour
{
    //[Header("Grilla")]
    //[SerializeField] private int _width = 20;
    //[SerializeField] private int _height = 12;
    //[SerializeField] private float _spacing = 1f;
    //[SerializeField, Range(0f, 0.5f)] private float _obstacleChance = 0.25f;
    //[SerializeField] private int _seed = 1;
//
    //[Header("Búsqueda")]
    //[SerializeField] private Vector2Int _start = new(0, 0);
    //[SerializeField] private Vector2Int _goal = new(19, 11);
    //[SerializeField, Range(0f, 3f)] private float _heuristicWeight = 1f; //0 = Dijkstra, 1 = A*
//
    //private bool[,] _blocked;
    //private NavGraph _graph;
    //private AStarPathfinder _pathfinder;
    //private readonly List<int> _path = new();
//
    //private void OnValidate()
    //{
    //    _width = Mathf.Max(1, _width);
    //    _height = Mathf.Max(1, _height);
    //    _start = new Vector2Int(Mathf.Clamp(_start.x, 0, _width - 1), Mathf.Clamp(_start.y, 0, _height - 1));
    //    _goal = new Vector2Int(Mathf.Clamp(_goal.x, 0, _width - 1), Mathf.Clamp(_goal.y, 0, _height - 1));
//
    //    //Los datos construidos ya no coinciden con el inspector, se ocultan hasta volver a buscar
    //    _graph = null;
    //}
//
    //[ContextMenu("Construir grafo y buscar")]
    //private void BuildAndSearch()
    //{
    //    _blocked = new bool[_width, _height];
    //    System.Random random = new System.Random(_seed);
//
    //    for(int x = 0; x < _width; x++)
    //        for(int y = 0; y < _height; y++)
    //            _blocked[x, y] = random.NextDouble() < _obstacleChance;
//
    //    //Inicio y meta nunca pueden estar bloqueados
    //    _blocked[_start.x, _start.y] = false;
    //    _blocked[_goal.x, _goal.y] = false;
//
    //    _graph = GridGraphBuilder.CreateNavGraph(_width, _height, _spacing, transform.position, _blocked);
    //    _pathfinder = new AStarPathfinder(_graph);
//
    //    int startNode = GridGraphBuilder.GetIndex(_start.x, _start.y, _height);
    //    int goalNode = GridGraphBuilder.GetIndex(_goal.x, _goal.y, _height);
//
    //    bool found = _pathfinder.FindPath(startNode, goalNode, _path, _heuristicWeight);
//
    //    if(found)
    //        Debug.Log($"Camino de {_path.Count} nodos, costo {_pathfinder.PathCost:F2}, nodos expandidos {_pathfinder.ExpandedCount}");
    //    else
    //        Debug.LogWarning($"Sin camino. Nodos expandidos {_pathfinder.ExpandedCount}");
    //}
//
    //private void OnDrawGizmos()
    //{
    //    if(_graph == null) return;
//
    //    //Aristas, una sola vez por par
    //    Gizmos.color = new Color(1f, 1f, 1f, 0.15f);
    //    for(int node = 0; node < _graph.NodeCount; node++)
    //    {
    //        Vector3 from = _graph.GetPosition(node);
    //        int count = _graph.GetNeighborCount(node);
//
    //        for(int k = 0; k < count; k++)
    //        {
    //            int neighbor = _graph.GetNeighbor(node, k);
    //            if(neighbor < node) continue;
//
    //            Gizmos.DrawLine(from, _graph.GetPosition(neighbor));
    //        }
    //    }
//
    //    //Nodos libres y bloqueados
    //    for(int x = 0; x < _width; x++)
    //    {
    //        for(int y = 0; y < _height; y++)
    //        {
    //            Vector3 pos = _graph.GetPosition(GridGraphBuilder.GetIndex(x, y, _height));
//
    //            if(_blocked[x, y])
    //            {
    //                Gizmos.color = Color.red;
    //                Gizmos.DrawCube(pos, Vector3.one * _spacing * 0.8f);
    //            }
    //            else
    //            {
    //                Gizmos.color = Color.gray;
    //                Gizmos.DrawSphere(pos, _spacing * 0.08f);
    //            }
    //        }
    //    }
//
    //    //Camino
    //    Gizmos.color = Color.yellow;
    //    Vector3 lift = Vector3.up * 0.1f;
    //    for(int i = 0; i < _path.Count - 1; i++)
    //        Gizmos.DrawLine(_graph.GetPosition(_path[i]) + lift, _graph.GetPosition(_path[i + 1]) + lift);
//
    //    //Inicio y meta
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawSphere(_graph.GetPosition(GridGraphBuilder.GetIndex(_start.x, _start.y, _height)), _spacing * 0.3f);
    //    Gizmos.color = Color.cyan;
    //    Gizmos.DrawSphere(_graph.GetPosition(GridGraphBuilder.GetIndex(_goal.x, _goal.y, _height)), _spacing * 0.3f);
    //}
}
