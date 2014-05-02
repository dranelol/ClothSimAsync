using UnityEngine;
using System;
using System.Collections.Generic;

public class MinHeap<T> where T : IComparable
{
    private int size;

    public int Size
    {
        get
        {
            return size;
        }

        set
        {
            size = value;
        }
    }

    private List<T> heap;

    public MinHeap()
    {
        heap = new List<T>();
        size = 0;
    }

    public MinHeap(List<T> copyList)
    {
        heap.AddRange(copyList);
        size = copyList.Count;
        buildHeap();

    }

    public bool IsEmpty()
    {
        return heap.Count == 0;
    }

    public T FindMin()
    {
        return heap[0];
    }

    public void DeleteMin()
    {
        size = size - 1;
        if (IsEmpty())
        {
            return;
        }
        heap[0] = heap[size];
        percolateDown(0);
    }

    private void buildHeap()
    {
        for (int i = size / 2; i > 0; i--)
        {
            percolateDown(i);
        }
    }

    public void Insert(T item)
    {
        Console.WriteLine("adding " + item.ToString());
        heap.Add(item);
        percolateUp(size);
        size++;
    }

    private void percolateUp(int node)
    {
        T temp = heap[node];
        for (; node > 0 && temp.CompareTo(heap[node / 2]) < 0; node = node / 2)
        {
            Console.WriteLine("inside percolate up loop");
            heap[node] = heap[node / 2];
        }
        heap[node] = temp;
    }

    private void percolateDown(int node)
    {
        int child;
        T temp = heap[node];

        for (; node * 2 <= size; node = child)
        {
            child = node * 2;

            //compare to returns less than 0 if left is less than right
            if (child != size && heap[child + 1].CompareTo(heap[child]) < 0)
            {
                child++;
            }

            if (heap[child].CompareTo(temp) < 0)
            {
                heap[node] = heap[child];
            }

            else
            {
                break;
            }
        }

        heap[node] = temp;
    }







}
