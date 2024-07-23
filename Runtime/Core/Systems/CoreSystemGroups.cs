namespace Core.Systems
{
	public static class CoreSystemGroups
	{
		public static SystemGroup CoreSystemGroup = new SystemGroup("Core");
		public static SystemGroup CorePhysicsEntitySystemGroup = new SystemGroup("Core.Entities.View");
		public static SystemGroup CoreViewEntitySystemGroup = new SystemGroup("Core.Entities.View");

	}
}