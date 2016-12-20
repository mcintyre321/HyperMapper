using System.Collections.Generic;
using System.Linq;
using HyperMapper.RepresentationModel;
using OneOf;

namespace HyperMapper.ResourceModel
{
    public class Resource
    {
        private readonly IEnumerable<MethodHandler> _methodHandlers;

        public Resource(IEnumerable<MethodHandler> methodHandlers)
        {
            _methodHandlers = methodHandlers;
        }
         
        

        public OneOf<MethodHandler, OneOf.Types.None> GetMethodHandler(Method method)
        {
            var handler = _methodHandlers.FirstOrDefault(m => m.Method == method);
            if (handler != null) return handler;
            return new OneOf.Types.None();
        }
    }
}