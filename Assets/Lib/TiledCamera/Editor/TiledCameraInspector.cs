using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Sark.RenderUtils
{
    [CustomEditor(typeof(TiledCamera))]
    public class TiledCameraInspector : Editor
    {
        [MenuItem("GameObject/TiledCamera/Create", false, 10)]
        public static void Create(MenuCommand command)
        {
            var existing = Camera.main;

            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing.gameObject);
            }

            var go = new GameObject("Tiled Camera");
            Undo.RegisterCreatedObjectUndo(go, "Create TiledCamera");
            go.AddComponent<TiledCamera>();
            go.AddComponent<AudioListener>();
            go.transform.position = new Vector3(0, 0, -10);
            go.tag = "MainCamera";

            Selection.activeGameObject = go;
        }

        public override void OnInspectorGUI()
        {
            var tar = target as TiledCamera;

            base.OnInspectorGUI();

            var cam = tar.Camera;

            int camWidth = cam.pixelWidth;
            int camHeight = cam.pixelHeight;

            var pixelCam = tar.PixelCamera;

            if (pixelCam == null || !pixelCam.runInEditMode)
                return;

            float pixelRatio = pixelCam.pixelRatio;

            int refResWidth = pixelCam.refResolutionX;
            int refResHeight = pixelCam.refResolutionY;

            EditorGUILayout.LabelField("Camera Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Camera Pixel Size: {camWidth}, {camHeight}");
            EditorGUILayout.LabelField($"Pixel Camera Ref Res: {refResHeight}, {refResHeight}");
            EditorGUILayout.LabelField($"Pixel Ratio: {pixelRatio}");

            if (GUILayout.Button("Snap To Origin"))
                tar.SnapViewportCornerToOrigin();
        }

        public override bool RequiresConstantRepaint() => true;

        [DrawGizmo(GizmoType.Active | GizmoType.Selected)]
        public static void OnGizmos(TiledCamera tar, GizmoType gizmoType)
        {
            var cam = tar.Camera;

            var so = new SerializedObject(tar);

            var drawGrid = so.FindProperty("_drawGrid").boolValue;

            if (!drawGrid || Camera.current != cam)
                return;

            float camDistance = cam.farClipPlane;
            float3 bl = cam.ViewportToWorldPoint(new Vector3(0, 0, camDistance));

            var tileCount = tar.TileCount;

            var gridColorEven = so.FindProperty("_gridColorEven").colorValue;
            var gridColorOdd = so.FindProperty("_gridColorOdd").colorValue;

            for (int x = 0; x < tileCount.x; ++x)
                for (int y = 0; y < tileCount.y; ++y)
                {
                    float3 p = bl + (new float3(x, y, 0));

                    float2 xy = math.floor(p.xy);
                    xy /= 2;
                    float checker = math.frac(xy.x + xy.y) * 2;

                    Color gridColor = Color.Lerp(gridColorEven, gridColorOdd, checker);

                    Gizmos.color = gridColor;
                    Gizmos.DrawCube(p + .5f, Vector3.one);
                }
        }
    }
}