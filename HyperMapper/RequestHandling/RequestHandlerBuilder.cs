using System;
using System.Threading.Tasks;
using HyperMapper.ResourceModel;
using OneOf;
using OneOf.Types;

namespace HyperMapper.RequestHandling
{
    public class RequestHandlerBuilder<TRep>
    {
        public RequestHandler<TRep> MakeRequestHandler(Uri baseUri, Router<TRep> router)
        {
            return async (request, modelBinder) =>
            {

                var requestUri = new Uri(request.Uri.PathAndQuery.ToString(), UriKind.Relative);

                var path = new BaseUrlRelativePath(requestUri.ToString().Substring(baseUri.ToString().Length));
                var target = router(path);

                OneOf<Resource<TRep>, None> oneOf = (await target);
                return await oneOf.Match(
                    resource => resource.GetMethodHandler(request.Method)
                        .Match(
                            async handler =>
                            {
                                var bindResult = await modelBinder(handler.Parameters);
                                var response = await bindResult.Match(
                                    failed => Task.FromResult<Response<TRep>>(new Response<TRep>.ModelBindingFailedResponse()),
                                    async boundModel => (await handler.Invoke(boundModel)).Match(
                                        representation =>
                                            (Response<TRep>) new Response<TRep>.RepresentationResponse(representation.Representation)
                                    ));
                                return response;
                            },
                            none => Task.FromResult((Response<TRep>) new Response<TRep>.MethodNotAllowed())),
                    none => Task.FromResult((Response<TRep>) new Response<TRep>.NotFoundResponse()));
            };
        }
    }
}