using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperMapper.Mapper
{
    class UriHelper
    {
        public static Uri Combine(Uri left, string right)
        {
            return  new Uri('/' + string.Join("/", (left.ToString() + '/' + right).Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries)), UriKind.Relative);
        }
    }
}
