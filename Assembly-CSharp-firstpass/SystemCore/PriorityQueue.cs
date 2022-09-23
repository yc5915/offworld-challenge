using UnityEngine;
using System.Collections.Generic;

namespace Offworld.SystemCore
{
    public class PriorityQueue<T>
    {
        private class Node
        {
            public readonly T value;
            private readonly int priority;
            private readonly int nodeID;

            public Node(T value, int priority, int nodeID)
            {
                this.value = value;
                this.priority = priority;
                this.nodeID = nodeID;
            }

            public int CompareTo(Node rhs)
            {
                if(priority != rhs.priority)
                    return (priority - rhs.priority); //ascending order
                else
                    return (nodeID - rhs.nodeID); //ascending order
            }
        }

        private List<Node> nodes = new List<Node>(10);
        private int nextNodeID = 0;

        public int Count { get { return nodes.Count; } }

        public PriorityQueue()
        {
        }

        //O(N)
        public void Clear()
        {
            nodes.Clear();
        }

        //O(1)
        public bool IsEmpty()
        {
            return nodes.IsEmpty();
        }

        //O(log(N)) amortized cost
        public void Enqueue(T value, int priority)
        {
            nodes.Add(new Node(value, priority, nextNodeID++));
            CascadeUp(Count - 1);
        }

        //O(1)
        public T Peek()
        {
            if(IsEmpty())
                throw new System.InvalidOperationException("PriorityQueue is empty");

            return nodes[0].value;
        }

        //O(log(N))
        public T Dequeue()
        {
            if(IsEmpty())
                throw new System.InvalidOperationException("PriorityQueue is empty");

            T result = nodes[0].value;

            //move last element to front and cascade down
            nodes[0] = nodes[Count - 1];
            nodes.RemoveAt(Count - 1);
            CascadeDown(0);
            return result;
        }

        private void CascadeUp(int index)
        {
            while(true)
            {
                if(index == 0) //reached root
                    break;

                //check if needs to switch with parent
                int parentIndex = GetParentIndex(index);
                if(nodes[index].CompareTo(nodes[parentIndex]) < 0)
                {
                    ListUtilities.Swap(nodes, index, parentIndex);
                    index = parentIndex;
                }
                else //no need to continue
                {
                    break;
                }
            }
        }

        private void CascadeDown(int index)
        {
            while(true)
            {
                //check if children
                int childIndex = GetFirstChildIndex(index);
                if(childIndex >= Count) //reached leaf
                    break;

                //find lowest child
                int childIndex2 = childIndex + 1;
                if((childIndex2 < Count) && (nodes[childIndex2].CompareTo(nodes[childIndex]) < 0))
                    childIndex = childIndex2;

                //check if needs to switch with child
                if(nodes[index].CompareTo(nodes[childIndex]) > 0)
                {
                    ListUtilities.Swap(nodes, index, childIndex);
                    index = childIndex;
                }
                else //no need to continue
                {
                    break;
                }
            }
        }

        private int GetParentIndex(int index)
        {
            return ((index + 1) >> 1) - 1; //index/2 (in base-1)
        }

        private int GetFirstChildIndex(int index)
        {
            return ((index + 1) << 1) - 1; //index*2 (in base-1)
        }
    }
}