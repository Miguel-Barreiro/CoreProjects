using System.Collections.Generic;
using Core.VSEngine.NestedVisualScripting;
using Core.VSEngine.Nodes;
using Core.VSEngine.Systems;
using UnityEngine;
using XNode;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif


#nullable enable

namespace Core.VSEngine
{
    [CreateAssetMenu(fileName = "new_actiongraph", menuName = "create Actiongraph", order = 1)]
    public sealed class ActionGraph : NodeGraph, ISerializationCallbackReceiver
    //                                                 , IJsonSavableAsset
    {
        [SerializeField]
        private string guid;
        public string Guid => guid;

        [SerializeField]
        private long fileId;
        public long FileId => fileId;

        [SerializeField] public List<LocalVariableDefinition> localVariables = new();
        public IReadOnlyList<LocalVariableDefinition> LocalVariables => localVariables;

#if UNITY_EDITOR
        public List<LocalVariableDefinition> LocalVariablesMutable => localVariables;
#endif

        [SerializeField] public List<LocalTriggerDefinition> localTriggers = new();
        public IReadOnlyList<LocalTriggerDefinition> LocalTriggers => localTriggers;

#if UNITY_EDITOR
        public List<LocalTriggerDefinition> LocalTriggersMutable => localTriggers;
#endif

        
        public OutputVSNode? OutputsNode => GetOutputsNode();
        public InputVSNode? InputsNode => GetInputsNode();
        public ExecutableNode? StartNode => GetStartNode();

        private static Dictionary<ActionGraph, ExecutableNode?> StartNodeCache = new Dictionary<ActionGraph, ExecutableNode?>();
        private static Dictionary<ActionGraph, InputVSNode?> InputNodeCache = new Dictionary<ActionGraph, InputVSNode?>();
        private static Dictionary<ActionGraph, OutputVSNode?> OutputNodeCache = new Dictionary<ActionGraph, OutputVSNode?>();
        private static Dictionary<ActionGraph, List<CooldownTimerNode>> TimerNodeCache = new Dictionary<ActionGraph, List<CooldownTimerNode>>();

        
#if UNITY_EDITOR

        [MenuItem("Assets/Create Action Graph", priority = -1000)]
        private static void Create()
        {
            string name = $"NEW_AG";

            
            var path = "Assets";
            var obj = Selection.activeObject;
            if (obj && AssetDatabase.Contains(obj))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!Directory.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                }
            }
            
            var dest = path + $"/{name}.asset";
            dest = AssetDatabase.GenerateUniqueAssetPath(dest);
            ScriptableObject previewObject = CreateInstance(typeof(ActionGraph)) as ScriptableObject;
            AssetDatabase.CreateAsset(previewObject, dest);
            AssetDatabase.Refresh();
            Selection.activeObject = previewObject;
        }
#endif
        
        public static void ClearCache()
        {
            StartNodeCache.Clear();
            InputNodeCache.Clear();
            OutputNodeCache.Clear();
            TimerNodeCache.Clear();
        }
        
        private OutputVSNode? GetOutputsNode()
        {
            if (!OutputNodeCache.ContainsKey(this))
            {
                OutputNodeCache.Add(this, VSBaseEngine.GetOutputNode(this)); 
            }
            return OutputNodeCache[this];
        }

        private InputVSNode? GetInputsNode()
        {
            if (!InputNodeCache.ContainsKey(this))
            {
                InputNodeCache.Add(this, VSBaseEngine.GetInputNode(this)); 
            }
            return InputNodeCache[this];
        }

        public IReadOnlyCollection<CooldownTimerNode> GetTimerNodes()
        {
            if(TimerNodeCache.TryGetValue(this, out var cachedNodes))
                return cachedNodes;

            List<CooldownTimerNode> cooldownTimerNodes = VSBaseEngine.GetTimerNodes(this);
            TimerNodeCache.Add(this, cooldownTimerNodes);
            
            return TimerNodeCache[this];
        }

        
        private ExecutableNode? GetStartNode()
        {
            if (!StartNodeCache.ContainsKey(this))
            {
                StartNodeCache.Add(this, VSBaseEngine.GetStartNode(this)); 
            }
            return StartNodeCache[this];
        }

        public void OnAfterDeserialize() { }

        public void OnBeforeSerialize()
        {
            if(LocalTriggers == null)
            {
                localTriggers = new List<LocalTriggerDefinition>();
            }
            if(LocalTriggers.Count == 0)
            {
                LocalTriggerDefinition defaultTimer = new LocalTriggerDefinition
                {
                    Name = "DefaultTimer",
                    CooldownReset = CooldownResetType.Restart,
                };
                localTriggers.Add(defaultTimer);
            }
            
            FetchGUIDAndFileId();
        }

        public void FetchGUIDAndFileId()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(GetInstanceID(), out guid, out fileId);
#endif
        }


        // public IJsonSavableAsset? GetChildAsset(long childFileId)
        // {
        //     if(nodes == null)
        //     {
        //         return null;
        //     }
        //
        //     for (int i = 0; i < nodes.Count; i++)
        //     {
        //         if (nodes[i] is IJsonSavableAsset savableNode && savableNode.FileId == childFileId)
        //         {
        //               return savableNode;
        //         }
        //     }
        //
        //     return null;
        // }
        //
        // public IEnumerable<IJsonSavableAsset> GetChildAssets()
        // {
        //     if(nodes == null)
        //     {
        //         yield break;
        //     }
        //
        //     foreach (Node node in nodes)
        //     {
        //         if (node is IJsonSavableAsset savableNode)
        //         {
        //             yield return savableNode;
        //         }
        //     }
        // }
    }
}
