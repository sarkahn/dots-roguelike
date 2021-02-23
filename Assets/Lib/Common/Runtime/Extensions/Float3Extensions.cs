using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Sark.Common.Float3Extensions
{
    public static class Float3Extension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 FlooredInt3(this float3 v)
        {
            return (int3)math.floor(v);
        }
    } 
}
