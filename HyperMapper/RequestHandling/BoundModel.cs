using System;

namespace HyperMapper.RequestHandling
{
    public class BoundModel
    {
        public BoundModel(Tuple<Key, object>[] args)
        {
            Args = args;
        }

        public Tuple<Key, object>[] Args { get; }
    }
}