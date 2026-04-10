namespace Core.VSEngine.Nodes
{
	public static partial class VSNodeMenuNames
	{
		public const string EVENTS_MENU = "Events";
		public const string MATH_MENU = "Math";
		public const string FLOW_MENU = "Flow";
		public const string ABILITIES_MENU = "Abilities";
		public const string LOGIC_MENU = "Logic";
		public const string UTILITIES_MENU = "Utilities";
		public const string TEST_MENU = "TEST";
		public const string GAME_MENU = "Game";
		public const string LIST_MENU = "List";
		
		
		//submenus
		public const string UNIT_TEST_MENU = "UnitTests";
		public const string VALUES_MENU = "Values";
		public const string NESTED_MENU = "Nested";
		

		//other node properties
		
		public const string DEFAULT_TINT = "#3d4254";
		public const string DEFAULT_HIGHER_TINT = "#2a2f3f";

		public const string WRITE_NODES_TINT = "#24173b";
		public const string LISTEN_NODES_TINT = "#215C32";
		public const string CONDITION_NODES_TINT = "#3d4254";
		public const string FLOW_NODES_TINT = DEFAULT_TINT;
		// public const string FLOW_NODES_TINT = "#0F3629";
		public const string LIST_NODES_TINT = "#3d4254";
		
		
		public const string DEBUG_NODES_TINT = "#452438";
		public const string DEBUG_TEST_NODES_TINT = "#530e68";

		public const string MATH_NODES_TINT = "#0D4F5C";
		public const string VALUES_NODES_TINT = "#141e60";
		
		public const string VARIABLE_NODES_TINT = DEFAULT_TINT;              //"#141e60";
		public const string VARIABLE_WRITE_NODES_TINT = DEFAULT_HIGHER_TINT; //"#0E234F";
		
		public const string SCRIPT_TINT = "#3d4254";
		public const string NOT_WORKING_TINT = "#C22211";

		
		
		//top order
		public const int VERY_IMPORTANT = 1;
		public const int IMPORTANT = 5;
		public const int MEDIUM = 10;
		public const int LOW = 15;
		public const string NESTED_TITLE = "[Nested] ";
	}
}
