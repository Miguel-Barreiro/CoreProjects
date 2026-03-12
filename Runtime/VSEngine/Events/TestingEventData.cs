using FixedPointy;
using UnityEngine;

namespace Core.VSEngine.Events
{
    public class TestingEventData : VSEventData
    {
        [VSField(IsWritable = false, IsReadable = true), SerializeField]
        public Fix Value = 42;
        
        [VSField(IsWritable = true, IsReadable = false), SerializeField]
        public string LogValue = "before";

        [VSField(IsWritable = true, IsReadable = true), SerializeField]
        public string TestProperty = "before";

    }
}