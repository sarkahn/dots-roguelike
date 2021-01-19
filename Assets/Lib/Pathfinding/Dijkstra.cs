using Unity.Collections;
using Unity.Jobs;

namespace Sark.Pathfinding
{
    public struct Dijkstra<T> : INativeDisposable
        where T : unmanaged, System.IEquatable<T>
    {
        NativePriorityQueue<T> frontier;
        NativeHashMap<T, T> cameFrom;
        NativeHashMap<T, int> costs;
        NativeList<T> neighbours;

        public Dijkstra(int len, Allocator allocator)
        {
            frontier = new NativePriorityQueue<T>(len, allocator);
            cameFrom = new NativeHashMap<T, T>(len, allocator);
            costs = new NativeHashMap<T, int>(len, allocator);
            neighbours = new NativeList<T>(8, allocator);
        }

        public int Cost(T index)
        {
            return costs.ContainsKey(index)  ? costs[index] : 0;
        }

        public void Calculate<Map>(Map map, T target)
            where Map : IPathingMap<T>
        {
            frontier.Enqueue(target, 0);
            costs[target] = 0;
            while(frontier.Length != 0)
            {
                var curr = frontier.Dequeue();
                neighbours.Clear();

                map.GetAvailableExits(curr, neighbours);

                for(int i = 0; i < neighbours.Length; ++i)
                {
                    var next = neighbours[i];
                    int newCost = costs[curr] + map.GetCost(curr, next);
                    if(!costs.TryGetValue(next, out int nextCost) || newCost < nextCost)
                    {
                        costs[next] = newCost;
                        int priority = newCost;
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = curr;
                    }
                }
            }
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            frontier.Dispose(inputDeps);
            cameFrom.Dispose(inputDeps);
            costs.Dispose(inputDeps);
            neighbours.Dispose(inputDeps);
            return inputDeps;
        }

        public void Dispose()
        {
            frontier.Dispose();
            cameFrom.Dispose();
            costs.Dispose();
            neighbours.Dispose();
        }
    } 
}
