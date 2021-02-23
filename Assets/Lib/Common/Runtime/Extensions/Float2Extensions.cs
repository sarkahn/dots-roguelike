using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Sark.Common.Float2Extensions
{
    public static class Float2Extension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 FlooredInt2(this float2 v)
        {
            return (int2)math.floor(v);
        }
    } 
}
