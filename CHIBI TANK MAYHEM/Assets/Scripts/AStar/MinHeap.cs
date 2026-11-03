using System;

public sealed class MinHeap
{
    private int[] _nodes;
    private float[] _priorities;
    private int _count;

    public int Count => _count;

    public MinHeap(int capacity)
    {
        _nodes = new int[capacity];
        _priorities = new float[capacity];
    }

    public void Clear() => _count = 0;

    public void Push(int node, float priority)
    {
        if(_count == _nodes.Length) Grow();

        _nodes[_count] = node;
        _priorities[_count] = priority;
        SiftUp(_count);
        _count++;
    }

    public int Pop()
    {
        int top = _nodes[0];

        _count--;
        _nodes[0] = _nodes[_count];
        _priorities[0] = _priorities[_count];
        SiftDown(0);

        return top;
    }

    private void Grow()
    {
        int newSize = Math.Max(4, _nodes.Length * 2);
        Array.Resize(ref _nodes, newSize);
        Array.Resize(ref _priorities, newSize);
    }


    //Regla MinHeap: Padre de index = (index - 1) / 2. Hijos de index = 2 * index + 1 y 2 * index + 2.
    // Cada padre es menor o igual que sus hijos. Por eso el mínimo siempre está en el índice 0.
    private void SiftUp(int index)
    {
        while(index > 0)
        {
            int parent = (index - 1) / 2;

            if(_priorities[index] >= _priorities[parent]) break;

            Swap(index, parent);
            index = parent;
        }
        
    }

    private void SiftDown(int i)
    {
        while(true)
        {
            int left = 2 * i + 1;
            int right = left + 1;
            int smallest = i;

            if(left < _count && _priorities[left] < _priorities[smallest]) smallest = left;
            if(right < _count && _priorities[right] < _priorities[smallest]) smallest = right;

            if(smallest == i) break;

            Swap(i, smallest);
            i = smallest;
        }
    }

    private void Swap(int a, int b)
    {
        (_nodes[a], _nodes[b]) = (_nodes[b], _nodes[a]);
        (_priorities[a], _priorities[b]) = (_priorities[b], _priorities[a]);
    }
}
