using UnityEngine;

namespace Core.Model
{
	public interface IKineticEntity : IPositionEntity
	{
		GameObject Prefab { get; }
	}
}