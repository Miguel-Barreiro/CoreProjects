using Core.Model.ModelSystems;
using UnityEngine;

namespace Core.Model
{
	
	
	public struct IKineticEntityData : IComponentData
	{
		public Vector3 Position;
		public GameObject Prefab;
		public Rigidbody Rigidbody;
		public EntId ID { get; set; }
	}
	
	public interface IKineticEntity : Component<IKineticEntityData> { }
}