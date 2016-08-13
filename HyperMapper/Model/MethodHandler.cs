using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HyperMapper.RequestHandling;

namespace HyperMapper.Model
{
    public class MethodHandler
    {
        private readonly Func<IEnumerable<Tuple<Key, object>>, Task<InvokeResult>> _invoke;

        public MethodHandler(Method method, Tuple<Key, Type>[] parameters, Func<IEnumerable<Tuple<Key, object>>, Task<InvokeResult>> invoke)
        {
            _invoke = invoke;
            Method = method;
            Parameters = parameters;
        }

        public Tuple<Key, Type>[] Parameters { get; }
        public Method Method { get; }

        public Task<InvokeResult> Invoke(MethodParameters args)
        {
            return _invoke(args.Args);
        }
    }
}