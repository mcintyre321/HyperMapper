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
        public static Func<BaseUrlRelativePath, OneOf<INode, None>> RouteByWalkingNode(INode root)
        {
            return path =>
            {
                var parts = path.GetParts();

                return parts
                    .Aggregate((OneOf<INode, None>) root,
                        (node, part) => node.Match<OneOf<INode, None>>(
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



        //public OneOf<Resource, OneOf.Types.None> GetChildByUriSegment(string part)
        //{
        //    foreach (var child in Children)
        //    {
        //        if (child.Follow != null && child.Uri.ToString() == this.Uri.ToString().TrimEnd('/') + "/" + part)
        //            return child.Follow();
        //    }

        //    return new OneOf.Types.None();
        //}

        public static Router MakeHypermediaRouterFromRootNode(RootNode root, ServiceLocatorDelegate serviceLocator)
        {
            return url =>
            {
                var nodeRouter = Routing.RouteByWalkingNode(root);
                return nodeRouter(url).Match<OneOf.OneOf<Resource, None>>(
                    node => Functions.MakeResourceFromNode(node, serviceLocator), none => none);
            };
        }
    }
    public delegate Tuple<object, Action> ServiceLocatorDelegate(Type type);

}