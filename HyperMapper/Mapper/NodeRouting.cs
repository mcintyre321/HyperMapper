using System;
using System.Linq;
using System.Threading.Tasks;
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
        public static Func<BaseUrlRelativePath, Task<OneOf<AbstractNode, None>>> RouteByWalkingNode(AbstractNode root)
        {
            return async path =>
            {
                var parts = path.GetParts();

                Task<OneOf<AbstractNode, None>> fetchRoot = Task.FromResult<OneOf<AbstractNode, None>>(root);
                return await parts
                    .Aggregate(fetchRoot, async (node, part) =>
                    {
                        var oneOf = (await node);
                        return await oneOf.Match(async abstractNode =>
                            {
                                var childNodes = (abstractNode as IHasChildNodes)?.ChildNodes ?? ChildNodes.Empty;

                                var child = childNodes.GetChild(part);
                                if (child == null) return new None();
                                var childNode = child.Item5();
                                return await childNode;
                            },
                            none => Task.FromResult((OneOf<AbstractNode, None>) none));
                    });
                
            };
        }

 
        public static Router<SemanticDocument> MakeHypermediaRouterFromRootNode(RootNode root, ServiceLocatorDelegate serviceLocator)
        {
            return async url =>
            {
                var nodeRouter = RouteByWalkingNode(root);

                var oneOf = await nodeRouter(url);
                var router = oneOf.Match<OneOf<Resource<SemanticDocument>, None>>(
                    node => Functions.MakeResourceFromNode(node, serviceLocator),
                    none => none
                );

                return router;
            };
        }
    }
}