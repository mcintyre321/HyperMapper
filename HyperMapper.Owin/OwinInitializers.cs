using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HyperMapper.Model;
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
            RequestHandling.BindModel bindModel = args => ModelBinder.BindArgsFromRequest(args, ctx.Request);
            var request = new Request(Method.Parse(ctx.Request.Method), ctx.Request.Uri)
            {
            };
            var response = await requestHandler(request, bindModel);
            var wrote = await response.Match<Task<bool>>(
                methodNotAllowed => Task.FromResult(false),
                notFoundResponse => Task.FromResult(false),
                modelBindingFailedResponse => Task.FromResult(false), 
                async representationResponse =>
                {
                    await ResponseWriter.Write(ctx, representationResponse.Representation, settings);
                    return true;
                });
            
        }
    }
}