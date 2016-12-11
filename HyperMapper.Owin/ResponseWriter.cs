using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HyperMapper.RepresentationModel;
using HyperMapper.Siren;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HyperMapper.Owin
{
    internal static class ResponseWriter
    {
        public static async Task Write(OwinContext ctx, Representation representation, Representor[] settings)
        {
            Func<string, string, Task> writeStringToResponse = async (contentType, body) =>
            {
                ctx.Response.ContentType = contentType;
                await ctx.Response.WriteAsync(body);
            };

            var accept = ctx.Request.Accept;
            var representor = settings.FirstOrDefault(r => r.AcceptTypes.Contains(ctx.Request.Accept));
            if (representor != null)
            {
                var response = representor.GetResponse(representation);
                await writeStringToResponse(response.Item1, response.Item2);
            }
            else
            {
                if (accept.Split(',').Contains("text/html"))
                {
                    var sirenRep = settings.OfType<SirenRepresentor>().Single();
                    var response = sirenRep.GetResponse(representation);
                    var index = new HyperMapper.Siren.Index() {Model = JToken.Parse(response.Item2)};
                    var transformText = index.TransformText();
                    await writeStringToResponse("text/html", transformText);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}