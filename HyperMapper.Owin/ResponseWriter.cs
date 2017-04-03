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
        public static async Task Write<TRep>(OwinContext ctx, TRep representation, FindUriForTerm termUriFinder, Representor<TRep>[] settings)
        {
            Func<string, string, Task> writeStringToResponse = async (contentType, body) =>
            {
                ctx.Response.ContentType = contentType;
                await ctx.Response.WriteAsync(body);
            };

            var accept = ctx.Request.Accept;
            var representor = settings.FirstOrDefault(r => r.AcceptTypes.Intersect(ctx.Request.Accept.Split(',')).Select(h => h.Trim()).Any());
            if (representor != null)
            {
                var response = await representor.GetResponse(representation, termUriFinder);
                await writeStringToResponse(response.Item1, response.Item2);
            }
            else
            {
                 
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}