using System;
using System.Collections.Generic;
using Core.Events;
using Core.Utils.Reflection;
using Core.VSEngine;
using UnityEditor;
using UnityEngine;

#if ODIN_INSPECTOR
using Core.VSEngine.Nodes.Events;
using Sirenix.OdinInspector.Editor;
#endif

namespace Core.Editor.VSEngine
{
#if ODIN_INSPECTOR
// #if false
    /// <summary>
    /// Odin-compatible drawer for <see cref="SerializedType"/>.
    /// Using OdinValueDrawer integrates with Odin's GUILayout context, which is required
    /// when drawing inside XNode's node editor (XNode uses Odin's objectTree.Draw when
    /// ODIN_INSPECTOR is defined). A plain [CustomPropertyDrawer] would corrupt
    /// XNode's GUILayout state.
    /// </summary>
    public class EventSerializedTypeDrawer : OdinValueDrawer<EventSerializedType>
    {
        private const string NoneLabel = "(None)";

        protected override void DrawPropertyLayout(GUIContent label)
        {
            SerializedType current = this.ValueEntry.SmartValue;
            string displayLabel = string.IsNullOrEmpty(current.TypeName) ? NoneLabel : current.TypeName;

            Rect rect = EditorGUILayout.GetControlRect();
            if (label != null)
                rect = EditorGUI.PrefixLabel(rect, label);

            if (EditorGUI.DropdownButton(rect, new GUIContent(displayLabel), FocusType.Keyboard))
            {
                string capturedAqn = current.AssemblyQualifiedName;
                IPropertyValueEntry<EventSerializedType> entry = this.ValueEntry;

                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(NoneLabel), string.IsNullOrEmpty(capturedAqn), () =>
                {
                    entry.SmartValue = new EventSerializedType();
                });
                menu.AddSeparator("");

                foreach ((string pretty, string qualifiedName) in EventSerializedTypeOptions.GetTypeOptions())
                {
                    bool   selected = capturedAqn == qualifiedName;
                    string p = pretty;
                    string q = qualifiedName;
                    menu.AddItem(new GUIContent(p), selected, () =>
                    {
                        Type t = Type.GetType(q);
                        entry.SmartValue = t != null ? new EventSerializedType(t) : new EventSerializedType();
                    });
                }

                menu.ShowAsContext();
            }
        }
    }

#else

    /// <summary>
    /// Fallback Unity IMGUI drawer used when Odin Inspector is not present.
    /// </summary>
    [CustomPropertyDrawer(typeof(EventSerializedType))]
    public class EventSerializedTypeDrawer : PropertyDrawer
    {
        private const string NoneLabel = "(None)";

        public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty typeName = property.FindPropertyRelative(nameof(EventSerializedType.TypeName));
            SerializedProperty aqn      = property.FindPropertyRelative(nameof(EventSerializedType.AssemblyQualifiedName));

            string currentLabel = string.IsNullOrEmpty(typeName.stringValue) ? NoneLabel : typeName.stringValue;

            EditorGUI.BeginProperty(position, label, property);
            Rect prefixRect = EditorGUI.PrefixLabel(position, label);
            EditorGUI.EndProperty();

            if (EditorGUI.DropdownButton(prefixRect, new GUIContent(currentLabel), FocusType.Keyboard))
            {
                UnityEngine.Object targetObject = property.serializedObject.targetObject;
                string propertyPath = property.propertyPath;
                string currentAqn   = aqn.stringValue;

                EditorApplication.delayCall += () =>
                {
                    if (targetObject == null) return;

                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent(NoneLabel), string.IsNullOrEmpty(currentAqn), () =>
                        Apply(targetObject, propertyPath, "", ""));
                    menu.AddSeparator("");

                    foreach ((string pretty, string qualifiedName) in SerializedTypeOptions.GetTypeOptions())
                    {
                        bool   selected = currentAqn == qualifiedName;
                        string p = pretty;
                        string q = qualifiedName;
                        menu.AddItem(new GUIContent(p), selected, () =>
                            Apply(targetObject, propertyPath, q, p));
                    }

                    menu.ShowAsContext();
                };
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;

        private static void Apply(UnityEngine.Object target, string path, string qualifiedName, string pretty)
        {
            var so   = new SerializedObject(target);
            var prop = so.FindProperty(path);
            if (prop == null) return;
            prop.FindPropertyRelative(nameof(SerializedType.AssemblyQualifiedName)).stringValue = qualifiedName;
            prop.FindPropertyRelative(nameof(SerializedType.TypeName)).stringValue              = pretty;
            so.ApplyModifiedProperties();
        }
    }

#endif

    // Shared type cache — used by both drawer variants
    internal static class EventSerializedTypeOptions
    {
        private static List<(string pretty, string aqn)> _cache;

        public static IEnumerable<(string pretty, string aqn)> GetTypeOptions()
        {
            if (_cache != null) return _cache;

            _cache = new List<(string, string)>();

            foreach (Type t in ReflectionUtils.GetAllTypesOf<BaseEntityEvent>())
            {
                if (t.IsAbstract || t.IsGenericType || t.AssemblyQualifiedName == null) continue;
                _cache.Add((SerializedTypeUtils.GeneratePrettyTypeName(t), t.AssemblyQualifiedName));
            }

            foreach (Type t in ReflectionUtils.GetAllTypesOf<BaseEvent>())
            {
                if (t.IsAbstract || t.IsGenericType || t.AssemblyQualifiedName == null) continue;
                if (_cache.Exists(o => o.aqn == t.AssemblyQualifiedName)) continue;
                _cache.Add((SerializedTypeUtils.GeneratePrettyTypeName(t), t.AssemblyQualifiedName));
            }

            _cache.Sort((a, b) => string.Compare(a.pretty, b.pretty, StringComparison.OrdinalIgnoreCase));
            return _cache;
        }
    }
}
