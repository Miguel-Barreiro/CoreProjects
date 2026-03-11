
using FixedPointy;
using UnityEngine;
using XNode;

#nullable enable

namespace Core.VSEngine.TestNodes
{
    [Node.CreateNodeMenu("Miguel/testing/TestLogNode (obsolete)", order = 2)]
    [Node.NodeTint("#452438")]
    public class TestLogNode : BasicFlowNode
    {
        [Header("OBSOLETE: use the new")]
        [Header("Utility/Debug/DebugLogNode")]
        [Header("OBSOLETE:")]
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
            }

            Fix valueResolved = Resolve<Fix>(nameof(value));
            string? stringValueResolved = Resolve<string>(nameof(valueString));

            Debug.Log($"VS: {Header}: value: {valueResolved} stringValue ({stringValueResolved})");
        }
    }
}