using System.Collections.Generic;
using Core.VSEngine;
using Core.VSEngine.Nodes;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Core.Editor.VSEngine
{
    [NodeGraphEditor.CustomNodeGraphEditor(typeof(ActionGraph))]
    public class ActionGraphEditor : NodeGraphEditor
    {
        private Vector2 _scrollPos;

        private const float PanelWidth = 260f;
        private const float PanelHeight = 280f;
        private const float Margin = 10f;
        private const float TypeWidth = 110f;
        private const float RemoveButtonWidth = 22f;

        public override void OnGUI()
        {
            ActionGraph graph = target as ActionGraph;
            if (graph == null) return;

            // Reset matrix so the panel is drawn in screen-space (immune to pan/zoom)
            Matrix4x4 savedMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.identity;

            Rect panelRect = new Rect(
                Margin,
                window.position.height - PanelHeight - Margin,
                PanelWidth,
                PanelHeight
            );

            GUI.Box(panelRect, GUIContent.none, GUI.skin.window);
            GUILayout.BeginArea(panelRect);
            DrawPanel(graph);
            GUILayout.EndArea();

            GUI.matrix = savedMatrix;
        }

        private void DrawPanel(ActionGraph graph)
        {
            GUILayout.Space(4f);
            GUILayout.Label("Graph Variables", EditorStyles.boldLabel);
            GUILayout.Space(2f);

            List<LocalVariableDefinition> vars = graph.LocalVariablesMutable;

            float listHeight = PanelHeight - 62f;
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(listHeight));

            int removeIndex = -1;
            for (int i = 0; i < vars.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                string newName = EditorGUILayout.TextField(vars[i].Name, GUILayout.ExpandWidth(true));
                NodeElementType newType = (NodeElementType)EditorGUILayout.EnumPopup(vars[i].Type, GUILayout.Width(TypeWidth));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(graph, "Edit Graph Variable");
                    vars[i].Name = newName;
                    vars[i].Type = newType;
                    EditorUtility.SetDirty(graph);
                }

                if (GUILayout.Button("-", GUILayout.Width(RemoveButtonWidth)))
                    removeIndex = i;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (removeIndex >= 0)
            {
                Undo.RecordObject(graph, "Remove Graph Variable");
                vars.RemoveAt(removeIndex);
                EditorUtility.SetDirty(graph);
            }

            GUILayout.Space(2f);
            if (GUILayout.Button("+ Add Variable"))
            {
                Undo.RecordObject(graph, "Add Graph Variable");
                vars.Add(new LocalVariableDefinition());
                EditorUtility.SetDirty(graph);
            }
        }
    }
}
