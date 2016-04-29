using System;
using System.Collections.Generic;

namespace HyperMapper.HyperModel
{
    public class SubEntityRef  
    {
        public IEnumerable<Rel> Rels { get; set; }
        public Uri Uri { get; set; }
        public Func<Entity> FetchEntity { get; set; }
        public IEnumerable<string> Classes { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
    }
}