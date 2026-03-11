using Core.Model;

namespace Core.VSEngine.Events
{
    public class EntityEventData : VSEventData
    {
        [VSField]
        public EntId EventEntityId;

        protected EntityEventData(EntId eventEntityId)
        {
            EventEntityId = eventEntityId;
        }
    }
}