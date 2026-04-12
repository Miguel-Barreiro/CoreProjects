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

        private const float PanelWidth        = 260f;
        private const float PanelHeight       = 300f;
        private const float Margin            = 10f;
        private const float TypeWidth         = 90f;
        private const float RemoveButtonWidth = 22f;

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

            GUILayout.Space(4f);
            GUILayout.Label("Graph Variables", EditorStyles.boldLabel);
            GUILayout.Space(2f);

            SerializedProperty varsProp = serializedObject.FindProperty(nameof(ActionGraph.localVariables));

            float listHeight = PanelHeight - 62f;
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(listHeight));

            int removeIndex = -1;
            for (int i = 0; i < varsProp.arraySize; i++)
            {
                DrawVariableRow(varsProp.GetArrayElementAtIndex(i), i, ref removeIndex);
                EditorGUILayout.Space(2f);
            }

            EditorGUILayout.EndScrollView();

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

            serializedObject.ApplyModifiedProperties();
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
