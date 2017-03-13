using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyperMapper.RepresentationModel;
using HyperMapper.Vocab;

namespace HyperMapper.Mapping
{
    public interface IHasHyperlinks
    {
        IEnumerable<Tuple<Term, Uri, string>> Hyperlinks { get; }
    }
}
