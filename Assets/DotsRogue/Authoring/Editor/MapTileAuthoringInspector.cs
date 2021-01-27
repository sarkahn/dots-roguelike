using UnityEditor;
using UnityEngine;

namespace DotsRogue.Authoring
{
    [CustomEditor(typeof(MapTileAuthoring))]
    public class AuthoringTileInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var tar = target as MapTileAuthoring;
            if (tar == null)
                return;

            EditorGUILayout.HelpBox(new GUIContent("Note: The final image drawn to a terminal is based entirely on the material and texture attached to that terminal. The preview image below may not be accurate to the final result."), wide: true);
        }
    } 
}
