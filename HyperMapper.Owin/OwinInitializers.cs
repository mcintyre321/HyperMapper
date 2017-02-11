using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HyperMapper.RepresentationModel;
using HyperMapper.RepresentationModel.Vocab;
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

        public static Func<AppFunc, AppFunc> UseRepresentors(Representor[] representors, RequestHandler requestHandler, FindUriForTerm termUriFinder, string basePath)
        {
            return app => env =>
            {
                var ctx = new OwinContext(env);
                if (!ctx.Request.Path.Value.StartsWith(basePath.ToString()))
                    return app(env);

                return HandleRequest(representors, ctx, requestHandler, termUriFinder);
            };
        }


        private static async Task HandleRequest(Representor[] settings, OwinContext ctx, RequestHandler requestHandler, FindUriForTerm termUriFinder)
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
                    await ResponseWriter.Write(ctx, representationResponse.Representation, termUriFinder, settings);
                    return true;
                },
                async createdResponse =>
                {
                    HashSet<Property> properties = new HashSet<Property>()
                    {
                        //new ValueProperty("Message", JToken.FromObject("CREATED"), new Term {new Term("ResultMessage"), }), 
                    };
                    await ResponseWriter.Write(ctx, new Representation(ctx.Request.Uri, properties), termUriFinder, settings);
                    return true;
                });
            
        }
    }
}