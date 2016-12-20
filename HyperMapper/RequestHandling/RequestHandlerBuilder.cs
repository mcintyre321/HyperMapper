using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HyperMapper.ResourceModel;
using OneOf;
using OneOf.Types;

namespace HyperMapper.RequestHandling
{
    public class BaseUrlRelativePath
    {
        private readonly string _path;

        public BaseUrlRelativePath(string path)
        {
            _path = path;
        }

        public IEnumerable<UrlPart> GetParts()
        {
            return _path.Split('/')
                    .Where(p => !String.IsNullOrEmpty(p))
                    .Select(s => new UrlPart(s));
        }
    }

    public delegate OneOf<Resource, None> Router(BaseUrlRelativePath path);
    public class RequestHandlerBuilder
    {
        public RequestHandler MakeRequestHandler(Uri baseUri, Router router)
        {
            return async (request, modelBinder) =>
            {

                var requestUri = new Uri(request.Uri.PathAndQuery.ToString(), UriKind.Relative);

                var path = new BaseUrlRelativePath(requestUri.ToString().Substring(baseUri.ToString().Length));
                var target = router(path);

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