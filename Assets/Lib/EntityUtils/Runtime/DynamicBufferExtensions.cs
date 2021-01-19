using Unity.Entities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Sark.EntityUtils.DynamicBufferExtensions
{
    public static class DynamicBufferExtensions
    {
        public static NativeArray<ArrayType> AsNativeArrayT<BufferType, ArrayType>(
            this DynamicBuffer<BufferType> buffer)
            where BufferType : unmanaged, IBufferElementData
            where ArrayType : unmanaged
        {
            return buffer.Reinterpret<ArrayType>().AsNativeArray();
        }

        public static void MemClear<T>(this DynamicBuffer<T> buffer)
            where T : unmanaged, IBufferElementData
        {
            unsafe
            {
                UnsafeUtility.MemClear(buffer.GetUnsafePtr(), buffer.Length);
            }
        }
    } 
}
