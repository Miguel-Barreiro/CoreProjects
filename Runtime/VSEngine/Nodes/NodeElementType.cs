using System;
using Core.Model;
using FixedPointy;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

namespace Core.VSEngine.Nodes
{
	public enum NodeElementType
	{
		Numbers,
		Entities,
		Bools,
		TileCoordinates,
		Positions, 
		Tags, 
	}
    
	public static class ElementTypeExtensions
	{
		public static Type GetLogicType(this NodeElementType type)
		{
			return type switch
			{
				NodeElementType.Numbers => typeof(Fix),
				NodeElementType.Entities => typeof(EntId),
				NodeElementType.Bools => typeof(bool),
				NodeElementType.Positions => typeof(Vector3),
				NodeElementType.TileCoordinates => typeof(Vector2Int),
				NodeElementType.Tags => typeof(Tag),
			};
		}
        
		public static bool Compare(this NodeElementType type, object a, object b)
		{
			return type switch
			{
				NodeElementType.Numbers => (Fix)a == (Fix)b,
				NodeElementType.Entities => (EntId)a == (EntId)b,
				NodeElementType.Bools => (bool)a == (bool)b,
				NodeElementType.TileCoordinates => (Vector2Int)a == (Vector2Int)b,
				NodeElementType.Positions => (Vector3)a == (Vector3)b, 
				NodeElementType.Tags => (Tag)a == (Tag)b,
			};
		}
	}
}