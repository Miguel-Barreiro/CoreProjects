using System;

namespace Core.Systems
{
	[AttributeUsage(AttributeTargets.Class)]
	public class OnDestroyComponentPropertiesAttribute : Attribute
	{
		public SystemPriority Priority { get; set; } = SystemPriority.Default;
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class OnCreateComponentPropertiesAttribute : Attribute
	{
		public SystemPriority Priority { get; set; } = SystemPriority.Default;
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class UpdateComponentPropertiesAttribute : Attribute
	{
		public SystemPriority Priority { get; set; } = SystemPriority.Default;
	}
	
	public enum SystemPriority
	{
		Early = 0, 
		Default = 1,
		Late = 2
	}
}