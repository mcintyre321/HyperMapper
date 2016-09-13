using System;
using System.Linq;
using System.Threading.Tasks;
using HyperMapper.ResourceModel;
using OneOf;
using OneOf.Types;

namespace HyperMapper.RequestHandling
{
    public delegate OneOf<Resource, None> Router(string path);
    public class RequestHandlerBuilder
    {
        public RequestHandler MakeRequestHandler(Uri baseUri, Func<Router> getRouter)
        {
            return async (request, modelBinder) =>
            {

                var requestUri = new Uri(request.Uri.PathAndQuery.ToString(), UriKind.Relative);

                var router = getRouter();
                var target = router(requestUri.ToString().Substring(baseUri.ToString().Length));

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
                                            (Response) new Response.RepresentationResponse(representation.Representation)
                                        ));
                                return response;
                            },
                            none => Task.FromResult((Response) new Response.MethodNotAllowed())),
                    none => Task.FromResult((Response) new Response.NotFoundResponse()));
            };
        }
    }
}