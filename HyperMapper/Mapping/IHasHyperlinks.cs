using System;
using System.Collections.Generic;
using HyperMapper.Vocab;

namespace HyperMapper.Mapping
{
    public interface IHasHyperlinks
    {
        IEnumerable<Tuple<Term, Uri, string>> Hyperlinks { get; }
    }
}
