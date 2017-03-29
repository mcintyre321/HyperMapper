using System;

namespace HyperMapper.Mapping
{
    public class OptionsFromAttribute : Attribute
    {
        internal string MethodName { get; }
        public OptionsFromAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}