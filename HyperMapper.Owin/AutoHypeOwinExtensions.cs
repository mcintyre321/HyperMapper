using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HyperMapper;
using HyperMapper.Models;
using HyperMapper.RequestHandling;
using HyperMapper.Siren;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Owin;
using Action = HyperMapper.Models.Action;
using Task = System.Threading.Tasks.Task;

namespace HyperMapper.Owin
{
    public static class AutoHypeOwinExtensions
    {
        private delegate Task AppFunc(IDictionary<string, object> env);


        static Func<AppFunc, AppFunc> UseHypermedia(HyperMapperSettings settings, Func<INode> getRootNode)
        {
            return app => env =>
            {
                var ctx = new OwinContext(env);
                if (!ctx.Request.Path.Value.StartsWith(settings.BasePath.ToString()))
                    return app(env);

                return HandleRequest(settings, getRootNode, ctx);
            };
        }

        private static async Task HandleRequest(HyperMapperSettings settings, Func<INode> getRootNode, OwinContext ctx)
        {
            var requestUri = ctx.Request.Uri;
            var isInvoke = ctx.Request.Method == "POST";
            RequestHandler.ModelBinder bind = (a) => BindArgsFromRequest(a, ctx.Request);

            var hypermediaObject = await RequestHandler.Handle(getRootNode, settings.BasePath, isInvoke, settings.ServiceLocator, requestUri, bind);

            var responseEntity = new SirenMapper().BuildFromHypermedia(hypermediaObject);
            var serializerSettings = settings.JsonSerializerSettings;
            var serializer = JsonSerializer.Create(serializerSettings);
            var objectAsJson = JToken.FromObject(responseEntity, serializer);

            if (ctx.Request.Accept.Split(',').Contains("application/json"))
            {
                ctx.Response.Headers.Set("Content-Type", "application/json");

                await ctx.Response.WriteAsync(objectAsJson.ToString(Formatting.Indented));
            }
            else if (ctx.Request.Accept.Split(',').Contains( "text/html"))
            {
                var index = new HyperMapper.Siren.Index() {Model = objectAsJson };
                var transformText = index.TransformText();
                await ctx.Response.WriteAsync(transformText);
            }
        }



        private static async Task<Tuple<Key, object>[]> BindArgsFromRequest(Tuple<Key, Type>[] argumentDefs,
            IOwinRequest request)
        {
            switch (request.ContentType.Split(';')[0])
            {
                case "application/json":
                {
                    using (var sr = new StreamReader(request.Body))
                    {
                        var rawBody = await sr.ReadToEndAsync();
                        var jObject = JObject.Parse(rawBody);
                        return argumentDefs
                            .Select(ai => Tuple.Create(ai.Item1, jObject[ai.Item1.ToString()]?.ToObject(ai.Item2)))
                            .ToArray();
                    }
                }
                case "application/x-www-form-urlencoded":
                    using (var sr = new StreamReader(request.Body))
                    {
                        var rawBody = await sr.ReadToEndAsync();
                        var dict = QueryStringHelper.QueryStringToDict(rawBody);
                        var jObject = JObject.Parse(JsonConvert.SerializeObject(dict));
                        return argumentDefs
                            .Select(ai => Tuple.Create(ai.Item1, jObject[ai.Item1.ToString()]?.ToObject(ai.Item2)))
                            .ToArray();
                    }
                    break;
            }
            throw new NotImplementedException();
        }



        public static IAppBuilder UseHypermedia(this IAppBuilder appBuilder, string path, Func<INode> func)
        {
            var hyperMapperSettings = new HyperMapperSettings()
            {
                BasePath = path
            };
            return UseHypermedia(appBuilder, func, hyperMapperSettings);
        }

        public static IAppBuilder UseHypermedia(this IAppBuilder appBuilder, Func<INode> func, HyperMapperSettings hyperMapperSettings)
        {
            return appBuilder.Use(UseHypermedia(hyperMapperSettings, func));
        }
    }
}