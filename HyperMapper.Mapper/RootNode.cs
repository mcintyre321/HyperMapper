using System;

namespace HyperMapper.Mapping
{
    [HyperMapper(UseTypeNameAsClassNameForEntity = false)]
    public class RootNode : Node
    {
        private readonly Uri _uri;
        public sealed override Uri Uri => _uri;

        public RootNode(string title, Uri uri) : base(title)
        {
            _uri = uri;
        }
    }
}
