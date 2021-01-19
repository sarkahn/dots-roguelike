using Sark.Common.GridUtil;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Sark.Common.NativeArrayExtensions
{
    public static class NativeArrayExtension
    {
        public static void MemClear<T>(this NativeArray<T> arr)
            where T : unmanaged
        {
            unsafe
            {
                UnsafeUtility.MemClear(arr.GetUnsafePtr(), arr.Length);
            }
        }

        public static GridData2D<T> AsGridData2D<T>(this NativeArray<T> arr, int w, int h)
            where T : unmanaged
        {
            return new GridData2D<T>(arr, w, h);
        }

        public static GridData2D<T> AsGridData2D<T>(this NativeArray<T> arr, int2 size)
            where T : unmanaged
        {
            return new GridData2D<T>(arr, size);
        }
    }
}