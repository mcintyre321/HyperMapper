using System;
using System.Collections.Generic;
using HyperMapper.Model;

namespace HyperMapper
{
    public abstract class Representor
    {
        public abstract Tuple<string, string> GetResponse(Representation hypermediaObject);
        public abstract IEnumerable<string> AcceptTypes { get;  }
    }
}