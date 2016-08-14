using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HyperMapper.RequestHandling;

namespace HyperMapper.Model
{
    public class MethodHandler
    {
        private readonly Func<IEnumerable<Tuple<Key, object>>, Task<InvokeResult>> _invoke;

        public MethodHandler(Method method, MethodParameter[] parameters, Func<IEnumerable<Tuple<Key, object>>, Task<InvokeResult>> invoke)
        {
            _invoke = invoke;
            Method = method;
            Parameters = parameters;
        }

        public MethodParameter[] Parameters { get; }
        public Method Method { get; }

        public Task<InvokeResult> Invoke(MethodArguments args)
        {
            return _invoke(args.Args);
        }
    }
}