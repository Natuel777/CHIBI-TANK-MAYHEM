using UnityEngine;
using System.Collections.Generic;

public sealed class NavGraph
{
    private Vector3[] _positions;
    private int[] _neighborStart, _neighbors;
    private float[] _neighborCosts;

    public int NodeCount => _positions.Length;

    public NavGraph(Vector3[] positions, List<int>[] neighbors, List<float>[] neighborsCost)
    {
        if(neighbors.Length != positions.Length || neighborsCost.Length != positions.Length)
            Debug.LogError("positions, neighbors y neighborsCost deben tener un elemento por nodo.");

        _positions = positions;
        _neighborStart = new int[positions.Length + 1];

        for(int i = 0; i < positions.Length; i++)
        {
            if(neighbors[i].Count != neighborsCost[i].Count)
                Debug.LogError($"El nodo {i} tiene {neighbors[i].Count} vecinos pero {neighborsCost[i].Count} costos.");

            // _neighborStart es un array que contiene el índice de inicio de los vecinos de cada nodo en el array _neighbors.
            _neighborStart[i + 1] = _neighborStart[i] + neighbors[i].Count;
        }

        int totalEdges = _neighborStart[positions.Length];
        _neighbors = new int[totalEdges];
        _neighborCosts = new float[totalEdges];

        for(int i = 0; i < positions.Length; i++)
        {
            // Cada valor de neighbors contiene todos los vecinos del nodo i.
            // List.CopyTo copia los elementos de la lista neighbors[i] en el array _neighbors a partir del índice _neighborStart[i].
            neighbors[i].CopyTo(_neighbors, _neighborStart[i]);
            // Cada valor de neighborsCost contiene los costos (distancia real) de las aristas del nodo i hacia cada uno de sus vecinos.
            // List.CopyTo copia los elementos de la lista neighborsCost[i] en el array _neighborCosts 
            // a partir del índice _neighborStart[i].
            neighborsCost[i].CopyTo(_neighborCosts, _neighborStart[i]);
        }
    }

    public Vector3 GetPosition(int node) => _positions[node];

    // El tramo de vecinos de node va de _neighborStart[node] a _neighborStart[node + 1] (sin incluir).
    // Su largo, es decir la cantidad de vecinos de node, es la resta entre ambos.
    public int GetNeighborCount(int node) => _neighborStart[node + 1] - _neighborStart[node];

    public int GetNeighbor(int node, int k)
    {
        int position = _neighborStart[node] + k;
        return _neighbors[position];
    } 

    public float GetNeighborCost(int node, int k)
    {
        int position = _neighborStart[node] + k;
        return _neighborCosts[position];
    } 

    public int FindNearestNode(Vector3 worldPos)
    {
        int nearestNode = -1;
        float nearestDistance = float.MaxValue;

        for(int i = 0; i < _positions.Length; i++)
        {
            float sqrDistance = (_positions[i] - worldPos).sqrMagnitude;

            if(sqrDistance < nearestDistance)
            {
                nearestDistance = sqrDistance;
                nearestNode = i;
            }
        }

        return nearestNode;
    }
}
