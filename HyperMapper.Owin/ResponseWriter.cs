using System;
using System.Linq;
using System.Threading.Tasks;
using HyperMapper.HyperModel;
using HyperMapper.Siren;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HyperMapper.Owin
{
    internal static class ResponseWriter
    {
        public static async Task Write(OwinContext ctx, Resource hypermediaObject, HyperMapperSettings settings)
        {
            Func<string, string, Task> writeStringToResponse = async (contentType, body) =>
            {
                ctx.Response.ContentType = contentType;
                await ctx.Response.WriteAsync(body);
            };

            var accept = ctx.Request.Accept;
            var responseEntity = new SirenMapper().BuildFromHypermedia(hypermediaObject);

            var serializerSettings = settings.JsonSerializerSettings;

            var serializer = JsonSerializer.Create(serializerSettings);
            var objectAsJson = JToken.FromObject(responseEntity, serializer);

            if (accept.Split(',').Contains("application/json"))
            {
                await writeStringToResponse("application/json", objectAsJson.ToString(Formatting.Indented));
            }
            else if (accept.Split(',').Contains("text/html"))
            {
                var index = new HyperMapper.Siren.Index() { Model = objectAsJson };
                var transformText = index.TransformText();
                await writeStringToResponse("text/html", transformText);
            }
        }
    }
}