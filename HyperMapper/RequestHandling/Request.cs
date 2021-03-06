using System;
using HyperMapper.ResourceModel;

namespace HyperMapper.RequestHandling
{
    public class Request
    {
        public Request(Method method, Uri uri)
        {
            Method = method;
            Uri = uri;
        }

        public Method Method { get; }
        public Uri Uri { get; }
    }
}