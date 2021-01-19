using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Sark.Pathfinding
{
    // Source: https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
    // https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp/blob/master/LICENSE.txt
    [NativeContainer]
    public unsafe struct NativePriorityQueue<T> : IDisposable
        where T : unmanaged
    {
        [NativeDisableUnsafePtrRestriction]
        internal UnsafeList* _nodes;

        Node LastUnsafe => ReadUnsafe(_nodes->Length - 1);

        public NativePriorityQueue(int initialCapacity, Allocator allocator)
        {
            var totalSize = UnsafeUtility.SizeOf<Node>() * (long)initialCapacity;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (allocator <= Allocator.None)
                throw new ArgumentException("Allocator must be Temp, TempJob, or Persistent", "allocator");
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity), $"{nameof(initialCapacity)} must be >= 0");

            if (totalSize > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity), $"Capacity * sizeof(T) cannot exceed {int.MaxValue} bytes");
             
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, allocator);
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);
#endif

            int nodeSize = UnsafeUtility.SizeOf<Node>();
            int nodeAlign = UnsafeUtility.AlignOf<Node>();
            _nodes = UnsafeList.Create(nodeSize, nodeAlign, initialCapacity, allocator);
            _nodes->Add<Node>(default);
        }

        public void SetCapacity(int capacity)
        {
            RequireWriteAccess();

            _nodes->SetCapacity<T>(capacity);
        }

        public int Length
        {
            get
            {
                RequireReadAccess();
                return LengthUnsafe;
            }
        }

        public bool IsEmpty => Length == 0;

        int LengthUnsafe => _nodes->Length - 1;

        public int Capacity
        {
            get
            {
                RequireReadAccess();

                return CapacityUnsafe;
            }
        }

        int CapacityUnsafe => _nodes->Capacity;

        Node ReadUnsafe(int index) => 
            UnsafeUtility.ReadArrayElement<Node>(_nodes->Ptr, index);

        void WriteUnsafe(int index, Node value) => 
            UnsafeUtility.WriteArrayElement(_nodes->Ptr, index, value);

        public bool Contains(Node node)
        {
            RequireReadAccess();

            var listNode = ReadUnsafe(node.Index);
            return node.Equals(listNode);
        }

        public void Enqueue(T value, int priority)
        {
            RequireWriteAccess();
            RequireReadAccess();

            var node = new Node
            {
                Value = value,
                Priority = priority,
                Index = LengthUnsafe + 1
            };

            // Add the node to the end of the list
            _nodes->Add<Node>(node);

            CascadeUpUnsafe(node);
        }

        public T Dequeue()
        {
            RequireReadAccess();
            RequireWriteAccess();

            Node returnMe = ReadUnsafe(1);
             
            //If the node is already the last node, we can remove it immediately
            if (LengthUnsafe == 1)
            {
                _nodes->RemoveAtSwapBack<Node>(1);
                return returnMe.Value;
            }

            var formerLast = RemoveAtSwapBackUnsafe(1);

            // Bubble down the swapped node, which was prevously the final node and is now in the first position
            CascadeDownUnsafe(formerLast);

            return returnMe.Value;
        }

        private void CascadeUpUnsafe(Node node)
        {
            //aka Heapify-up
            int parent;
            if (node.Index > 1)
            {
                parent = node.Index >> 1;
                Node parentNode = ReadUnsafe(parent);

                if (HasHigherOrEqualPriority(parentNode, node))
                    return;

                //Node has lower priority value, so move parent down the heap to make room
                WriteUnsafe(node.Index, parentNode);
                parentNode.Index = node.Index;
                node.Index = parent;
            }
            else
            {
                return;
            }
            while (parent > 1)
            {
                parent >>= 1;
                Node parentNode = ReadUnsafe(parent);
                if (HasHigherOrEqualPriority(parentNode, node))
                    break;

                //Node has lower priority value, so move parent down the heap to make room
                WriteUnsafe(node.Index, parentNode);
                parentNode.Index = node.Index;
                node.Index = parent;
            }
            WriteUnsafe(node.Index, node);
        }

        private void CascadeDownUnsafe(Node node)
        {
            //aka Heapify-down
            int finalQueueIndex = node.Index;
            int childLeftIndex = 2 * finalQueueIndex;

            // If leaf node, we're done
            if (childLeftIndex > LengthUnsafe)
                return;

            // Check if the left-child is higher-priority than the current node
            int childRightIndex = childLeftIndex + 1;
            Node childLeft = ReadUnsafe(childLeftIndex);

            if (HasHigherPriority(childLeft, node))
            {
                // Check if there is a right child. If not, swap and finish.
                if (childRightIndex > LengthUnsafe)
                {
                    node.Index = childLeftIndex;
                    childLeft.Index = finalQueueIndex;
                    WriteUnsafe(finalQueueIndex, childLeft);
                    WriteUnsafe(childLeftIndex, node);
                    return;
                }
                // Check if the left-child is higher-priority than the right-child
                Node childRight = ReadUnsafe(childRightIndex);
                if (HasHigherPriority(childLeft, childRight))
                {
                    // left is highest, move it up and continue
                    childLeft.Index = finalQueueIndex;
                    WriteUnsafe(finalQueueIndex, childLeft);
                    finalQueueIndex = childLeftIndex;
                }
                else
                {
                    // right is even higher, move it up and continue
                    childRight.Index = finalQueueIndex;
                    WriteUnsafe(finalQueueIndex, childRight);
                    finalQueueIndex = childRightIndex;
                }
            }
            // Not swapping with left-child, does right-child exist?
            else if (childRightIndex > LengthUnsafe)
            {
                return;
            }
            else
            {
                // Check if the right-child is higher-priority than the current node
                Node childRight = ReadUnsafe(childRightIndex);
                if (HasHigherPriority(childRight, node))
                {
                    childRight.Index = finalQueueIndex;
                    WriteUnsafe(finalQueueIndex, childRight);
                    finalQueueIndex = childRightIndex;
                }
                // Neither child is higher-priority than current, so finish and stop.
                else
                {
                    return;
                }
            }

            while (true)
            {
                childLeftIndex = 2 * finalQueueIndex;

                // If leaf node, we're done
                if (childLeftIndex > LengthUnsafe)
                {
                    node.Index = finalQueueIndex;
                    WriteUnsafe(finalQueueIndex, node);
                    break;
                }

                // Check if the left-child is higher-priority than the current node
                childRightIndex = childLeftIndex + 1;
                childLeft = ReadUnsafe(childLeftIndex);
                if (HasHigherPriority(childLeft, node))
                {
                    // Check if there is a right child. If not, swap and finish.
                    if (childRightIndex > LengthUnsafe)
                    {
                        node.Index = childLeftIndex;
                        childLeft.Index = finalQueueIndex;
                        WriteUnsafe(finalQueueIndex, childLeft);
                        WriteUnsafe(childLeftIndex, node);
                        break;
                    }
                    // Check if the left-child is higher-priority than the right-child
                    Node childRight = ReadUnsafe(childRightIndex);
                    if (HasHigherPriority(childLeft, childRight))
                    {
                        // left is highest, move it up and continue
                        childLeft.Index = finalQueueIndex;
                        WriteUnsafe(finalQueueIndex, childLeft);
                        finalQueueIndex = childLeftIndex;
                    }
                    else
                    {
                        // right is even higher, move it up and continue
                        childRight.Index = finalQueueIndex;
                        WriteUnsafe(finalQueueIndex, childRight);
                        finalQueueIndex = childRightIndex;
                    }
                }
                // Not swapping with left-child, does right-child exist?
                else if (childRightIndex > LengthUnsafe)
                {
                    node.Index = finalQueueIndex;
                    WriteUnsafe(finalQueueIndex, node);
                    break;
                }
                else
                {
                    // Check if the right-child is higher-priority than the current node
                    Node childRight = ReadUnsafe(childRightIndex);
                    if (HasHigherPriority(childRight, node))
                    {
                        childRight.Index = finalQueueIndex;
                        WriteUnsafe(finalQueueIndex, childRight);
                        finalQueueIndex = childRightIndex;
                    }
                    // Neither child is higher-priority than current, so finish and stop.
                    else
                    {
                        node.Index = finalQueueIndex;
                        WriteUnsafe(finalQueueIndex, node);
                        break;
                    }
                }
            }
        }

        public Node this[int index]
        {
            get
            {
                RequireReadAccess();
                return ReadUnsafe(index + 1);
            }
        }

        private bool HasHigherPriority(Node higher, Node lower) => higher.Priority < lower.Priority;

        private bool HasHigherOrEqualPriority(Node higher, Node lower) => higher.Priority <= lower.Priority;

        public void Clear()
        {
            RequireWriteAccess();
            _nodes->Clear();
            _nodes->Add<Node>(default);
        }

        /// <summary>
        /// Remove the node at the given index and swap the last node into it's place. Note you should call 
        /// a cascade function immediately after this to ensure correct state. 
        /// This modifies the length of the queue.
        /// </summary>
        Node RemoveAtSwapBackUnsafe(int index)
        {
            // Swap the last node with the node at the given index
            var formerLast = LastUnsafe;
            formerLast.Index = index;
            WriteUnsafe(index, formerLast);
            _nodes->RemoveAtSwapBack<Node>(LengthUnsafe);
            return formerLast;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Node : IEquatable<Node>
        {
            public T Value;
            public int Priority;
            internal int Index;

            public bool Equals(Node other)
            {
                return Index == other.Index && Priority == other.Priority &&
                    EqualityComparer<T>.Default.Equals(Value, other.Value);
            }

            public override string ToString() => $"Node [{Value}, {Priority}]";
        }

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
            UnsafeList.Destroy(_nodes);
            _nodes = null;
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Clear(ref m_DisposeSentinel);

            var jobHandle = new NativePriorityQueueDisposeJob { Data = new NativePriorityQueueDispose 
            { 
                m_ListData = _nodes, 
                m_Safety = m_Safety } 
            }.Schedule(inputDeps);

            AtomicSafetyHandle.Release(m_Safety);
#else
            var jobHandle = new NativePriorityQueueDisposeJob 
            { 
                Data = new NativePriorityQueueDispose { m_ListData = _nodes } 
            }.Schedule(inputDeps);
#endif
            _nodes = null;

            return jobHandle;
        }

        public void Enqueue(T value, float cost)
        {
            Enqueue(value, (int)cost);
        }


        #region SAFETY
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;

        [NativeSetClassTypeToNullOnSchedule]
        internal DisposeSentinel m_DisposeSentinel;
#endif

        [System.Diagnostics.Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void RequireReadAccess()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
        }

        [System.Diagnostics.Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void RequireWriteAccess()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
        }
        #endregion

        [NativeContainer]
        internal unsafe struct NativePriorityQueueDispose
        {
            [NativeDisableUnsafePtrRestriction]
            public UnsafeList* m_ListData;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            internal AtomicSafetyHandle m_Safety;
#endif

            public void Dispose()
            {
                UnsafeList.Destroy(m_ListData);
            }
        }

        [BurstCompile]
        internal unsafe struct NativePriorityQueueDisposeJob : IJob
        {
            internal NativePriorityQueueDispose Data;

            public void Execute()
            {
                Data.Dispose();
            }
        }
    }
}