using System;

namespace HyperMapper.Mapper
{
    public class HyperMapperAttribute : Attribute 
    {
        public bool UseTypeNameAsClassNameForEntity { get; set; } = true;
    }
}