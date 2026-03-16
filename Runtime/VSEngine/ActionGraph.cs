using System.Collections.Generic;
using Core.VSEngine.NestedVisualScripting;
using UnityEngine;
using XNode;

#nullable enable

namespace Core.VSEngine
{
    [CreateAssetMenu(fileName = "new_actiongraph", menuName = "!Game/Visual Scripting/Actiongraph", order = 1)]
    public sealed class ActionGraph : NodeGraph, ISerializationCallbackReceiver
    //                                                 , IJsonSavableAsset
    {
        [SerializeField]
        private string guid;
        public string Guid => guid;

        [SerializeField]
        private long fileId;
        public long FileId => fileId;
        
        public OutputVSNode? OutputsNode => GetOutputsNode();
        public InputVSNode? InputsNode => GetInputsNode();
        public ExecutableNode? StartNode => GetStartNode();

        private static Dictionary<ActionGraph, ExecutableNode?> StartNodeCache = new Dictionary<ActionGraph, ExecutableNode?>();
        private static Dictionary<ActionGraph, InputVSNode?> InputNodeCache = new Dictionary<ActionGraph, InputVSNode?>();
        private static Dictionary<ActionGraph, OutputVSNode?> OutputNodeCache = new Dictionary<ActionGraph, OutputVSNode?>();

        public static void ClearCache()
        {
            StartNodeCache.Clear();
            InputNodeCache.Clear();
            OutputNodeCache.Clear();
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
