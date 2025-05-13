using Core.Model.ModelSystems;
using Core.Systems;
using UnityEngine;

namespace Core.Model
{
	
	[ComponentDataProperties(Priority = SystemPriority.Early)]
	public struct KineticComponentData : IComponentData
	{
		// public Vector3 Position;
		public GameObject Prefab;
		public Rigidbody Rigidbody;
		public EntId ID { get; set; }

		public void Init()
		{
			// Position = Vector3.zero;
			Prefab = null;
			Rigidbody = null;
		}
	}
	
	public interface IKineticComponent : IPositionComponent, Component<KineticComponentData> { }
}