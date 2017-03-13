using System;
using System.Linq;
using HyperMapper.Mapping;
using HyperMapper.RepresentationModel;
using HyperMapper.RequestHandling;
using HyperMapper.ResourceModel;
using OneOf;
using OneOf.Types;

namespace HyperMapper.Mapper
{
    public class NodeRouting
    {
        public static Func<BaseUrlRelativePath, OneOf<AbstractNode, None>> RouteByWalkingNode(AbstractNode root)
        {
            return path =>
            {
                var parts = path.GetParts();

                return parts
                    .Aggregate((OneOf<AbstractNode, None>) root,
                        (node, part) => node.Match<OneOf<AbstractNode, None>>(
                            childNode =>
                            {
                                var child = childNode.GetChild(part);
                                if (child == null) return new None();
                                return child;
                            },
                            none => none)
                    );
                
            };
        }

 
        public static Router<SemanticDocument> MakeHypermediaRouterFromRootNode(RootNode root, ServiceLocatorDelegate serviceLocator)
        {
            return url =>
            {
                Func<BaseUrlRelativePath, OneOf<AbstractNode, None>> nodeRouter = RouteByWalkingNode(root);

                var router = nodeRouter(url).Match<OneOf<Resource<SemanticDocument>, None>>(
                    node => Functions.MakeResourceFromNode(node, serviceLocator),
                    none => none
                );

                return router;
            };
        }
    }
}