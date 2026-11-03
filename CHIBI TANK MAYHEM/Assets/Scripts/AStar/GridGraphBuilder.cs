using UnityEngine;
using System.Collections.Generic;

// Este script se encarga de setear y crear un grafo de navegación (NavGraph).
public static class GridGraphBuilder
{
    private static readonly Vector2Int[] _directions =
    {
        new(0, 1), new(1, 0), new(0, -1), new(-1, 0)
    };

    public static NavGraph CreateNavGraph(int width, int height, float spacing, Vector3 origin, bool[,] blocked)
    {
        int nodeCount = width * height;

        //Array con la posición en el mundo de cada nodo del grafo
        Vector3[] positions = new Vector3[nodeCount];
        
        //Listas con los vecinos de cada nodo y el costo (distancia real) de la arista hacia cada uno de ellos
        List<int>[] neighbors = new List<int>[nodeCount];
        List<float>[] neighborsCost = new List<float>[nodeCount];

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                int index = GetIndex(x, y, height);

                // Se setea la posición de cada nodo en mundo. Parte de un Vector3 origin y se le multiplica un spacing entre nodos.
                positions[index] = origin + new Vector3(x, 0f, y) * spacing;
                neighbors[index] = new List<int>();
                neighborsCost[index] = new List<float>();
            }
        }

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                if(IsBlocked(blocked, x, y)) continue;

                int index = GetIndex(x, y, height);

                foreach(Vector2Int dir in _directions)
                {
                    int nx = x + dir.x;
                    int ny = y + dir.y;

                    //Verificamos que el vecino no se salga del grid y que no este bloqueado
                    if(nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                    
                    if(IsBlocked(blocked, nx, ny)) continue;

                    // Obtenemos el index del vecino, lo agregamos como vecino válido del nodo index
                    // y también agregamos el costo de la arista (distancia real entre el nodo y su vecino).
                    int neighborIndex = GetIndex(nx, ny, height);
                    neighbors[index].Add(neighborIndex);
                    neighborsCost[index].Add(Vector3.Distance(positions[neighborIndex], positions[index]));
                }
            }
        }

        return new NavGraph(positions, neighbors, neighborsCost);
    }

    // Index de una matriz 2D = x * height + y
    public static int GetIndex(int x, int y, int height) => x * height + y;

    private static bool IsBlocked(bool[,] blocked, int x, int y) => blocked != null && blocked[x, y];
}
