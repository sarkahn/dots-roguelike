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
    } 
}
