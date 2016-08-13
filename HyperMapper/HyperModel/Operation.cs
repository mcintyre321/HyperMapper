//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using HyperMapper.RequestHandling;

//namespace HyperMapper.HyperModel
//{
//    public class Operation 
//    {
//        private readonly Func<Tuple<Key, object>[], System.Threading.Tasks.Task<object>> _invoke;

//        public Operation(Key key, string title, string method, Uri href, string contentType, ActionField[] fields, Func<Tuple<Key, object>[], Task<object>> invoke, Tuple<Key, Type>[] argumentInfo)
//        {
//            _invoke = invoke;
//            ArgumentInfo = argumentInfo;
//            Key = key;
//            Title = title;
//            Method = method;
//            Href = href;
//            ContentType = contentType;
//            Fields = fields;
//        }

//        public Key Key { get; set; }
//        public string Title { get; private set; }
//        public string Method { get; private set; }
//        public Uri Href { get; private set; }
//        public string ContentType { get; private set; }
//        public IEnumerable<ActionField> Fields { get; private set; }

//        public Tuple<Key, Type>[] ArgumentInfo { get; } 
//        public Task<OperationResult> Invoke(Tuple<Key, object>[] args)
//        {
//            await _invoke(args);
//                return new OperationResult();
//            ;
//        }

        
//    }

//    class OperationResult { }

//}