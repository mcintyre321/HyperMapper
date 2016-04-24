using System;

namespace HyperMapper
{
    public class HyperMapperAttribute : Attribute 
    {
        public bool UseTypeNameAsClassNameForEntity { get; set; } = true;
    }
}