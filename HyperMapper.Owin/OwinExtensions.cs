using System;
using HyperMapper;
using HyperMapper.Mapping;
using HyperMapper.RequestHandling;
using Owin;
using Action = HyperMapper.HyperModel.Action;

namespace HyperMapper.Owin
{
    public static class OwinExtensions
    {
        public static IAppBuilder UseHypermedia(this IAppBuilder appBuilder,
            Func<RootNode> getRootNode,
            HyperMapperSettings settings)
        {

            var poco = new RequestHandlerBuilder();
            var requestHandler = poco.MakeRequestHandler(new Uri(settings.BasePath, UriKind.Relative), getRootNode, settings.ServiceLocator);
             

            return appBuilder.Use(OwinInitializers.UseHypermedia(settings, requestHandler));
        }
    }
}