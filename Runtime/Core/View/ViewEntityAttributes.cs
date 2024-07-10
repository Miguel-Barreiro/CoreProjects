using Core.Model;
using UnityEngine;
	
#nullable enable

namespace Core.View
{
	public class EntityViewAtributes
	{
		public EntityViewAtributes(EntId id)
		{
			GameObject = null;
			ID = id;
		}

		public EntId ID { get; }
		public GameObject? GameObject { get; internal set; }
	}
}