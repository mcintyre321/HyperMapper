using System;
using HyperMapper;
using HyperMapper.Model;
using HyperMapper.RequestHandling;
using Owin;

namespace HyperMapper.Owin
{
    public static class OwinExtensions
    {
        public static IAppBuilder UseHypermedia(this IAppBuilder appBuilder,
            Func<Resource> getRootResource,
            HyperMapperSettings settings)
        {

            var poco = new RequestHandlerBuilder();
            var requestHandler = poco.MakeRequestHandler(new Uri(settings.BasePath, UriKind.Relative), getRootResource, settings.ServiceLocator);
             

            return appBuilder.Use(OwinInitializers.UseHypermedia(settings, requestHandler));
        }
    }
}