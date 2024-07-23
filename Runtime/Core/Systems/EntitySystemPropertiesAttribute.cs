using System;

namespace Core.Systems
{
	[AttributeUsage(AttributeTargets.Class)]
	public class EntitySystemPropertiesAttribute : Attribute
	{
		public SystemPriority SystemPriority{ get; set; }
	}

	public enum SystemPriority
	{
		Early = 0, 
		Default = 1,
		Late = 2
	}
}