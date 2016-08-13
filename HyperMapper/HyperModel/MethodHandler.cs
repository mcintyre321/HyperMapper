using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HyperMapper.RequestHandling;

namespace HyperMapper.HyperModel
{
    public class MethodHandler
    {
        private readonly Func<IEnumerable<Tuple<Key, object>>, Task<InvokeResult>> _invoke;

        public MethodHandler(string method, Tuple<Key, Type>[] argumentInfo, Func<IEnumerable<Tuple<Key, object>>, Task<InvokeResult>> invoke)
        {
            _invoke = invoke;
            Method = method;
            ArgumentInfo = argumentInfo;
        }

        public Tuple<Key, Type>[] ArgumentInfo { get; }
        public string Method { get; }

        public Task<InvokeResult> Invoke(BoundModel args)
        {
            return _invoke(args.Args);
        }

        
    }
}