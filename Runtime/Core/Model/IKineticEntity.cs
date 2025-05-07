using Core.Model.ModelSystems;
using UnityEngine;

namespace Core.Model
{
	
	
	public struct IKineticEntityData : IComponentData
	{
		public Vector3 Position { get; set; }
		public GameObject Prefab { get; set; }
		public Rigidbody Rigidbody { get; set; }
		public EntId ID { get; set; }
	}
	
	public interface IKineticEntity : Component<IKineticEntityData>
	{
		GameObject Prefab { get; }
	}
}