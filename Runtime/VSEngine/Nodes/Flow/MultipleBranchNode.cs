using Core.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using VSEngine;
using XNode;

namespace Core.VSEngine.Nodes
{
    
    [CreateNodeMenu(VSNodeMenuNames.FLOW_MENU+"/Multiple Paths", order = VSNodeMenuNames.IMPORTANT)]
    [NodeTint(VSNodeMenuNames.FLOW_NODES_TINT)]
    public class MultipleBranchNode : BasicFlowNode
    {
        [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict, dynamicPortList = true), SerializeField]
        private Control?[] Sequence = null;

        protected override void Action()
        {
            MultipleContinue(nameof(Sequence));
        }
        
        public override OperationResult<object> GetValue(string portName)
        {
            throw new System.NotImplementedException();
        }
    }
}