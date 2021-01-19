using UnityEngine;
using System.Collections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.CompilerServices;

namespace Sark.Common.NativeListExtensions
{
    public static class NativeListExtension
    {
        public static T PopLast<T>(this NativeList<T> list) where T : unmanaged
        {
            int end = list.Length - 1;
            var v = list[end];
            list.RemoveAt(end);
            return v;
        }

        //https://forum.unity.com/threads/nativelist-remove-and-removeat-methods.778859/#post-5183357
        public static void Reverse<T>(this NativeList<T> list)
            where T : struct
        {
            var length = list.Length;
            var index1 = 0;

            for (var index2 = length - 1; index1 < index2; --index2)
            {
                var obj = list[index1];
                list[index1] = list[index2];
                list[index2] = obj;
                ++index1;
            }
        }

        public static unsafe void Insert<T>(this NativeList<T> list, int index, T item)
            where T : struct
        {
            if (list.Length == list.Capacity - 1)
            {
                list.Capacity *= 2;
            }

            // Inserting at end same as an add
            if (index == list.Length)
            {
                list.Add(item);
                return;
            }

            if (index < 0 || index > list.Length)
            {
                throw new System.IndexOutOfRangeException();
            }

            // add a default value to end to list to increase length by 1
            list.Add(default);

            int elemSize = UnsafeUtility.SizeOf<T>();
            byte* basePtr = (byte*)list.GetUnsafePtr();

            var from = (index * elemSize) + basePtr;
            var to = (elemSize * (index + 1)) + basePtr;
            var size = elemSize * (list.Length - index - 1); // -1 because we added an extra fake element

            UnsafeUtility.MemMove(to, from, size);
            //MyMemMove(to, from, size);

            list[index] = item;
        }

        //// bug 1293420_hlot51i0ql7346j0 - In burst 1.4.1/1.4.2 LLVM converts MemMove to MemCopy
        //[MethodImpl(MethodImplOptions.NoOptimization)]
        //private static unsafe void MyMemMove(void* to, void* from, long size)
        //{
        //    UnsafeUtility.MemMove(to, from, size);
        //}
    }
}