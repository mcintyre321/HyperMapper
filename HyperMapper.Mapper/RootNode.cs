﻿using System;
using HyperMapper.RepresentationModel;

namespace HyperMapper.Mapping
{
    [HyperMapper(UseTypeNameAsClassNameForEntity = false)]
    public class RootNode : Node
    {
        private readonly Uri _uri;
        public sealed override Uri Uri => _uri;

        public RootNode(string title, Uri uri, Term[] terms) : base(title, terms)
        {
            _uri = uri;
        }
    }
}
