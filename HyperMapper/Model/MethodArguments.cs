using System;

namespace HyperMapper.Model
{
    public class MethodArguments
    {
        public MethodArguments(Tuple<Key, object>[] args)
        {
            Args = args;
        }

        public Tuple<Key, object>[] Args { get; }
    }
}