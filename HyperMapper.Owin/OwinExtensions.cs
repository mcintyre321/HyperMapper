using System;
using HyperMapper.RequestHandling;
using Owin;
using HyperMapper.RepresentationModel;

namespace HyperMapper.Owin
{
    public static class OwinExtensions
    {
        public static IAppBuilder RouteWithRepresentors(this IAppBuilder appBuilder, Router router, FindUriForTerm termUriFinder, Representor[] representors, string basePath)
        {

            var poco = new RequestHandlerBuilder();
            var requestHandler = poco.MakeRequestHandler(new Uri(basePath, UriKind.Relative), router);
             

            return appBuilder.Use(OwinInitializers.UseRepresentors(representors, requestHandler, termUriFinder, basePath));
        }
    }
}