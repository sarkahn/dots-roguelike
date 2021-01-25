using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Unity.Entities;

namespace Sark.Terminals.Authoring
{
    public class MenuItems
    {
        [MenuItem("GameObject/Terminals/Create Terminal", false, 10)]
        public static void CreateTerminal()
        {
            var go = new GameObject("Terminal");
            Undo.RegisterCreatedObjectUndo(go, "CreateTerminal");

            go.AddComponent<TerminalAuthoring>();
            go.AddComponent<TerminalRendererAuthoring>();
            go.AddComponent<ConvertToEntity>();

            Selection.activeGameObject = go;
        }

        [MenuItem("GameObject/Terminals/Create Authoring Tile", false, 11)]
        public static void CreateAuthoringTile()
        {
            var go = new GameObject("Authoring Tile");
            Undo.RegisterCreatedObjectUndo(go, "CreateAuthoringTile");

            go.AddComponent<AuthoringTile>();

            Selection.activeGameObject = go;
        }
    } 
}
