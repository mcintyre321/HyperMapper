using System;
using System.Linq;
using HyperMapper.RequestHandling;
using HyperMapper.ResourceModel;
using OneOf;
using OneOf.Types;

namespace HyperMapper.Mapper
{
    public class Routing
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

 
        public static Router MakeHypermediaRouterFromRootNode(RootNode root, GlossaryNode glossaryNode, ServiceLocatorDelegate serviceLocator)
        {
            return url =>
            {
                //route to the special glossary childnode
                if (url.ToString().Split('/').FirstOrDefault() == glossaryNode.UrlPart)
                    return Functions.MakeResourceFromNode(glossaryNode, serviceLocator);

                var nodeRouter = Routing.RouteByWalkingNode(root);
                return nodeRouter(url).Match<OneOf.OneOf<Resource, None>>(
                    node => Functions.MakeResourceFromNode(node, serviceLocator), none => none);
            };
        }
    }
    
    public delegate Tuple<object, Action> ServiceLocatorDelegate(Type type);

    public static class DefaultServiceLocatorDelegate
    {
        public static ServiceLocatorDelegate CreateUsingEmptyCtorAndDisposeIfAvailable = type =>
        {
            var instance = Activator.CreateInstance(type);

            return Tuple.Create(instance, new Action(() =>
            {
                if (instance is IDisposable)
                {
                    ((IDisposable)instance).Dispose();
                }
            }));
        };

    }

}