using Unity.Collections;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;

namespace EntityUtils.SystemBaseExtensions
{
    public static class SystemBaseExtension
    {
        public static NativeArray<ArrayType> GetSingletonBufferArray
            <BufferType,ArrayType>(this SystemBase s)
            where BufferType : unmanaged, IBufferElementData
            where ArrayType : unmanaged
        {
            var e = s.GetSingletonEntity<BufferType>();
            return s.GetBufferArray<BufferType,ArrayType>(e);
        }

        public static DynamicBuffer<T> GetSingletonBuffer<T>(this SystemBase sys)
            where T : unmanaged, IBufferElementData
        {
            var ent = sys.GetSingletonEntity<T>();
            return sys.GetBuffer<T>(ent);
        }

        public static NativeArray<ArrayType> GetBufferArray
            <BufferType, ArrayType>(this SystemBase s, Entity e)
            where BufferType : unmanaged, IBufferElementData
            where ArrayType : unmanaged
        {
            return s.GetBuffer<BufferType>(e).Reinterpret<ArrayType>().AsNativeArray();
        }

        public static JobHandle GetSingletonASync<T>(this SystemBase system, JobHandle dependency, Allocator allocator, out NativeReference<T> singleton)
            where T : unmanaged, IComponentData
        {
            return GetComponentASync<T>(system, dependency, allocator,
                system.GetSingletonEntity<T>(), out singleton);
        }

        public static JobHandle GetComponentASync<T>(this SystemBase system, JobHandle dependency, Allocator allocator, Entity e, out NativeReference<T> singleton)
            where T : unmanaged, IComponentData
        {
            singleton = new NativeReference<T>(allocator);

            return new GetSingletonJob<T>
            {
                Entity = e,
                GetComponent = system.GetComponentDataFromEntity<T>(true),
                Singleton = singleton,
            }.Schedule(dependency);
        }

        [BurstCompile] // probably won't be bursted anyway atm
        private struct GetSingletonJob<T> : IJob
            where T : unmanaged, IComponentData
        {
            public Entity Entity;
            [ReadOnly] public ComponentDataFromEntity<T> GetComponent;

            [WriteOnly]
            public NativeReference<T> Singleton;

            public void Execute()
            {
                Singleton.Value = GetComponent[Entity];
            }
        }

    }
}