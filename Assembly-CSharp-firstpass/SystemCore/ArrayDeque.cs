using UnityEngine;
using System.Collections.Generic;

namespace Offworld.SystemCore
{
    public class ArrayDeque<T> : IEnumerable<T>
    {
        private T [] data;
        private int start, size;

        public int Count
        { 
            get { return size; } 
        }

        public ArrayDeque()
        {
        }

        public ArrayDeque(int capacity)
        {
            EnsureCapacity(capacity);
        }

        public ArrayDeque(IEnumerable<T> values)
        {
            foreach (T value in values)
                AddBack(value);
        }

        //O(N)
        public void Clear()
        {
            for(int i=0; i<size; i++)
                data[GetIndexAt(i)] = default(T);
            size = 0;
            start = 0;
        }

        //O(1)
        public bool IsEmpty()
        {
            return (size == 0);
        }

        //O(1) amortized cost
        public void AddBack(T value)
        {
            EnsureCapacity(size + 1);
            int index = GetIndexAt(size);
            data[index] = value;
            size++;
        }

        //O(1) amortized cost
        public void AddFront(T value)
        {
            EnsureCapacity(size + 1);
            int index = GetIndexAt(-1);
            data[index] = value;
            start = index;
            size++;
        }

        //O(1)
        public T RemoveBack()
        {
            if(IsEmpty())
                throw new System.InvalidOperationException("ArrayDeque is empty");

            int index = GetIndexAt(size - 1);
            T result = data[index];
            data[index] = default(T);
            size--;
            return result;
        }

        //O(1)
        public T RemoveFront()
        {
            if(IsEmpty())
                throw new System.InvalidOperationException("ArrayDeque is empty");
        
            int index = GetIndexAt(0);
            T result = data[index];
            data[index] = default(T);
            start = GetIndexAt(1);
            size--;
            return result;
        }

        //O(1)
        public T PeekBack()
        {
            if(IsEmpty())
                throw new System.InvalidOperationException("ArrayDeque is empty");

            int index = GetIndexAt(size - 1);
            return data[index];
        }

        //O(1)
        public T PeekFront()
        {
            if(IsEmpty())
                throw new System.InvalidOperationException("ArrayDeque is empty");
        
            int index = GetIndexAt(0);
            return data[index];
        }

        //O(1)
        public T GetAt(int index)
        {
            index = GetIndexAt(index);
            return data[index];
        }

        //O(1)
        public T this[int index]
        {
            get { return GetAt(index); }
        }

        //IEnumerable<T>
        public IEnumerator<T> GetEnumerator()
        {
            for(int i=0; i<size; i++)
                yield return GetAt(i);
        }

        //IEnumerable - http://stackoverflow.com/questions/11296810/how-do-i-implement-ienumerablet
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private int GetIndexAt(int index)
        {
            return MathUtilities.WrapInt(start + index, data.Length);
        }

        public void EnsureCapacity(int capacity)
        {
            if(data == null) //initialize nodes
            {
                int newSize = Mathf.Max(10, capacity);
                data = new T [newSize];
            }
            else if(data.Length < capacity)
            {
                //grow array by 50% and copy nodes
                int newSize = Mathf.Max(data.Length * 3 / 2, capacity);
                T [] newData = new T [newSize];
                for(int i=0; i<size; i++)
                {
                    int fromIndex = GetIndexAt(i);
                    newData[i] = data[fromIndex];
                }
                data = newData;
                start = 0;
            }
        }
    }
}