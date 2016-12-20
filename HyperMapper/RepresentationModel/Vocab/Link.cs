using System;
using System.Linq;
using HyperMapper.ResourceModel;

namespace HyperMapper.RepresentationModel.Vocab
{
    public class Link : Property
    {
        public Link(string name, Uri uri, Term term) : base(name, term)
        {
            Uri = uri;
        }

        public Uri Uri { get; private set; }

        public Func<Resource> Follow { get; set; }
 
    }
}