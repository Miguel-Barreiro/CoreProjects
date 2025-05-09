using Core.Model.ModelSystems;
using UnityEngine;

namespace Core.Model
{
	
	
	public struct KineticEntityData : IComponentData
	{
		public Vector3 Position;
		public GameObject Prefab;
		public Rigidbody Rigidbody;
		public EntId ID { get; set; }

		public void Init()
		{
			Position = Vector3.zero;
			Prefab = null;
			Rigidbody = null;
		}
	}
	
	public interface IKineticEntity : Component<KineticEntityData> { }
}