using Unity.Collections;
using Unity.Entities;

namespace Sark.EntityUtils
{

    public struct BufferJobData<T>
        where T : unmanaged, IBufferElementData
    {
        Entity entity;
        BufferFromEntity<T> bfe;

        public BufferJobData(SystemBase system, Entity e, bool readOnly = false)
        {
            entity = e;
            bfe = system.GetBufferFromEntity<T>(readOnly);
        }

        public DynamicBuffer<T> Get() => Get(entity);
        public NativeArray<Q> GetArray<Q>()
            where Q : unmanaged
            => GetArray<Q>(entity);
        public DynamicBuffer<T> Get(Entity e) =>
            bfe[e];
        public NativeArray<Q> GetArray<Q>(Entity e) where Q : unmanaged =>
            bfe[e].Reinterpret<Q>().AsNativeArray();

    }

    public struct ComponentJobData<T>
        where T : unmanaged, IComponentData
    {
        Entity entity;
        ComponentDataFromEntity<T> cdfe;

        public ComponentJobData(SystemBase system, Entity e, bool readOnly = false)
        {
            entity = e;
            cdfe = system.GetComponentDataFromEntity<T>(readOnly);
        }

        public T Get() => cdfe[entity];
        public void Set(T v) => cdfe[entity] = v;
    }

}
