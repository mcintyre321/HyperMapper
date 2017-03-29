using System.Reflection;
using HyperMapper.Mapper;

namespace HyperMapper.Mapping
{
    internal class MethodInfoNodeFactory
    {
        public static MethodInfoNode Create(ExposeAttribute att, MethodInfo mi, AbstractNode parent)
        {
            return new MethodInfoNode(parent, mi);
        }
    }
}