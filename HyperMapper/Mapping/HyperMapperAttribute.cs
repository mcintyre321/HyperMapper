using System;

namespace HyperMapper.Mapping
{
    public class HyperMapperAttribute : Attribute 
    {
        public bool UseTypeNameAsClassNameForEntity { get; set; } = true;
    }
}