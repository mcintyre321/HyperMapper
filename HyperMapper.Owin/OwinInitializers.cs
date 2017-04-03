using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HyperMapper.RepresentationModel;
using HyperMapper.RequestHandling;
using HyperMapper.ResourceModel;
using Microsoft.Owin;
using Newtonsoft.Json.Linq;
using OneOf;

namespace HyperMapper.Owin
{
    static class OwinInitializers
    {
        public delegate Task AppFunc(IDictionary<string, object> env);

        public static Func<AppFunc, AppFunc> UseRepresentors<TRep>(Representor<TRep>[] representors, RequestHandler<TRep> requestHandler, FindUriForTerm termUriFinder, string basePath)
        {
            return app => env =>
            {
                var ctx = new OwinContext(env);
                if (!ctx.Request.Path.Value.StartsWith(basePath.ToString()))
                    return app(env);

                return HandleRequest(representors, ctx, requestHandler, termUriFinder);
            };
        }


        private static async Task HandleRequest<TRep>(Representor<TRep>[] representors, OwinContext ctx, RequestHandler<TRep> requestHandler, FindUriForTerm termUriFinder)
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
                    await ResponseWriter.Write(ctx, representationResponse.Representation, termUriFinder, representors);
                    return true;
                },
                async createdResponse =>
                {
                    await ResponseWriter.Write(ctx, createdResponse.Representation, termUriFinder, representors);
                    return true;
                });
            
        }
    }
}