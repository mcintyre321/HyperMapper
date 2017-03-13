using System;
using HyperMapper.Mapper;
using HyperMapper.Mapping;
using HyperMapper.RequestHandling;
using Owin;
using HyperMapper.RepresentationModel;

namespace HyperMapper.Owin
{
    public static class OwinExtensions
    {
        public static IAppBuilder ExposeRootNodeAsHypermediaApi(this IAppBuilder appBuilder, RootNode root, ServiceLocatorDelegate serviceLocatorDelegate, Representor<SemanticDocument>[] representors)
        {
            var router = NodeRouting.MakeHypermediaRouterFromRootNode(root, serviceLocatorDelegate);
            FindUriForTerm locatorDelegate = term => root.GlossaryNode.GetUriForTerm(term);
            return appBuilder.ExposeRouterAsHypermediaApi(router, locatorDelegate, representors, root.Uri.ToString());
        }

        public static IAppBuilder ExposeRouterAsHypermediaApi(this IAppBuilder appBuilder, Router<SemanticDocument> router, FindUriForTerm termUriFinder, Representor<SemanticDocument>[] representors, string basePath)
        {
            var poco = new RequestHandlerBuilder<SemanticDocument>();
            var requestHandler = poco.MakeRequestHandler(new Uri(basePath, UriKind.Relative), router);

            return appBuilder.Use(OwinInitializers.UseRepresentors(representors, requestHandler, termUriFinder, basePath));
        }
    }
}