using XNode;

namespace Core.VSEngine.Events
{
    [Node.CreateNodeMenu("Miguel/events/test/TestingListenNode", order = 2)]
    [Node.NodeWidth(300)]
    public sealed class OnTestingEventNode : EventListenNode<TestingEvent>
    {
    }
}