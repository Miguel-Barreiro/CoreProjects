using System.Collections.Generic;
using Core.Model;
using Core.Systems;
using Core.Zenject.Source.Factories.Pooling.Static;
using FixedPointy;
using UnityEngine;
using Zenject;

namespace Core.VSEngine
{
	public interface IBaseMemoryArena
	{
		public List<EntId> GetEntIdList();
		public List<Fix> GetNumberList();
		public List<FixVec3> GetFixVec3List();
		public List<Vector3> GetVec3List();
	}

	public class BaseMemoryArena : IBaseMemoryArena, IInitSystem, IOnDestroySystem
	{
		[Inject] private readonly SystemsController SystemsController = null!;
		
		private readonly ListPool<EntId> EntPoolList = new ListPool<EntId>();
		private readonly ListPool<Fix> NumberPoolList = new ListPool<Fix>();
		private readonly ListPool<FixVec3> FixVec3PoolList = new ListPool<FixVec3>();
		private readonly ListPool<Vector3> Vec3PoolList = new ListPool<Vector3>();
		
		public List<EntId> GetEntIdList()
		{
			return EntPoolList.GetNewList();
		}

		public List<Fix> GetNumberList()
		{
			return NumberPoolList.GetNewList();
		}
		public List<FixVec3> GetFixVec3List()
		{
			return FixVec3PoolList.GetNewList();
		}
		public List<Vector3> GetVec3List()
		{
			return Vec3PoolList.GetNewList();
		}

		protected virtual void Clear()
		{
			EntPoolList.Clear();
			NumberPoolList.Clear();
			FixVec3PoolList.Clear();
			Vec3PoolList.Clear();
		}

		public void Initialize()
		{
			SystemsController.OnEndFrame += Clear;
		}
		public void OnDestroy()
		{
			SystemsController.OnEndFrame -= Clear;
		}
		
		
		
		protected sealed class ListPool<T>
		{
			private readonly StaticMemoryPool<List<T>> Pool = new StaticMemoryPool<List<T>>(OnSpawned, OnDespawned);
			private readonly List<List<T>> AllUsedLists = new List<List<T>>();
			
			private static void OnDespawned(List<T> obj) { obj.Clear(); }
			private static void OnSpawned(List<T> obj) { }
			
			public List<T> GetNewList()
			{
				var list = Pool.Spawn();
				AllUsedLists.Add(list);
				return list;
			}
			
			public void Clear()
			{
				foreach (var list in AllUsedLists)
					Pool.Despawn(list);
				AllUsedLists.Clear();
			}
		}
	}
	
}