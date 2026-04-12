using Core.VSEngine.Nodes;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.VSEngine
{
    /// <summary>
    /// Editor-only extension methods for <see cref="NodeElementType"/>.
    /// Complements the runtime <c>ElementTypeExtensions</c> class.
    /// </summary>
    public static class ElementTypeEditorExtensions
    {
        /// <summary>
        /// Draws the appropriate inline value control for the given type.
        /// <paramref name="variableElement"/> is the <see cref="SerializedProperty"/>
        /// for a single <see cref="Core.VSEngine.LocalVariableDefinition"/> array element.
        /// Undo is handled automatically via the parent <c>SerializedObject</c>.
        /// </summary>
        public static void DrawValueField(this NodeElementType type, SerializedProperty variableElement)
        {
            switch (type)
            {
                case NodeElementType.Numbers:
                    EditorGUILayout.PropertyField(
                        variableElement.FindPropertyRelative("NumberValue"),
                        GUIContent.none, GUILayout.MinWidth(40));
                    break;

                case NodeElementType.Bools:
                    EditorGUILayout.PropertyField(
                        variableElement.FindPropertyRelative("BoolValue"),
                        GUIContent.none, GUILayout.Width(18));
                    break;

                // case NodeElementType.Entities:
                //     EditorGUILayout.PropertyField(
                //         variableElement.FindPropertyRelative("EntityIdValue"),
                //         GUIContent.none, GUILayout.MinWidth(40));
                //     break;

                case NodeElementType.Positions:
                    EditorGUILayout.PropertyField(
                        variableElement.FindPropertyRelative("PositionValue"),
                        GUIContent.none, GUILayout.MinWidth(40));
                    break;

                case NodeElementType.TileCoordinates:
                    EditorGUILayout.PropertyField(
                        variableElement.FindPropertyRelative("TileCoordValue"),
                        GUIContent.none, GUILayout.MinWidth(40));
                    break;

                case NodeElementType.Tags:
                    EditorGUILayout.LabelField("—", GUILayout.Width(20));
                    break;
            }
        }
    }
}
