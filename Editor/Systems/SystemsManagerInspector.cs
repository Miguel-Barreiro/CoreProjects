using System;
using System.Collections.Generic;
using System.Linq;
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
        
        private Dictionary<string, bool> _installerFoldouts = new Dictionary<string, bool>();
        private Dictionary<(string, SystemGroup), bool> _groupFoldouts = new ();
        private Dictionary<object, bool> _systemFoldouts = new Dictionary<object, bool>();
        
        private Vector2 _scrollPosition;

        
        private readonly Dictionary<string, Dictionary<SystemGroup, List<object>>> GroupedSystemsCache = new();

        private static readonly Color INSTALLER_FOLDOUT_COLOR = new Color(0.1f, 0.9f, 0.1f);
        private GUIStyle INSTALLER_BUTTON_STYLE = null;
        private GUIStyle GROUP_BUTTON_STYLE = null;
        private GUIStyle SYSTEM_BUTTON_STYLE = null;
        
        
        private GUIStyle ACTIVE_BUTTON_STYLE = null;
        private GUIStyle INACTIVE_BUTTON_STYLE = null;

        private void OnEnable()
        {
            INSTALLER_BUTTON_STYLE = new GUIStyle(EditorStyles.toolbarButton);
            INSTALLER_BUTTON_STYLE.fontSize = 16;
            INSTALLER_BUTTON_STYLE.fontStyle = FontStyle.Bold;
            INSTALLER_BUTTON_STYLE.alignment = TextAnchor.MiddleLeft;
            INSTALLER_BUTTON_STYLE.margin = new RectOffset(4, 4, 3, 3);
            INSTALLER_BUTTON_STYLE.padding = new RectOffset(4, 4, 3, 3);
            
            GROUP_BUTTON_STYLE = new GUIStyle(EditorStyles.toolbarButton);
            GROUP_BUTTON_STYLE.fontSize = 12;
            GROUP_BUTTON_STYLE.fontStyle = FontStyle.Bold;
            GROUP_BUTTON_STYLE.alignment = TextAnchor.MiddleLeft;
            GROUP_BUTTON_STYLE.margin = new RectOffset(10, 4, 3, 3);
            GROUP_BUTTON_STYLE.padding = new RectOffset(10, 4, 3, 3);
            
            SYSTEM_BUTTON_STYLE = new GUIStyle(EditorStyles.toolbarButton);
            SYSTEM_BUTTON_STYLE.fontSize = 11;
            SYSTEM_BUTTON_STYLE.alignment = TextAnchor.MiddleLeft;
            SYSTEM_BUTTON_STYLE.margin = new RectOffset(20, 4, 2, 2);
            SYSTEM_BUTTON_STYLE.padding = new RectOffset(20, 4, 2, 2);
            
            ACTIVE_BUTTON_STYLE = new GUIStyle(EditorStyles.miniButton);
            ACTIVE_BUTTON_STYLE.normal.textColor = Color.green;
            ACTIVE_BUTTON_STYLE.fontStyle = FontStyle.Bold;
            
            INACTIVE_BUTTON_STYLE = new GUIStyle(EditorStyles.miniButton);
            INACTIVE_BUTTON_STYLE.normal.textColor = Color.red;
            INACTIVE_BUTTON_STYLE.fontStyle = FontStyle.Bold;

        }

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
            
            DrawSystemsByGroup(allSystems, manager.SystemsContainer);

            EditorGUILayout.EndScrollView();
        }

        private void GetAllSystems(SystemsContainer container, List<Object> result)
        {
            result.AddRange(container.GetAllSystems());
        }



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
                

                Color previousBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = INSTALLER_FOLDOUT_COLOR;

                // Group header with count
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                string buttonText = _installerFoldouts[installer] ? $"▼ {installer}" : $"► {installer}";
                if (GUILayout.Button(buttonText, INSTALLER_BUTTON_STYLE, GUILayout.ExpandWidth(true)))
                {
                    _installerFoldouts[installer] = !_installerFoldouts[installer];
                }
                
                EditorGUILayout.EndVertical();
                

                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = previousBackgroundColor;

                if (_installerFoldouts[installer])
                {
                   DrawInstallerGroups(installer, systemsbyGroup);
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
            }

            void DrawInstallerGroups(string installer, Dictionary<SystemGroup,List<object>> installerGroups)
            {
                EditorGUI.indentLevel++;

                foreach ((SystemGroup systemGroup, List<object> systems)  in installerGroups)
                {
                    (string installer, SystemGroup systemGroup) groupKey = (installer, systemGroup);
                   
                    if (!_groupFoldouts.ContainsKey(groupKey))
                        _groupFoldouts[groupKey] = false;
                    
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    // Group header with count
                    EditorGUILayout.BeginHorizontal();
                    
                    string buttonText = _groupFoldouts[groupKey] ? $"▼ {systemGroup.Name} ({systems.Count()} systems)" : $"► {systemGroup.Name} ({systems.Count()} systems)";
                    if (GUILayout.Button(buttonText, GROUP_BUTTON_STYLE, GUILayout.ExpandWidth(true)))
                    {
                        _groupFoldouts[groupKey] = !_groupFoldouts[groupKey];
                    }
                    
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
                EditorGUI.indentLevel--;

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
            
            
            // Create button for system active state instead of toggle
            if (isActiveSystem)
            {
                string buttonText = systemActive ? "ACTIVE" : "PAUSED";
                GUIStyle buttonStyle = systemActive ? ACTIVE_BUTTON_STYLE : INACTIVE_BUTTON_STYLE;
                
                if (GUILayout.Button(buttonText, buttonStyle, GUILayout.Width(60)))
                {
                    ((ISystem)system).Active = !systemActive;
                }
            }
            
            string foldoutButtonText = _systemFoldouts[system] ? $"▼ {system.GetType().Name}" : $"► {system.GetType().Name}";
            if (GUILayout.Button(foldoutButtonText, SYSTEM_BUTTON_STYLE, GUILayout.ExpandWidth(true)))
            {
                _systemFoldouts[system] = !_systemFoldouts[system];
            }
            
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