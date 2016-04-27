using System;

namespace HyperMapper.DomainMapping
{
    public class HyperMapperAttribute : Attribute 
    {
        public bool UseTypeNameAsClassNameForEntity { get; set; } = true;
    }
}