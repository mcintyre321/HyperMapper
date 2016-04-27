﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyperMapper.HyperModel;
using Newtonsoft.Json.Linq;
using OneOf;
using Action = HyperMapper.HyperModel.Action;

namespace HyperMapper.RequestHandling
{
    public static class RequestHandler
    {
        public delegate Task<Tuple<Key, object>[]> ModelBinder(Tuple<Key, Type>[] argumentDesc);

        public static async Task<Entity> Handle(Func<Entity> getRootNode, string basePath, bool isInvoke, Func<Type, object> serviceLocator, Uri requestUri, ModelBinder bind)
        {
            var rootNode = getRootNode();
            var separator = new [] { '/'};

            var requestUriParts = requestUri.PathAndQuery.ToString()
                .Substring(basePath.Length)
                .Split(separator, StringSplitOptions.RemoveEmptyEntries);

            IWalkable target = rootNode;
            IWalkable prev = null;
            foreach (var requestUriPart in requestUriParts)
            {
                prev = target;
                target = target.Walk(requestUriPart);
            }


            if (isInvoke)
            {
                var action = (Action) target;
                var args = await bind(action.ArgumentInfo);
                var result = await action.Invoke(args);
                target = prev;
            }
            return (Entity) target;
        }
    }
}
