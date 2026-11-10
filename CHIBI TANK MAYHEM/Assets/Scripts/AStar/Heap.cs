using UnityEngine;
using System.Collections;
using System;

public class Heap<T> where T : IHeapItem<T>
{
    private T[] _items;
    private int _currentItemCount;

    public int Count => _currentItemCount;

    public Heap(int maxHeapSize)
    {
        _items = new T[maxHeapSize];
    }

    public void Add(T item)
    {
        item.HeapIndex = _currentItemCount; 
        _items[_currentItemCount] = item;
        SortUp(item);
        _currentItemCount++;
    }

    public T Pop()
    {
        T firstItem = _items[0];
        _currentItemCount--;
        _items[0] = _items[_currentItemCount];
        _items[0].HeapIndex = 0;
        SortDown(_items[0]);
        return firstItem;
    }

    // Verificación de si el heap cumple la regla 
    //"El padre de cada nodo debe tener un valor menor a su hijo"
    // Si no se cumple, se intercambia padre con hijo hacien (n - 1) / 2 para obtener el índice del padre.
    private void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;

        while(true)
        {
            T parentItem = _items[parentIndex];

            // Si el item es mayor que su padre, se intercambian.
            if(item.CompareTo(parentItem) > 0)
                Swap(item, parentItem);

            else return;

            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    //Obtenemos el index del hijo izquierdo y derecho de un nodo, y verificamos si el nodo item es menor que alguno de sus hijos.
    //Si lo es, se intercambia con el hijo mayor y se repite el proceso hasta que el nodo item sea mayor que sus hijos.
    private void SortDown(T item)
    {
        while(true)
        {
            int leftChildrenIndex = 2 * item.HeapIndex + 1;
            int rightChildrenIndex = 2 * item.HeapIndex + 2;

            if(leftChildrenIndex >= _currentItemCount) return;

            int swapIndex = leftChildrenIndex;

            if(rightChildrenIndex < _currentItemCount && _items[leftChildrenIndex].CompareTo(_items[rightChildrenIndex]) < 0)
                swapIndex = rightChildrenIndex;

            if(item.CompareTo(_items[swapIndex]) >= 0) return;

            Swap(item, _items[swapIndex]);
        }
    }

    private void Swap(T itemA, T itemB)
    {
        _items[itemA.HeapIndex] = itemB;
        _items[itemB.HeapIndex] = itemA;
        int itemAIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
    }

    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    public bool Contains(T item) => Equals(_items[item.HeapIndex], item);
}

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex {get; set;}
}
