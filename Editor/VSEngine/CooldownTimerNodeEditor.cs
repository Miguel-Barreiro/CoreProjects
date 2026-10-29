using System;
using Core.VSEngine;
using Core.VSEngine.Nodes;
using UnityEditor;
using XNodeEditor;

namespace Core.Editor.VSEngine
{
    /// <summary>
    /// Replaces the plain TimerIndex int field with a dropdown populated
    /// from the ActionGraph's declared LocalTriggers (timers).
    /// </summary>
    [CustomNodeEditor(typeof(CooldownTimerNode))]
    public class CooldownTimerNodeEditor : NodeEditor
    {
        private static readonly string[] Excludes = { "m_Script", "graph", "position", "ports" };

        // CooldownTimerNode._TimerIndex is private, so nameof can't reach it
        private const string TimerIndexFieldName = "_TimerIndex";

        public override void OnBodyGUI()
        {
            serializedObject.Update();

            ActionGraph graph = target.graph as ActionGraph;

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (Array.IndexOf(Excludes, iterator.name) >= 0) continue;

                if (iterator.name == TimerIndexFieldName && graph != null)
                {
                    DrawTimerDropdown(iterator, graph);
                }
                else
                {
                    NodeEditorGUILayout.PropertyField(iterator, true);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawTimerDropdown(SerializedProperty indexProp, ActionGraph graph)
        {
            int timerCount = graph.LocalTriggers.Count;
            int currentIndex = indexProp.intValue;
            bool isValid = currentIndex >= 0 && currentIndex < timerCount;

            // An out of range index gets an extra entry so it stays visible instead of silently snapping to another timer
            string[] names = new string[isValid ? timerCount : timerCount + 1];
            for (int i = 0; i < timerCount; i++)
            {
                string timerName = graph.LocalTriggers[i].Name;
                names[i] = string.IsNullOrEmpty(timerName) ? $"Timer {i}" : timerName;
            }
            if (!isValid)
                names[timerCount] = $"<Missing timer {currentIndex}>";

            EditorGUI.BeginChangeCheck();
            int selected = EditorGUILayout.Popup("Timer", isValid ? currentIndex : timerCount, names);
            if (EditorGUI.EndChangeCheck() && selected < timerCount)
                indexProp.intValue = selected;
        }
    }
}
