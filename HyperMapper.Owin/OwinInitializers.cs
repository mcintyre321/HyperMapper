using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HyperMapper.Models;
using HyperMapper.RequestHandling;
using Microsoft.Owin;

namespace HyperMapper.Owin
{
    static class OwinInitializers
    {
        public delegate Task AppFunc(IDictionary<string, object> env);

        public static Func<AppFunc, AppFunc> UseHypermedia(HyperMapperSettings settings, Func<Entity> getRootNode)
        {
            return app => env =>
            {
                var ctx = new OwinContext(env);
                if (!ctx.Request.Path.Value.StartsWith(settings.BasePath.ToString()))
                    return app(env);

                return HandleRequest(settings, ctx, getRootNode);
            };
        }


        private static async Task HandleRequest(HyperMapperSettings settings, OwinContext ctx, Func<Entity> fetchRootEntity)
        {
            var requestUri = ctx.Request.Uri;
            var isInvoke = ctx.Request.Method == "POST";

            RequestHandler.ModelBinder bind = (argumentDesc) =>
                ModelBinder.BindArgsFromRequest(argumentDesc, ctx.Request);

            var hypermediaObject = await RequestHandler.Handle(
                fetchRootEntity,
                settings.BasePath,
                isInvoke,
                settings.ServiceLocator,
                requestUri,
                bind);

            await ResponseWriter.Write(ctx, hypermediaObject, settings);
        }
    }
}