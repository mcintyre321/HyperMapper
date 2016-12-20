using System;
using System.Collections.Generic;
using System.Linq;
using HyperMapper.ResourceModel;

namespace HyperMapper.RepresentationModel.Vocab
{
    public class Operation : Property
    {
        public IEnumerable<MethodParameter> Parameters { get; set; }
        public Uri Uri { get; private set; }


        public Operation(string name, IEnumerable<MethodParameter> parameters, Uri uri, Term term) : base(name, term)
        {
            Parameters = parameters;
            Uri = uri;
        }
    }
}