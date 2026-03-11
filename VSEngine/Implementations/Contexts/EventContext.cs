
using Core.Model;

namespace Core.VSEngine.Contexts
{
    public sealed class EventContext : RuntimeContexts
    {
        private EventContext() { }

        public EntId ExecutionOwner { get; private set; }
        public EntId EventEntity { get; private set; }
        
        public static EventContext New(EntId executionOwner, EntId eventEntity)
        {
            EventContext eventContext = new EventContext();
            eventContext.ExecutionOwner = executionOwner;
            eventContext.EventEntity = eventEntity;
            return eventContext;
        }
    }
}