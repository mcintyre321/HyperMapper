using System;
using HyperMapper.RepresentationModel;

namespace HyperMapper.Mapper
{
    [HyperMapper(UseTypeNameAsClassNameForEntity = false)]
    public class RootNode : Node
    {
        private readonly Uri _uri;
        public sealed override Uri Uri => _uri;

        public RootNode(string title, Uri uri, Term term) : base(title, term)
        {
            _uri = uri;
        }
    }
}
