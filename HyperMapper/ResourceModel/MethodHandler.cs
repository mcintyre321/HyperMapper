using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HyperMapper.ResourceModel
{
    public class MethodHandler
    {
        public delegate Task<InvokeResult> InvokeMethodDelegate(IEnumerable<Tuple<Key, object>>  args);
        private readonly InvokeMethodDelegate _invoke;

        public MethodHandler(Method method, MethodParameter[] parameters, InvokeMethodDelegate invoke)
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