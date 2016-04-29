using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HyperMapper.HyperModel;
using HyperMapper.RequestHandling;
using Microsoft.Owin;

namespace HyperMapper.Owin
{
    static class OwinInitializers
    {
        public delegate Task AppFunc(IDictionary<string, object> env);

        public static Func<AppFunc, AppFunc> UseHypermedia(HyperMapperSettings settings, RequestHandler requestHandler)
        {
            return app => env =>
            {
                var ctx = new OwinContext(env);
                if (!ctx.Request.Path.Value.StartsWith(settings.BasePath.ToString()))
                    return app(env);

                return HandleRequest(settings, ctx, requestHandler);
            };
        }


        private static async Task HandleRequest(HyperMapperSettings settings, OwinContext ctx, RequestHandler requestHandler)
        {
            RequestHandling.ModelBinder modelBinder = args => ModelBinder.BindArgsFromRequest(args, ctx.Request);
            var hypermediaObject = await requestHandler(ctx.Request.Uri, ctx.Request.Method == "POST", modelBinder);
            await ResponseWriter.Write(ctx, hypermediaObject, settings);
        }
    }
}