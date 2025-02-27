using System.Collections.Generic;
using System.Linq;
using Core.Model;
using Core.Systems;
using Core.Utils.CachedDataStructures;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Core.Editor.Systems
{
    [CustomEditor(typeof(SystemsManager))]
    public class SystemsManagerInspector : UnityEditor.Editor
    {
        private bool _showAllSystems = false;
        private bool _showSystemGroups = true;
        private Dictionary<SystemGroup, bool> _groupFoldouts = new Dictionary<SystemGroup, bool>();
        private string _searchFilter = "";
        private Vector2 _scrollPosition;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Systems Overview", EditorStyles.boldLabel);

            SystemsManager manager = (SystemsManager)target;
            if (manager.SystemsContainer == null)
            {
                EditorGUILayout.HelpBox("Systems Container is not initialized.", MessageType.Warning);
                return;
            }

            // Search filter
            _searchFilter = EditorGUILayout.TextField("Search", _searchFilter);

            // Display options
            EditorGUILayout.BeginHorizontal();
            _showAllSystems = EditorGUILayout.ToggleLeft("Show All Systems", _showAllSystems, GUILayout.Width(150));
            _showSystemGroups = EditorGUILayout.ToggleLeft("Group by Type", _showSystemGroups, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Get all systems
            using CachedList<Object> allSystems = ListCache<Object>.Get();
            GetAllSystems(manager.SystemsContainer, allSystems);
            
            if (allSystems.Count == 0)
            {
                EditorGUILayout.HelpBox("No systems registered.", MessageType.Info);
                return;
            }

            // Begin scrollable area
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(400));

            if (_showSystemGroups)
            {
                DrawSystemsByGroup(allSystems);
            }
            else
            {
                DrawAllSystems(allSystems);
            }

            EditorGUILayout.EndScrollView();

            // Add buttons for common actions
            // EditorGUILayout.Space();
            // EditorGUILayout.BeginHorizontal();
            //
            // if (GUILayout.Button("Expand All"))
            // {
            //     foreach (var group in System.Enum.GetValues(typeof(SystemGroup)))
            //     {
            //         _groupFoldouts[(SystemGroup)group] = true;
            //     }
            //     _showAllSystems = true;
            // }
            //
            // if (GUILayout.Button("Collapse All"))
            // {
            //     foreach (var group in System.Enum.GetValues(typeof(SystemGroup)))
            //     {
            //         _groupFoldouts[(SystemGroup)group] = false;
            //     }
            //     _showAllSystems = false;
            // }
            //
            // EditorGUILayout.EndHorizontal();
        }

        private void GetAllSystems(SystemsContainer container, List<Object> result)
        {
            // This is a placeholder - you'll need to implement a way to get all systems from the container
            // The actual implementation depends on how SystemsContainer stores its systems
            result.AddRange(container.GetAllSystems());
            
            // Example (modify based on your actual SystemsContainer implementation):
            // foreach (var system in container.GetAllSystems())
            // {
            //     systems.Add(system);
            // }
            
        }

        private void DrawSystemsByGroup(List<object> allSystems)
        {
            // Group systems by their SystemGroup
            IEnumerable<IGrouping<SystemGroup,object>> groupedSystems = 
                allSystems.GroupBy(s => GetSystemGroup(s));
            // .OrderBy(g => g.Key);

            foreach (IGrouping<SystemGroup,object> group in groupedSystems)
            {
                if (!_groupFoldouts.ContainsKey(group.Key))
                {
                    _groupFoldouts[group.Key] = true;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Group header with count
                EditorGUILayout.BeginHorizontal();
                _groupFoldouts[group.Key] = EditorGUILayout.Foldout(_groupFoldouts[group.Key], 
                    $"{group.Key.Name} ({group.Count()} systems)", true, EditorStyles.foldoutHeader);
                EditorGUILayout.EndHorizontal();

                if (_groupFoldouts[group.Key])
                {
                    EditorGUI.indentLevel++;
                    foreach (object system in group)
                    {
                        DrawSystemInfo(system);
                    }
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }
        }

        private void DrawAllSystems(List<object> allSystems)
        {
            foreach (Object system in allSystems)
            {
                DrawSystemInfo(system);
            }
        }

        private void DrawSystemInfo(object system)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // System name and type
            EditorGUILayout.LabelField(system.GetType().Name, EditorStyles.boldLabel);
            
            // Show additional system info if expanded
            if (_showAllSystems)
            {
                EditorGUI.indentLevel++;
                
                // Display system properties
                EditorGUILayout.LabelField("Type: " + system.GetType().FullName);
                EditorGUILayout.LabelField("Group: " + GetSystemGroup(system));
                
                // Display interfaces implemented
                var interfaces = system.GetType().GetInterfaces();
                if (interfaces.Length > 0)
                {
                    EditorGUILayout.LabelField("Interfaces:", EditorStyles.boldLabel);
                    foreach (var iface in interfaces)
                    {
                        EditorGUILayout.LabelField("• " + iface.Name);
                    }
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private SystemGroup GetSystemGroup(object system)
        {
            // Try to get the system group from the system
            // This is a placeholder - implement based on your actual system structure
            if (system is ISystem systemWithGroup)
            {
                return systemWithGroup.Group;
            }
            
            return ISystem.DefaultGroup;
        }
    }
} 