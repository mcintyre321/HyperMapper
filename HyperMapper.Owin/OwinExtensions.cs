using System;
using HyperMapper;
using HyperMapper.Models;
using HyperMapper.RequestHandling;
using Owin;
using Action = HyperMapper.Models.Action;

namespace HyperMapper.Owin
{
    public static class OwinExtensions
    {
        public static IAppBuilder UseHypermedia(this IAppBuilder appBuilder, string path, Func<RootNode> func)
        {
            var hyperMapperSettings = new HyperMapperSettings()
            {
                BasePath = path
            };
            return UseHypermedia(appBuilder, func , hyperMapperSettings);
        }

        public static IAppBuilder UseHypermedia(this IAppBuilder appBuilder,
            Func<RootNode> getRootNode,
            HyperMapperSettings settings)
        {
            var getRootEntity = new Func<Entity>(() =>
            {
                var entityMapper = new PocoToHypermediaEntityMapper();
                var baseUri = new Uri(settings.BasePath, UriKind.Relative);
                return entityMapper.Map(baseUri, getRootNode(), settings.ServiceLocator);
            });

            return UseHypermedia(appBuilder, getRootEntity, settings);
        }

        public static IAppBuilder UseHypermedia(this IAppBuilder appBuilder,
            Func<Entity> func,
            HyperMapperSettings hyperMapperSettings)
        {

            return appBuilder.Use(OwinInitializers.UseHypermedia(hyperMapperSettings, func));
        }
    }
}