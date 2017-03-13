using System;
using System.Collections.Generic;
using HyperMapper.RepresentationModel;

namespace HyperMapper
{
    public abstract class Representor<TRep>
    {

        public abstract Tuple<string, string> GetResponse(TRep hypermediaObject, FindUriForTerm termUriFinder);
        public abstract IEnumerable<string> AcceptTypes { get;  }
    }
}