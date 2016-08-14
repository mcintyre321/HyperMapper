using System;
using System.Linq;
using System.Threading.Tasks;
using HyperMapper.Model;
using OneOf;

namespace HyperMapper.RequestHandling
{
    public class RequestHandlerBuilder
    {
        public RequestHandler MakeRequestHandler(Uri baseUri, Func<Resource> getRootNode, Func<Type, object> serviceLocator)
        {
            return async (request, modelBinder) =>
            {
                 
                var requestUri = new Uri(request.Uri.PathAndQuery.ToString(), UriKind.Relative);

                var root = getRootNode();
                var parts = requestUri.ToString().Substring(baseUri.ToString().Length).Split('/')
                    .Where(p => !String.IsNullOrEmpty(p));

                var target = parts
                    .Aggregate((OneOf<Resource, None>) root,
                        (x, part) => x.Match(
                            childEntity => childEntity.GetChildByUriSegment(part),
                            none => none)
                    );

                return await target.Match(
                    resource => resource.GetMethodHandler(request.Method)
                        .Match(
                            async handler =>
                            {

                                var bindResult = await modelBinder(handler.Parameters);
                                var response = await bindResult.Match<Task<Response>>(
                                    failed => Task.FromResult<Response>(new Response.ModelBindingFailedResponse()),
                                    async boundModel => (await handler.Invoke(boundModel)).Match(
                                        representation =>
                                            new Response.RepresentationResponse(representation.Representation)
                                        ));
                                return response;
                            },
                            none => Task.FromResult((Response) new Response.MethodNotAllowed())),
                    none => Task.FromResult((Response) new Response.NotFoundResponse()));
            };
        }
    }
}