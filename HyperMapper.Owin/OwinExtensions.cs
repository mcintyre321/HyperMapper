using System;
using HyperMapper;
using HyperMapper.RequestHandling;
using HyperMapper.ResourceModel;
using Owin;

namespace HyperMapper.Owin
{
    public static class OwinExtensions
    {
        public static IAppBuilder UseHypermedia(this IAppBuilder appBuilder,
            Func<Router> router,
            HyperMapperSettings settings)
        {

            var poco = new RequestHandlerBuilder();
            var requestHandler = poco.MakeRequestHandler(new Uri(settings.BasePath, UriKind.Relative), router);
             

            return appBuilder.Use(OwinInitializers.UseHypermedia(settings, requestHandler));
        }
    }
}