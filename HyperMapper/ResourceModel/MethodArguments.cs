using System;

namespace HyperMapper.ResourceModel
{
    public class MethodArguments
    {
        public MethodArguments(Tuple<UrlPart, object>[] args)
        {
            Args = args;
        }

        public Tuple<UrlPart, object>[] Args { get; }
    }
}