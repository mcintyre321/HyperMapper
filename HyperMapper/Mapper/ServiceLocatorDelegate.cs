using System;

namespace HyperMapper.Mapper
{
    public delegate Tuple<object, Action> ServiceLocatorDelegate(Type type);
}