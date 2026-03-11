using FixedPointy;
using UnityEngine;
using XNode;

[CreateNodeMenuAttribute("Miguel/testing/TestCombatNode")]
[NodeTint("#3d4254")]
public class NewNode : Node {

	[SerializeField]
	[TextArea(2, 10)]
	private string Header;

	[Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
	private Fix In;
	
	[Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
	private Fix Out;
	
	// Use this for initialization
	protected override void Init() {
		base.Init();
		
	}

	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port) {
		return null; // Replace this
	}
}