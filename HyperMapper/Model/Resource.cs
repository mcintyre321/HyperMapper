using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyperMapper.RequestHandling;
using OneOf;

namespace HyperMapper.Model
{
    public class Resource
    {
        private readonly IEnumerable<MethodHandler> _methodHandlers;

        public Resource(Uri uri, string[] @class,
            IEnumerable<Link> children, IEnumerable<MethodHandler> methodHandlers)
        {
            _methodHandlers = methodHandlers;
            Uri = uri;
            Class = @class;
            Children = children;
        }

        public Uri Uri { get; private set; }
        public IEnumerable<string> Class { get; private set; }
        public IEnumerable<Link> Children { get; }



        public OneOf<Resource, None> GetChildByUriSegment(string part)
        {
            foreach (var child in Children)
            {
                if (child.Follow != null && child.Uri.ToString() == this.Uri.ToString().TrimEnd('/') + "/" + part)
                    return child.Follow();
            }

            return new None();
        }


        public OneOf<MethodHandler, None> GetMethodHandler(Method method)
        {
            var handler = _methodHandlers.FirstOrDefault(m => m.Method == method);
            if (handler != null) return handler;
            return new None();
        }
    }
}