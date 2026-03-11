using System.Collections.Generic;

#nullable enable

namespace Core.VSEngine
{
    public interface IInjectableCombatNode
    {
        public void Inject();
    }

    public class CombatActionNode : ActionNode, IInjectableCombatNode
    {
        public IEnumerable<IActionGraphContext>? Contexts { get; private set; }

        public void Inject()
        {
        }
    }
}
