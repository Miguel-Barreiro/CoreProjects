using System;
using FixedPointy;

namespace Core.VSEngine.Events
{
    
    [Serializable]
    public sealed class TestingEvent : VSEvent<TestingEventData>
    {
        //Testing if it will override from data
        [VSField]
        public Fix Value = 5;

        [VSField(IsWritable = true, IsReadable = true)]
        public Fix NewValue = 42;

        public TestingEvent(TestingEventData vsEventData) : base(vsEventData) { }
    }
}