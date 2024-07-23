using System;

namespace Core.Systems
{
	[AttributeUsage(AttributeTargets.Class)]
	public class EntitySystemPropertiesAttribute : Attribute
	{
		public SystemPriority LifetimePriority { get; set; } = SystemPriority.Default;
		public SystemPriority UpdatePriority { get; set; } = SystemPriority.Default;
	}

	public enum SystemPriority
	{
		Early = 0, 
		Default = 1,
		Late = 2
	}
}