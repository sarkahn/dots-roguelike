using Unity.Collections;
using Unity.Jobs;
using System;

using Sark.Common.NativeListExtensions;

namespace Sark.Pathfinding
{
    // https://www.redblobgames.com/pathfinding/a-star/implementation.html
    /// <summary>
    /// A generic pathfinding struct that works in jobs and burst.
    /// </summary>
    /// <typeparam name="T">The type used to represent a point in the path. IE: int2, int.</typeparam>
    /// 
    //public struct AStar<T> : INativeDisposable 
    //    where T : unmanaged, System.IEquatable<T>
    //{
    //    NativePriorityQueue<T> _frontier;
    //    NativeHashMap<T,T> _parents;
    //    NativeHashMap<T,int> _costs;
    //    NativeList<T> _neighbours;

    //    public AStar(int len, Allocator allocator)
    //    {
    //        _frontier = new NativePriorityQueue<T>(len, allocator);
    //        _parents = new NativeHashMap<T, T>(len, allocator);
    //        _costs = new NativeHashMap<T, int>(len, allocator);
    //        _neighbours = new NativeList<T>(8, allocator);
    //    }

    //   // public void FindPath<Map>(Map map, T start, T end, NativeList<T> output) 
    //   //     where Map : IPathingMap<T>
    //    public void FindPath(IPathingMap<T> map, T start, T end, NativeList<T> output)
    //    {
    //        _frontier.Enqueue(start, 0);

    //        _costs[start] = 0;

    //        while (_frontier.Length > 0)
    //        {
    //            var currNode = _frontier.Dequeue();

    //            var curr = currNode;
    //            if (curr.Equals(end))
    //                break;

    //            _neighbours.Clear();
    //            map.GetAvailableExits(curr, _neighbours);

    //            for (int i = 0; i < _neighbours.Length; ++i)
    //            {
    //                var next = _neighbours[i];

    //                int newCost = _costs[curr] + map.GetCost(curr, next);

    //                if (!_costs.TryGetValue(next, out int nextCost) || newCost < nextCost)
    //                {
    //                    _costs[next] = newCost;
    //                    int priority = newCost + (int)map.GetDistance(next, end);
    //                    _frontier.Enqueue(next, priority);
    //                    _parents[next] = curr;
    //                }
    //            }
    //        }

    //        GetPath(start, end, output);
    //    }

    //    void GetPath(T start, T end, NativeList<T> output)
    //    {
    //        if (!_parents.ContainsKey(end))
    //            // Pathfinding failed
    //            return;

    //        T curr = end;

    //        while ( !curr.Equals(start) )
    //        {
    //            output.Add(curr);
    //            curr = _parents[curr];
    //        }

    //        output.Add(start);
    //        output.Reverse();
    //    }

    //    public NativeKeyValueArrays<T,int> GetCosts(Allocator allocator)
    //    {
    //        return _costs.GetKeyValueArrays(allocator);
    //    }

    //    public NativeArray<T> GetVisited(Allocator allocator)
    //    {
    //        return _parents.GetKeyArray(allocator);
    //    }

    //    public void Clear()
    //    {
    //        _frontier.Clear();
    //        _costs.Clear();
    //        _parents.Clear();
    //    }

    //    public JobHandle Dispose(JobHandle inputDeps)
    //    {
    //        _frontier.Dispose(inputDeps);
    //        _parents.Dispose(inputDeps);
    //        _costs.Dispose(inputDeps);
    //        _neighbours.Dispose(inputDeps);
    //        return inputDeps;
    //    }

    //    public void Dispose()
    //    {
    //        _frontier.Dispose();
    //        _parents.Dispose();
    //        _costs.Dispose();
    //        _neighbours.Dispose();
    //    }
    //}

    // Non-generic implementation of astar to work around this bug in Project Tiny:
    // https://forum.unity.com/threads/1297609-wasm-build-constrained-opcode-was-followed-a-call-rather-than-a-callvirt.1017544/
    public struct AStar : INativeDisposable
    {
        NativePriorityQueue<int> _frontier;
        NativeHashMap<int, int> _parents;
        NativeHashMap<int, int> _costs;
        NativeList<int> _neighbours;

        public AStar(int len, Allocator allocator)
        {
            _frontier = new NativePriorityQueue<int>(len, allocator);
            _parents = new NativeHashMap<int, int>(len, allocator);
            _costs = new NativeHashMap<int, int>(len, allocator);
            _neighbours = new NativeList<int>(8, allocator);
        }

        // public void FindPath<Map>(Map map, T start, T end, NativeList<T> output) 
        //     where Map : IPathingMap<T>
        public void FindPath(PathingBitGrid map, int start, int end, NativeList<int> output)
        {
            _frontier.Enqueue(start, 0);

            _costs[start] = 0;

            while (_frontier.Length > 0)
            {
                var currNode = _frontier.Dequeue();

                var curr = currNode;
                if (curr.Equals(end))
                    break;

                _neighbours.Clear();
                map.GetAvailableExits(curr, _neighbours);

                for (int i = 0; i < _neighbours.Length; ++i)
                {
                    var next = _neighbours[i];

                    int newCost = _costs[curr] + map.GetCost(curr, next);

                    if (!_costs.TryGetValue(next, out int nextCost) || newCost < nextCost)
                    {
                        _costs[next] = newCost;
                        int priority = newCost + (int)map.GetDistance(next, end);
                        _frontier.Enqueue(next, priority);
                        _parents[next] = curr;
                    }
                }
            }

            GetPath(start, end, output);
        }

        void GetPath(int start, int end, NativeList<int> output)
        {
            if (!_parents.ContainsKey(end))
                // Pathfinding failed
                return;

            int curr = end;

            while (!curr.Equals(start))
            {
                output.Add(curr);
                curr = _parents[curr];
            }

            output.Add(start);
            output.Reverse();
        }

        public NativeKeyValueArrays<int, int> GetCosts(Allocator allocator)
        {
            return _costs.GetKeyValueArrays(allocator);
        }

        public NativeArray<int> GetVisited(Allocator allocator)
        {
            return _parents.GetKeyArray(allocator);
        }

        public void Clear()
        {
            _frontier.Clear();
            _costs.Clear();
            _parents.Clear();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            _frontier.Dispose(inputDeps);
            _parents.Dispose(inputDeps);
            _costs.Dispose(inputDeps);
            _neighbours.Dispose(inputDeps);
            return inputDeps;
        }

        public void Dispose()
        {
            _frontier.Dispose();
            _parents.Dispose();
            _costs.Dispose();
            _neighbours.Dispose();
        }
    }

    public interface IPathingMap<T> where T : unmanaged, IEquatable<T>
    {
        void GetAvailableExits(T pos, NativeList<T> output);
        int GetCost(T a, T b);
        float GetDistance(T a, T b);
    }
}
