using System;
using System.Collections.Generic;
using System.Linq;
using HyperMapper.RepresentationModel;
using OneOf;

namespace HyperMapper.ResourceModel
{
    public class Resource
    {
        private readonly IEnumerable<MethodHandler> _methodHandlers;

        public Resource(string title, Uri uri, string[] @class,
            IEnumerable<Link> children, IEnumerable<MethodHandler> methodHandlers)
        {
            _methodHandlers = methodHandlers;
            Title = title;
            Uri = uri;
            Class = @class;
            Children = children;
        }

        public string Title { get; set; }
        public Uri Uri { get; private set; }
        public IEnumerable<string> Class { get; private set; }
        public IEnumerable<Link> Children { get; }



        public OneOf<Resource, OneOf.Types.None> GetChildByUriSegment(string part)
        {
            foreach (var child in Children)
            {
                if (child.Follow != null && child.Uri.ToString() == this.Uri.ToString().TrimEnd('/') + "/" + part)
                    return child.Follow();
            }

            return new OneOf.Types.None();
        }


        public OneOf<MethodHandler, OneOf.Types.None> GetMethodHandler(Method method)
        {
            var handler = _methodHandlers.FirstOrDefault(m => m.Method == method);
            if (handler != null) return handler;
            return new OneOf.Types.None();
        }
    }
}