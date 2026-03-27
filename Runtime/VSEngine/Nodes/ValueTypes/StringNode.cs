using Core.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.VSEngine.Nodes
{
    [NodeWidth(300)]
    [CreateNodeMenu(VSNodeMenuNames.VALUES_MENU +"/String", order = VSNodeMenuNames.IMPORTANT)]
    [NodeTint(VSNodeMenuNames.VALUES_NODES_TINT)]
    public class StringNode : SimpleValueNode<string>
    {
        [SerializeField, HideLabel]
        [TextArea(3, 10)]
        private string Text;
        

        public override OperationResult<object> GetValue(string portName)
        {
            return OperationResult<object>.Success(Text);
            
        }
    }
}