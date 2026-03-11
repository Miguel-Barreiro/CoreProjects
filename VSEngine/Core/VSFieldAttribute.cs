using System;

namespace Core.VSEngine
{
    public class VSFieldAttribute : Attribute
    {
        public bool IsWritable { get; set; } = false;
        public bool IsReadable { get; set; } = true;
    }
}