using System;
using System.Collections.Generic;
using Core.Utils;
using Core.Zenject.Source.Factories.Pooling.Static;
using UnityEngine;

namespace Core.Model.ModelSystems
{
	public abstract class MultiArrayComponentContainer<TComponentData>  : ComponentContainer<TComponentData>
		where TComponentData : struct, IComponentData
	{
		protected abstract int ComponentArrayCount { get; }
		
		// the index 0 is the main array
		public PushBackArray<TComponentData>[] ComponentArrays;
		
		private readonly Dictionary<EntId, ComponentAttributes> ComponentIndexByOwner = new Dictionary<EntId, ComponentAttributes>();
		
		
		/// This is a dummy component to return when the requested component is not found
		/// do not override it
		public TComponentData Invalid = new TComponentData()
		{
			ID = EntId.Invalid
		};

		protected MultiArrayComponentContainer(uint maxNumber)
		{
			RebuildWithMax(maxNumber);
		}

		protected void SwitchToArray(EntId target, uint newArrayIndex)
		{
			if (!ComponentIndexByOwner.TryGetValue(target, out ComponentAttributes attributes))
				return;
			
			uint arrayType = attributes.ArrayType;
			uint index = attributes.Index;

			if(attributes.ArrayType == newArrayIndex)
				return;
			
			TComponentData componentData = GetComponent(target);
			
			ref PushBackArray<TComponentData> pushBackArray = ref ComponentArrays[arrayType];
			RemoveFromArray(index, ref pushBackArray);
			
			ref PushBackArray<TComponentData> newPushBackArray = ref ComponentArrays[newArrayIndex];
			newPushBackArray.Add(componentData, out uint newIndex);
			attributes.Index = newIndex;
			attributes.ArrayType = newArrayIndex;
		}
		
		protected void RemoveFromArray(uint index, ref PushBackArray<TComponentData> pushBackArray)
		{
			pushBackArray.Remove(index, out uint changedIndex);
			
			ref TComponentData componentData = ref pushBackArray.Items[changedIndex];
			EntId changedEntityID = componentData.ID;

			ComponentIndexByOwner[changedEntityID].Index = changedIndex;
		}

		
		public virtual void SetupComponent(EntId owner)
		{
			if (ComponentIndexByOwner.ContainsKey(owner))
			{
				Debug.LogError($"Component(TestDD1ComponentData) already exists for owner {owner}");
				return;
			}

			ref PushBackArray<TComponentData> pushBackArray = ref ComponentArrays[0];
			if (!pushBackArray.SetupNew(out uint newIndex))
			{
				Debug.LogError($"No available space for new component(TestDD1ComponentData), FILLED[{pushBackArray.Count}] ");
				return;
			}

			TComponentData[] componentDatas = pushBackArray.Items;
			componentDatas[newIndex].ID = owner;
			ComponentIndexByOwner[owner] = ComponentAttributes.New(newIndex, 0);
			componentDatas[newIndex].Init();
		}

		public virtual void RemoveComponent(EntId owner)
		{
			if (!ComponentIndexByOwner.TryGetValue(owner, out ComponentAttributes attributes))
				return;
			
			uint arrayType = attributes.ArrayType;
			uint index = attributes.Index;

			ref PushBackArray<TComponentData> pushBackArray = ref ComponentArrays[arrayType];
			
			RemoveFromArray(index, ref pushBackArray);
			ComponentIndexByOwner.Remove(owner);
		}
		
		public ref TComponentData GetComponent(EntId owner)
		{
			if (!ComponentIndexByOwner.TryGetValue(owner, out ComponentAttributes attributes))
			{
				Debug.LogError($"Component({typeof(TComponentData)}) not found for owner {owner}");
				return ref Invalid;
			}
			uint arrayType = attributes.ArrayType;
			uint index = attributes.Index;

			ref PushBackArray<TComponentData> pushBackArray = ref ComponentArrays[arrayType];
			return ref pushBackArray.Items[index];
		}
		
		
		
		public virtual void RebuildWithMax(uint maxNumber)
		{
			ComponentIndexByOwner.Clear();
			ComponentArrays = new PushBackArray<TComponentData>[ComponentArrayCount];
			for (int i = 0; i < ComponentArrayCount; i++)
				ComponentArrays[i] = new PushBackArray<TComponentData>(maxNumber);
		}
		
		// the index 0 is the main array
		public uint Count => ComponentArrays[0].Count;
		public uint MaxCount => ComponentArrays[0].MaxCount;

		
		
		
		private class ComponentAttributes
		{
			private static readonly StaticMemoryPool<ComponentAttributes> Pool =
				new StaticMemoryPool<ComponentAttributes>(OnSpawned, OnDespawned);

			private static void OnDespawned(ComponentAttributes obj) { }
			private static void OnSpawned(ComponentAttributes obj) {}

			public uint Index;
			public uint ArrayType;
			public static ComponentAttributes New(uint index, uint arrayType)
			{
				ComponentAttributes attributes = Pool.Spawn();
				
				attributes.Index = index;
				attributes.ArrayType = arrayType;

				return attributes;
			}

			public ComponentAttributes() {}
		}
	}
}