using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperMapper.Mapping.ResultTypes
{
    public class Created
    {
        public Created(AbstractNode node)
        {
            Node = node;
        }

        public AbstractNode Node { get; }

    }
    public class Modified
    {
        public Modified(AbstractNode node)
        {
            Node = node;
        }

        public AbstractNode Node { get; }

    }

}
