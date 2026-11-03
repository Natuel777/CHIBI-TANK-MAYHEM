using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder
{
    private readonly NavGraph _graph;
    private readonly MinHeap _minheap;
    private readonly float[] _gScore;
    private readonly int[] _cameFrom;
    private readonly int[] _gStamp;
    private readonly int[] _closedStamp;
    private int _currentStamp;

    public int ExpandedCount { get; private set; }
    public float PathCost { get; private set; }

    public AStarPathfinder(NavGraph graph)
    {
        _graph = graph;
        _minheap = new MinHeap(graph.NodeCount);
        _gScore = new float[graph.NodeCount];
        _cameFrom = new int[graph.NodeCount];
        _gStamp = new int[graph.NodeCount];
        _closedStamp = new int[graph.NodeCount];
    }

    public bool FindPath(int start, int goal, List<int> path, float heuristicWeight = 1f)
    {
        path.Clear();
        ExpandedCount = 0;
        PathCost = 0f;

        if(start < 0 || goal < 0) return false;

        _currentStamp++;
        _minheap.Clear();
        _gStamp[start] = _currentStamp;
        _gScore[start] = 0f;
        _cameFrom[start] = -1;
        _minheap.Push(start, heuristicWeight * Heuristic(start, goal));
        
        while(_minheap.Count > 0)
        {
            int current = _minheap.Pop();

            if(_closedStamp[current] == _currentStamp) continue;

            _closedStamp[current] = _currentStamp;
            ExpandedCount++;

            if(current == goal) 
            { 
                PathCost = _gScore[goal]; 
                BuildPath(goal, path); 
                return true; 
            }

            int currentNodeNeighborcount = _graph.GetNeighborCount(current);

            for(int k = 0; k < currentNodeNeighborcount; k++)
            {
                int neighbor = _graph.GetNeighbor(current, k);
                
                if(_closedStamp[neighbor] == _currentStamp) continue;

                // newG es lo que cuesta llegar al vecino pasando por el nodo current.
                float newG = _gScore[current] + _graph.GetNeighborCost(current, k);
                
                // Si el vecino ya tenía un costo en esta búsqueda y el nuevo no lo mejora, se ignora.
                if(_gStamp[neighbor] == _currentStamp && newG >= _gScore[neighbor]) continue;

                _gStamp[neighbor] = _currentStamp;
                _gScore[neighbor] = newG;
                _cameFrom[neighbor] = current;
                _minheap.Push(neighbor, newG + heuristicWeight * Heuristic(neighbor, goal));
            }
        }

        return false;
    }

    private float Heuristic(int a, int b) => Vector3.Distance(_graph.GetPosition(a), _graph.GetPosition(b));

    private void BuildPath(int goal, List<int> path)
    {
        int node = goal;

        while(node != -1)
        {
            path.Add(node);
            node = _cameFrom[node];
        }

        // La lista de nodos con el camino hacia goal queda al reves, List.Reverse() la invierte.
        path.Reverse();
    }
}
