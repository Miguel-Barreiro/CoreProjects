using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VSEngine
{

	// public interface IVSListenerComponent : Component<VSListenerComponentData> { }
	//
	// [StructLayout(LayoutKind.Auto)]
	// public struct VSListenerComponentData : IComponentData
	// {
	// 	public EntId ID { get; set; }
	//
	// 	public void Init() { }
	// }

	// public sealed class VSListenerContainer : MultiArrayComponentContainer<VSListenerComponentData>
	// {
	// 	public Dictionary<>
	// 	
	// 	public VSListenerContainer() : base(100) { }
	//
	// 	protected override int ComponentArrayCount => LISTENER_ARRAYS_COUNT;
	// 	private const int LISTENER_ARRAYS_COUNT = 3;
	// 	// Array 0 is for listeners that are currently inactive
	// 	// Array 1 is for listeners that are currently active and should be updated every Day
	// 	// Array 2 is for listeners that are currently active but should not be updated every Day
	// 	
	// 	
	// 	public override void RemoveComponent(EntId owner)
	// 	{
	// 		
	// 		if (!ComponentIndexByOwner.TryGetValue(owner, out ComponentAttributes attributes))
	// 			return;
	// 		
	// 		uint arrayType = attributes.ArrayType;
	// 		uint index = attributes.Index;
	//
	// 		ref PushBackArray<TComponentData> pushBackArray = ref ComponentArrays[arrayType];
	// 		
	// 		RemoveFromArray(index, ref pushBackArray);
	// 		ComponentIndexByOwner.Remove(owner);
	// 	}
	// }
	
}

