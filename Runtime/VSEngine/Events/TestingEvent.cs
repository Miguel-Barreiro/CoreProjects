using System;
using Core.Events;
using FixedPointy;

namespace Core.VSEngine.Events
{
    
    [Serializable]
    public sealed class TestingEvent : Event<TestingEvent>
    {
        //Testing if it will override from data
        [VSField(IsWritable = true)]
        public Fix Value = 5;

        [VSField(IsWritable = true, IsReadable = true)]
        public Fix NewValue = 42;
        
        public override void Execute()
        {
            
        }
    }
}