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
        
        private Dictionary<string, bool> _installerFoldouts = new Dictionary<string, bool>();
        private Dictionary<(string, SystemGroup), bool> _groupFoldouts = new ();
        private Dictionary<object, bool> _systemFoldouts = new Dictionary<object, bool>();
        
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
            // _showAllSystems = EditorGUILayout.ToggleLeft("Show All Systems", _showAllSystems, GUILayout.Width(150));
            // _showSystemGroups = EditorGUILayout.ToggleLeft("Group by Type", _showSystemGroups, GUILayout.Width(150));
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

            // if (_showSystemGroups)
            // {
                DrawSystemsByGroup(allSystems, manager.SystemsContainer);
            // }
            // else
            // {
            //     DrawAllSystems(allSystems, manager.SystemsContainer);
            // }

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

        private readonly Dictionary<string, Dictionary<SystemGroup, List<object>>> GroupedSystemsCache = new();
        
        
        private void DrawSystemsByGroup(List<object> allSystems, SystemsContainer managerSystemsContainer)
        {
            ResetGroupedSystemsCache();

            foreach ((string installer, Dictionary<SystemGroup,List<object>> systemsbyGroup) in GroupedSystemsCache)
            {
                if (!_installerFoldouts.ContainsKey(installer))
                {
                    _installerFoldouts[installer] = true;
                }
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Group header with count
                EditorGUILayout.BeginHorizontal();
                _installerFoldouts[installer] = EditorGUILayout.Foldout(_installerFoldouts[installer], 
                                                                        $"{installer}", 
                                                                        true, 
                                                                        EditorStyles.foldoutHeader);
                EditorGUILayout.EndHorizontal();

                if (_installerFoldouts[installer])
                {
                   DrawInstallerGroups(installer, systemsbyGroup);
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            void DrawInstallerGroups(string installer, Dictionary<SystemGroup,List<object>> installerGroups)
            {

                foreach ((SystemGroup systemGroup, List<object> systems)  in installerGroups)
                {
                    (string installer, SystemGroup systemGroup) groupKey = (installer, systemGroup);
                   
                    if (!_groupFoldouts.ContainsKey(groupKey))
                        _groupFoldouts[groupKey] = false;
                    
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    // Group header with count
                    EditorGUILayout.BeginHorizontal();
                    _groupFoldouts[groupKey] = EditorGUILayout.Foldout(_groupFoldouts[groupKey], 
                                                                       $"{systemGroup.Name} ({systems.Count()} systems)",
                                                                       true,
                                                                       EditorStyles.foldoutHeader);
                    EditorGUILayout.EndHorizontal();

                    if (_groupFoldouts[groupKey])
                    {
                        EditorGUI.indentLevel++;
                        foreach (object system in systems)
                        {
                            DrawSystemInfo(system, managerSystemsContainer);
                        }
                        EditorGUI.indentLevel--;
                    }
    
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }
            }

            void ResetGroupedSystemsCache()
            {
                GroupedSystemsCache.Clear();
                // Group systems by their SystemGroup
                foreach (object system in allSystems)
                {
                    string installerName = managerSystemsContainer.GetInstallerName(system);
                    if (!GroupedSystemsCache.ContainsKey(installerName))
                    {
                        GroupedSystemsCache[installerName] = new Dictionary<SystemGroup, List<object>>();
                    }
                
                    SystemGroup group = GetSystemGroup(system);

                    if (!GroupedSystemsCache[installerName].ContainsKey(group))
                    {
                        GroupedSystemsCache[installerName][group] = new();
                    }
                    GroupedSystemsCache[installerName][group].Add(system);
                }
            }
        }


        private void DrawAllSystems(List<object> allSystems, SystemsContainer managerSystemsContainer)
        {
            foreach (Object system in allSystems)
            {
                DrawSystemInfo(system, managerSystemsContainer);
            }
        }

        private void DrawSystemInfo(object system, SystemsContainer managerSystemsContainer)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // System name and type with toggle button
            EditorGUILayout.BeginHorizontal();
            
            // Check if system implements ISystem interface to access Active property
            bool isActiveSystem = system is ISystem;
            bool systemActive = isActiveSystem ? ((ISystem)system).Active : false;

            if (!_systemFoldouts.ContainsKey(system))
            {
                _systemFoldouts.Add(system, false);
            }
            
            
            // Create toggle for system active state
            if (isActiveSystem)
            {
                EditorGUI.BeginChangeCheck();
                bool newActiveState = EditorGUILayout.Toggle(systemActive, GUILayout.Width(20));
                if (EditorGUI.EndChangeCheck())
                {
                    ((ISystem)system).Active = newActiveState;
                }
            }
            
            _systemFoldouts[system] = EditorGUILayout.Foldout(_systemFoldouts[system], 
                                                              system.GetType().Name,
                                                              true,
                                                              EditorStyles.foldoutHeader);
            EditorGUILayout.EndHorizontal();
            
            // Show additional system info if expanded
            if (_systemFoldouts[system])
            {
                EditorGUI.indentLevel++;
                
                // Display system properties
                EditorGUILayout.LabelField("Type: " + system.GetType().FullName);
                EditorGUILayout.LabelField("Installer: " + managerSystemsContainer.GetInstallerName(system));
                
                
                if (isActiveSystem)
                {
                    EditorGUILayout.LabelField("Status: " + (systemActive ? "Active" : "Inactive"), 
                        systemActive ? EditorStyles.boldLabel : new GUIStyle(EditorStyles.label) { normal = { textColor = Color.gray } });
                    EditorGUILayout.LabelField("Group: " + ((ISystem)system).Group.Name);
                }
                
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