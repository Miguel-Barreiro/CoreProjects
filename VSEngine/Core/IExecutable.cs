using System.Collections.Generic;
using XNode;

namespace Core.VSEngine
{
	public interface IExecutable
	{
		public NodeExecutionResult ResolveControl(List<Node> nextNodes);
	}
}