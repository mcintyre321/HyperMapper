using System.Collections.Generic;
using System.Linq;
using HyperMapper.RepresentationModel;
using OneOf;

namespace HyperMapper.ResourceModel
{
    public class Resource<TRep>
    {
        private readonly IEnumerable<MethodHandler<TRep>> _methodHandlers;

        public Resource(IEnumerable<MethodHandler<TRep>> methodHandlers)
        {
            _methodHandlers = methodHandlers;
        }
         
        

        public OneOf<MethodHandler<TRep>, OneOf.Types.None> GetMethodHandler(Method method)
        {
            var handler = _methodHandlers.FirstOrDefault(m => m.Method == method);
            if (handler != null) return handler;
            return new OneOf.Types.None();
        }
    }
}