using UnityEngine;

using UnityEditor;
using Unity.Mathematics;

namespace Sark.Terminals.Authoring
{
    [CustomEditor(typeof(TerminalAuthoring))]
    public class TerminalAuthoringInspector : Editor
    {
        private void OnSceneGUI()
        {
            if (Tools.current != Tool.Rect)
                return;

            var tar = target as TerminalAuthoring;
            if (tar == null)
                return;

            float3 center = tar.transform.position;
            float2 size = tar.Size * tar.TileSize;
            float3 pos = tar.transform.position;
            var col = ColorFromName(tar.name);

            Rect originalArea = new Rect(pos.xy, size);

            var newArea = RectUtils.ResizeRect(originalArea, Handles.DotHandleCap, Color.blue, col, HandleUtility.GetHandleSize(center) * .08f, tar.TileSize.y);

            float3 newCenter = PosHandle(center, tar.TileSize.y);
            if (!newCenter.Equals(tar.transform.position))
                tar.transform.position = newCenter;

            if (newArea != originalArea)
            {
                tar.Size = (int2)(math.floor(newArea.size));
            }

        }

        float3 PosHandle(float3 p, float snap)
        {
            float size = HandleUtility.GetHandleSize(p) * .08f;
            p = Handles.Slider2D(p, Vector3.back, Vector3.right, Vector3.up, size, Handles.RectangleHandleCap, 1);
            return p;
        }

        static Color ColorFromName(string name)
        {
            var hash = CalculateHash(name);
            UnityEngine.Random.InitState((int)hash);
            return UnityEngine.Random.ColorHSV(0, 1, .15f, .85f);
        }

        static ulong CalculateHash(string read)
        {
            ulong hashedValue = 3074457345618258791ul;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }
    } 
}
