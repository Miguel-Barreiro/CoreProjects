using System.Runtime.InteropServices;
using Core.Model.ModelSystems;
using Core.Systems;
using FixedPointy;
using UnityEngine;
using Zenject;

namespace Core.Model
{
	public interface IRandomSystem
	{
		Fix Range(Fix min, Fix max);
	}
	
	public sealed class RandomSystem : IRandomSystem, IInitSystem, IOnDestroyEntitySystem
	{
		[Inject] private readonly RandomModel RandomModel = null!;
		[Inject] private readonly SystemsController SystemsController = null!;
		[Inject] private readonly BasicCompContainer<RandomData> RandomContainer = null!;

		public Fix Range(Fix min, Fix max)
		{
			return Random.Range((float) min, (float)max);
		}

		private void OnStartFrame()
		{
			ref RandomData randomData = ref RandomContainer.GetComponent(RandomModel.ID);
			Random.InitState(randomData.CurrentSeed);
		}
		
		public void Initialize()
		{
			SystemsController.OnStartFrame += OnStartFrame;
		}

		public void OnDestroyEntity(EntId destroyedEntityId)
		{
			SystemsController.OnStartFrame -= OnStartFrame;
		}
	}


	public sealed class RandomModel : Entity, IRandom { }

	[StructLayout(LayoutKind.Auto)]
	public struct RandomData : IComponentData
	{
		public EntId ID { get; set; }
		public int CurrentSeed;

		public void Init() { }
	}

	public interface IRandom : Component<RandomData> { }
}