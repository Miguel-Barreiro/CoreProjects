using System.Collections.Generic;
using Core.VSEngine;
using Core.VSEngine.Nodes;
using Core.VSEngine.Systems;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace Core.Editor.VSEngine
{
    [NodeGraphEditor.CustomNodeGraphEditor(typeof(ActionGraph))]
    public class ActionGraphEditor : NodeGraphEditor
    {
        private Vector2 _scrollPos;

        private const float PanelWidth        = 260f;
        private const float PanelHeight       = 420f;
        private const float Margin            = 10f;
        private const float TypeWidth         = 90f;
        private const float RemoveButtonWidth = 22f;

        // LocalTriggerDefinition._CooldownStat is private, so nameof can't reach it
        private const string CooldownStatFieldName = "_CooldownStat";

        private static readonly Color DuplicateStatTint = new Color(1f, 0f, 0f, 0.3f);
        private readonly HashSet<int> _duplicateStatTimers = new HashSet<int>();

        // The raw keyboardControl ID that the text field set on MouseDown.
        // Used to re-establish focus after xNode's Controls() clears it on MouseUp.
        // We store the ID directly because FocusTextInControl(name) calls
        // GetControlID outside the draw loop and resolves to the wrong ID.
        private int _panelFocusedControlId = 0;

        private Rect GetPanelRect() => new Rect(
            Margin,
            window.position.height - PanelHeight - Margin,
            PanelWidth,
            PanelHeight
        );

        public override void OnGUI()
        {
            if (target == null) return;

            // Reset matrix so the panel is screen-space (immune to pan/zoom)
            Matrix4x4 savedMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.identity;

            Rect panelRect = GetPanelRect();
            Event e = Event.current;

            // Save state BEFORE DrawPanel because GUI controls inside it call
            // e.Use() (e.g. the text field on MouseUp releases hotControl via
            // e.Use()), which changes e.type to EventType.Used and would make
            // our post-draw checks invisible to the MouseUp event.
            EventType eventType   = e.type;
            bool      mouseInPanel = panelRect.Contains(e.mousePosition);

            // Snapshot keyboardControl BEFORE drawing so we can detect which
            // kind of control was actually clicked inside the panel.
            int kbBefore = GUIUtility.keyboardControl;

            GUI.Box(panelRect, GUIContent.none, GUI.skin.window);
            GUILayout.BeginArea(panelRect);
            DrawPanel();
            GUILayout.EndArea();

            // ── Focus fix ────────────────────────────────────────────────────
            // xNode's Controls() fires EditorGUI.FocusTextInControl(null) +
            // editingTextField=false on every MouseUp outside a node, which
            // includes our panel.  We counter this by:
            //   1. On MouseDown: record the text field's ID ONLY if keyboard
            //      control actually changed inside DrawPanel. Buttons, enum
            //      popups and other passive controls do NOT change
            //      keyboardControl, so they leave _panelFocusedControlId at 0.
            //      Without this guard, clicking a type-dropdown after editing a
            //      name field would carry the stale text-field ID into MouseUp
            //      and the onLateGUI would reactivate the text field while the
            //      dropdown was trying to open.
            //   2. On MouseUp in panel: restore the saved ID directly in
            //      onLateGUI (which runs after Controls() in the same pass).
            if (eventType == EventType.MouseDown && mouseInPanel)
            {
                int kbAfter = GUIUtility.keyboardControl;
                // Only track the ID when a keyboard-focusable control (i.e. a
                // text field) was newly clicked — detected by the ID changing.
                _panelFocusedControlId = (kbAfter != kbBefore) ? kbAfter : 0;
            }
            else if (eventType == EventType.MouseUp && mouseInPanel
                     && _panelFocusedControlId != 0)
            {
                int idToRestore = _panelFocusedControlId;
                window.onLateGUI += () =>
                {
                    GUIUtility.keyboardControl       = idToRestore;
                    EditorGUIUtility.editingTextField = true;
                };
            }
            else if (eventType == EventType.MouseUp && !mouseInPanel)
            {
                // Clicked outside — let xNode clear focus normally.
                _panelFocusedControlId = 0;
            }

            GUI.matrix = savedMatrix;
        }

        private void DrawPanel()
        {
            serializedObject.Update();

            float listHeight = PanelHeight - 8f;
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(listHeight));

            DrawVariablesSection();
            GUILayout.Space(8f);
            DrawTimersSection();

            EditorGUILayout.EndScrollView();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawVariablesSection()
        {
            GUILayout.Space(4f);
            GUILayout.Label("Graph Variables", EditorStyles.boldLabel);
            GUILayout.Space(2f);

            SerializedProperty varsProp = serializedObject.FindProperty(nameof(ActionGraph.localVariables));

            int removeIndex = -1;
            for (int i = 0; i < varsProp.arraySize; i++)
            {
                DrawVariableRow(varsProp.GetArrayElementAtIndex(i), i, ref removeIndex);
                EditorGUILayout.Space(2f);
            }

            if (removeIndex >= 0)
            {
                varsProp.DeleteArrayElementAtIndex(removeIndex);
            }

            GUILayout.Space(2f);
            if (GUILayout.Button("+ Add Variable"))
            {
                varsProp.InsertArrayElementAtIndex(varsProp.arraySize);
                // Reset the new element to defaults
                SerializedProperty newElem = varsProp.GetArrayElementAtIndex(varsProp.arraySize - 1);
                newElem.FindPropertyRelative(nameof(LocalVariableDefinition.Name)).stringValue        = "";
                newElem.FindPropertyRelative(nameof(LocalVariableDefinition.NumberValue)).floatValue  = 0f;
                newElem.FindPropertyRelative(nameof(LocalVariableDefinition.BoolValue)).boolValue     = false;
                newElem.FindPropertyRelative(nameof(LocalVariableDefinition.PositionValue)).vector3Value  = Vector3.zero;
                newElem.FindPropertyRelative(nameof(LocalVariableDefinition.TileCoordValue)).vector2IntValue = Vector2Int.zero;
            }
        }

        private void DrawTimersSection()
        {
            GUILayout.Label("Timers", EditorStyles.boldLabel);
            GUILayout.Space(2f);

            SerializedProperty timersProp = serializedObject.FindProperty(nameof(ActionGraph.localTriggers));

            ((ActionGraph)target).FindTimersWithDuplicateStats(_duplicateStatTimers);
            if (_duplicateStatTimers.Count > 0)
                EditorGUILayout.HelpBox("Each timer must use a different Cooldown Stat.", MessageType.Error);

            int removeIndex = -1;
            for (int i = 0; i < timersProp.arraySize; i++)
            {
                DrawTimerRow(timersProp.GetArrayElementAtIndex(i), i, _duplicateStatTimers.Contains(i), ref removeIndex);
                EditorGUILayout.Space(2f);
            }

            // Index 0 is the default timer and must never be removed
            if (removeIndex > 0)
            {
                timersProp.DeleteArrayElementAtIndex(removeIndex);
            }

            GUILayout.Space(2f);
            if (GUILayout.Button("+ Add Timer"))
            {
                timersProp.InsertArrayElementAtIndex(timersProp.arraySize);
                // Reset the new element to defaults (InsertArrayElementAtIndex copies the previous one)
                SerializedProperty newElem = timersProp.GetArrayElementAtIndex(timersProp.arraySize - 1);
                newElem.FindPropertyRelative(nameof(LocalTriggerDefinition.Name)).stringValue = "";
                newElem.FindPropertyRelative(nameof(LocalTriggerDefinition.CooldownReset)).enumValueIndex = (int)CooldownResetType.Restart;
                newElem.FindPropertyRelative(CooldownStatFieldName).objectReferenceValue = null;
            }
        }

        private void DrawTimerRow(SerializedProperty elem, int index, bool hasDuplicateStat, ref int removeIndex)
        {
            bool isDefault = index == 0;
            SerializedProperty nameProp  = elem.FindPropertyRelative(nameof(LocalTriggerDefinition.Name));
            SerializedProperty resetProp = elem.FindPropertyRelative(nameof(LocalTriggerDefinition.CooldownReset));
            SerializedProperty statProp  = elem.FindPropertyRelative(CooldownStatFieldName);

            Rect rowRect = EditorGUILayout.BeginVertical();
            if (hasDuplicateStat)
                EditorGUI.DrawRect(rowRect, DuplicateStatTint);

            // ── Row 1: name + remove button ──────────────────────────────
            EditorGUILayout.BeginHorizontal();
            if (isDefault)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextField(ActionGraph.DefaultTimerName, GUILayout.ExpandWidth(true));
                    GUILayout.Button("-", GUILayout.Width(RemoveButtonWidth));
                }
            }
            else
            {
                GUI.SetNextControlName("timerName_" + index);
                EditorGUI.BeginChangeCheck();
                string newName = EditorGUILayout.TextField(nameProp.stringValue, GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                    nameProp.stringValue = newName;
                if (GUILayout.Button("-", GUILayout.Width(RemoveButtonWidth)))
                    removeIndex = index;
            }
            EditorGUILayout.EndHorizontal();

            // ── Row 2: reset type + cooldown stat ────────────────────────
            // The default timer uses the global default timer stat, so it has no stat field.
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(resetProp, GUIContent.none, GUILayout.Width(TypeWidth));
            if (isDefault)
                GUILayout.Label("Default stat", EditorStyles.miniLabel);
            else
                EditorGUILayout.PropertyField(statProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawVariableRow(SerializedProperty elem, int index, ref int removeIndex)
        {
            SerializedProperty nameProp = elem.FindPropertyRelative(nameof(LocalVariableDefinition.Name));
            SerializedProperty typeProp = elem.FindPropertyRelative(nameof(LocalVariableDefinition.Type));
            NodeElementType currentType = (NodeElementType)typeProp.enumValueIndex;

            // ── Row 1: name + remove button ──────────────────────────────
            // Use TextField directly (not PropertyField) so GUI.SetNextControlName
            // reliably names the single text-field control for focus tracking.
            EditorGUILayout.BeginHorizontal();
            string controlName = "varName_" + index;
            GUI.SetNextControlName(controlName);
            EditorGUI.BeginChangeCheck();
            string newName = EditorGUILayout.TextField(nameProp.stringValue, GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck())
                nameProp.stringValue = newName;
            if (GUILayout.Button("-", GUILayout.Width(RemoveButtonWidth)))
                removeIndex = index;
            EditorGUILayout.EndHorizontal();

            // ── Row 2: type enum + value field ────────────────────────────
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(typeProp, GUIContent.none, GUILayout.Width(TypeWidth));
            currentType.DrawValueField(elem);
            EditorGUILayout.EndHorizontal();
        }
    }
}
