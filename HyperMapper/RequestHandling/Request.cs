using System;

namespace HyperMapper.RequestHandling
{
    public class Request
    {
        public string Method { get; set; }
        public Uri Uri { get; set; }
    }
}