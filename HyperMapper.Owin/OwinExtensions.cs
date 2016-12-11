using System;
using HyperMapper;
using HyperMapper.RequestHandling;
using HyperMapper.ResourceModel;
using Owin;

namespace HyperMapper.Owin
{
    public static class OwinExtensions
    {
        public static IAppBuilder RouteWithRepresentors(this IAppBuilder appBuilder, Router router, Representor[] representors, string basePath)
        {

            var poco = new RequestHandlerBuilder();
            var requestHandler = poco.MakeRequestHandler(new Uri(basePath, UriKind.Relative), router);
             

            return appBuilder.Use(OwinInitializers.UseRepresentors(representors, requestHandler, basePath));
        }
    }
}