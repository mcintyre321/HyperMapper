using System;
using System.Collections.Generic;
using HyperMapper.ResourceModel;

namespace HyperMapper.RepresentationModel
{
    public class Operation
    {
        public IEnumerable<MethodParameter> Parameters { get; set; }
        public Uri Uri { get; private set; }
        public string Name { get; private set; }


        public Operation(string name, IEnumerable<MethodParameter> parameters, Uri uri)
        {
            Name = name;
            Parameters = parameters;
            Uri = uri;
        }
    }
}