#nullable enable

using Core.Utils;
using FixedPointy;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.TestNodes
{
    [Node.CreateNodeMenu(VSNodeMenuNames.TEST_MENU+"/Debug Log Node", order = VSNodeMenuNames.IMPORTANT)]
    [Node.NodeTint(VSNodeMenuNames.DEBUG_NODES_TINT)]
    public class DebugLogNode : BasicFlowNode
    {
        [SerializeField]
        private bool Pause = false;
        [SerializeField]
        [TextArea(2, 10)]
        private string Header;

        [Node.Input(Node.ShowBackingValue.Never, Node.ConnectionType.Override, Node.TypeConstraint.Strict), SerializeField]
        private Fix value;

        [Node.Input(Node.ShowBackingValue.Unconnected, Node.ConnectionType.Override, Node.TypeConstraint.Strict), SerializeField]
        [TextArea(3, 10)]
        private string valueString;

        protected override void Action()
        {
            if (Pause)
            {
                Debug.Log($"PAUSE");
                Debug.DebugBreak();
            }

            OperationResult<Fix> valueResolved = Resolve<Fix>(nameof(value));
            if (valueResolved.IsFailure)
            {
                Debug.Log($"VS: {Header}: value: null , stringValue (null)");    
                return;
            }

            OperationResult<string> stringValueResolved = Resolve<string>(nameof(valueString));
            if (valueResolved.IsFailure)
            {
                Debug.Log($"VS: {Header}: value: {valueResolved},  stringValue (null)");    
                return;
            }

            Debug.Log($"VS: {Header}: value: {valueResolved},  stringValue ({stringValueResolved})");
        }
    }
}