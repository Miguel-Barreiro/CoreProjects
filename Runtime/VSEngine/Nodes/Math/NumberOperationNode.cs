using Core.Utils;
using FixedPointy;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.Math
{
    [CreateNodeMenu(VSNodeMenuNames.MATH_MENU+"/Operator", order = 2)]
    [NodeTint(VSNodeMenuNames.MATH_NODES_TINT)]

    public class NumberOperationNode : ValueOnlyNode
    {
        [SerializeField]
        private MathOperation type = MathOperation.Sum;
        public MathOperation Type => type;
        
        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
        private Fix InputA = Fix.Zero;
        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
        private Fix InputB = Fix.Zero;

        [Output(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.Strict), SerializeField]
        private Fix Result;
        
        
        public override OperationResult<object> GetValue(string portName)
        {
            return portName switch
            {
                nameof(Result) => Calculate(),
                _ => OperationResult<object>.Failure($"Incorrect port name given{portName} to node {name} in {graph.name}"),
            };
        }

        private OperationResult<object> Calculate()
        {
            OperationResult<Fix> inputA = Resolve<Fix>(nameof(InputA)); 
            OperationResult<Fix> inputB = Resolve<Fix>(nameof(InputB));
            if(Check(inputA.IsFailure, "inputA is null"))
                return OperationResult<object>.Failure("inputA is null");

            if(Check(inputB.IsFailure, "inputB is null"))
                return OperationResult<object>.Failure("inputA is null");
            
            return type switch
            {
                MathOperation.Sum => OperationResult<object>.Success(inputA.Result + inputB.Result),
                MathOperation.Multiplication => OperationResult<object>.Success(inputA.Result * inputB.Result),
                MathOperation.Subtraction => OperationResult<object>.Success(inputA.Result - inputB.Result),
                MathOperation.Division => OperationResult<object>.Success(inputA.Result / inputB.Result),
                MathOperation.Modulo => OperationResult<object>.Success(inputA.Result % inputB.Result),
            };
        }
        
        public enum MathOperation
        {
            Sum, 
            Subtraction,
            Multiplication,
            Division,
            Modulo,
        }
    }
    
    public static class MathOperationExtension{
        public static string Header(this NumberOperationNode.MathOperation operation)
        {
            return operation switch
            {
                NumberOperationNode.MathOperation.Sum => "Plus",
                NumberOperationNode.MathOperation.Subtraction => "Minus",
                NumberOperationNode.MathOperation.Multiplication => "Multiply",
                NumberOperationNode.MathOperation.Division => "Divide",
                NumberOperationNode.MathOperation.Modulo => "Modulo",
                _ => "?",
            };
        }
    
    }
}