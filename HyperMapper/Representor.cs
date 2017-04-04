using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HyperMapper.RepresentationModel;

namespace HyperMapper
{
    public abstract class Representor<TRep>
    {

        public abstract Task<Tuple<string, string>> GetResponse(TRep hypermediaObject, FindUriForTerm termUriFinder);
        public abstract IEnumerable<string> AcceptTypes { get;  }
    }
}