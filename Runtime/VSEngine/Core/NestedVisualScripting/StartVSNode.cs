using UnityEngine;
using XNode;
using Core.VSEngine;
using Core.VSEngine.Nodes;
using UnityEngine.Serialization;

namespace Core.VSEngine.NestedVisualScripting
{
    
    [Node.CreateNodeMenu(VSNodeMenuNames.FLOW_MENU+"/" + VSNodeMenuNames.NESTED_MENU +"/"+ VSNodeMenuNames.NESTED_TITLE+ "Start", order = VSNodeMenuNames.LOW)]
    [NodeTint(VSNodeMenuNames.SCRIPT_TINT)]
    [NodeWidth(600)]
    public class StartVSNode : ExecutableNode
    {
        [Node.Output(Node.ShowBackingValue.Never)]
        [SerializeField]
        private Control? Continue = null;
        
        public override void Execute()
        {
            ContinueWith(nameof(Continue));
        }
    }
}