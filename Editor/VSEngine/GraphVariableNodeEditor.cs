using System;
using System.Linq;
using Core.VSEngine;
using Core.VSEngine.Nodes.LocalVariables;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace Core.Editor.VSEngine
{
    [CustomNodeEditor(typeof(GetGraphVariableNode))]
    public class GetGraphVariableNodeEditor : GraphVariableNodeEditorBase { }

    [CustomNodeEditor(typeof(SetGraphVariableNode))]
    public class SetGraphVariableNodeEditor : GraphVariableNodeEditorBase { }

    [CustomNodeEditor(typeof(DebugGraphVariableNode))]
    public class DebugGraphVariableNodeEditor : GraphVariableNodeEditorBase { }

    /// <summary>
    /// Replaces the plain VariableName text field with a dropdown populated
    /// from the ActionGraph's declared LocalVariables. All serialized fields
    /// (including flow ports Enter/Continue) are drawn via the standard xNode
    /// property iteration. The dynamic "Value" port (Get/Set nodes only) is
    /// drawn explicitly since it has no backing serialized field.
    /// </summary>
    public abstract class GraphVariableNodeEditorBase : NodeEditor
    {
        private static readonly string[] Excludes = { "m_Script", "graph", "position", "ports" };

        public override void OnBodyGUI()
        {
            serializedObject.Update();

            ActionGraph graph = target.graph as ActionGraph;
            string[] declaredNames = graph != null && graph.LocalVariables.Count > 0
                ? graph.LocalVariables.Select(v => v.Name).ToArray()
                : null;

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (Array.IndexOf(Excludes, iterator.name) >= 0) continue;

                if (iterator.name == "VariableName" && declaredNames != null)
                {
                    DrawVariableDropdown(iterator, declaredNames);
                }
                else
                {
                    NodeEditorGUILayout.PropertyField(iterator, true);
                }
            }

            // Dynamic "Value" port has no backing serialized field, so it must
            // be drawn explicitly. Only Get/Set nodes have this port.
            NodePort valueOutput = target.GetOutputPort("Value");
            NodePort valueInput  = target.GetInputPort("Value");
            if (valueOutput != null)
                NodeEditorGUILayout.PortField(new GUIContent("Value"), valueOutput);
            else if (valueInput != null)
                NodeEditorGUILayout.PortField(new GUIContent("Value"), valueInput);

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawVariableDropdown(SerializedProperty prop, string[] names)
        {
            int currentIndex = Array.IndexOf(names, prop.stringValue);
            if (currentIndex < 0) currentIndex = 0;

            EditorGUI.BeginChangeCheck();
            int selected = EditorGUILayout.Popup("Variable", currentIndex, names);
            if (EditorGUI.EndChangeCheck())
                prop.stringValue = names[selected];
        }
    }
}
