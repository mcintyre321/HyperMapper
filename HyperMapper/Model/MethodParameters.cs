using System;

namespace HyperMapper.RequestHandling
{
    public class MethodParameters
    {
        public MethodParameters(Tuple<Key, object>[] args)
        {
            Args = args;
        }

        public Tuple<Key, object>[] Args { get; }
    }
}