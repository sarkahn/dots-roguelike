using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AuthoringTile))]
public class AuthoringTileInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var tar = target as AuthoringTile;
        if (tar == null)
            return;

        EditorGUILayout.HelpBox(new GUIContent("Note: The final image drawn to a terminal is based entirely on the material and texture attached to that terminal. The preview image below may not be accurate to the final result."), wide:true);
    }
}
